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
    public class WorkDetailRepository
    {
        private readonly DatabaseContext _context;

        public WorkDetailRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<WorkDetail> GetAllWorkDetails()
        {
            var workDetails = new List<WorkDetail>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM ДетальРаботы", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    workDetails.Add(new WorkDetail
                    {
                        Код = (int)reader["Код"],
                        КодРаботы = (int)reader["КодРаботы"],
                        КодДетали = (int)reader["КодДетали"],
                        Количество = (int)reader["Количество"]
                    });
                }
            }

            return workDetails;
        }

        public void AddWorkDetail(WorkDetail workDetail)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(
                    "INSERT INTO ДетальРаботы (КодРаботы, КодДетали, Количество) VALUES (@КодРаботы, @КодДетали, @Количество)", connection);

                command.Parameters.AddWithValue("@КодРаботы", workDetail.КодРаботы);
                command.Parameters.AddWithValue("@КодДетали", workDetail.КодДетали);
                command.Parameters.AddWithValue("@Количество", workDetail.Количество);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateWorkDetail(WorkDetail workDetail)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(
                    "UPDATE ДетальРаботы SET КодРаботы = @КодРаботы, КодДетали = @КодДетали, Количество = @Количество WHERE Код = @Код", connection);

                command.Parameters.AddWithValue("@Код", workDetail.Код);
                command.Parameters.AddWithValue("@КодРаботы", workDetail.КодРаботы);
                command.Parameters.AddWithValue("@КодДетали", workDetail.КодДетали);
                command.Parameters.AddWithValue("@Количество", workDetail.Количество);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteWorkDetail(int id)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("DELETE FROM ДетальРаботы WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", id);

                command.ExecuteNonQuery();
            }
        }

        public List<object> GetWorkDetailsCosts()
        {
            var results = new List<object>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(@"
                            SELECT 
                                w.Код AS WorkId,
                                SUM(d.Стоимость * wd.Количество) AS TotalCost
                            FROM Работа w
                            JOIN ДетальРаботы wd ON w.Код = wd.КодРаботы
                            JOIN Деталь d ON wd.КодДетали = d.Код
                            GROUP BY w.Код", connection);

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new
                    {
                        WorkId = (int)reader["WorkId"],
                        TotalCost = (decimal)reader["TotalCost"]
                    });
                }
            }

            return results;
        }

    }
}
