using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Data.SqlClient;
using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class WorkRepository
    {
        private readonly DatabaseContext _context;

        public WorkRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<Work> GetWorksByOrderId(int orderId)
        {
            var works = new List<Work>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Работа WHERE КодЗаказа = @orderId", connection);
                command.Parameters.AddWithValue("@orderId", orderId);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    works.Add(new Work
                    {
                        Код = (int)reader["Код"],
                        Описание = reader["Описание"].ToString(),
                        Стоимость = (decimal)reader["Стоимость"],
                    });
                }
            }

            return works;
        }

        public List<Work> GetAllWorks()
        {
            var works = new List<Work>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Работа", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    works.Add(new Work
                    {
                        Код = (int)reader["Код"],
                        КодЗаказа = (int)reader["КодЗаказа"],
                        КодМастера = (int)reader["КодМастера"],
                        Описание = reader["Описание"].ToString(),
                        Стоимость = (decimal)reader["Стоимость"],
                        КодВидаРаботы = (int)reader["КодВидаРаботы"]
                    });
                }
            }

            return works;
        }

        public void AddWork(Work work)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(
                    "INSERT INTO Работа (КодЗаказа, КодМастера, Описание, Стоимость, КодВидаРаботы) VALUES (@КодЗаказа, @КодМастера, @Описание, @Стоимость, @КодВидаРаботы)", connection);

                command.Parameters.AddWithValue("@КодЗаказа", work.КодЗаказа);
                command.Parameters.AddWithValue("@КодМастера", work.КодМастера);
                command.Parameters.AddWithValue("@Описание", work.Описание);
                command.Parameters.AddWithValue("@Стоимость", work.Стоимость);
                command.Parameters.AddWithValue("@КодВидаРаботы", work.КодВидаРаботы);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateWork(Work work)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(
                    "UPDATE Работа SET КодЗаказа = @КодЗаказа, КодМастера = @КодМастера, Описание = @Описание, Стоимость = @Стоимость, КодВидаРаботы = @КодВидаРаботы WHERE Код = @Код", connection);

                command.Parameters.AddWithValue("@Код", work.Код);
                command.Parameters.AddWithValue("@КодЗаказа", work.КодЗаказа);
                command.Parameters.AddWithValue("@КодМастера", work.КодМастера);
                command.Parameters.AddWithValue("@Описание", work.Описание);
                command.Parameters.AddWithValue("@Стоимость", work.Стоимость);
                command.Parameters.AddWithValue("@КодВидаРаботы", work.КодВидаРаботы);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteWork(int id)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("DELETE FROM Работа WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", id);

                command.ExecuteNonQuery();
            }
        }
    }
}

