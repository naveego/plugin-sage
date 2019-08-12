using System;
using System.Collections.Generic;
using Pub;

namespace PluginSage.API.Discover
{
    public static partial class Discover
    {
        private static Dictionary<string, Schema> InsertSchemaDictionary = new Dictionary<string, Schema>
        {
            {SalesOrdersModule(), SalesOrders()}
        };
    }
}