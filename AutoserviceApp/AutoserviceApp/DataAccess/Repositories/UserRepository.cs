using System.Data.SqlClient;
using AutoserviceApp.Models;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        private readonly DatabaseContext _context;

        public UserRepository(DatabaseContext context) : base(context) { _context = context; }
 
        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT Id, Login, Role FROM Users ORDER BY Role, Login", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = (int)reader["Id"],
                        Login = reader["Login"].ToString(),
                        Role = reader["Role"].ToString()
                    });
                }
            }

            return users;
        }
        public User GetUserByLogin(string login)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Users WHERE Login = @login", connection);
                command.Parameters.AddWithValue("@login", login);

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = (int)reader["Id"],
                        Login = reader["Login"].ToString(),
                        PasswordHash = reader["PasswordHash"].ToString(),
                        Salt = reader["Salt"].ToString(),
                        Role = reader["Role"].ToString()
                    };
                }
            }
            return null;
        }

        public void AddUser(User newUser, string newPassword)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("INSERT INTO Users (Login, PasswordHash, Salt, Role) VALUES (@Login, @PasswordHash, @Salt, @Role)", connection);

                command.Parameters.AddWithValue("@Login", newUser.Login);

                (string passwordHash, string passwordSalt) = UpdateUserPassword(newPassword);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@Salt", passwordSalt);

                command.Parameters.AddWithValue("@Role", newUser.Role);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateUser(User newUser, string newPassword)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("UPDATE Users SET Login = @Login, PasswordHash = @PasswordHash, Salt = @Salt, Role = @Role WHERE Id = @UserId", connection);

                command.Parameters.AddWithValue("@UserId", newUser.Id);
                command.Parameters.AddWithValue("@Login", newUser.Login);

                (string passwordHash, string passwordSalt) = UpdateUserPassword(newPassword);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@Salt", passwordSalt);

                command.Parameters.AddWithValue("@Role", newUser.Role);

                command.ExecuteNonQuery();
            }
        }

        public (string passwordHash, string passwordSalt) UpdateUserPassword(string newPassword)
        {
            string passwordSalt = PasswordHelper.GenerateSalt();
            string passwordHash = PasswordHelper.HashPassword(newPassword, passwordSalt);

            return (passwordHash, passwordSalt);
        }

        public void DeleteUser(int id)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand($"DELETE FROM {_tableName} WHERE Id = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();
            }
        }
    }
}