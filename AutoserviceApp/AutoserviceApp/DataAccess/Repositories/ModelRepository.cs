using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class ModelRepository : BaseRepository<Model>
    {
        public ModelRepository(DatabaseContext context) : base(context) { }
    }
}