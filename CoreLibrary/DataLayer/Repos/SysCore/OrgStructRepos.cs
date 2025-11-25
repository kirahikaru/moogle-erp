using static Dapper.SqlMapper;

namespace DataLayer.Repos.SysCore;

/// <summary>
/// Organization Structure
/// </summary>
public interface IOrgStructRepos : IBaseRepos<OrgStruct>
{
	Task<List<OrgStruct>> SearchAsync(
		int pgSize = 0,
		int pgNo = 0,
		string? objectCode = null,
		string? objectName = null,
		int? parentId = null,
		int? orgStructTypeId = null,
		string? hierarchyPath = null,
		int? defaultConfidentialityLevel = null);

	Task<DataPagination> GetSearchPaginationAsync(
		int pgSize = 0,
		string? objectCode = null,
		string? objectName = null,
		int? parentId = null,
		int? orgStructTypeId = null,
		string? hierarchyPath = null,
		int? defaultConfidentialityLevel = null);

	/// <summary>
	/// Get list of Organization structure that can be valid parent of a given organization structure
	/// E.g. a valid parent of an organization structure whose type is 'Function' would be a list of organization structure which type is 'Department'/'Company'
	/// </summary>
	/// <param name="orgStruct"></param>
	/// <returns></returns>
	Task<List<OrgStruct>> GetValidParentsAsync(OrgStruct orgStruct);

	Task<OrgStruct?> GetByUserAsync(string? userUserId);
	Task<OrgStruct?> GetByUserAsync(int userId);

	Task<OrgStruct?> GetFullAsync(int id);

	Task<int> InsertFullAsync(OrgStruct orgStruct, User user);

	Task<bool> UpdateFullAsync(OrgStruct orgStruct, User user);

	Task<bool> DeleteFullAsync(OrgStruct orgStruct, User user);

	Task<DataResult<OrgStruct>> GetForSelectMenuAsync(
		int pgSize = 0, int pgNo = 0,
		string? searchText = null,
		List<int>? excludeIdList = null);

	Task<List<DropDownListItem>> GetValidParentsAsync(int objectId, string objectCode, string hierarchyPath, int objOrgLevel, string? searchText = null);
	Task<List<DropDownListItem>> GetAllWithChildAsync();
}


public class OrgStructRepos(IDbContext dbContext) : BaseRepos<OrgStruct>(dbContext, OrgStruct.DatabaseObject), IOrgStructRepos
{
	public override async Task<KeyValuePair<int, IEnumerable<OrgStruct>>> SearchNewAsync(
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

		sbSql.LeftJoin($"{OrgStruct.MsSqlTable} pr ON pr.Id=t.ParentId");
		sbSql.LeftJoin($"{OrgStructType.MsSqlTable} typ ON typ.Id=t.OrgStructTypeId");

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

		var dataList = await cn.QueryAsync<OrgStruct, OrgStruct, OrgStructType, OrgStruct>(
				sql, (obj, parent, type) =>
				{
					obj.Parent = parent;
                    obj.Type = type;

					return obj;
				}, param, splitOn: "Id");

		string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;
		int dataCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
		return new(dataCount, dataList);
	}

	public async Task<OrgStruct?> FindByCodeAsync(string code)
    {
        string sql = $"SELECT * FROM {OrgStruct.MsSqlTable} WHERE IsDeleted=0 and ObjectCode=@Code";

        using var cn = DbContext.DbCxn;

        return await cn.QuerySingleOrDefaultAsync<OrgStruct>(sql, new { code });
    }

