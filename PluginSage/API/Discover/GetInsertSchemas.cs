using System.Collections.Generic;
using Naveego.Sdk.Plugins;


namespace PluginSage.API.Discover
{
    public static partial class Discover
    {
        public static List<Schema> GetAllInsertSchemas()
        {
            var schemas = new List<Schema>();

            foreach (var schema in InsertSchemaDictionary)
            {
                schemas.Add(schema.Value);
            }
            
            return schemas;
        }

        public static Schema GetInsertSchemaForModule(string module)
        {
            return InsertSchemaDictionary[module];
        }
    }
}