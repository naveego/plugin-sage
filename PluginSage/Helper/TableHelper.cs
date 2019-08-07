using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PluginSage.Interfaces;
using Pub;

namespace PluginSage.Helper
{
    public class TableHelper : ITableHelper
    {
        public string TableName { get; set; }
        public List<string> Keys { get; set; }

        public TableHelper(string dataSource)
        {
            switch (dataSource)
            {
                case "Sales Orders":
                    TableName = "SO_SalesOrderHeader";
                    Keys = new List<string> {"SalesOrderNo"};
                    return;
                case "Sales Order Detail":
                    TableName = "SO_SalesOrderDetail";
                    Keys = new List<string> {"SalesOrderNo", "LineKey"};
                    return;
                case "Customer Information":
                    TableName = "AR_Customer";
                    Keys = new List<string> {"ARDivisionNo", "CustomerNo"};
                    return;
                case "Invoice History":
                    TableName = "AR_InvoiceHistoryHeader";
                    Keys = new List<string> {"InvoiceNo", "HeaderSeqNo"};
                    return;
                case "Invoice History Detail":
                    TableName = "AR_InvoiceHistoryDetail";
                    Keys = new List<string> {"InvoiceNo", "HeaderSeqNo", "DetailSeqNo"};
                    return;
                case "Item Information":
                    TableName = "CI_Item";
                    Keys = new List<string> {"ItemCode"};
                    return;
                case "Shipping Addresses":
                    TableName = "SO_ShipToAddress";
                    Keys = new List<string> {"ARDivisionNo", "CustomerNo", "ShipToCode"};
                    return;
                default:
                    Logger.Error($"Data source {dataSource} not known. Unable to get config.");
                    return;
            }
        }

        /// <summary>
        /// Builds select query
        /// </summary>
        /// <returns></returns>
        public string GetSelectQuery()
        {
            return $"SELECT * FROM {TableName}";
        }

        /// <summary>
        /// Builds insert query
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public string GetInsertQuery(List<Property> properties, Dictionary<string, string> record)
        {
            var columns = new StringBuilder();
            var values = new StringBuilder();

            foreach (var property in properties)
            {
                var index = property.Id;
                if (record.ContainsKey(index))
                {
                    switch (property.Type)
                    {
                        case PropertyType.Datetime:
                            if (DateTime.TryParse(record[index], out var rawDate))
                            {
                                columns.Append($"{index},");
                                values.Append($"{{d '{rawDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'}},");
                            }
                            break;
                        case PropertyType.Float:
                            if (decimal.TryParse(record[index], out var rawDec))
                            {
                                columns.Append($"{index},");
                                values.Append($"{rawDec},");
                            }
                            break;
                        default:
                            if (!string.IsNullOrEmpty(record[index]))
                            {
                                columns.Append($"{index},");
                                values.Append($"'{record[index].Replace("'", "''")}',");
                            }
                            break;
                    }
                }
            }

            if (columns.Length > 0)
            {
                columns.Length--;
            }

            if (values.Length > 0)
            {
                values.Length--;
            }

            return $"INSERT INTO {TableName} ({columns}) VALUES ({values})";
        }

        /// <summary>
        /// Builds update query
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public string GetUpdateQuery(List<Property> properties, Dictionary<string, string> record)
        {
            var values = new StringBuilder();
            var keys = new StringBuilder();

            foreach (var property in properties)
            {
                var index = property.Id;
                if (!property.IsKey && record.ContainsKey(index))
                {
                    switch (property.Type)
                    {
                        case PropertyType.Datetime:
                            if (DateTime.TryParse(record[index], out var rawDate))
                            {
                                values.Append($"{index}={{d '{rawDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}'}},");
                            }
                            break;
                        case PropertyType.Float:
                            if (decimal.TryParse(record[index], out var rawDec))
                            {
                                values.Append($"{index}={rawDec},");
                            }
                            break;
                        default:
                            if (!string.IsNullOrEmpty(record[index]))
                            {
                                values.Append($"{index}='{record[index].Replace("'", "''")}',");
                            }
                            break;
                    }
                }
            }
            
            if (values.Length > 0)
            {
                values.Length--;
            }

            foreach (var key in Keys)
            {
                keys.Append(key == Keys.Last() ? $"{key}='{record[key]}'" : $" {key}='{record[key]}' AND ");
            }
            
            return $"UPDATE {TableName} SET {values} WHERE {keys}";
        }
    }
}