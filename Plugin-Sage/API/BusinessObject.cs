using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Plugin_Sage.Helper;
using Plugin_Sage.Interfaces;
using Pub;

namespace Plugin_Sage.API
{
    public class BusinessObject : IBusinessObject
    {
        private readonly ISessionService _session;
        private readonly IDispatchObject _busObject;

        /// <summary>
        /// Creates a business object
        /// </summary>
        /// <param name="session"></param>
        /// <param name="busObject"></param>
        public BusinessObject(ISessionService session, IDispatchObject busObject)
        {
            _session = session;
            _busObject = busObject;
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
                var metadata = GetMetadata();
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

                return GetRecord(columnsObject);
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
                var metadata = GetMetadata();
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
                    outList.Add(GetRecord(columnsObject));

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
        /// Writes a record back to Sage
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public string UpdateSingleRecord(Record record)
        {
            Dictionary<string, dynamic> recordObject;
            string[] keyColumnsObject = GetKeys();
            
            // convert record json into object
            try
            {
                recordObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(record.DataJson);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

            // set key column value to enable editing of record
            try
            {
                var key = keyColumnsObject[0];
                var keyRecord = recordObject[key];
                _busObject.InvokeMethod("nSetKeyValue", key, keyRecord.ToString());
                _busObject.InvokeMethod("nSetKey");
            }
            catch (Exception e)
            {
                Logger.Error("Error updating single record");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }

            // write out all other columns
            try
            {
                // remove key column as it is already set
                recordObject.Remove(keyColumnsObject[0]);

                foreach (var col in recordObject)
                {
                    _busObject.InvokeMethod("nSetValue", col.Key, col.Value.ToString());
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error updating single record");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                return _session.GetError();
            }
            
            return "";
        }

        /// <summary>
        /// Gets the keys for the current business object
        /// </summary>
        /// <returns></returns>
        public string[] GetKeys()
        {
            // get key columns for current business object
            try
            {
                var keyColumns = _busObject.InvokeMethod("sGetKeyColumns");
                var keyColumnsObject = keyColumns.ToString().Split(System.Convert.ToChar(352));
                return keyColumnsObject;
            }
            catch (Exception e)
            {
                Logger.Error("Error updating single record");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the table metadata that the business object is connected to
        /// </summary>
        /// <returns>the columns of the table and the number of records in the table</returns>
        private (string[] columnsObject, object recordCount) GetMetadata()
        {
            // get metadata
            try
            {
                var dataSources = _busObject.InvokeMethod("sGetDataSources");
                var dataSourcesObject = dataSources.ToString().Split(System.Convert.ToChar(352));

                var columns = _busObject.InvokeMethod("sGetColumns", dataSourcesObject[0]);
                var columnsObject = columns.ToString().Split(System.Convert.ToChar(352));

                var recordCount = _busObject.InvokeMethod("nGetRecordCount", dataSourcesObject[0]);

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
        /// <returns>A record object as a dictionary</returns>
        private Dictionary<string, dynamic> GetRecord(string[] columnsObject)
        {
            // init output record
            var outDic = new Dictionary<string, dynamic>();

            // get information from header table
            try
            {
                // get single record
                var data = new object[] {"", ""};
                _busObject.InvokeMethodByRef("nGetRecord", data);
                var salesOrderObject = data[0].ToString().Split(System.Convert.ToChar(352));

                for (var i = 0; i < columnsObject.Length; i++)
                {
                    outDic[columnsObject[i]] = salesOrderObject[i];
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error getting record");
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }

            return outDic;
        }
    }
}