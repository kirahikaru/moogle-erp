namespace DataLayer.Repos.SystemCore;
public interface IEduQualRepos : IBaseRepos<EduQual>
{

}

public class EduQualRepos(IConnectionFactory connectionFactory) : BaseRepos<EduQual>(connectionFactory, EduQual.DatabaseObject), IEduQualRepos
{
}