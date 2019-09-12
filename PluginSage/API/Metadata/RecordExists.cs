using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PluginSage.Helper;
using PluginSage.Interfaces;
using Pub;

namespace PluginSage.API.Metadata
{
    public static partial class Metadata
    {
        /// <summary>
        /// Checks if a record exists in Sage
        /// </summary>
        /// <param name="busObject"></param>
        /// <param name="session"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public static bool RecordExists(IDispatchObject busObject, ISessionService session, Record record)
        {
            Dictionary<string, dynamic> recordObject;
            string[] keyColumnsObject = GetKeys(busObject, session);

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
                foreach (var key in keyColumnsObject)
                {
                    if (recordObject.ContainsKey(key))
                    {
                        var keyRecord = recordObject[key];
                        busObject.InvokeMethod("nSetKeyValue", key, keyRecord.ToString());
                    }
                }

                busObject.InvokeMethod("nSetKey");
            }
            catch (Exception e)
            {
                Logger.Error("Error finding single record");
                Logger.Error(session.GetError());
                Logger.Error(e.Message);
                return false;
            }

            try
            {
                var retVal = busObject.InvokeMethod("nFind").ToString();
                Logger.Info($"Find: {retVal}");
                return retVal == "1";
            }
            catch (Exception e)
            {
                Logger.Error("Error finding single record");
                Logger.Error(session.GetError());
                Logger.Error(e.Message);
                return false;
            }
        }
    }
}