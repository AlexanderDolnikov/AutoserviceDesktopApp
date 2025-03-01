using AutoserviceApp.Models;
using DocumentFormat.OpenXml.InkML;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceApp.DataAccess.Repositories
{
    internal class WorkTypeRepository
    {
        private readonly DatabaseContext _context;

        public WorkTypeRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<WorkType> GetAllWorkTypes()
        {
            var workTypes = new List<WorkType>();

            using (var connection = _context.GetConnection())
            {
                string query = "SELECT Код, Название FROM ВидРаботы";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            workTypes.Add(new WorkType
                            {
                                Код = reader.GetInt32(0),
                                Название = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return workTypes;
        }


        public string GetWorkTypeNameById(int detailId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT Название FROM ВидРаботы WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", detailId);

                var result = command.ExecuteScalar();
                return result?.ToString() ?? "Неизвестно";
            }
        }

    }
}
