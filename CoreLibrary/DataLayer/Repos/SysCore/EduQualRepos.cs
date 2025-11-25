namespace DataLayer.Repos.SysCore;
public interface IEduQualRepos : IBaseRepos<EduQual>
{

}

public class EduQualRepos(IDbContext dbContext) : BaseRepos<EduQual>(dbContext, EduQual.DatabaseObject), IEduQualRepos
{
}