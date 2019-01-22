using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugin_Sage.Helper;

namespace Plugin_Sage.API
{
    public class BusinessObjectService
    {
        private readonly SessionService _session;
        private DispatchObject _busObject;

        public BusinessObjectService(SessionService session)
        {
            _session = session;
        }
        
        public BusinessObjectService(SessionService session, string dataSource)
        {
            _session = session;

            try
            {
                SetBusObject(dataSource);
            }
            catch (Exception e)
            {
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        public void SetBusObject(string dataSource)
        {
            var config = new BusinessObjectConfig(dataSource);

            try
            {
                _session.SetModule(config.Module);
                var taskId = (int) _session.GetoSS().InvokeMethod("nLookupTask", config.HeaderTable);
                _session.GetoSS().InvokeMethod("nSetProgram", taskId);
                _busObject = new DispatchObject(_session.Getpvx()
                    .InvokeMethod("NewObject", config.TableName, _session.GetoSS().GetObject()));
            }
            catch (Exception e)
            {
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }
        }
        
        public Dictionary<string, dynamic> GetSingleRecord()
        {
            // get metadata
            var dataSources = _busObject.InvokeMethod("sGetDataSources");
            var dataSourcesObject = dataSources.ToString().Split(System.Convert.ToChar(352));

            var columns = _busObject.InvokeMethod("sGetColumns", dataSourcesObject[0]);
            var columnsObject = columns.ToString().Split(System.Convert.ToChar(352));

            var recordCount = _busObject.InvokeMethod("nGetRecordCount", dataSourcesObject[0]);

            // init output record
            var salesOrderDic = new Dictionary<string, dynamic>();

            if (recordCount.ToString() == "0")
            {
                return salesOrderDic;
            }
            
            var salesOrder = new object[] {"", ""};
            _busObject.InvokeMethodByRef("nGetRecord", salesOrder);
            var salesOrderObject = salesOrder[0].ToString().Split(System.Convert.ToChar(352));

            
            for (var i = 0; i < columnsObject.Length; i++)
            {
                salesOrderDic[columnsObject[i]] = salesOrderObject[i];
            }

            // get all lines records
            var soLines = new DispatchObject(_busObject.GetProperty("oLines"));
            salesOrderDic["LINES"] = GetLines(soLines);

            return salesOrderDic;
        }

        public List<Dictionary<string, dynamic>> GetAllRecords()
        {
            try
            {
                // get metadata
                var dataSources = _busObject.InvokeMethod("sGetDataSources");
                var dataSourcesObject = dataSources.ToString().Split(System.Convert.ToChar(352));

                var columns = _busObject.InvokeMethod("sGetColumns", dataSourcesObject[0]);
                var columnsObject = columns.ToString().Split(System.Convert.ToChar(352));

                var recordCount = _busObject.InvokeMethod("nGetRecordCount", dataSourcesObject[0]);

                // init output list
                var outList = new List<Dictionary<string, dynamic>>();

                if (recordCount.ToString() == "0")
                {
                    return outList;
                }

                // go to first record
                _busObject.InvokeMethod("nMoveFirst");

                // get all records
                do
                {
                    var salesOrder = new object[] {"", ""};
                    _busObject.InvokeMethodByRef("nGetRecord", salesOrder);
                    var salesOrderObject = salesOrder[0].ToString().Split(System.Convert.ToChar(352));

                    var salesOrderDic = new Dictionary<string, dynamic>();
                    for (var i = 0; i < columnsObject.Length; i++)
                    {
                        salesOrderDic[columnsObject[i]] = salesOrderObject[i];
                    }

                    // get all lines records
                    var soLines = new DispatchObject(_busObject.GetProperty("oLines"));
                    salesOrderDic["LINES"] = GetLines(soLines);

                    outList.Add(salesOrderDic);

                    _busObject.InvokeMethod("nMoveNext");
                } while (_busObject.GetProperty("nEOF").ToString() == "0");

                // return all records
                return outList;
            }
            catch (Exception e)
            {
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        private List<Dictionary<string, dynamic>> GetLines(DispatchObject soLines)
        {
            try
            {
                // get metadata
                var dataSources = soLines.InvokeMethod("sGetDataSources");
                var dataSourcesObject = dataSources.ToString().Split(System.Convert.ToChar(352));

                var columns = soLines.InvokeMethod("sGetColumns", dataSourcesObject[0]);
                var columnsObject = columns.ToString().Split(System.Convert.ToChar(352));

                var recordCount = soLines.InvokeMethod("nGetRecordCount", dataSourcesObject[0]);

                // init output list
                var outList = new List<Dictionary<string, dynamic>>();

                if (recordCount.ToString() == "0")
                {
                    return outList;
                }

                // go to first record
                soLines.InvokeMethod("nMoveFirst");

                // get all records
                do
                {
                    var salesOrderLines = new object[] {"", ""};
                    soLines.InvokeMethodByRef("nGetRecord", salesOrderLines);
                    var salesOrderLinesObject = salesOrderLines[0].ToString().Split(System.Convert.ToChar(352));

                    var salesOrderLinesDic = new Dictionary<string, dynamic>();
                    for (var i = 0; i < columnsObject.Length; i++)
                    {
                        salesOrderLinesDic[columnsObject[i]] = salesOrderLinesObject[i];
                    }

                    outList.Add(salesOrderLinesDic);

                    soLines.InvokeMethod("nMoveNext");
                } while (soLines.GetProperty("nEOF").ToString() == "0");

                // return all records
                return outList;
            }
            catch (Exception e)
            {
                Logger.Error(_session.GetError());
                Logger.Error(e.Message);
                throw;
            }
        }
    }
}