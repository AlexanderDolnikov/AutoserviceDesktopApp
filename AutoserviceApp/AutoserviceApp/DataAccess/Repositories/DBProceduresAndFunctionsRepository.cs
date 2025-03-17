using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoserviceApp.DataAccess.Models.DBProceduresAndFunctionsModels;

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


    }
}
