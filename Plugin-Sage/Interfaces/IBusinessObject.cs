using System.Collections.Generic;

namespace Plugin_Sage.Interfaces
{
    public interface IBusinessObject
    {
        List<Dictionary<string, dynamic>> GetAllRecords();
        Dictionary<string, dynamic> GetSingleRecord();
        string[] GetKeys();
    }
}