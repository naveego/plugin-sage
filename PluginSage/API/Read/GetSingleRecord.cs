using System;
using System.Collections.Generic;
using PluginSage.Helper;
using PluginSage.Interfaces;

namespace PluginSage.API.Read
{
    public static partial class Read
    {
        /// <summary>
        /// Gets the first record from the table the business object is connected to
        /// </summary>
        /// <param name="busObject"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static Dictionary<string, dynamic> GetSingleRecord(IDispatchObject busObject, ISessionService session)
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
                Logger.Error(e, "Error getting meta data for single record");
                Logger.Error(e, e.Message);
                Logger.Error(e, session.GetError());
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
                busObject.InvokeMethod("nMoveFirst");

                return GetRecord(busObject, session, columnsObject);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error getting single record");
                Logger.Error(e, e.Message);
                Logger.Error(e, session.GetError());
                throw;
            }
        }
    }
}