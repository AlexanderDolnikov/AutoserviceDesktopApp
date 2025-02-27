using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using AutoserviceApp.Models;
using AutoserviceApp.Models.TaskQueries;
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

                var command = new SqlCommand("SELECT * FROM Деталь", connection);
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

        public List<PopularDetail> GetQuery2Result()
        {
            var details = new List<PopularDetail>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("sp_GetPopularDetails", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    details.Add(new PopularDetail
                    {
                        Название = reader["Название"].ToString(),
                        КоличествоЗаказов = (int)reader["КоличествоЗаказов"]
                    });
                }
            }

            return details;
        }

        public List<GroupedDetail> GetQuery3Result()
        {
            var details = new List<GroupedDetail>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(@"
            SELECT d.Название AS DetailName, 
                   SUM(dr.Количество) AS TotalQuantity, 
                   SUM(dr.Количество * d.Стоимость) AS TotalCost
            FROM Деталь d
            JOIN ДетальРаботы dr ON d.Код = dr.КодДетали
            GROUP BY d.Название", connection);

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    details.Add(new GroupedDetail
                    {
                        DetailName = reader["DetailName"].ToString(),
                        TotalQuantity = (int)reader["TotalQuantity"],
                        TotalCost = (decimal)reader["TotalCost"]
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

        public List<Detail> GetDetailsWithPriceAbove(decimal priceFrom)
        {
            var details = new List<Detail>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Деталь WHERE Стоимость >= @priceFrom", connection);
                command.Parameters.AddWithValue("@priceFrom", priceFrom);

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
        public List<Detail> GetDetailsWithPriceAboveAndKeyword(decimal priceFrom, string keyword)
        {
            var details = new List<Detail>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(
                    "SELECT * FROM Деталь WHERE Стоимость >= @priceFrom AND Название LIKE @keyword", connection);
                command.Parameters.AddWithValue("@priceFrom", priceFrom);
                command.Parameters.AddWithValue("@keyword", $"%{keyword}%");

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


        public List<Detail> GetPopularDetails()
        {
            var details = new List<Detail>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(
                    "SELECT d.Код, d.Название, d.Стоимость, d.Производитель " +
                    "FROM Деталь d " +
                    "JOIN ДетальРаботы dr ON d.Код = dr.КодДетали " +
                    "GROUP BY d.Код, d.Название, d.Стоимость, d.Производитель " +
                    "HAVING COUNT(dr.Код) > 1", connection);

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

        public List<Detail> SearchDetailsByName(string name)
        {
            var details = new List<Detail>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Деталь WHERE Название LIKE @name", connection);
                command.Parameters.AddWithValue("@name", $"%{name}%");

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

        public List<Detail> SearchDetailsByPrice(decimal price)
        {
            var details = new List<Detail>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Деталь WHERE Стоимость = @price", connection);
                command.Parameters.AddWithValue("@price", price);

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

        public List<Detail> SearchDetailsByManufacturer(string manufacturer)
        {
            var details = new List<Detail>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Деталь WHERE Производитель LIKE @manufacturer", connection);
                command.Parameters.AddWithValue("@manufacturer", $"%{manufacturer}%");

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

        public List<Detail> SortDetailsByPriceAscending()
        {
            return GetAllDetails().OrderBy(d => d.Стоимость).ToList();
        }

        public List<Detail> SortDetailsByPriceDescending()
        {
            return GetAllDetails().OrderByDescending(d => d.Стоимость).ToList();
        }

        public List<object> GetGroupedDetails()
        {
            var results = new List<object>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(@"
                            SELECT 
                                d.Название AS DetailName,
                                SUM(wd.Количество) AS TotalQuantity,
                                SUM(wd.Количество * d.Стоимость) AS TotalCost
                            FROM Деталь d
                            JOIN ДетальРаботы wd ON d.Код = wd.КодДетали
                            GROUP BY d.Название", connection);

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new
                    {
                        DetailName = reader["DetailName"].ToString(),
                        TotalQuantity = (int)reader["TotalQuantity"],
                        TotalCost = (decimal)reader["TotalCost"]
                    });
                }
            }

            return results;
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