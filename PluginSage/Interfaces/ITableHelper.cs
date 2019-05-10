using System.Collections.Generic;
using Pub;

namespace PluginSage.Interfaces
{
    public interface ITableHelper
    {
        string TableName { get; set; }
        List<string> Keys { get; set; }
        string GetSelectQuery();
        string GetInsertQuery(List<Property> properties, Dictionary<string, string> record);
        string GetUpdateQuery(List<Property> properties, Dictionary<string, string> record);
    }
}