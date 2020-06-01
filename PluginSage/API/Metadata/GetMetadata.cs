using System;
using PluginSage.Helper;
using PluginSage.Interfaces;

namespace PluginSage.API.Metadata
{
    public static partial class Metadata
    {
        /// <summary>
        /// Gets the table metadata that the business object is connected to
        /// </summary>
        /// <returns>the columns of the table and the number of records in the table</returns>
        public static (string[] columnsObject, object recordCount) GetMetadata(IDispatchObject busObject, ISessionService session)
        {
            // get metadata
            try
            {
                var dataSources = busObject.InvokeMethod("sGetDataSources");
                var dataSourcesObject = dataSources.ToString().Split(System.Convert.ToChar(352));

                var columns = busObject.InvokeMethod("sGetColumns", dataSourcesObject[0]);
                var columnsObject = columns.ToString().Split(System.Convert.ToChar(352));

                var recordCount = busObject.InvokeMethod("nGetRecordCount", dataSourcesObject[0]);

                return (columnsObject, recordCount);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error getting metadata");
                Logger.Error(e, e.Message);
                Logger.Error(e, session.GetError());
                throw;
            }
        }
    }
}