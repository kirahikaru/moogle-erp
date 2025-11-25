// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using DataLayer.Models.SystemCore.NonPersistent;
using System.Reflection;
using System.Resources;

namespace DataLayer.Repos;
public class ShellBaseRepos<TEntity> : IShellBaseRepos<TEntity> where TEntity : class
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly DatabaseObj _dbObj;
    internal IEnumerable<PropertyInfo> ObjectProperties;
    internal ResourceManager _errMsgResxMngr;

    public ShellBaseRepos(IConnectionFactory connectionFactory, DatabaseObj dbObj)
    {
        _connectionFactory = connectionFactory;

        _dbObj = dbObj;

		ObjectProperties = typeof(TEntity).GetProperties().Where(x => !x.PropertyType.FullName!.StartsWith("DataLayerCore", StringComparison.OrdinalIgnoreCase));

        _errMsgResxMngr = new ResourceManager("ErrorMessages", Assembly.GetExecutingAssembly());
    }

    public DatabaseObj DbObject => _dbObj;

	public IConnectionFactory ConnectionFactory => _connectionFactory;
}