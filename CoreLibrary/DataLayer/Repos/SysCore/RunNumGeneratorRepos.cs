using DataLayer.GlobalConstant;

namespace DataLayer.Repos.SysCore;

public interface IRunNumGeneratorRepos : IBaseRepos<RunNumGenerator>
{
	Task<string?> GenerateRunningNumberAsync(string objectClassName, DateTime businessDate);
}

public class RunNumGeneratorRepos(IDbContext dbContext) : BaseRepos<RunNumGenerator>(dbContext, RunNumGenerator.DatabaseObject), IRunNumGeneratorRepos
{
	public async Task<string?> GenerateRunningNumberAsync(string objectClassName, DateTime businessDate)
    {
        var sql = $"SELECT * FROM {RunNumGenerator.MsSqlTable} WHERE IsDeleted=0 AND ObjectClassName=@ObjectClassName";

        using var cn = DbContext.DbCxn;

        // <!IMPORTANT> Connection required to be open before calling BeginTransaction() function
        if (cn.State != ConnectionState.Open) cn.Open();

        using var tran = cn.BeginTransaction();
        try
        {
            var rng = cn.QueryFirstOrDefault<RunNumGenerator>(sql, new { ObjectClassName = new DbString { Value = objectClassName, IsAnsi = true } });

            if (rng != null)
            {
                var rngCountSql = $"UPDATE CurrentNumber=CurrentNumber+1, ModifiedDateTime=GETDATE() OUTPUT inserted.CurrentNumber " +
                                  $"FROM {RunNumGeneratorCounter.MsSqlTable} WHERE IsDeleted=0 AND RunningNumberGeneratorId=@RunningNumberGeneratorId";
                
                DynamicParameters param = new();

                param.Add("@RunningNumberGeneratorId", rng.Id);

                switch (rng.ResetInterval)
                {
                    case SystemIntervals.YEARLY:
                        {
                            rngCountSql += " AND IntervalYear=@IntervalYear AND IntervalMonth IS NULL AND IntervalDay IS NULL";
                            param.Add("@IntervalYear", businessDate.Year);
                        }
                        break;
                    case SystemIntervals.MONTHLY:
                        {
                            rngCountSql += " AND IntervalYear=@IntervalYear AND IntervalMonth=@IntervalMonth AND IntervalDay IS NULL";
                            param.Add("@IntervalYear", businessDate.Year);
                            param.Add("@IntervalMonth", businessDate.Month);
                        }
                        break;
                    case SystemIntervals.DAILY:
                        {
                            rngCountSql += " AND IntervalYear=@IntervalYear AND IntervalMonth=@IntervalMonth AND IntervalDay=@IntervalDay";
                            param.Add("@IntervalYear", businessDate.Year);
                            param.Add("@IntervalMonth", businessDate.Month);
                            param.Add("@IntervalDay", businessDate.Day);
                        }
                        break;
                };

                int curNo = await cn.ExecuteScalarAsync<int>(sql, param);

                tran.Commit();
                return string.Format(rng.DisplayFormat!, rng.Prefix, businessDate, curNo, rng.Suffix);
            }
            else return null;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }
}