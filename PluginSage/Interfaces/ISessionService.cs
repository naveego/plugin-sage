namespace PluginSage.Interfaces
{
    public interface ISessionService
    {
        IBusinessObject MakeBusinessObject(string module);
        string GetError();
    }
}