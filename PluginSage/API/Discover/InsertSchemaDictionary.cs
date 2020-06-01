using System;
using System.Collections.Generic;
using Naveego.Sdk.Plugins;


namespace PluginSage.API.Discover
{
    public static partial class Discover
    {
        private static readonly Dictionary<string, Schema> InsertSchemaDictionary = new Dictionary<string, Schema>
        {
//            {SalesOrdersModule(), SalesOrders()}
        };

        public static int GetTotalInsertSchemas()
        {
            return InsertSchemaDictionary.Count;
        }
    }
}