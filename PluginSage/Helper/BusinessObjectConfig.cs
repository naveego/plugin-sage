using Naveego.Sdk.Logging;

namespace PluginSage.Helper
{
    public class BusinessObjectConfig
    {
        public string Module { get; }
        public string BusObjectName { get; }
        public string TaskName { get; }
        public bool IsDetails { get; }

        /// <summary>
        /// Creates a config object for defined sage data sources
        /// Data sources added as customer need dictates
        /// Data source names come from the ModulesList in the manifest.json
        /// </summary>
        /// <param name="dataSource"></param>
        public BusinessObjectConfig(string dataSource)
        {
            switch (dataSource)
            {
                case "Sales Orders":
                    Module = "S/O";
                    BusObjectName = "SO_SalesOrder_bus";
                    TaskName = "SO_SalesOrder_ui";
                    IsDetails = false;
                    return;
                case "Sales Order Detail":
                    Module = "S/O";
                    BusObjectName = "SO_SalesOrder_bus";
                    TaskName = "SO_SalesOrder_ui";
                    IsDetails = true;
                    return;
                case "Customer Information":
                    Module = "A/R";
                    BusObjectName = "AR_Customer_bus";
                    TaskName = "AR_Customer_ui";
                    IsDetails = false;
                    return;
                case "Invoice History":
                    Module = "A/R";
                    BusObjectName = "AR_InvoiceHistoryInquiry_bus";
                    TaskName = "AR_InvoiceHistoryInquiry_ui";
                    IsDetails = false;
                    return;
                case "Invoice History Detail":
                    Module = "A/R";
                    BusObjectName = "AR_InvoiceHistoryInquiryDetail_bus";
                    TaskName = "AR_InvoiceHistoryInquiry_ui";
                    IsDetails = false;
                    return;
                case "Item Information":
                    Module = "C/I";
                    BusObjectName = "CI_ItemCode_bus";
                    TaskName = "CI_ItemCode_ui";
                    IsDetails = false;
                    return;
                case "Shipping Addresses":
                    Module = "S/O";
                    BusObjectName = "SO_ShipToAddress_bus";
                    TaskName = "SO_ShipToAddress_ui";
                    IsDetails = false;
                    return;
                default:
                    Logger.Error(null, $"Data source {dataSource} not known. Unable to get config.");
                    return;
            }
        }
    }
}