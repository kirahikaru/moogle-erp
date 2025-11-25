using DataLayer.Models.SysCore;
using System.Reflection;

namespace DataLayer.Repos;

public static class QueryGenerator
{
    public static string GenerateDeleteQuery(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            return string.Empty;

        return $"UPDATE {tableName} SET IsDeleted=1, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime WHERE Id=@Id";
    }

    public static string GenerateClaimRunningNumberQuery(Type objType)
    {
        ArgumentNullException.ThrowIfNull(objType, nameof(objType));

        var properties = GetPropertiesList(objType);

        // For Lead App Case
        if (properties.Contains("Status") || properties.Contains("WorkflowStatus"))
        {
            var sql = $"UPDATE {SysRunNum.MsSqlTable} " +
                      @"SET LinkedObjectId=@LinkedObjectId, LinkedObjectType=ObjectType, ModifiedUser=@ModifiedUser, ModifiedDateTime=@ModifiedDateTime
                            WHERE IsDeleted=0 AND IsLocked=1 AND LinkedObjectId IS NULL AND LinkedObjectType IS NULL AND ObjectCode=@ObjectCode AND LockedByUserId=@UserId";

            return sql;

        }

        return "";
    }

    public static string GenerateInsertQuery(Type objType, string tableName)
    {
        ArgumentNullException.ThrowIfNull(objType, nameof(objType));

        var insertQuery = new StringBuilder($"INSERT INTO {tableName}");

        insertQuery.Append('(');

        var properties = GetPropertiesList(objType);

        properties.ForEach(property =>
        {
            if (!property.Equals("Id", StringComparison.OrdinalIgnoreCase) && !property.Equals("ObjectId", StringComparison.OrdinalIgnoreCase))
                insertQuery.Append($"[{property}],");
        });

        insertQuery
            .Remove(insertQuery.Length - 1, 1)
            .Append(") VALUES (");

        properties.ForEach(property =>
        {
            if (!property.Equals("Id", StringComparison.OrdinalIgnoreCase) && !property.Equals("ObjectId", StringComparison.OrdinalIgnoreCase))
            {
                insertQuery.Append($"@{property},");
            }
        });

        insertQuery
            .Remove(insertQuery.Length - 1, 1)
            .Append("); SELECT CAST(SCOPE_IDENTITY() as int)");

        return insertQuery.ToString();
    }

    public static string GenerateUpdateQuery(Type objType, string tableName)
    {
        ArgumentNullException.ThrowIfNull(objType, nameof(objType));
        
        var updateQuery = new StringBuilder($"UPDATE {tableName} SET ");
        var properties = GetPropertiesList(objType);

        properties.ForEach(property =>
        {
            if (!property.Equals("Id", StringComparison.OrdinalIgnoreCase) && !property.Equals("ObjectId", StringComparison.OrdinalIgnoreCase))
            {
                updateQuery.Append($"{property}=@{property},");
            }
        });

        updateQuery.Remove(updateQuery.Length - 1, 1); //remove last comma
        updateQuery.Append(" WHERE Id=@Id");

        return updateQuery.ToString();
    }

    private static List<string> GetPropertiesList(Type objType)
    {
        IEnumerable<PropertyInfo> properties = objType.GetProperties().Where(x => !x.PropertyType.FullName!.StartsWith("DataLayerCore", StringComparison.OrdinalIgnoreCase));

        return (from prop in properties
                let attributes = prop.GetCustomAttributes(typeof(DescriptionAttribute), false)
                where attributes.Length <= 0 || (attributes[0] as DescriptionAttribute)?.Description != "ignore"
                select prop.Name).ToList();
    }

    public static string GenerateNonWorkflowUpdateQuery(Type objType, string tableName)
    {
        ArgumentNullException.ThrowIfNull(objType, nameof(objType));

        var updateQuery = new StringBuilder($"UPDATE {tableName} SET ");
        var properties = GetPropertiesList(objType);

        List<string> ingoreProperties = new()
        {
            "Id",
            "ObjectId",
            "WorkflowStatus"
        };

        properties.ForEach(property =>
        {
            if (!ingoreProperties.Contains(property))
            {
                updateQuery.Append($"{property}=@{property},");
            }
        });

        updateQuery.Remove(updateQuery.Length - 1, 1); //remove last comma
        updateQuery.Append(" WHERE Id=@Id");

        return updateQuery.ToString();
    }
}