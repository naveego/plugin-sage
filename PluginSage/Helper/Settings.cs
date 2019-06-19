using System;

namespace PluginSage.Helper
{
    public class Settings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CompanyCode { get; set; }
        public string HomePath { get; set; }
        public string[] ModulesList { get; set; }
        
        /// <summary>
        /// Validates the settings input object
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Validate()
        {
            if (String.IsNullOrEmpty(Username))
            {
                throw new Exception("the Username property must be set");
            }
            
            if (String.IsNullOrEmpty(Password))
            {
                throw new Exception("the Password property must be set");
            }
            
            if (String.IsNullOrEmpty(CompanyCode))
            {
                throw new Exception("the CompanyCode property must be set");
            }
            
            if (String.IsNullOrEmpty(HomePath))
            {
                throw new Exception("the HomePath property must be set");
            }
        }
    }
    
    
}