using AutoserviceApp.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class BaseRepository<T> where T : class
    {
        protected readonly DatabaseContext _context;
        protected readonly string _tableName;
        protected readonly Func<SqlDataReader, T> _mapper;

        public BaseRepository(DatabaseContext context)
        {
            _context = context;
            _mapper = EntityMapper.GetMapper<T>();
            _tableName = EntityMapper.GetTableName<T>();
        }

        public T GetById(int id)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand($"SELECT * FROM {_tableName} WHERE Код = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                var reader = command.ExecuteReader();
                return reader.Read() ? _mapper(reader) : null;
            }
        }

        public List<T> GetAll()
        {
            var resultItems = new List<T>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand($"SELECT * FROM {_tableName}", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    resultItems.Add(_mapper(reader));
                }
            }

            return resultItems;
        }

        public void Add(string insertQuery, Action<SqlCommand> setParameters)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand(insertQuery, connection);
                setParameters(command);

                command.ExecuteNonQuery();
            }
        }

        public void Update(string updateQuery, Action<SqlCommand> setParameters)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand(updateQuery, connection);
                setParameters(command);

                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand($"DELETE FROM {_tableName} WHERE Код = @id", connection);
                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();
            }
        }
    }
}
