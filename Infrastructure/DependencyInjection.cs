using System.Data;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDbConnection>((sp) => new MySqlConnection("Server=156.67.83.101;Port=12221;Database=rockstar;Uid=mysql;Pwd=8251f5d5f3afc47f;"));

        services.AddScoped<HealthcheckRepository>();
        services.AddScoped<QuestionRepository>();
        services.AddScoped<UserRepository>();
    }
}