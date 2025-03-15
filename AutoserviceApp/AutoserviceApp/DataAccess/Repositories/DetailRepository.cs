using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class DetailRepository : BaseRepository<Detail>
    {

        public DetailRepository(DatabaseContext context) : base(context) { }
    }
}