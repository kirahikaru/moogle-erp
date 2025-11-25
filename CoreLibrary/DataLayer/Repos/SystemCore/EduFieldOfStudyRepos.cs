namespace DataLayer.Repos.SystemCore;

public interface IEduFieldOfStudyRepos : IBaseRepos<EduFieldOfStudy>
{

}

public class EduFieldOfStudyRepos(IConnectionFactory connectionFactory) : BaseRepos<EduFieldOfStudy>(connectionFactory, EduFieldOfStudy.DatabaseObject), IEduFieldOfStudyRepos
{
}