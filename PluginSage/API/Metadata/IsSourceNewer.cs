using System;
using System.Collections.Generic;
using System.Linq;
using Naveego.Sdk.Logging;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginSage.Helper;
using PluginSage.Interfaces;


namespace PluginSage.API.Metadata
{
    public static partial class Metadata
    {
        /// <summary>
        /// Checks if the source system has newer data than the requested write back
        /// </summary>
        /// <param name="busObject"></param>
        /// <param name="session"></param>
        /// <param name="record"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static bool IsSourceNewer(IDispatchObject busObject, ISessionService session, Record record, Schema schema)
        {
            Dictionary<string, dynamic> recordObject;
            string[] columnsObject;
            string[] keyColumnsObject = GetKeys(busObject, session);

            // convert record json into object
            try
            {
                recordObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(record.DataJson);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }

            // get metadata
            try
            {
                var metadata = GetMetadata(busObject, session);
                columnsObject = metadata.columnsObject;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error getting meta data for record date check");
                Logger.Error(e, e.Message);
                Logger.Error(e, session.GetError());
                throw;
            }

            // set key column value 
            try
            {
                var key = keyColumnsObject[0];
                var keyRecord = recordObject[key];
                busObject.InvokeMethod("nSetKeyValue", key, keyRecord.ToString());
                busObject.InvokeMethod("nSetKey");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error setting key for record date check");
                Logger.Error(e, e.Message);
                Logger.Error(e, session.GetError());
                throw;
            }

            // move pointer to records
            try
            {
//                // get source record
//                busObject.InvokeMethod("nFind");
//                var srcRecordObject = GetRecord(columnsObject);
//
//                // get modified key from schema
//                var modifiedKey = schema.Properties.First(x => x.IsUpdateCounter);
//
//                // if source is newer than request then exit
//                if (recordObject.ContainsKey(modifiedKey.Id) && srcRecordObject.ContainsKey(modifiedKey.Id))
//                {
//                    if (recordObject[modifiedKey.Id] != null && srcRecordObject[modifiedKey.Id] != null)
//                    {
//                        return DateTime.Parse((string) recordObject[modifiedKey.Id]) <=
//                               DateTime.Parse((string) srcRecordObject[modifiedKey.Id]);
//                    }
//                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error checking date for record date check");
                Logger.Error(e, e.Message);
                Logger.Error(e, session.GetError());
                return false;
            }
        }
    }
}