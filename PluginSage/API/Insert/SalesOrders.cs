using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PluginSage.Helper;
using PluginSage.Interfaces;
using Pub;

namespace PluginSage.API.Insert
{
    public static partial class Insert
    {
        public static string SalesOrders(IDispatchObject busObject, ISessionService session, Record record)
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
                Logger.Error(e.Message);
                throw;
            }
            
            // set key column value to enable editing of record
            try
            {
                SetLogParams("InsertSingleRecord", "nGetNextSalesOrderNo", "", "");
                var data = new object[] {""};
                busObject.InvokeMethodByRef("nGetNextSalesOrderNo", data);
                var keyValue = data[0].ToString();
                var key = keyColumnsObject[0];

                SetLogParams("InsertSingleRecord", "nSetKey", key, keyValue);
                busObject.InvokeMethod("nSetKey", keyValue);
                
                // remove key column as it is already set
                recordObject.Remove(key);
            }
            catch (Exception e)
            {
                var error = GetErrorMessage();
                Logger.Error("Error inserting single record");
                Logger.Error(error);
                Logger.Error(e.Message);
                return error;
            }

            // write out all other columns
            try
            {
                foreach (var col in recordObject)
                {
                    if (col.Value != null)
                    {
                        SetLogParams("InsertSingleRecord", "nSetValue", col.Key, col.Value);
                        busObject.InvokeMethod("nSetValue", col.Key, col.Value.ToString());
                    }
                }

                SetLogParams("InsertSingleRecord", "nWrite", "", "");
                busObject.InvokeMethod("nWrite");
            }
            catch (Exception e)
            {
                var error = GetErrorMessage();
                Logger.Error("Error inserting single record");
                Logger.Error(error);
                Logger.Error(e.Message);
                return error;
            }

            return "";
        }
    }
}