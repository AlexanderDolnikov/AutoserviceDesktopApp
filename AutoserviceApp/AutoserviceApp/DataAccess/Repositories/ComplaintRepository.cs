using System.Data.SqlClient;
using AutoserviceApp.Models;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class ComplaintRepository : BaseRepository<Complaint>
    {
        private readonly DatabaseContext _context;

        public ComplaintRepository(DatabaseContext context) : base(context) { _context = context; }
 
        public List<Complaint> GetComplaintsByWorkId(int workId)
        {
            var complaints = new List<Complaint>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Жалоба WHERE КодРаботы = @workId ORDER BY Дата DESC, Описание ASC", connection);
                command.Parameters.AddWithValue("@workId", workId);

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    complaints.Add(new Complaint
                    {
                        Код = (int)reader["Код"],
                        КодРаботы = (int)reader["КодРаботы"],
                        Описание = reader["Описание"].ToString(),
                        Дата = (DateTime)reader["Дата"]
                    });
                }

            }

            return complaints;
        }
    }
}