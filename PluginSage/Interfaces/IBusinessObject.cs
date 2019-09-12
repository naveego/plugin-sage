using System.Collections.Generic;
using Pub;

namespace PluginSage.Interfaces
{
    public interface IBusinessObject
    {
        List<Dictionary<string, dynamic>> GetAllRecords();
        Dictionary<string, dynamic> GetSingleRecord();
        string UpdateSingleRecord(Record record);
        string InsertSingleRecord(Record record, string module);
        bool RecordExists(Record record);
        string[] GetKeys();
        bool IsSourceNewer(Record record, Schema schema);
    }
}