using System;
using System.Runtime.InteropServices;
using Plugin_Sage.Helper;

namespace Plugin_Sage.API
{
    public class SessionService
    {
        private DispatchObject _pvx;
        private DispatchObject _oSS;

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
                _oSS.InvokeMethod("nSetUser", settings.User, settings.Pwd);
                _oSS.InvokeMethod("nSetCompany", settings.CompanyCode);
            }
            catch (Exception e)
            {
                Logger.Error(GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        public DispatchObject Getpvx()
        {
            return _pvx;
        }

        public DispatchObject GetoSS()
        {
            return _oSS;
        }

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

        public string GetError()
        {
            return (string) _oSS.GetProperty("sLastErrorMsg");
        }
    }
}