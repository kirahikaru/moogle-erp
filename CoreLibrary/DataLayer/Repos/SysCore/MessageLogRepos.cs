namespace DataLayer.Repos.SysCore;

public interface IMessageLogRepos : IBaseRepos<MessageLog>
{
    Task<List<MessageLog>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        List<string>? messageTypeList = null,
        string? customerId = null,
        List<string>? customerTypeList = null,
        string? policyNumber = null,
        List<string>? telcoOperatorList = null,
        string? senderPlatformID = null,
        string? receiverPlatformID = null,
        List<string>? messageStatusList = null,
        string? sender = null,
        List<string>? channelList = null);

    Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        List<string>? messageTypeList = null,
        string? customerId = null,
        List<string>? customerTypeList = null,
        string? policyNumber = null,
        List<string>? telcoOperatorList = null,
        string? senderPlatformID = null,
        string? receiverPlatformID = null,
        List<string>? messageStatusList = null,
        string? sender = null,
        List<string>? channelList = null);
}

public class MessageLogRepos(IDbContext dbContext) : BaseRepos<MessageLog>(dbContext, MessageLog.DatabaseObject), IMessageLogRepos
{
	public async Task<List<MessageLog>> SearchAsync(
        int pgSize = 0, int pgNo = 0,
        List<string>? messageTypeList = null,
        string? customerId = null,
        List<string>? customerTypeList = null,
        string? policyNumber = null,
        List<string>? telcoOperatorList = null,
        string? senderPlatformID = null,
        string? receiverPlatformID = null,
        List<string>? messageStatusList = null,
        string? sender = null,
        List<string>? channelList = null)
    {

        if (pgNo < 0 || pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (messageTypeList != null && messageTypeList.Count > 0)
        {
            if (messageTypeList.Count == 1)
            {
                sbSql.Where("t.MessageType=@MessageType");
                param.Add("@MessageType", messageTypeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MessageType=@MessageTypeList");
                param.Add("@MessageTypeList", messageTypeList);
            }
        }

        if (!string.IsNullOrEmpty(customerId))
        {
            sbSql.Where("t.CustromerId=@CustromerId");
            param.Add("@CustromerId", customerId, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(policyNumber))
        {
            sbSql.Where("t.ReferenceNumber=@PolicyNumber");
            param.Add("@PolicyNumber", policyNumber, DbType.AnsiString);
        }

        if (customerTypeList != null && customerTypeList.Count > 0)
        {
            if (customerTypeList.Count == 1)
            {
                sbSql.Where("t.CustomerType=@CustomerType");
                param.Add("@CustomerType", new DbString { Value = customerTypeList[0], IsAnsi = true });
            }
            else
            {
                sbSql.Where("t.CustomerType IN @CustomerTypeList");
                param.Add("@CustomerTypeList", customerTypeList);
            }
        }

        if (!string.IsNullOrEmpty(senderPlatformID))
        {
            sbSql.Where("t.SenderPlatformID IS NOT NULL");
            sbSql.Where("t.SenderPlatformID LIKE '%'+@SenderPlatformID+'%'");
            param.Add("@SenderPlatformID", senderPlatformID, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(receiverPlatformID))
        {
            sbSql.Where("t.ReceiverPlatformID IS NOT NULL");
            sbSql.Where("t.ReceiverPlatformID LIKE '%'+@ReceiverPlatformID+'%'");
            param.Add("@ReceiverPlatformID", receiverPlatformID, DbType.AnsiString);
        }

        if (telcoOperatorList != null && telcoOperatorList.Count > 0)
        {
            if (telcoOperatorList.Count == 1)
            {
                sbSql.Where("t.TelcoOperator=@TelcoOperator");
                param.Add("@TelcoOperator", telcoOperatorList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.TelcoOperator IN @TelcoOperatorList");
                param.Add("@TelcoOperatorList", telcoOperatorList);
            }
        }

        if (messageStatusList != null && messageStatusList.Count > 0)
        {
            if (messageStatusList.Count == 1)
            {
                sbSql.Where("t.MessageStatus=@MessageStatus");
                param.Add("@MessageStatus", messageStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MessageStatus IN @MessageStatusList");
                param.Add("@MessageStatusList", messageStatusList);
            }
        }

        if (channelList != null && channelList.Count > 0)
        {
            if (channelList.Count == 1)
            {
                sbSql.Where("t.hannel=@Channel");
                param.Add("@Channel", channelList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.Channel IN @ChannelList");
                param.Add("@ChannelList", channelList);
            }
        }

        if (!string.IsNullOrEmpty(sender))
        {
            sbSql.Where("t.Sender LIKE '%'+@Sender+'%'");
            param.Add("@Sender", sender, DbType.AnsiString);
        }
        #endregion

        
        sbSql.OrderBy("t.CreatedDateTime DESC");

        string sql;

        if (pgNo == 0 && pgSize == 0)
        {
            sql = sbSql.AddTemplate($"SELECT * FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/").RawSql;
        }
        else
        {
            //throw new NotImplementedException();
            param.Add("@PageSize", pgSize);
            param.Add("@PageNo", pgNo);

            sql = sbSql.AddTemplate($";WITH pg AS (SELECT Id FROM {DbObject.MsSqlTable} t /**where**/ /**orderby**/ OFFSET @PageSize * (@PageNo - 1) rows FETCH NEXT @PageSize ROW ONLY) " +
                  $"SELECT t.* FROM {DbObject.MsSqlTable} t INNER JOIN pg p ON p.Id=t.Id /**orderby**/").RawSql;
        }

        using var cn = DbContext.DbCxn;

        List<MessageLog> messageLogs = (await cn.QueryAsync<MessageLog>(sql, param)).AsList();

        return messageLogs;
    }

    public async Task<DataPagination> GetSearchPaginationAsync(
        int pgSize = 0,
        List<string>? messageTypeList = null,
        string? customerId = null,
        List<string>? customerTypeList = null,
        string? policyNumber = null,
        List<string>? telcoOperatorList = null,
        string? senderPlatformID = null,
        string? receiverPlatformID = null,
        List<string>? messageStatusList = null,
        string? sender = null,
        List<string>? channelList = null)
    {
        if (pgSize < 0)
            throw new ArgumentOutOfRangeException(nameof(pgSize), _errMsgResxMngr.GetString("PageSize_PageNo_Negative", CultureInfo.CurrentUICulture));

        SqlBuilder sbSql = new();
        DynamicParameters param = new();

        sbSql.Where("t.IsDeleted=0");

        #region Form Search Conditions
        if (messageTypeList != null && messageTypeList.Count > 0)
        {
            if (messageTypeList.Count == 1)
            {
                sbSql.Where("t.MessageType=@MessageType");
                param.Add("@MessageType", messageTypeList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MessageType=@MessageTypeList");
                param.Add("@MessageTypeList", messageTypeList);
            }
        }

        if (!string.IsNullOrEmpty(customerId))
        {
            sbSql.Where("t.CustromerId=@CustromerId");
            param.Add("@CustromerId", customerId, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(policyNumber))
        {
            sbSql.Where("t.ReferenceNumber=@PolicyNumber");
            param.Add("@PolicyNumber", policyNumber, DbType.AnsiString);
        }

        if (customerTypeList != null && customerTypeList.Count > 0)
        {
            if (customerTypeList.Count == 1)
            {
                sbSql.Where("t.CustomerType=@CustomerType");
                param.Add("@CustomerType", new DbString { Value = customerTypeList[0], IsAnsi = true });
            }
            else
            {
                sbSql.Where("t.CustomerType IN @CustomerTypeList");
                param.Add("@CustomerTypeList", customerTypeList);
            }
        }

        if (!string.IsNullOrEmpty(senderPlatformID))
        {
            sbSql.Where("t.SenderPlatformID IS NOT NULL");
            sbSql.Where("t.SenderPlatformID LIKE '%'+@SenderPlatformID+'%'");
            param.Add("@SenderPlatformID", senderPlatformID, DbType.AnsiString);
        }

        if (!string.IsNullOrEmpty(receiverPlatformID))
        {
            sbSql.Where("t.ReceiverPlatformID IS NOT NULL");
            sbSql.Where("t.ReceiverPlatformID LIKE '%'+@ReceiverPlatformID+'%'");
            param.Add("@ReceiverPlatformID", receiverPlatformID, DbType.AnsiString);
        }

        if (telcoOperatorList != null && telcoOperatorList.Count > 0)
        {
            if (telcoOperatorList.Count == 1)
            {
                sbSql.Where("t.TelcoOperator=@TelcoOperator");
                param.Add("@TelcoOperator", telcoOperatorList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.TelcoOperator IN @TelcoOperatorList");
                param.Add("@TelcoOperatorList", telcoOperatorList);
            }
        }

        if (messageStatusList != null && messageStatusList.Count > 0)
        {
            if (messageStatusList.Count == 1)
            {
                sbSql.Where("t.MessageStatus=@MessageStatus");
                param.Add("@MessageStatus", messageStatusList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.MessageStatus IN @MessageStatusList");
                param.Add("@MessageStatusList", messageStatusList);
            }
        }

        if (channelList != null && channelList.Count > 0)
        {
            if (channelList.Count == 1)
            {
                sbSql.Where("t.hannel=@Channel");
                param.Add("@Channel", channelList[0], DbType.AnsiString);
            }
            else
            {
                sbSql.Where("t.Channel IN @ChannelList");
                param.Add("@ChannelList", channelList);
            }
        }

        if (!string.IsNullOrEmpty(sender))
        {
            sbSql.Where("t.Sender LIKE '%'+@Sender+'%'");
            param.Add("@Sender", sender, DbType.AnsiString);
        }
        #endregion

        string sql = sbSql.AddTemplate($"SELECT COUNT(*) FROM {DbObject.MsSqlTable} t /**where**/").RawSql;

        using var cn = DbContext.DbCxn;

        decimal recordCount = await cn.ExecuteScalarAsync<int>(sql, param);
        int pageCount = pgSize == 0 ? 1 : (int)Math.Ceiling(recordCount / pgSize);

        DataPagination pagination = new()
        {
            ObjectType = typeof(CambodiaProvince).Name,
            PageSize = pgSize,
            PageCount = pageCount,
            RecordCount = (int)recordCount
        };

        return pagination;
    }
}