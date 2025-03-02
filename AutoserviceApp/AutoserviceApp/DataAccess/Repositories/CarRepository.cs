using System.Collections.Generic;
using System.Data.SqlClient;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class CarRepository
    {
        private readonly DatabaseContext _context;

        public CarRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<Car> GetAllCars()
        {
            var cars = new List<Car>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Автомобиль", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    cars.Add(new Car
                    {
                        Код = (int)reader["Код"],
                        КодМодели = (int)reader["КодМодели"],
                        НомернойЗнак = reader["НомернойЗнак"].ToString(),
                        ГодВыпуска = (int)reader["ГодВыпуска"]
                    });
                }
            }
            return cars;
        }

        public void AddCar(Car car)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO Автомобиль (КодМодели, НомернойЗнак, ГодВыпуска) VALUES (@КодМодели, @НомернойЗнак, @ГодВыпуска)", connection);
                command.Parameters.AddWithValue("@КодМодели", car.КодМодели);
                command.Parameters.AddWithValue("@НомернойЗнак", car.НомернойЗнак);
                command.Parameters.AddWithValue("@ГодВыпуска", car.ГодВыпуска);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateCar(Car car)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Автомобиль SET КодМодели = @КодМодели, НомернойЗнак = @НомернойЗнак, ГодВыпуска = @ГодВыпуска WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@КодМодели", car.КодМодели);
                command.Parameters.AddWithValue("@НомернойЗнак", car.НомернойЗнак);
                command.Parameters.AddWithValue("@ГодВыпуска", car.ГодВыпуска);
                command.Parameters.AddWithValue("@Код", car.Код);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteCar(int carId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Автомобиль WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", carId);
                command.ExecuteNonQuery();
            }
        }

        public Car GetCarById(int carId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Автомобиль WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", carId);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new Car
                    {
                        Код = (int)reader["Код"],
                        КодМодели = (int)reader["КодМодели"],
                        НомернойЗнак = reader["НомернойЗнак"].ToString(),
                        ГодВыпуска = (int)reader["ГодВыпуска"]
                    };
                }
            }
            return null;
        }
    }
}