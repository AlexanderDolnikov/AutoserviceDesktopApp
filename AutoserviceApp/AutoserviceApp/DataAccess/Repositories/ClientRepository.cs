using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class ClientRepository : BaseRepository<Client>
    {
        public ClientRepository(DatabaseContext context) : base(context) { }
    }
}