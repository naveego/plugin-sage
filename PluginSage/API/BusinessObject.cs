using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PluginSage.API.Insert;
using PluginSage.Helper;
using PluginSage.Interfaces;
using Pub;

namespace PluginSage.API
{
    public class BusinessObject : IBusinessObject
    {
        private readonly ISessionService _session;
        private readonly IDispatchObject _busObject;

        private string _command = "";
        private string _method = "";
        private string _value = "";
        private string _variable = "";


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
            return Read.Read.GetSingleRecord(_busObject, _session);
        }

        /// <summary>
        /// Gets all records from the table the business object is connected to
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string, dynamic>> GetAllRecords()
        {
            return Read.Read.GetAllRecords(_busObject, _session);
        }

        /// <summary>
        /// Writes an updated record back to Sage
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public string UpdateSingleRecord(Record record)
        {
            return Update.Update.UpdateSingleRecord(_busObject, _session, record);
        }

        /// <summary>
        /// Writes a new record back to Sage
        /// </summary>
        /// <param name="record"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public string InsertSingleRecord(Record record, string module)
        {
            switch (module)
            {
                case "Sales Orders Insert":
                    return Insert.Insert.SalesOrders(_busObject, _session, record);
                default:
                    return $"Requested module: {module} does not support insert writebacks";
            }
        }

        /// <summary>
        /// Checks if a record exists in Sage
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public bool RecordExists(Record record)
        {
            return Metadata.Metadata.RecordExists(_busObject, _session, record);
        }

        /// <summary>
        /// Gets the keys for the current business object
        /// </summary>
        /// <returns></returns>
        public string[] GetKeys()
        {
            return Metadata.Metadata.GetKeys(_busObject, _session);
        }


        /// <summary>
        /// Checks if the source system has newer data than the requested write back
        /// </summary>
        /// <param name="record"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public bool IsSourceNewer(Record record, Schema schema)
        {
            return Metadata.Metadata.IsSourceNewer(_busObject, _session, record, schema);
        }

        private void SetLogParams(string method, string command, string variable, string value)
        {
            _method = method;
            _command = command;
            _variable = variable;
            _value = value;
        }

        private string GetErrorMessage()
        {
            var sessionError = _session.GetError();

            return
                $"Error: {sessionError}, Method: {_method}, Command: {_command}, Variable: {_variable}, Value: {_value}";
        }
    }
}