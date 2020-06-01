using System;
using System.Collections.Generic;
using PluginSage.Helper;
using PluginSage.Interfaces;

namespace PluginSage.API.Read
{
    public static partial class Read
    {
        /// <summary>
        /// Gets all records from the table the business object is connected to
        /// </summary>
        /// <param name="busObject"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static List<Dictionary<string, dynamic>> GetAllRecords(IDispatchObject busObject, ISessionService session)
        {
            string[] columnsObject;
            object recordCount;

            // get metadata
            try
            {
                var metadata = Metadata.Metadata.GetMetadata(busObject, session);
                columnsObject = metadata.columnsObject;
                recordCount = metadata.recordCount;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error getting meta data for all records");
                Logger.Error(e, e.Message);
                Logger.Error(e, session.GetError());
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
                busObject.InvokeMethod("nMoveFirst");

                do
                {
                    // add record
                    outList.Add(GetRecord(busObject, session, columnsObject));

                    // move to next record
                    busObject.InvokeMethod("nMoveNext");

                    // keep going until no more records
                } while (busObject.GetProperty("nEOF").ToString() == "0");

                // return all records
                return outList;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error getting all records");
                Logger.Error(e, e.Message);
                Logger.Error(e, session.GetError());
                throw;
            }
        }
    }
}