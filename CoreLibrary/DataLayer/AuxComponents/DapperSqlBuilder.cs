namespace DataLayer.AuxComponents;
/// <summary>
/// Source: Encrypt & Decrypt a String in C#
/// Url: https://www.selamigungor.com/post/7/encrypt-decrypt-a-string-in-csharp (OBSOLETE FOR .NET 7)
/// 
/// https://code-maze.com/csharp-string-encryption-decryption/
/// </summary>
public class DapperSqlBuilder : SqlBuilder
{
    public DapperSqlBuilder(bool forBaseObj, string? mainTblVarName) : base()
    {
        if (forBaseObj)
            if (string.IsNullOrEmpty(mainTblVarName))
                this.Where("IsDeleted=0");
            else
                this.Where($"{mainTblVarName!}.IsDeleted=0");
    }

    public static string GenInsertSql(string tableName, List<string> fields, string dbType)
    {
        if (dbType == DatabaseTypes.POSTGRESQL)
        {
            return $"INSERT INTO {tableName} ({string.Join(", ", fields)}) VALUES (@{string.Join(", @", fields)}) RETURNING id";
        }
        else
        {
            return $"INSERT INTO {tableName} ({string.Join(", ", fields)}) VALUES (@{string.Join(", @", fields)}) SELECT SCOPE_IDENTITY()";
        }
    }

    public static string GenUpdateSql(string tableName, List<string> fields, string dbType)
    {
        if (dbType == DatabaseTypes.POSTGRESQL)
        {
            return $"UPDATE {tableName} SET {string.Join(", ", fields.Select(f => $"{f}=@{f}"))} WHERE id=@id";
        }
        else
        {
            return $"UPDATE {tableName} SET {string.Join(", ", fields.Select(f => $"{f}=@{f}"))} WHERE id=@id";
        }
	}
}