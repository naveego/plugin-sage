using System;
using PluginSage.Helper;
using PluginSage.Interfaces;

namespace PluginSage.API.Metadata
{
    public static partial class Metadata
    {
        /// <summary>
        /// Gets the keys for the current business object
        /// </summary>
        /// <param name="busObject"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static string[] GetKeys(IDispatchObject busObject, ISessionService session)
        {
            // get key columns for current business object
            try
            {
                var keyColumns = busObject.InvokeMethod("sGetKeyColumns");
                var keyColumnsObject = keyColumns.ToString().Split(System.Convert.ToChar(352));
                return keyColumnsObject;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error getting keys");
                Logger.Error(e, e.Message);
                Logger.Error(e, session.GetError());
                throw;
            }
        }
    }
}