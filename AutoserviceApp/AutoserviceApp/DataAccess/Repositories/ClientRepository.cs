using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class ClientRepository : BaseRepository<Client>
    {
        private readonly DatabaseContext _context;

        public ClientRepository(DatabaseContext context) : base(context) { }

        public void Add(Client client)
        {
            Add($"INSERT INTO {_tableName} (Имя, Фамилия, Телефон, Адрес) VALUES (@Имя, @Фамилия, @Телефон, @Адрес)",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Имя", client.Имя);
                    cmd.Parameters.AddWithValue("@Фамилия", client.Фамилия);
                    cmd.Parameters.AddWithValue("@Телефон", client.Телефон);
                    cmd.Parameters.AddWithValue("@Адрес", client.Адрес);
                });
        }

        public void Update(Client client)
        {
            Update($"UPDATE {_tableName} SET Имя = @Имя, Фамилия = @Фамилия, Телефон = @Телефон, Адрес = @Адрес WHERE Код = @Код",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@Код", client.Код);
                    cmd.Parameters.AddWithValue("@Имя", client.Имя);
                    cmd.Parameters.AddWithValue("@Фамилия", client.Фамилия);
                    cmd.Parameters.AddWithValue("@Телефон", client.Телефон);
                    cmd.Parameters.AddWithValue("@Адрес", client.Адрес);
                });
        }
    }
}