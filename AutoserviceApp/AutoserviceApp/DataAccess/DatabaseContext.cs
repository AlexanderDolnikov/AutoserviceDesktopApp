using System.Data.SqlClient;
using System.Configuration;

namespace AutoserviceApp.DataAccess
{
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext()
        {
            // Читаем строку подключения из App.config
            _connectionString = ConfigurationManager.ConnectionStrings["AutoserviceDb"].ConnectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}

