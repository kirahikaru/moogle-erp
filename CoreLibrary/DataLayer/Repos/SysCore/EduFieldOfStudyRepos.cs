namespace DataLayer.Repos.SysCore;

public interface IEduFieldOfStudyRepos : IBaseRepos<EduFieldOfStudy>
{

}

public class EduFieldOfStudyRepos(IDbContext dbContext) : BaseRepos<EduFieldOfStudy>(dbContext, EduFieldOfStudy.DatabaseObject), IEduFieldOfStudyRepos
{
}