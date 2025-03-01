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
                string query = "SELECT Код, Имя, Фамилия, Телефон, Специализация FROM Мастер";
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

    }
}

