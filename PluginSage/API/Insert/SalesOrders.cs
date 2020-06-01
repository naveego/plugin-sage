using System;
using System.Collections.Generic;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginSage.DataContracts;
using PluginSage.Helper;
using PluginSage.Interfaces;


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
            
            Dictionary<string, object> recordObject;
            string[] keyColumnsObject = Metadata.Metadata.GetKeys(busObject, session);

            // convert record json into object
            try
            {
                recordObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(record.DataJson);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
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
                Logger.Error(e, "Error inserting single record");
                Logger.Error(e, e.Message);
                Logger.Error(e, error);
                return error;
            }

            // set and validate all required properties
            var recordKey = "ARDivisionNo$";
            if (recordObject.ContainsKey(recordKey))
            {
                var recordValue = recordObject[recordKey];
                if (recordValue != null)
                {
                    try
                    {
                        SetLogParams("InsertSingleRecord", "nSetValue", recordKey, recordValue.ToString());
                        busObject.InvokeMethod("nSetValue", recordKey, recordValue.ToString());
                        recordObject.Remove(recordKey);
                    }
                    catch (Exception e)
                    {
                        var error = GetErrorMessage();
                        Logger.Error(e, "Error inserting single record");
                        Logger.Error(e, e.Message);
                        Logger.Error(e, error);
                        return error;
                    }
                }
                else
                {
                    return $"{recordKey} was null";
                }
            }
            else
            {
                return $"{recordKey} must be set";
            }

            recordKey = "CustomerNo$";
            if (recordObject.ContainsKey(recordKey))
            {
                var recordValue = recordObject[recordKey];
                if (recordValue != null)
                {
                    try
                    {
                        SetLogParams("InsertSingleRecord", "nSetValue", recordKey, recordValue.ToString());
                        busObject.InvokeMethod("nSetValue", recordKey, recordValue.ToString());
                        recordObject.Remove(recordKey);
                    }
                    catch (Exception e)
                    {
                        var error = GetErrorMessage();
                        Logger.Error(e, "Error inserting single record");
                        Logger.Error(e, e.Message);
                        Logger.Error(e, error);
                        return error;
                    }
                }
                else
                {
                    return $"{recordKey} was null";
                }
            }
            else
            {
                return $"{recordKey} must be set";
            }

            recordKey = "LineItems";
            if (recordObject.ContainsKey(recordKey))
            {
                var recordValue = recordObject[recordKey];
                if (recordValue != null)
                {
                    
                    try
                    {
                        var linesBusObject = new DispatchObject(busObject.GetProperty("oLines"));
                        var lineItems = JsonConvert.DeserializeObject<List<LineItem>>(JsonConvert.SerializeObject(recordValue));

                        foreach (var lineItem in lineItems)
                        {
                            SetLogParams("InsertSingleRecord", "nSetValue", recordKey, lineItem.ItemCode);
                            linesBusObject.InvokeMethod("nAddLine");
                            linesBusObject.InvokeMethod("nSetValue", "ItemCode$", lineItem.ItemCode);
                            linesBusObject.InvokeMethod("nSetValue", "QuantityOrdered", lineItem.QuantityOrdered.ToString());
                        }
                        
                        recordObject.Remove(recordKey);
                    }
                    catch (Exception e)
                    {
                        var error = GetErrorMessage();
                        Logger.Error(e, "Error inserting single record");
                        Logger.Error(e, e.Message);
                        Logger.Error(e, error);
                        return error;
                    }
                }
                else
                {
                    return $"{recordKey} was null";
                }
            }
            else
            {
                return $"{recordKey} must be set";
            }

            // write out all other columns
            try
            {
                foreach (var col in recordObject)
                {
                    if (col.Value != null)
                    {
                        SetLogParams("InsertSingleRecord", "nSetValue", col.Key, col.Value.ToString());
                        busObject.InvokeMethod("nSetValue", col.Key, col.Value.ToString());
                    }
                }

                SetLogParams("InsertSingleRecord", "nWrite", "", "");
                busObject.InvokeMethod("nWrite");
            }
            catch (Exception e)
            {
                var error = GetErrorMessage();
                Logger.Error(e, "Error inserting single record");
                Logger.Error(e, e.Message);
                Logger.Error(e, error);
                return error;
            }

            return "";
        }
    }
}