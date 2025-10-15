using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace DOCHUB.APP.Base
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;

        protected BaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("postgresql")!;
        }

        protected IDbConnection CrearConexion()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}