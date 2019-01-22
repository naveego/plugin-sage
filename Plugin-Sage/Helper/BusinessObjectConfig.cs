using System.Collections.Generic;

namespace Plugin_Sage.Helper
{
    public class BusinessObjectConfig
    {
        public string Module { get; }
        public string HeaderTable { get; }
        public string TableName { get; }

        public BusinessObjectConfig(string dataSource)
        {
            switch (dataSource)
            {
                case "Sales Orders":
                    Module = "S/O";
                    HeaderTable = "SO_SalesOrder_ui";
                    TableName = "SO_SalesOrder_bus";
                    return;
                default:
                    Logger.Error($"Data source {dataSource} not known. Unable to get config.");
                    return;
            }
        }
    }
}