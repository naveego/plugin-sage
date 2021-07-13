using System;
using System.Collections.Generic;
using Naveego.Sdk.Logging;
using PluginSage.Helper;
using PluginSage.Interfaces;

namespace PluginSage.API.Read
{
    public static partial class Read
    {
        /// <summary>
        /// Gets a record from the table the business object is connected to
        /// </summary>
        /// <param name="columnsObject"></param>
        /// <param name="busObject"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        private static Dictionary<string, dynamic> GetRecord(IDispatchObject busObject, ISessionService session, string[] columnsObject)
        {
            // init output record
            var outDic = new Dictionary<string, dynamic>();

            // get information from header table
            try
            {
                // get single record
                var data = new object[] {"", ""};
                busObject.InvokeMethodByRef("nGetRecord", data);
                var salesOrderObject = data[0].ToString().Split(System.Convert.ToChar(352));

                for (var i = 0; i < columnsObject.Length; i++)
                {
                    outDic[columnsObject[i]] = salesOrderObject[i];
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                Logger.Error(e, session.GetError());
                throw;
            }

            return outDic;
        }
    }
}