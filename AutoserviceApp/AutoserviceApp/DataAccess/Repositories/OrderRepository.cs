using AutoserviceApp.DataAccess.Models;
using AutoserviceApp.Models;
using System.Data.SqlClient;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class OrderRepository : BaseRepository<Order>
    {
        public OrderRepository(DatabaseContext context) : base(context) { }

        public List<OrderWithInfo> GetAll()
        {
            var orders = new List<OrderWithInfo>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM vw_OrdersWithInfo", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new OrderWithInfo
                    {
                        Код = (int)reader["Код"],
                        ДатаНачала = (DateTime)reader["ДатаНачала"],
                        ДатаОкончания = reader["ДатаОкончания"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["ДатаОкончания"],
                        КодКлиента = (int)reader["КодКлиента"],
                        ФамилияКлиента = reader["ФамилияКлиента"].ToString(),
                        КодАвтомобиля = (int)reader["КодАвтомобиля"],
                        НомернойЗнакАвтомобиля = reader["НомернойЗнакАвтомобиля"].ToString()
                    });
                }
            }
            return orders;
        }

    }
}