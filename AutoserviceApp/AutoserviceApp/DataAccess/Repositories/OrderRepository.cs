using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class OrderRepository : BaseRepository<Order>
    {
        public OrderRepository(DatabaseContext context) : base(context) { }
    }
}