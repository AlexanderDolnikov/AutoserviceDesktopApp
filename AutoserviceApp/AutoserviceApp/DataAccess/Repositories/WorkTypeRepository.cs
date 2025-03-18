using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    internal class WorkTypeRepository : BaseRepository<WorkType>
    {
        public WorkTypeRepository(DatabaseContext context) : base(context) { }
    }
}