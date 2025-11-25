namespace DataLayer.Repos.SysCore;

public interface ITermAndConditionRepos : IBaseRepos<TermAndCondition>
{
	Task<List<TermAndCondition>> GetByLinkedObjectAsync(string linkedObjectType, int? linkedObjectId, string linkedRecordID, string languageCode = "");
	Task<bool> IsTermAndConditionAcceptedAsync(string userId,
											   string documentTemplateCode,
											   string languageCode,
											   string version,
											   string linkedObjectType,
											   int? linkedObjectId,
											   string linkedRecordID);
}

public class TermAndConditionRepos(IDbContext dbContext) : BaseRepos<TermAndCondition>(dbContext, TermAndCondition.DatabaseObject), ITermAndConditionRepos
{
	public async Task<List<TermAndCondition>> GetByLinkedObjectAsync(string linkedObjectType, int? linkedObjectId, string linkedRecordID, string languageCode = "")
    {
        SqlBuilder sbSql = new();

        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");

        param.Add("@LinkedObjectType", linkedObjectType);

        if (linkedObjectId != null)
        {
            sbSql.Where("t.LinkedObjectId=@LinkedObjectId");
            param.Add("@LinkedObjectId", linkedObjectId.Value);
        }

        if (linkedRecordID.IsAtLeast(1))
        {
            sbSql.Where("t.LinkedRecordID=@LinkedRecordID");
            param.Add("@LinkedRecordID", linkedRecordID, DbType.AnsiString);
        }

        if (languageCode.IsAtLeast(1))
        {
            sbSql.Where("t.LanguageCode=@LanguageCode");
            param.Add("@LanguageCode", languageCode, DbType.AnsiString);
        }

        using var cn = DbContext.DbCxn;
        var sql = sbSql.AddTemplate($"SELECT * FROM {TermAndCondition.MsSqlTable} /**where**/").RawSql;
        return (await cn.QueryAsync<TermAndCondition>(sql, param)).ToList();
    }

    public async Task<bool> IsTermAndConditionAcceptedAsync(string userId,
                                               string documentTemplateCode,
                                               string languageCode,
                                               string version,
                                               string linkedObjectType,
                                               int? linkedObjectId,
                                               string linkedRecordID)
    {

        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.IsAccepted=1");
        sbSql.Where("t.AcceptedUserId=@AcceptedUserId");
        sbSql.Where("t.DocumentTemplateCode=@DocumentTemplateCode");
        sbSql.Where("t.ContentVersion=@ContentVersion");
        sbSql.Where("t.LinkedObjectType=@LinkedObjectType");

        DynamicParameters param = new();
        param.Add("@AcceptedUserId", userId);
        //param.Add("@LanguageCode", languageCode);
        param.Add("@DocumentTemplateCode", documentTemplateCode, DbType.AnsiString);
        param.Add("@ContentVersion", version);
        param.Add("@LinkedObjectType", linkedObjectType, DbType.AnsiString);

        if (linkedObjectId != null)
        {
            sbSql.Where("t.LinkedObjectId=@LinkedObjectId");
            param.Add("@LinkedObjectId", linkedObjectId.Value, DbType.Int32);
        }

        if (linkedRecordID.IsAtLeast(1))
        {
            sbSql.Where("t.LinkedRecordID=@LinkedRecordID");
            param.Add("@LinkedRecordID", linkedRecordID, DbType.AnsiString);
        }

        if (languageCode.IsAtLeast(1))
        {
            sbSql.Where("t.LanguageCode=@LanguageCode");
            param.Add("@LanguageCode", languageCode, DbType.AnsiString);
        }

        using var cn = DbContext.DbCxn;
        var sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {TermAndCondition.MsSqlTable} /**where**/").RawSql;
        int count = await cn.ExecuteScalarAsync<int>(sql, param);
        return count > 0;
    }
}