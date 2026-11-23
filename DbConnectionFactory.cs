using MySqlConnector;

namespace HandyConnect.Api.Data;

public interface IDbConnectionFactory
{
    Task<MySqlConnection> OpenAsync(CancellationToken ct = default);
}

public sealed class MySqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    private readonly string _connectionString =
        configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default is missing.");

    public async Task<MySqlConnection> OpenAsync(CancellationToken ct = default)
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        return connection;
    }
}
