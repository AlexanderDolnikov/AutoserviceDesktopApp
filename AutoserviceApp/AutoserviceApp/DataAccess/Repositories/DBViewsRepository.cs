using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                        ОбщийДоход = (decimal)reader["ОбщийДоход"]
                    });
                }
            }
            return incomeList;
        }

        public List<MasterWithWorkAmounts> GetWorkStats()
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


    }
}
