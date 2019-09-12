using Newtonsoft.Json;

namespace PluginSage.DataContracts
{
    public class LineItem
    {
        [JsonProperty("")]
        public string ItemCode { get; set; }
        public int QuantityOrdered { get; set; }
    }
}