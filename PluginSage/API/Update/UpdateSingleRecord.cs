using System;
using System.Collections.Generic;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginSage.Helper;
using PluginSage.Interfaces;


namespace PluginSage.API.Update
{
    public static partial class Update
    {
        /// <summary>
        /// Writes an updated record back to Sage
        /// </summary>
        /// <param name="busObject"></param>
        /// <param name="session"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public static string UpdateSingleRecord(IDispatchObject busObject, ISessionService session, Record record)
        {
            string _command = "";
            string _method = "";
            string _value = "";
            string _variable = "";
            
            void SetLogParams (string method, string command, string variable, string value)
            {
                _method = method;
                _command = command;
                _variable = variable;
                _value = value;
            }
            
            string GetErrorMessage()
            {
                var sessionError = session.GetError();

                return
                    $"Error: {sessionError}, Method: {_method}, Command: {_command}, Variable: {_variable}, Value: {_value}";
            }

            Dictionary<string, dynamic> recordObject;
            string[] keyColumnsObject = Metadata.Metadata.GetKeys(busObject, session);

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

            // set key column value to enable editing of record
            try
            {
                foreach (var key in keyColumnsObject)
                {
                    SetLogParams("UpdateSingleRecord", "nSetKeyValue", key, "");
                    var keyRecord = recordObject[key];
                    SetLogParams("UpdateSingleRecord", "nSetKeyValue", key, keyRecord.ToString());
                    busObject.InvokeMethod("nSetKeyValue", key, keyRecord.ToString());
                }

                SetLogParams("UpdateSingleRecord", "nSetKey", "", "");
                busObject.InvokeMethod("nSetKey");
            }
            catch (Exception e)
            {
                var error = GetErrorMessage();
                Logger.Error(e, "Error updating single record");
                Logger.Error(e, e.Message);
                Logger.Error(e, error);
                return error;
            }

            // write out all other columns
            try
            {
                // remove key column as it is already set
                recordObject.Remove(keyColumnsObject[0]);

                foreach (var col in recordObject)
                {
                    if (col.Value != null)
                    {
                        SetLogParams("UpdateSingleRecord", "nSetValue", col.Key, col.Value.ToString());
                        busObject.InvokeMethod("nSetValue", col.Key, col.Value.ToString());
                    }
                }

                SetLogParams("UpdateSingleRecord", "nWrite", "", "");
                busObject.InvokeMethod("nWrite");
            }
            catch (Exception e)
            {
                var error = GetErrorMessage();
                Logger.Error(e, "Error updating single record");
                Logger.Error(e, e.Message);
                Logger.Error(e, error);
                return error;
            }

            return "";
        }
    }
}