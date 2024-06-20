using System.Data;
using Common.Models;
using Dapper;

namespace Infrastructure.Repositories;

public class UserRepository
{
    private readonly IDbConnection _dbConnection;

    public UserRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<User> GetFromUniqueLink(string uniqueLink)
    {
        const string sql = @"SELECT * FROM user WHERE id = (SELECT user_id from invitation_link where uniqueLink = @uniqueLink)";

        var parameters = new DynamicParameters();
        parameters.Add("@uniqueLink", uniqueLink);

        var user = await _dbConnection.QuerySingleAsync<User>(sql, parameters);
        return user;
    }
}