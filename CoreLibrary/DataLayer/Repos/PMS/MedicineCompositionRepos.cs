using DataLayer.Models.PMS;

namespace DataLayer.Repos.PMS;

public interface IMedicineCompositionRepos : IBaseRepos<MedicineComposition>
{
	Task<List<MedicineComposition>> GetByMedicineIdAsync(int medicineId);
}

public class MedicineCompositionRepos(IDbContext dbContext) : BaseRepos<MedicineComposition>(dbContext, MedicineComposition.DatabaseObject), IMedicineCompositionRepos
{
	public async Task<List<MedicineComposition>> GetByMedicineIdAsync(int medicineId)
    {
        SqlBuilder sbSql = new();

        sbSql.Where("t.IsDeleted=0");
        sbSql.Where("t.MedicineId=@MedicineId");

        sbSql.LeftJoin($"{MedicalComposition.MsSqlTable} mc ON mc.Id=t.MedicalCompositionId");
        sbSql.LeftJoin($"{UnitOfMeasure.MsSqlTable} uom ON uom.IsDeleted=0 AND uom.ObjectCode=t.UnitCode");

        var sql = sbSql.AddTemplate($"SELECT * FROM {MedicineComposition.MsSqlTable} t /**leftjoin**/ /**where**/").RawSql;

        var param = new { MedicineId = medicineId };

        using var cn = DbContext.DbCxn;

        List<MedicineComposition> result = (await cn.QueryAsync<MedicineComposition, MedicalComposition, UnitOfMeasure, MedicineComposition>(sql, (medicineComposition, medicalComposition, uom) =>
        {
            medicineComposition.MedicalComposition = medicalComposition;
            medicineComposition.Unit = uom;

            return medicineComposition;
        }, param, splitOn: "Id")).AsList();

        return result;
    }
}