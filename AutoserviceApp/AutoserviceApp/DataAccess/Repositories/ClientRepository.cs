using AutoserviceApp.Models;
using System.Data.SqlClient;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class ClientRepository : BaseRepository<Client>
    {
        public ClientRepository(DatabaseContext context) : base(context) { }

        public override List<Client> GetAll()
        {
            var clients = new List<Client>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM vw_Clients", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    clients.Add(new Client
                    {
                        Код = (int)reader["Код"],
                        Имя = reader["Имя"].ToString(),
                        Фамилия = reader["Фамилия"].ToString(),
                        Телефон = reader["Телефон"].ToString(),
                        Адрес = reader["Адрес"].ToString()
                    });
                }
            }
            return clients;
        }
    }
}