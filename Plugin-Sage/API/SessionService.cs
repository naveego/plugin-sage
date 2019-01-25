using System;
using System.Runtime.InteropServices;
using Plugin_Sage.Helper;

namespace Plugin_Sage.API
{
    public class SessionService
    {
        private DispatchObject _pvx;
        private DispatchObject _oSS;

        /// <summary>
        /// Creates a session service based on the provided settings
        /// </summary>
        /// <param name="settings"></param>
        public SessionService(Settings settings)
        {
            try
            {
                _pvx = new DispatchObject("ProvideX.Script");
                _pvx.InvokeMethod("Init", settings.HomePath);

                _oSS = new DispatchObject(_pvx.InvokeMethod("NewObject", "SY_Session"));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

            try
            {
                _oSS.InvokeMethod("nSetUser", settings.Username, settings.Password);
                _oSS.InvokeMethod("nSetCompany", settings.CompanyCode);
            }
            catch (Exception e)
            {
                Logger.Error(GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the pvx member object
        /// </summary>
        /// <returns></returns>
        public DispatchObject Getpvx()
        {
            return _pvx;
        }

        /// <summary>
        /// Gets the oss member object
        /// </summary>
        /// <returns></returns>
        public DispatchObject GetoSS()
        {
            return _oSS;
        }

        /// <summary>
        /// Sets the module for the session
        /// </summary>
        /// <param name="moduleCode"></param>
        public void SetModule(string moduleCode)
        {
            try
            {
                var date = DateTime.Now.ToString("MMddyyyy");
                _oSS.InvokeMethod("nSetDate", moduleCode, date);
                _oSS.InvokeMethod("nSetModule", moduleCode);
            }
            catch (Exception e)
            {
                Logger.Error(GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Returns the object describing the users current security permissions
        /// </summary>
        /// <returns></returns>
        public string GetSecurityAccess()
        {
            try
            {
                return _oSS.GetProperty("oSecurity").ToString();
            }
            catch (Exception e)
            {
                Logger.Error(GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the most recent error created within the session
        /// </summary>
        /// <returns></returns>
        public string GetError()
        {
            try
            {
                return (string) _oSS.GetProperty("sLastErrorMsg");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }
    }
}