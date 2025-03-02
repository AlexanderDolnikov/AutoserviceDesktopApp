using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseContext _context;

        public UserRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT Id, Login, Role FROM Users", connection);
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
        public void AddUser(string login, string password, string role)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                // Генерируем соль и хеш пароля
                string salt = PasswordHelper.GenerateSalt();
                string hashedPassword = PasswordHelper.HashPassword(password, salt);

                var command = new SqlCommand(
                    "INSERT INTO Users (Login, PasswordHash, Salt, Role) VALUES (@Login, @PasswordHash, @Salt, @Role)",
                    connection);

                command.Parameters.AddWithValue("@Login", login);
                command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                command.Parameters.AddWithValue("@Salt", salt);
                command.Parameters.AddWithValue("@Role", role);

                command.ExecuteNonQuery();
            }
        }


        public void UpdateUser(int userId, string newLogin, string newRole)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(
                    "UPDATE Users SET Login = @Login, Role = @Role WHERE Id = @UserId",
                    connection);

                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Login", newLogin);
                command.Parameters.AddWithValue("@Role", newRole);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateUserPassword(int userId, string newPassword)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                // Генерируем новую соль и хеш пароля
                string newSalt = PasswordHelper.GenerateSalt();
                string newPasswordHash = PasswordHelper.HashPassword(newPassword, newSalt);

                var command = new SqlCommand(
                    "UPDATE Users SET PasswordHash = @PasswordHash, Salt = @Salt WHERE Id = @UserId",
                    connection);

                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
                command.Parameters.AddWithValue("@Salt", newSalt);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteUser(int userId)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("DELETE FROM Users WHERE Id = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.ExecuteNonQuery();
            }
        }
    }
}

