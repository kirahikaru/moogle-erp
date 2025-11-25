using System.Reflection;

namespace DataLayer.Repos.SystemCore;

public interface IDocumentTemplateRepos : IBaseRepos<DocumentTemplate>
{
	Task<DocumentTemplate?> GetLatestAsync(string templateCode, string languageCode);
	Task<List<DocumentTemplate>> GetByDocumentTypeCodeAsync(string documentTypeCode, List<string>? languageCodes = null);
	List<string> GetAllApplicableModels();
}

public class DocumentTemplateRepos(IConnectionFactory connectionFactory) : BaseRepos<DocumentTemplate>(connectionFactory, DocumentTemplate.DatabaseObject), IDocumentTemplateRepos
{
	public List<string> GetAllApplicableModels()
    {
        List<string> result = [];
        Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "DataLayerCore")!;

        Type[] types = [.. assembly.GetTypes().Where(x => x.Namespace != null && x.Namespace.StartsWith("DataLayer.Models.PAS"))];

        foreach (Type type in types)
        {
			Type[] implementedInterfaces = [.. ((TypeInfo)type).ImplementedInterfaces];

            if (implementedInterfaces.Any(x => x.Name == "IDocumentTemplateModelEnabled"))
                result.Add(type.Name);
        }

        return result;
    }

    public async Task<DocumentTemplate?> GetLatestAsync(string templateCode, string languageCode)
    {
        var sql = $"SELECT * FROM {DocumentTemplate.MsSqlTable} WHERE IsDeleted=0 AND ObjectCode=@ObjectCode AND LanguageCode=@LanguageCode AND IsInUsed=1";

        var param = new { ObjectCode = templateCode, LanguageCode = languageCode };

        using var cn = ConnectionFactory.GetDbConnection()!;

        return await cn.QuerySingleOrDefaultAsync<DocumentTemplate?>(sql, param).ConfigureAwait(false);
    }

    public async Task<List<DocumentTemplate>> GetByDocumentTypeCodeAsync(string documentTypeCode, List<string>? languageCodes = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();
        sbSql.Select("*");
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.DocumentTypeCode=@DocumentTypeCode");

        param.Add("@DocumentTypeCode", documentTypeCode, DbType.AnsiString);

        if (languageCodes != null && languageCodes.Count != 0)
        {
            if (languageCodes.Count == 1)
            {
                sbSql.Where("t.LanguageCode=@LanguageCode");
                param.Add("@LanguageCode", languageCodes[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.LanguageCode IN @LanguageCodeList");
                param.Add("@LanguageCodeList", languageCodes);
            }
        }

        var sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = ConnectionFactory.GetDbConnection()!;

        List<DocumentTemplate> result = (await cn.QueryAsync<DocumentTemplate>(sql, param)).AsList();

        return result;
    }
}