using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugin_Sage.API;
using Plugin_Sage.Helper;

namespace Plugin_Sage
{
    ///<summary>
    /// Summary description for Class1.
    ///</summary>
    class Program
    {
        ///<summary>
        /// The main entry point for the application.
        ///</summary>
        [STAThread]
        static void Main(string[] args)
        {
            var settings = new Settings
            {
                User = "DEV",
                Password = "iL7M2BOdC",
                CompanyCode = "ABC",
                HomePath = @"C:\Sage\Sage 100 Advanced\MAS90\Home"
            };

            var sessionSvc = new SessionService(settings);

            var salesOrderSvc = new BusinessObjectService(sessionSvc, "SalesOrders");

            var records = salesOrderSvc.GetAllRecords();

            Console.WriteLine(JsonConvert.SerializeObject(records[9]["LINES"]));
        }
    }
}