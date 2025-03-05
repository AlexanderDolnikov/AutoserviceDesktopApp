using System.Data.SqlClient;
using AutoserviceApp.Models;

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

                var command = new SqlCommand("SELECT * FROM Клиент ORDER BY Фамилия, Имя, Телефон", connection);
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

        public void AddClient(Client client)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO Клиент (Имя, Фамилия, Телефон, Адрес) VALUES (@Имя, @Фамилия, @Телефон, @Адрес)", connection);
                command.Parameters.AddWithValue("@Имя", client.Имя);
                command.Parameters.AddWithValue("@Фамилия", client.Фамилия);
                command.Parameters.AddWithValue("@Телефон", client.Телефон);
                command.Parameters.AddWithValue("@Адрес", (object)client.Адрес ?? DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateClient(Client client)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Клиент SET Имя = @Имя, Фамилия = @Фамилия, Телефон = @Телефон, Адрес = @Адрес WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", client.Код);
                command.Parameters.AddWithValue("@Имя", client.Имя);
                command.Parameters.AddWithValue("@Фамилия", client.Фамилия);
                command.Parameters.AddWithValue("@Телефон", client.Телефон);
                command.Parameters.AddWithValue("@Адрес", (object)client.Адрес ?? DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteClient(int clientId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Клиент WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", clientId);
                command.ExecuteNonQuery();
            }
        }
    }
}
