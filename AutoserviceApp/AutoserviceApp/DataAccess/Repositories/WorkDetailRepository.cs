using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class WorkDetailRepository : BaseRepository<WorkDetail>
    {
        public WorkDetailRepository(DatabaseContext context) : base(context) { }
     }
}