namespace DataLayer.Repos.SysCore;

public interface IPersonInfoDocRepos : IBaseRepos<PersonInfoDoc>
{
	Task<IEnumerable<PersonInfoDoc>> GetByPersonAsync(int personId);
	Task<IEnumerable<PersonInfoDoc>> GetByPersonAsync(string personCode);
}

public class PersonInfoDocRepos(IDbContext dbContext) : BaseRepos<PersonInfoDoc>(dbContext, PersonInfoDoc.DatabaseObject), IPersonInfoDocRepos
{

	public override async Task<KeyValuePair<int, IEnumerable<PersonInfoDoc>>> SearchNewAsync(
		int pgSize = 0, int pgNo = 0, string? searchText = null,
		IEnumerable<SqlSortCond>? sortConds = null,
		IEnumerable<SqlFilterCond>? filterConds = null,
		List<int>? excludeIdList = null)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");

		#region Form Search Conditions
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:"))
			{
				sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+@SearchText+'%'");
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.CurrentCultureIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("(UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%' OR UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%')");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}

		if (excludeIdList != null && excludeIdList.Count != 0)
		{
			sbSql.Where("t.Id NOT IN @ExcludeIdList");
			param.Add("@ExcludeIdList", excludeIdList);
		}
        #endregion

        sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.IssuingCountryCode");
		sbSql.LeftJoin($"{DocumentType.MsSqlTable} docType ON docType.IsDeleted=0 AND docType.ObjectCode=t.DocumentTypeCode");
		sbSql.LeftJoin($"{Document.MsSqlTable} doc ON doc.IsDeleted=0 AND doc.Id=t.DocumentId");

		foreach (string order in GetSearchOrderbBy())
        {
            sbSql.OrderBy(order);
        }

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		}
		else
		{
			param.Add("@PageSize", pgSize);
			param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate(
				$";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
				$"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
		}

		using var cn = DbContext.DbCxn;

		var dataList = await cn.QueryAsync<PersonInfoDoc, Country, DocumentType, Document, PersonInfoDoc>(sql, 
            (obj, cty, docType, doc) => {
                obj.IssuingCountry = cty;
                obj.DocumentType = docType;
                obj.Document = doc;

                return obj;
            }, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}


	public async Task<IEnumerable<PersonInfoDoc>> GetByPersonAsync(int personId)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.PersonId=@PersonId");
		param.Add("@PersonId", personId);

		sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.IssuingCountryCode");
		sbSql.LeftJoin($"{DocumentType.MsSqlTable} docType ON docType.IsDeleted=0 AND docType.ObjectCode=t.DocumentTypeCode");
		sbSql.LeftJoin($"{Document.MsSqlTable} doc ON doc.IsDeleted=0 AND doc.Id=t.DocumentId");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		var dataList = await cn.QueryAsync<PersonInfoDoc, Country, DocumentType, Document, PersonInfoDoc>(sql,
			(obj, cty, docType, doc) => {
				obj.IssuingCountry = cty;
				obj.DocumentType = docType;
				obj.Document = doc;

				return obj;
			}, param, splitOn: "Id");


		return dataList;
	}
	public async Task<IEnumerable<PersonInfoDoc>> GetByPersonAsync(string personCode)
	{
		DynamicParameters param = new();
		SqlBuilder sbSql = new();

		sbSql.Where("t.IsDeleted=0");
		sbSql.Where("t.PersonCode=@PersonCode");
		param.Add("@PersonCode", personCode);

		sbSql.LeftJoin($"{Country.MsSqlTable} cty ON cty.IsDeleted=0 AND cty.ObjectCode=t.IssuingCountryCode");
		sbSql.LeftJoin($"{DocumentType.MsSqlTable} docType ON docType.IsDeleted=0 AND docType.ObjectCode=t.DocumentTypeCode");
		sbSql.LeftJoin($"{Document.MsSqlTable} doc ON doc.IsDeleted=0 AND doc.Id=t.DocumentId");

		using var cn = DbContext.DbCxn;
		string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
		var dataList = await cn.QueryAsync<PersonInfoDoc, Country, DocumentType, Document, PersonInfoDoc>(sql,
		(obj, cty, docType, doc) => {
			obj.IssuingCountry = cty;
			obj.DocumentType = docType;
			obj.Document = doc;

			return obj;
		}, param, splitOn: "Id");


		return dataList;
	}
}