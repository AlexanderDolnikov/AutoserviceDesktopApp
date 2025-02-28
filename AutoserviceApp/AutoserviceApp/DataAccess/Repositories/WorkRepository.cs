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

                if (work.КодЗаказа == 0)
                {
                    throw new Exception("Ошибка: Код заказа не может быть 0.");
                }

                var checkOrderCommand = new SqlCommand("SELECT COUNT(*) FROM Заказ WHERE Код = @КодЗаказа", connection);
                checkOrderCommand.Parameters.AddWithValue("@КодЗаказа", work.КодЗаказа);
                int orderCount = (int)checkOrderCommand.ExecuteScalar();

                if (orderCount == 0)
                {
                    throw new Exception($"Ошибка: Заказ с Код = {work.КодЗаказа} не существует.");
                }

                var command = new SqlCommand(
                    "UPDATE Работа SET КодМастера = @КодМастера, Описание = @Описание, Стоимость = @Стоимость, КодВидаРаботы = @КодВидаРаботы WHERE Код = @Код",
                    connection
                );

                command.Parameters.AddWithValue("@Код", work.Код);
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

                // Удаляем все детали работы перед удалением самой работы
                var deleteDetails = new SqlCommand("DELETE FROM ДетальРаботы WHERE КодРаботы = @КодРаботы", connection);
                deleteDetails.Parameters.AddWithValue("@КодРаботы", id);
                deleteDetails.ExecuteNonQuery();

                // Теперь можно удалить работу
                var deleteWork = new SqlCommand("DELETE FROM Работа WHERE Код = @Код", connection);
                deleteWork.Parameters.AddWithValue("@Код", id);
                deleteWork.ExecuteNonQuery();
            }
        }

    }
}

