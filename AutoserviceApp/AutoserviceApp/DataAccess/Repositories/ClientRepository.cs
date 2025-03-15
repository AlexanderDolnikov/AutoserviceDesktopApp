using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class ClientRepository : BaseRepository<Client>
    {
        private readonly DatabaseContext _context;

        public ClientRepository(DatabaseContext context) : base(context) { }
    }
}