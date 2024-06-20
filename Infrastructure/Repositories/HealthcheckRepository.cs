using System.Data;
using Common.Models;
using Dapper;

namespace Infrastructure.Repositories;

public class HealthcheckRepository
{
    private readonly IDbConnection _dbConnection;
    private readonly QuestionRepository _questionRepository;

    public HealthcheckRepository(IDbConnection dbConnection, QuestionRepository questionRepository)
    {
        _dbConnection = dbConnection;
        _questionRepository = questionRepository;
    }

    public async Task<HealthCheck> GetFromLink(string uniqueLink)
    {
        const string sql = @"SELECT hc.*, il.version_id AS versionId, il.user_id AS userId
                        FROM healthcheck hc
                        JOIN invitation_link il ON hc.id = il.healthcheck_id
                        WHERE il.uniqueLink = @uniqueLink;";
        var parameters = new DynamicParameters();
        parameters.Add("@uniqueLink", uniqueLink);
        var healthCheck = await _dbConnection.QuerySingleAsync<HealthCheck>(sql, parameters);
        healthCheck.IsSubmitted = await GetIsSubmitted(uniqueLink);
        healthCheck.Company = await GetCompany(uniqueLink);
        healthCheck.Questions = await _questionRepository.GetAll(uniqueLink, healthCheck.VersionId);

        return healthCheck;
    }

    public async Task<string> GetLinkFromId(int id)
    {
        const string sql = @"SELECT uniqueLink FROM invitation_link WHERE healthcheck_id = @id";
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        var link = await _dbConnection.QuerySingleAsync<string>(sql, parameters);
        return link;
    }

    public async Task<string> GetLinkFronIdAndVersionAndUser(int id, int version, int user)
    {
        const string sql =
            @"SELECT uniqueLink FROM invitation_link WHERE healthcheck_id = @id AND version_id = @versionId AND user_id = @userId AND id = (SELECT MAX(id) FROM invitation_link WHERE healthcheck_id = @id AND version_id = @versionId AND user_id = @userId )";
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        parameters.Add("@versionId", version);
        parameters.Add("@userId", user);
        var link = await _dbConnection.QuerySingleAsync<string>(sql, parameters);
        return link;
    }

    public async Task Submit(int id, int version)
    {
        const string sql =
            @"UPDATE invitation_link SET isUsed = 1 WHERE healthcheck_id = @id AND version_id = @version_id";
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        parameters.Add("@version_id", version);
        await _dbConnection.ExecuteAsync(sql, parameters);
    }

    public async Task<List<HealthCheck>> GetUserHealthChecks(int userid)
    {
        const string sql = @"SELECT uniqueLink FROM invitation_link WHERE user_id = @userId";
        var parameters = new DynamicParameters();
        parameters.Add("@userId", userid);
        var links = await _dbConnection.QueryAsync<string>(sql, parameters);
        List<HealthCheck> healthChecks = [];
        foreach (var link in links)
        {
            healthChecks.Add(await GetFromLink(link));
        }

        return healthChecks;
    }

    private async Task<bool> GetIsSubmitted(string uniqueLink)
    {
        const string sql = @"SELECT isUsed FROM invitation_link WHERE uniqueLink = @uniqueLink";
        var parameters = new DynamicParameters();
        parameters.Add("@uniqueLink", uniqueLink);
        var submitted = await _dbConnection.QuerySingleAsync<bool>(sql, parameters);
        return submitted;
    }

    private async Task<Company> GetCompany(string uniqueLink)
    {
        const string sql =
            @"SELECT c.name, c.location, c.logo FROM invitation_link il JOIN user u ON il.user_id = u.id JOIN squad_member sm ON u.id = sm.user_id JOIN company_squad cs ON sm.squad_id = cs.squad_id JOIN company c ON cs.company_id = c.id WHERE il.uniqueLink=@UniqueLink";
        var parameters = new DynamicParameters();
        parameters.Add("@uniqueLink", uniqueLink);
        var companies = await _dbConnection.QueryAsync<Company>(sql, parameters);
        return companies.First();
    }
}