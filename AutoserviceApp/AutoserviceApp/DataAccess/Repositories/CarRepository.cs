using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
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