	public override async Task<List<OrgStruct>> QuickSearchAsync(int pgSize = 0, int pgNo = 0, string? searchText = null, List<int>? excludeIdList = null)
	{
		if (pgNo < 0 && pgSize < 0)
			throw new ArgumentOutOfRangeException(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

		DynamicParameters param = new();
		SqlBuilder sbSql = new();
		sbSql.Where("t.IsDeleted=0");

		#region Form Search Condition
		if (!string.IsNullOrEmpty(searchText))
		{
			if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else if (searchText.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
			{
				sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText.Replace("code:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
			}
			else
			{
				sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
				param.Add("@SearchText", searchText, DbType.AnsiString);
			}
		}
        #endregion

        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=t.OrgStructTypeId");
		sbSql.LeftJoin($"{OrgStruct.MsSqlTable} pr ON pr.Id=t.ParentId");

		sbSql.OrderBy("t.HierarchyPath ASC");
		sbSql.OrderBy("t.ObjectName ASC");

		string sql;

		if (pgNo == 0 && pgSize == 0)
		{
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
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

		var dataList = (await cn.QueryAsync<OrgStruct, OrgStructType, OrgStruct, OrgStruct>(sql,
							(obj, orgStructType, parent) => {
								obj.Type = orgStructType;
								obj.Parent = parent;
								return obj;
							}, param, splitOn: "Id")).AsList();

		return dataList;
	}

	/// <summary>
	/// Search Organization Structure
	/// </summary>
	public async Task<List<OrgStruct>> SearchAsync(
        int pgSize = 0,
        int pgNo = 0,
        string? objectCode = null,
        string? objectName = null,
        int? parentId = null,
        int? orgStructTypeId = null,
        string? hierarchyPath = null,
        int? defaultConfidentialityLevel = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (parentId != null && parentId > 0)
        {
            sbSql.Where("t.ParentId=@ParentId");
            param.Add("@ParentId", parentId);
        }

        if (orgStructTypeId != null && orgStructTypeId > 0)
        {
            sbSql.Where("t.OrgStructTypeId=@OrgStructTypeId");
            param.Add("@OrgStructTypeId", orgStructTypeId);
        }

        if (!string.IsNullOrEmpty(hierarchyPath))
        {
            sbSql.Where("t.HierarchyPath LIKE @HierarchyPath+'%'");
            param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);
        }

        if (defaultConfidentialityLevel != null && defaultConfidentialityLevel > 0)
        {
            sbSql.Where("t.DefaultConfidentialityLevel=@DefaultConfidentialityLevel");
            param.Add("@DefaultConfidentialityLevel", defaultConfidentialityLevel);
        }
        #endregion

        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=t.OrgStructTypeId");
		sbSql.LeftJoin($"{OrgStruct.MsSqlTable} pr ON pr.Id=t.ParentId");

		sbSql.OrderBy("t.HierarchyPath ASC");
		sbSql.OrderBy("t.ObjectName ASC");

		string sql;

        if (pgNo == 0 && pgSize == 0)
        {
			sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            //throw new NotImplementedException();
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
			sql = sbSql.AddTemplate(
                $";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                $"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.Id IN (SELECT Id FROM pg) /**orderby**/").RawSql;
        }


        using var cn = DbContext.DbCxn;

        var dataList = (await cn.QueryAsync<OrgStruct, OrgStructType, OrgStruct, OrgStruct>(sql, 
                            (obj, orgStructType, parent) => {
                                obj.Type = orgStructType;
                                obj.Parent = parent;
                                return obj;
                            },param , splitOn:"Id")).AsList();

        return dataList;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        string? objectCode = null,
        string? objectName = null,
        int? parentId = null,
        int? orgStructTypeId = null,
        string? hierarchyPath = null,
        int? defaultConfidentialityLevel = null)
    {
        if (pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();
        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+@ObjectCode+'%'");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@ObjectName)+'%'");
            param.Add("@ObjectName", objectName, DbType.AnsiString);
        }

        if (parentId != null && parentId > 0)
        {
            sbSql.Where("t.ParentId=@ParentId");
            param.Add("@ParentId", parentId);
        }

        if (orgStructTypeId != null && orgStructTypeId > 0)
        {
            sbSql.Where("t.OrgStructTypeId=@OrgStructTypeId");
            param.Add("@OrgStructTypeId", orgStructTypeId);
        }

        if (!string.IsNullOrEmpty(hierarchyPath))
        {
            sbSql.Where("t.HierarchyPath LIKE @HierarchyPath+'%'");
            param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);
        }

        if (defaultConfidentialityLevel != null && defaultConfidentialityLevel > 0)
        {
            sbSql.Where("t.DefaultConfidentialityLevel=@DefaultConfidentialityLevel");
            param.Add("@DefaultConfidentialityLevel", defaultConfidentialityLevel);
        }
        #endregion

        var sbSqlTempl = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/");

        using var cn = DbContext.DbCxn;

		decimal recordCount = await cn.ExecuteScalarAsync<int>(sbSqlTempl.RawSql, param);
		int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

		DataPagination pagingResult = new()
        {
            ObjectType = typeof(OrgStruct).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagingResult;
    }

    public async Task<List<OrgStruct>> GetValidParentsAsync(OrgStruct orgStruct)
    {
        if (orgStruct == null)
            throw new ArgumentNullException(nameof(orgStruct), _errMsgResxMngr.GetString("Null", CultureInfo.CurrentUICulture));

        var fndOrgStructTypeQry = $"SELECT * FROM {OrgStructType.MsSqlTable} c " +
                                  $"LEFT JOIN {OrgStructType.MsSqlTable} p ON p.IsDeleted=0 AND p.Id=c.ParentId " +
                                  $"WHERE c.IsDeleted=0 AND c.Id=@OrgStructTypeId";
        var param = new { orgStruct.OrgStructTypeId};


        using var cn = DbContext.DbCxn;

        OrgStructType? orgStructType = (await cn.QueryAsync<OrgStructType, OrgStructType, OrgStructType>(
                                                            fndOrgStructTypeQry, (child, parent) =>
                                                            {
                                                                child.Parent = parent;
                                                                return child;
                                                            }, param, splitOn: "Id")).FirstOrDefault();


        if (orgStructType != null)
        {
            SqlBuilder sbSql = new();
            sbSql.LeftJoin($"{OrgStruct.MsSqlTable} os ON os.IsDeleted=0 AND os.Id=t.ParentId");
            sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.IsDeleted=0 AND ost.Id=t.OrgStructTypeId");
            sbSql.Where("t.IsDeleted=0");
            sbSql.Where("t.OrgStructTypeId=@OrgStructTypeId");

            var sqlTempl = sbSql.AddTemplate($"SELECT * FROM {OrgStruct.MsSqlTable} t /**leftjoin**/ /**where**/");

            List<OrgStruct> result = (await cn.QueryAsync<OrgStruct, OrgStruct, OrgStructType, OrgStruct>(
                                                    sqlTempl.RawSql, (child, parent, type) =>
                                                    {
                                                        child.Parent = parent;
                                                        child.Type = type;
                                                        return child;
                                                    }, new { OrgStructTypeId = orgStructType.ParentId }, splitOn: "Id")).ToList();
            return result;
        }
        else
            return new();
    }

    public async Task<OrgStruct?> GetByUserAsync(string? userUserId)
    {
        var sql = $"SELECT os.*, ost.* " +
                  $"FROM {User.MsSqlTable} u " +
                  $"LEFT JOIN {OrgStruct.MsSqlTable} os ON os.Id=u.OrgStructId " +
                  $"LEFT JOIN {OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId " +
                  $"WHERE u.IsDeleted=0 AND u.UserId=@UserId";

        var param = new { UserId = new DbString { Value = userUserId, IsAnsi = true } };

        using var cn = DbContext.DbCxn;

        OrgStruct? result = (await cn.QueryAsync<OrgStruct, OrgStructType, OrgStruct>(
                                                sql, (orgStruct, orgStructType) =>
                                                {
                                                    orgStruct.Type = orgStructType;

                                                    return orgStruct;
                                                }, param, splitOn: "Id")).SingleOrDefault();

        return result;
    }

    public async Task<OrgStruct?> GetByUserAsync(int userId)
    {
        var sql = $"SELECT os.*, ost.* FROM {User.MsSqlTable} u " +
                  $"LEFT JOIN {OrgStruct.MsSqlTable} os " +
                  $"LEFT JOIN {OrgStructType.MsSqlTable} ost ON ost.Id=os.OrgStructTypeId " +
                  $"WHERE u.IsDeleted=0 AND u.Id=@Id";

        var param = new { Id = userId };

        using var cn = DbContext.DbCxn;
        OrgStruct? result = (await cn.QueryAsync<OrgStruct, OrgStructType, OrgStruct>(
                                                sql, (orgStruct, orgStructType) =>
                                                {
                                                    orgStruct.Type = orgStructType;

                                                    return orgStruct;
                                                }, param, splitOn: "Id")).SingleOrDefault();

        return result;
    }

    public async Task<OrgStruct?> GetFullAsync(int id)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.Id=@Id");

        sbSql.LeftJoin($"{DbObject.MsSqlTable} pr ON pr.IsDeleted=0 AND pr.Id=t.ParentId");
        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=t.OrgStructTypeId");

        string sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**leftjoin**/ WHERE t.IsDeleted=0 AND t.Id=@Id").RawSql;

        //var commCtgQry = $"SELECT ctg.* FROM {CommunicationCategory.MsSqlTable} ctg " +
        //                 $"INNER JOIN {CommCategoryOrgStructureMapping.MsSqlTable} map ON map.IsDeleted=0 AND map.CommunicationCategoryId=ctg.Id " +
        //                 $"WHERE map.OrgStructId IS NOT NULL and map.OrgStructId=@Id";

        var param = new { Id = id };

        using var cn = DbContext.DbCxn;
        var data = (await cn.QueryAsync<OrgStruct, OrgStruct, OrgStructType, OrgStruct>(
                                                sql, (obj, parent, type) =>
                                                {
													obj.Parent = parent;
													obj.Type = type;
                                                    return obj;
                                                }, param, splitOn: "Id")).FirstOrDefault();

        return data;
    }

    public async Task<int> InsertFullAsync(OrgStruct orgStruct, User user)
    {
        if (orgStruct == null || orgStruct.Id > 0)
            throw new Exception();

        if (user == null || user.Id <= 0)
            throw new Exception();

        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        orgStruct.CreatedUser = user.UserName;
        orgStruct.ModifiedUser = user.UserName;
        orgStruct.CreatedDateTime = khTimestamp;
        orgStruct.ModifiedDateTime = khTimestamp;

        var insOrgStructQry = QueryGenerator.GenerateInsertQuery(orgStruct.GetType(), OrgStruct.MsSqlTable);
        //var insCommCtgMapQry = QueryGenerator.GenerateInsertQuery(typeof(CommCategoryOrgStructureMapping), CommCategoryOrgStructureMapping.DatabaseObject);

        using var cn = DbContext.DbCxn;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using (var tran = cn.BeginTransaction())
        {
            try
            {
                int objId = await cn.QuerySingleOrDefaultAsync<int>(insOrgStructQry, orgStruct, tran);

                //if (objId > 0 && orgStruct.ApplicableCommunicationCategories != null)
                //{
                //    foreach (CommunicationCategory ctg in orgStruct.ApplicableCommunicationCategories)
                //    {
                //        if (ctg.Id <= 0) continue;

                //        CommCategoryOrgStructureMapping mapping = new CommCategoryOrgStructureMapping
                //        {
                //            CommunicationCategoryId = ctg.Id,
                //            OrgStructId = objId,
                //            CreatedUser = user.UserName,
                //            ModifiedUser = user.UserName,
                //            CreatedDateTime = khTimestamp,
                //            ModifiedDateTime = khTimestamp
                //        };

                //        await cn.QuerySingleOrDefaultAsync<int>(insCommCtgMapQry, mapping, tran);
                //    }
                //}

                tran.Commit();
                return objId;
            }
            catch (Exception)
            {
                tran.Rollback();
                return -1;
            }
        }
    }

    public async Task<bool> UpdateFullAsync(OrgStruct orgStruct, User user)
    {
        if (orgStruct == null || orgStruct.Id <= 0)     //error if object is null or new, update must happen on existing object
            throw new Exception();

        if (user == null || user.Id <= 0)       //error if user is null or new
            throw new Exception();

        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        orgStruct.CreatedUser = user.UserName;
        orgStruct.ModifiedUser = user.UserName;
        orgStruct.CreatedDateTime = khTimestamp;
        orgStruct.ModifiedDateTime = khTimestamp;

        var updOrgStructQry = QueryGenerator.GenerateUpdateQuery(orgStruct.GetType(), OrgStruct.MsSqlTable);
        //var insCommCtgMapQry = QueryGenerator.GenerateInsertQuery(typeof(CommCategoryOrgStructureMapping), CommCategoryOrgStructureMapping.DatabaseObject);
        //var delCommCtgMapQry = $"UPDATE {CommCategoryOrgStructureMapping.MsSqlTable} SET IsDeleted=1, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime " +
        //                       $"WHERE IsDeleted=0 AND CommunicationCategoryId=@CommunicationCategoryId AND OrgStructId=@OrgStructId";

        using var cn = DbContext.DbCxn;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

		using var tran = cn.BeginTransaction();
		try
		{
			int updCount = await cn.ExecuteAsync(updOrgStructQry, orgStruct, tran);

			if (updCount > 0)
			{
				//if (orgStruct.ApplicableCommunicationCategories != null)
				//{
				//    foreach (CommunicationCategory ctg in orgStruct.ApplicableCommunicationCategories)
				//    {
				//        var selectComm = $"SELECT COUNT(DISTINCT 1) FROM {CommCategoryOrgStructureMapping.MsSqlTable} WHERE " +
				//            $"CommunicationCategoryId=@CommunicationCategoryId AND OrgStructId=@OrgStructId AND IsDeleted=0";
				//        var selectParam = new { CommunicationCategoryId = ctg.Id, OrgStructId = orgStruct.Id };
				//        var isExisted = await cn.ExecuteScalarAsync<bool>(selectComm, selectParam, tran);

				//        if (!isExisted)
				//        {
				//            CommCategoryOrgStructureMapping mapping = new CommCategoryOrgStructureMapping
				//            {
				//                CommunicationCategoryId = ctg.Id,
				//                OrgStructId = orgStruct.Id,
				//                CreatedUser = user.UserName,
				//                ModifiedUser = user.UserName,
				//                CreatedDateTime = khTimestamp,
				//                ModifiedDateTime = khTimestamp
				//            };

				//            await cn.QuerySingleOrDefaultAsync<int>(insCommCtgMapQry, mapping, tran);
				//        }
				//        else if (ctg.IsDeleted)
				//        {
				//            var param = new { CommunicationCategoryId = ctg.Id, OrgStructId = orgStruct.Id, ModifiedUser = user.UserName, ModifiedDateTime = khTimestamp };
				//            await cn.ExecuteAsync(delCommCtgMapQry, param, tran);
				//        }
				//    }
				//}
			}

			tran.Commit();
			return updCount > 0;
		}
		catch
		{
			tran.Rollback();
			throw;
		}
	}

    public async Task<bool> DeleteFullAsync(OrgStruct orgStruct, User user)
    {
        if (orgStruct == null || orgStruct.Id <= 0) //error if object is null or new, update must happen on existing object
            throw new Exception();

        if (user == null || user.Id <= 0) //error if user is null or new
            throw new Exception();

        DateTime khTimestamp = DateTime.UtcNow.AddHours(7);

        orgStruct.CreatedUser = user.UserName;
        orgStruct.ModifiedUser = user.UserName;
        orgStruct.CreatedDateTime = khTimestamp;
        orgStruct.ModifiedDateTime = khTimestamp;

        var deleteOrgStruc = QueryGenerator.GenerateDeleteQuery(OrgStruct.MsSqlTable);
        //var delCommCtgMapQry = $"UPDATE {CommCategoryOrgStructureMapping.MsSqlTable} SET IsDeleted=1, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime " +
        //           $"WHERE IsDeleted=0 AND CommunicationCategoryId=@CommunicationCategoryId AND OrgStructId=@OrgStructId";

        using var cn = DbContext.DbCxn;
        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();
        using var tran = cn.BeginTransaction();

        try
        {
            int delCount = await cn.ExecuteAsync(deleteOrgStruc, orgStruct, tran);

            //if (orgStruct.ApplicableCommunicationCategories != null)
            //{
            //    foreach (CommunicationCategory ctg in orgStruct.ApplicableCommunicationCategories)
            //    {
            //        if (ctg.Id > 0)
            //        {
            //            var comCateDeleteParam = new
            //            {
            //                ModifiedUser = user.UserName,
            //                ModifiedDateTime = khTimestamp,
            //                CommunicationCategoryId = ctg.Id,
            //                OrgStructId = orgStruct.Id
            //            };

            //            await cn.ExecuteAsync(delCommCtgMapQry, comCateDeleteParam, tran);
            //        }
            //    }
            //}

            tran.Commit();
            return delCount > 0;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    public async Task<DataResult<OrgStruct>> GetForSelectMenuAsync(int pgSize = 0, int pgNo = 0, 
        string? searchText = null, 
        List<int>? excludeIdList = null)
    {
        if (pgNo < 0 && pgSize < 0)
            throw new Exception(_errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("id:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else if (searchText.StartsWith("code:", StringComparison.OrdinalIgnoreCase))
            {
                sbSql.Where("UPPER(t.ObjectCode) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText.Replace("code:", "", StringComparison.OrdinalIgnoreCase), DbType.AnsiString);
            }
            else
            {
                sbSql.Where("UPPER(t.ObjectName) LIKE '%'+UPPER(@SearchText)+'%'");
                param.Add("@SearchText", searchText, DbType.AnsiString);
            }
        }

		if (excludeIdList != null && excludeIdList.Count != 0)
        {
            sbSql.Where("t.Id NOT IN @ExcludeIdList");
            param.Add("@ExcludeIdList", excludeIdList);
        }
        #endregion

        string sql;
        sbSql.OrderBy("t.ObjectName ASC");

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);
            sql = sbSql.AddTemplate($";WITH pg AS (SELECT t.Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT * FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        DataResult<OrgStruct> dataResult = new();

        dataResult.Records = (await cn.QueryAsync<OrgStruct>(sql, param)).AsList();

        string sqlCount = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sqlCount, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        dataResult.Pagination = new()
        {
            ObjectType = typeof(OrgStruct).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return dataResult;
    }

    public async Task<List<DropDownListItem>> GetValidParentsAsync(
        int objectId, 
        string objectCode, 
        string hierarchyPath, 
        int objOrgLevel, 
        string? searchText = null)
    {
        DynamicParameters param = new();
        SqlBuilder sbSql = new();

        sbSql.Select("'ObjectId'=t.Id")
            .Select("t.ObjectCode")
            .Select("t.ObjectName")
            .Select("t.HierarchyPath");

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("ost.OrgLevel<@ObjOrgLevel");
        sbSql.Where("t.Id<>@ObjectId");
        param.Add("@ObjectId", objectId);
        param.Add("@ObjOrgLevel", objOrgLevel);

        if (objectId > 0)
        {
            sbSql.Where("t.Id<>@Id");
            param.Add("@Id", objectId);
        }

        if (!string.IsNullOrEmpty(objectCode))
        {
            sbSql.Where("t.ObjectCode<>@ObjectCode");
            param.Add("@ObjectCode", objectCode, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(hierarchyPath))
        {
            sbSql.Where("t.HierarchyPath NOT LIKE @HierarchyPath+'>%'");
            param.Add("@HierarchyPath", hierarchyPath, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            sbSql.Where("LOWER(t.ObjectName) LIKE '%'+LOWER(@SearchText)+'%'");
            param.Add("@SearchText", searchText, DbType.AnsiString);
        }

        sbSql.LeftJoin($"{OrgStructType.MsSqlTable} ost ON ost.Id=t.OrgStructTypeId");
        
        sbSql.OrderBy("t.HierarchyPath");
		sbSql.OrderBy("t.ObjectName");

		using var cn = DbContext.DbCxn;

        string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        var dataList = (await cn.QueryAsync<DropDownListItem>(sql, param)).AsList();

        return dataList;
    }

	public async Task<List<DropDownListItem>> GetAllWithChildAsync()
    {
		SqlBuilder sbSql = new();

		sbSql.Select("'ObjectId'=t.Id")
			.Select("t.ObjectCode")
			.Select("t.ObjectName")
			.Select("t.HierarchyPath");

		sbSql.Where("t.IsDeleted=0");

		sbSql.LeftJoin($"(SELECT pos.ParentId, 'ChildCount'=COUNT(*) FROM {DbObject.MsSqlTable} pos WHERE pos.IsDeleted=0 AND pos.IsEnabled=1 AND pos.ParentId IS NOT NULL GROUP BY pos.ParentId) pr ON pr.ParentId=t.Id");
		sbSql.Where("pr.ChildCount IS NOT NULL");

		sbSql.OrderBy("t.HierarchyPath");
		sbSql.OrderBy("t.ObjectName");

		string sql = sbSql.AddTemplate($"SELECT /**select**/ FROM {DbObject.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

		using var cn = DbContext.DbCxn;
		var dataList = (await cn.QueryAsync<DropDownListItem>(sql)).AsList();

		return dataList;
	}
}