using Microsoft.Extensions.Options;

namespace DataLayer.Repos;

public interface IUowMoogleKhErpPg : IUowMoogleKhErp
{
	
}

public class UowMoogleKhErpPg : UowMoogleKhErp, IUowMoogleKhErpPg
{
    public UowMoogleKhErpPg(IOptionsMonitor<DatabaseConfig> dbConfigs) : base(dbConfigs, DatabaseTypes.POSTGRESQL)
    {

    }
}