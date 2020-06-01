using System.Collections.Generic;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginSage.DataContracts;


namespace PluginSage.API.Discover
{
    public static partial class Discover
    {
        public static string SalesOrdersModule()
        {
            return "Sales Orders Insert";
        }
        
        public static Schema SalesOrders()
        {
            var module = SalesOrdersModule();
            var schema = new Schema
            {
                Id = module,
                Name = module,
                Description = "",
                PublisherMetaJson = JsonConvert.SerializeObject(new PublisherMetaJson
                {
                    Module = module
                }),
                DataFlowDirection = Schema.Types.DataFlowDirection.Write
            };

            var properties = new List<Property>
            {
                new Property
                {
                    Id = "ARDivisionNo$",
                    Name = "ARDivisionNo$",
                    Type = PropertyType.String,
                    IsKey = true,
                    IsCreateCounter = false,
                    IsUpdateCounter = false,
                    TypeAtSource = "string",
                    IsNullable = false
                },
                new Property
                {
                    Id = "CustomerNo$",
                    Name = "CustomerNo$",
                    Type = PropertyType.String,
                    IsKey = true,
                    IsCreateCounter = false,
                    IsUpdateCounter = false,
                    TypeAtSource = "string",
                    IsNullable = false
                },
                new Property
                {
                    Id = "LineItems",
                    Name = "LineItems",
                    Type = PropertyType.Json,
                    IsKey = true,
                    IsCreateCounter = false,
                    IsUpdateCounter = false,
                    TypeAtSource = "json array",
                    IsNullable = false
                },
            };

            schema.Properties.AddRange(properties);

            return schema;
        }
    }
}