using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class ClientRepository
    {
        private readonly DatabaseContext _context;

        public ClientRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Клиент", connection);
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

        public Client GetClientById(int clientId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Клиент WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", clientId);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new Client
                    {
                        Код = (int)reader["Код"],
                        Имя = reader["Имя"].ToString(),
                        Фамилия = reader["Фамилия"].ToString(),
                        Телефон = reader["Телефон"].ToString(),
                        Адрес = reader["Адрес"].ToString()
                    };
                }
            }
            return null;
        }
    }
}
