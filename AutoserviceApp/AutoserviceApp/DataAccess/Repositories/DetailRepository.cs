using System.Data.SqlClient;
using AutoserviceApp.Models;
using System.Data;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class DetailRepository
    {
        private readonly DatabaseContext _context;

        public DetailRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<Detail> GetAllDetails()
        {
            var details = new List<Detail>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Деталь ORDER BY Название ASC, Стоимость DESC", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    details.Add(new Detail
                    {
                        Код = (int)reader["Код"],
                        Название = reader["Название"].ToString(),
                        Стоимость = (decimal)reader["Стоимость"],
                        Производитель = reader["Производитель"].ToString()
                    });
                }
            }

            return details;
        }

        public void AddDetail(Detail detail)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("sp_AddDetail", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@Название", detail.Название);
                command.Parameters.AddWithValue("@Стоимость", detail.Стоимость);
                command.Parameters.AddWithValue("@Производитель", detail.Производитель);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateDetail(Detail detail)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(
                    "UPDATE Деталь SET Название = @Название, Стоимость = @Стоимость, Производитель = @Производитель WHERE Код = @Код", connection);

                command.Parameters.AddWithValue("@Код", detail.Код);
                command.Parameters.AddWithValue("@Название", detail.Название);
                command.Parameters.AddWithValue("@Стоимость", detail.Стоимость);
                command.Parameters.AddWithValue("@Производитель", detail.Производитель);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteDetail(int id)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("DELETE FROM Деталь WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", id);

                command.ExecuteNonQuery();
            }
        }

        public string GetDetailNameById(int detailId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT Название FROM Деталь WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", detailId);

                var result = command.ExecuteScalar();
                return result?.ToString() ?? "Неизвестно";
            }
        }

    }
}