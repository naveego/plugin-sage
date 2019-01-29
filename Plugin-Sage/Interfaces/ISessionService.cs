using Plugin_Sage.Plugin;

namespace Plugin_Sage.Interfaces
{
    public interface ISessionService
    {
        IBusinessObject MakeBusinessObject(string module);
        string GetError();
    }
}