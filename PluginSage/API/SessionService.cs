using System;
using PluginSage.Helper;
using PluginSage.Interfaces;

namespace PluginSage.API
{
    public class SessionService : ISessionService
    {
        private IDispatchObject _pvx;
        private IDispatchObject _oSS;

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

        /// <summary>
        /// Creates a business object to use for the given module
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public IBusinessObject MakeBusinessObject(string module)
        {
            // get config for the given data source
            var config = new BusinessObjectConfig(module);

            // make the business object
            try
            {
                SetModule(config.Module);
                var taskId = (int) _oSS.InvokeMethod("nLookupTask", config.TaskName);
                _oSS.InvokeMethod("nSetProgram", taskId);
                var busObject =
                    new DispatchObject(_pvx.InvokeMethod("NewObject", config.BusObjectName, _oSS.GetObject()));

                if (config.IsDetails)
                {
                    var linesBusObject = new DispatchObject(busObject.GetProperty("oLines"));
                    return new BusinessObject(this, linesBusObject);
                }

                return new BusinessObject(this, busObject);
            }
            catch (Exception e)
            {
                Logger.Error("Error setting business service object");
                Logger.Error(GetError());
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Sets the module for the session
        /// </summary>
        /// <param name="moduleCode"></param>
        private void SetModule(string moduleCode)
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
    }
}