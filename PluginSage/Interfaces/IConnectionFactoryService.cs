namespace PluginSage.Interfaces
{
    public interface IConnectionFactoryService
    {
        IConnectionService MakeConnectionObject();
        ICommandService MakeCommandObject(string query, IConnectionService connection);
        ITableHelper MakeTableHelper(string dataSource);
    }
}