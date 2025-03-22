using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoserviceApp.DataAccess.Models.DBProceduresAndFunctionsModels;
using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    internal class DBProceduresAndFunctionsRepository
    {
        private readonly DatabaseContext _context;

        public DBProceduresAndFunctionsRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<WorkTypeStats> GetWorkTypesStats()
        {
            var workStatsList = new List<WorkTypeStats>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("EXEC usp_GetWorkTypesStats", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    workStatsList.Add(new WorkTypeStats
                    {
                        НазваниеВидаРаботы = reader["НазваниеВидаРаботы"].ToString(),
                        КоличествоРабот = (int)reader["КоличествоРабот"]
                    });
                }
            }
            return workStatsList;
        }

        public List<Order> GetOrdersForPeriod(DateTime startDate, DateTime endDate)
        {
            var orders = new List<Order>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("usp_GetOrdersForDatePeriod", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new Order
                    {
                        Код = (int)reader["Код"],
                        ДатаНачала = (DateTime)reader["ДатаНачала"],
                        ДатаОкончания = reader["ДатаОкончания"] as DateTime?,
                        КодКлиента = (int)reader["КодКлиента"],
                        КодАвтомобиля = (int)reader["КодАвтомобиля"]
                    });
                }
            }
            return orders;
        }

        public decimal GetTotalWorksCostForOrder(int orderId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT dbo.fn_TotalWorksCostForOrder(@OrderId)", connection);
                command.Parameters.AddWithValue("@OrderId", orderId);

                var result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
        }

        public int MergeWorkDetailsByWorkId(int workId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("usp_MergeWorkDetailsByWorkID", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@кодРаботы", workId);
                var outputParam = new SqlParameter("@counter", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParam);

                command.ExecuteNonQuery();
                return (int)outputParam.Value;
            }
        }

        public int MergeWorkDetailsByOrderId(int orderId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("usp_MergeWorkDetailsByOrderID", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@кодЗаказа", orderId);
                var outputParam = new SqlParameter("@changesCounter", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParam);

                command.ExecuteNonQuery();
                return (int)outputParam.Value;
            }
        }


    }
}
