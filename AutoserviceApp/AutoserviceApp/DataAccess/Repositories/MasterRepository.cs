using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class MasterRepository : BaseRepository<Master>
    {
        public MasterRepository(DatabaseContext context) : base(context) { }
    }
}