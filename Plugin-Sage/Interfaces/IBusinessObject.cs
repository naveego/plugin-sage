using System.Collections.Generic;
using Pub;

namespace Plugin_Sage.Interfaces
{
    public interface IBusinessObject
    {
        List<Dictionary<string, dynamic>> GetAllRecords();
        Dictionary<string, dynamic> GetSingleRecord();
        string UpdateSingleRecord(Record record);
    }
}