using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class CarRepository : BaseRepository<Car>
    {
        public CarRepository(DatabaseContext context) : base(context) { }
    }
}