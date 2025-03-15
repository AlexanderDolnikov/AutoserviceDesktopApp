using AutoserviceApp.Models;
using DocumentFormat.OpenXml.CustomProperties;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            _tableName = EntityMapper.GetTableName(typeof(T));
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

        public void Add(T db_object)
        {
            // получаем свойства
            var properties = EntityMapper.GetCachedProperties(typeof(T));

            // создаем query
            string fieldNames = string.Join(", ", properties.Select(p => p.Name));
            string fieldValues = string.Join(", ", properties.Select(p => "@" + p.Name));

            string query = $"INSERT INTO {_tableName} ({fieldNames}) VALUES ({fieldValues})";

            // подключаемся к бд
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(query, connection);

                // вставляем значения
                foreach (var prop in properties)
                {
                    command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(db_object) ?? DBNull.Value);
                }

                command.ExecuteNonQuery();
            }
        }

        public void Update(T db_object)
        {
            // получаем свойства
            var properties = EntityMapper.GetCachedProperties(typeof(T));

            // создаем query
            var setFields = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

            string query = $"UPDATE {_tableName} SET {setFields} WHERE Код = @Код";

            // подключаемся к бд
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(query, connection);

                // вставляем значения
                foreach (var prop in properties)
                {
                    command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(db_object) ?? DBNull.Value);
                }

                // получаем id нашего объекта
                command.Parameters.AddWithValue("@Код", typeof(T).GetProperty("Код").GetValue(db_object));
            
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
