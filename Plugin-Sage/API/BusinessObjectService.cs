using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Plugin_Sage.Helper;

namespace Plugin_Sage.API
{
    public class BusinessObjectService
    {
        private readonly SessionService _session;
        private DispatchObject _busObject;

        /// <summary>
        /// Creates a service without a preset business object
        /// </summary>
        /// <param name="session"></param>
        public BusinessObjectService(SessionService session)
        {
            _session = session;
        }

        /// <summary>
        /// Creates a s service with a preset business object based on the data source
        /// </summary>
        /// <param name="session"></param>
        /// <param name="dataSource"></param>
        public BusinessObjectService(SessionService session, string dataSource)
        {
            _session = session;

            try
            {
                SetBusObject(dataSource);
            }
            catch (Exception e)
            {
                Logger.Error("Error creating business service object");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Sets the business object based on the data source
        /// </summary>
        /// <param name="dataSource"></param>
        public void SetBusObject(string dataSource)
        {
            // get config for the given data source
            var config = new BusinessObjectConfig(dataSource);

            // set the business object
            try
            {
                _session.SetModule(config.Module);
                var taskId = (int) _session.GetoSS().InvokeMethod("nLookupTask", config.TaskName);
                _session.GetoSS().InvokeMethod("nSetProgram", taskId);
                _busObject = new DispatchObject(_session.Getpvx()
                    .InvokeMethod("NewObject", config.BusObjectName, _session.GetoSS().GetObject()));
            }
            catch (Exception e)
            {
                Logger.Error("Error setting business service object");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the first record from the table the business object is connected to
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, dynamic> GetSingleRecord()
        {
            string[] columnsObject;
            object recordCount;

            // get metadata
            try
            {
                var metadata = GetMetadata(_busObject);
                columnsObject = metadata.columnsObject;
                recordCount = metadata.recordCount;
            }
            catch (Exception e)
            {
                Logger.Error("Error getting meta data for single record");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }

            // get information from tables
            try
            {
                // return empty if no records
                if (recordCount.ToString() == "0")
                {
                    return new Dictionary<string, dynamic>();
                }

                // go to first record
                _busObject.InvokeMethod("nMoveFirst");

                return GetRecord(columnsObject, _busObject);
            }
            catch (Exception e)
            {
                Logger.Error("Error getting single record");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets all records from the table the business object is connected to
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string, dynamic>> GetAllRecords()
        {
            string[] columnsObject;
            object recordCount;

            // get metadata
            try
            {
                var metadata = GetMetadata(_busObject);
                columnsObject = metadata.columnsObject;
                recordCount = metadata.recordCount;
            }
            catch (Exception e)
            {
                Logger.Error("Error getting meta data for all records");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }

            // init output list
            var outList = new List<Dictionary<string, dynamic>>();

            // get information from tables
            try
            {
                // return empty if no records
                if (recordCount.ToString() == "0")
                {
                    return new List<Dictionary<string, dynamic>>();
                }

                // go to first record
                _busObject.InvokeMethod("nMoveFirst");

                do
                {
                    // add record
                    outList.Add(GetRecord(columnsObject, _busObject));

                    // move to next record
                    _busObject.InvokeMethod("nMoveNext");

                    // keep going until no more records
                } while (_busObject.GetProperty("nEOF").ToString() == "0");

                // return all records
                return outList;
            }
            catch (Exception e)
            {
                Logger.Error("Error getting all records");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the table metadata that the business object is connected to
        /// </summary>
        /// <param name="busObject"></param>
        /// <returns>the columns of the table and the number of records in the table</returns>
        private (string[] columnsObject, object recordCount) GetMetadata(DispatchObject busObject)
        {
            // get metadata
            try
            {
                var dataSources = busObject.InvokeMethod("sGetDataSources");
                var dataSourcesObject = dataSources.ToString().Split(System.Convert.ToChar(352));

                var columns = busObject.InvokeMethod("sGetColumns", dataSourcesObject[0]);
                var columnsObject = columns.ToString().Split(System.Convert.ToChar(352));

                var recordCount = busObject.InvokeMethod("nGetRecordCount", dataSourcesObject[0]);

                return (columnsObject, recordCount);
            }
            catch (Exception e)
            {
                Logger.Error("Error getting metadata");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets a record from the table the business object is connected to
        /// </summary>
        /// <param name="columnsObject"></param>
        /// <param name="busObject"></param>
        /// <returns>A record object as a dictionary</returns>
        private Dictionary<string, dynamic> GetRecord(string[] columnsObject, DispatchObject busObject)
        {
            // init output record
            var outDic = new Dictionary<string, dynamic>();

            // get information from header table
            try
            {
                // get single record
                var data = new object[] {"", ""};
                busObject.InvokeMethodByRef("nGetRecord", data);
                var salesOrderObject = data[0].ToString().Split(System.Convert.ToChar(352));

                for (var i = 0; i < columnsObject.Length; i++)
                {
                    outDic[columnsObject[i]] = salesOrderObject[i];
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error getting record header");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }

            return outDic;
        }
    }
}