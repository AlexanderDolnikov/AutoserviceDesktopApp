using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class WorkRepository : BaseRepository<Work>
    {
        public WorkRepository(DatabaseContext context) : base(context) { }
    }
}