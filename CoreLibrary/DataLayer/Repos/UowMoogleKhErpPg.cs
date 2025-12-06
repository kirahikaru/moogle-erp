using Microsoft.Extensions.Options;

namespace DataLayer.Repos;

public interface IUowMoogleKhErpPg : IUowMoogleKhErp
{
	
}

public class UowMoogleKhErpPg(IOptionsMonitor<DatabaseConfig> dbConfigs) : UowMoogleKhErp(dbConfigs, DatabaseTypes.POSTGRESQL), IUowMoogleKhErpPg
{
}