using System.Data.SqlClient;
using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class MasterRepository
    {
        private readonly DatabaseContext _context;

        public MasterRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<Master> GetAllMasters()
        {
            var masters = new List<Master>();

            using (var connection = _context.GetConnection())
            {
                string query = "SELECT * FROM Мастер ORDER BY Фамилия, Имя, Телефон, Специализация";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            masters.Add(new Master
                            {
                                Код = reader.GetInt32(0),
                                Имя = reader.GetString(1),
                                Фамилия = reader.GetString(2),
                                Телефон = reader.GetString(3),
                                Специализация = reader.GetString(4)
                            });
                        }
                    }
                }
            }

            return masters;
        }

        public string GetMasterNameById(int detailId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT Фамилия FROM Мастер WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", detailId);

                var result = command.ExecuteScalar();
                return result?.ToString() ?? "Неизвестно";
            }
        }

        public void AddMaster(Master master)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO Мастер (Имя, Фамилия, Телефон, Специализация) VALUES (@Имя, @Фамилия, @Телефон, @Специализация)", connection);

                command.Parameters.AddWithValue("@Имя", master.Имя);
                command.Parameters.AddWithValue("@Фамилия", master.Фамилия);
                command.Parameters.AddWithValue("@Телефон", master.Телефон ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Специализация", master.Специализация ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateMaster(Master master)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Мастер SET Имя = @Имя, Фамилия = @Фамилия, Телефон = @Телефон, Специализация = @Специализация WHERE Код = @Код", connection);

                command.Parameters.AddWithValue("@Код", master.Код);
                command.Parameters.AddWithValue("@Имя", master.Имя);
                command.Parameters.AddWithValue("@Фамилия", master.Фамилия);
                command.Parameters.AddWithValue("@Телефон", master.Телефон ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Специализация", master.Специализация ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteMaster(int masterId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Мастер WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", masterId);
                command.ExecuteNonQuery();
            }
        }
    }
}