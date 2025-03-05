using System.Data.SqlClient;
using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class ModelRepository
    {
        private readonly DatabaseContext _context;

        public ModelRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<Model> GetAllModels()
        {
            var models = new List<Model>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Модель ORDER BY Название", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    models.Add(new Model
                    {
                        Код = (int)reader["Код"],
                        Название = reader["Название"].ToString()
                    });
                }
            }
            return models;
        }

        public void AddModel(Model model)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO Модель (Название) VALUES (@Название)", connection);
                command.Parameters.AddWithValue("@Название", model.Название);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateModel(Model model)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Модель SET Название = @Название WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Название", model.Название);
                command.Parameters.AddWithValue("@Код", model.Код);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteModel(int modelId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Модель WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", modelId);
                command.ExecuteNonQuery();
            }
        }

        public string GetModelNameById(int modelId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT Название FROM Модель WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", modelId);
                return command.ExecuteScalar()?.ToString() ?? "Неизвестно";
            }
        }
    }
}
