// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using DataLayer.Models.SysCore.NonPersistent;
using System.Reflection;
using System.Resources;

namespace DataLayer.Repos;

public interface IShellBaseRepos<TEntity> where TEntity : class
{
	IDbContext DbContext { get; }
	// warning CA1716: Rename virtual/interface member IBaseRepos<TEntity>.Get(int) so that it no longer conflicts with the reserved language keyword 'Get'. Using a reserved keyword as the name of a virtual/interface member makes it harder for consumers in other languages to override/implement the member.

	//virtual TEntity Get(int id);
	//TEntity GetByCode(string objectCode);
	//List<TEntity> GetAll();
	//int Insert(TEntity entity);
	//int Update(TEntity entity);
	//int Delete(int id, string username);
	//int HardDelete(int id);
	//bool IsDuplicatedCode(int objectId, string objectCode);
	//int GetExistingObjectIdByCode(int objectId, string objectCode);
	////int GetPageCount(int pageSize);

	//Task<TEntity> GetAsync(int Id);
	//Task<TEntity> GetByCodeAsync(string objectCode);
	//Task<int> GetPageCountAsync(int pageSize);
	//Task<List<TEntity>> GetAllAsync();
	//Task<int> InsertAsync(TEntity entity);
	//Task<int> UpdateAsync(TEntity entity);
	//Task<int> DeleteAsync(int id, string username);
	//Task<int> HardDeleteAsync(int id);
	//Task<bool> IsDuplicateCodeAsync(int objectId, string objectCode);
	//Task<int> GetExistingObjectIdByCodeAsync(int objectId, string objectCode);
}
public class ShellBaseRepos<TEntity>(IDbContext dbContext, DatabaseObj dbObj) : IShellBaseRepos<TEntity> where TEntity : class
{
    private readonly IDbContext _dbContext = dbContext;
    private readonly DatabaseObj _dbObj = dbObj;
    internal IEnumerable<PropertyInfo> ObjectProperties = typeof(TEntity).GetProperties().Where(x => !x.PropertyType.FullName!.StartsWith("DataLayerCore", StringComparison.OrdinalIgnoreCase));
    internal ResourceManager _errMsgResxMngr = new ResourceManager("ErrorMessages", Assembly.GetExecutingAssembly());

	public DatabaseObj DbObject => _dbObj;

	public IDbContext DbContext => _dbContext;
}