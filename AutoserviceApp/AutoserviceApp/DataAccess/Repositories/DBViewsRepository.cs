using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoserviceApp.DataAccess.Models;
using AutoserviceApp.Models;
using AutoserviceApp.Models.DBViewsModels;

namespace AutoserviceApp.DataAccess.Repositories
{
    class DBViewsRepository
    {
        private readonly DatabaseContext _context;

        public DBViewsRepository(DatabaseContext context)
        {
            _context = context;
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
                        ДатаНачала = (DateTime)reader["ДатаНачала"],
                        ДатаОкончания = reader["ДатаОкончания"] as DateTime?,
                        КодКлиента = (int)reader["КодКлиента"],
                        КодАвтомобиля = (int)reader["КодАвтомобиля"]
                    });
                }
            }
            return orders;
        }

        public List<MasterWithWorkAmounts> GetMasterWithWorkAmounts()
        {
            var workStatsList = new List<MasterWithWorkAmounts>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM vw_MasterWorksAmounts", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    workStatsList.Add(new MasterWithWorkAmounts
                    {
                        КодМастера = (int)reader["КодМастера"],
                        КоличествоРабот = (int)reader["КоличествоРабот"]
                    });
                }
            }
            return workStatsList;
        }

        public List<MonthlyIncome> GetMonthlyIncome()
        {
            var incomeList = new List<MonthlyIncome>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM vw_MonthlyIncome", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    incomeList.Add(new MonthlyIncome
                    {
                        Месяц = reader["Месяц"].ToString(),
                        КоличествоЗаказов = (int)reader["КоличествоЗаказов"],
                        ОбщийДоход = (decimal)reader["ОбщийДоход"]
                    });
                }
            }
            return incomeList;
        }

    }
}
