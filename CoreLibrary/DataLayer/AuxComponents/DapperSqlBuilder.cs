using Dapper;
using System.Security.Cryptography;

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
}