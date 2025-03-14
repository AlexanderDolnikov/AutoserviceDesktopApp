using System.Data.SqlClient;
using System.Windows;
using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class OrderRepository
    {
        private readonly DatabaseContext _context;
        private readonly WorkRepository _workRepository;
        private readonly ComplaintRepository _complaintRepository;
        private readonly WorkDetailRepository _workDetailRepository;

        public OrderRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<Order> GetAllOrders()
        {
            var orders = new List<Order>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Заказ ORDER BY ДатаНачала DESC, ДатаОкончания DESC, КодКлиента, КодАвтомобиля", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new Order
                    {
                        Код = (int)reader["Код"],
                        ДатаНачала = (DateTime)reader["ДатаНачала"],
                        ДатаОкончания = reader.IsDBNull(reader.GetOrdinal("ДатаОкончания")) ? (DateTime?)null : (DateTime)reader["ДатаОкончания"],
                        КодКлиента = (int)reader["КодКлиента"],
                        КодАвтомобиля = (int)reader["КодАвтомобиля"]
                    });
                }
            }

            return orders;
        }

        public void AddOrder(Order order)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO Заказ (ДатаНачала, ДатаОкончания, КодКлиента, КодАвтомобиля) VALUES (@ДатаНачала, @ДатаОкончания, @КодКлиента, @КодАвтомобиля)", connection);
                command.Parameters.AddWithValue("@ДатаНачала", order.ДатаНачала);

                if (order.ДатаОкончания.HasValue)
                    command.Parameters.AddWithValue("@ДатаОкончания", order.ДатаОкончания.Value);
                else
                    command.Parameters.AddWithValue("@ДатаОкончания", DBNull.Value);

                command.Parameters.AddWithValue("@КодКлиента", order.КодКлиента);
                command.Parameters.AddWithValue("@КодАвтомобиля", order.КодАвтомобиля);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateOrder(Order order)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("UPDATE Заказ SET ДатаНачала = @ДатаНачала, ДатаОкончания = @ДатаОкончания, КодКлиента = @КодКлиента, КодАвтомобиля = @КодАвтомобиля WHERE Код = @Код", connection);

                command.Parameters.AddWithValue("@Код", order.Код);
                command.Parameters.AddWithValue("@ДатаНачала", order.ДатаНачала);

                if (order.ДатаОкончания.HasValue)
                    command.Parameters.AddWithValue("@ДатаОкончания", order.ДатаОкончания.Value);
                else
                    command.Parameters.AddWithValue("@ДатаОкончания", DBNull.Value);
                
                command.Parameters.AddWithValue("@КодКлиента", order.КодКлиента);
                command.Parameters.AddWithValue("@КодАвтомобиля", order.КодАвтомобиля);

                command.ExecuteNonQuery();
            }
        }

        public Order GetOrderById(int orderId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Заказ WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", orderId);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new Order
                    {
                        Код = (int)reader["Код"],
                        ДатаНачала = (DateTime)reader["ДатаНачала"],
                        ДатаОкончания = (DateTime)reader["ДатаОкончания"],
                        КодКлиента = (int)reader["КодКлиента"],
                        КодАвтомобиля = (int)reader["КодАвтомобиля"]
                    };
                }
            }
            return null;
        }

        public void DeleteOrder(int orderId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Заказ WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", orderId);
                command.ExecuteNonQuery();
            }
        }

    }
}
