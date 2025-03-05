using AutoserviceApp.Models;
using System.Data.SqlClient;

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
                connection.Open(); 
                
                var command = new SqlCommand("SELECT * FROM ВидРаботы ORDER BY Название", connection);
                var reader = command.ExecuteReader(); 
                
                while (reader.Read())
                {
                    workTypes.Add(new WorkType
                    {
                        Код = (int)reader["Код"],
                        Название = reader["Название"].ToString()
                    });
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

        public void AddWorkType(WorkType workType)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO ВидРаботы (Название) VALUES (@Название)", connection);
                command.Parameters.AddWithValue("@Название", workType.Название);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateWorkType(WorkType workType)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("UPDATE ВидРаботы SET Название = @Название WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", workType.Код);
                command.Parameters.AddWithValue("@Название", workType.Название);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteWorkType(int workTypeId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("DELETE FROM ВидРаботы WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", workTypeId);
                command.ExecuteNonQuery();
            }
        }
    }
}
