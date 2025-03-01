using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Data.SqlClient;
using AutoserviceApp.Models;
using System.Windows;

namespace AutoserviceApp.DataAccess.Repositories
{
    public class ComplaintRepository
    {
        private readonly DatabaseContext _context;

        public ComplaintRepository(DatabaseContext context)
        {
            _context = context;
        }

        public List<Complaint> GetAllComplaints()
        {
            var complaints = new List<Complaint>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Жалоба", connection);
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

        public void AddComplaint(Complaint complaint)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(
                    "INSERT INTO Жалоба (КодРаботы, Описание, Дата) VALUES (@КодРаботы, @Описание, @Дата)", connection);

                command.Parameters.AddWithValue("@КодРаботы", complaint.КодРаботы);
                command.Parameters.AddWithValue("@Описание", complaint.Описание);
                command.Parameters.AddWithValue("@Дата", complaint.Дата);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateComplaint(Complaint complaint)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand(
                    "UPDATE Жалоба SET КодРаботы = @КодРаботы, Описание = @Описание, Дата = @Дата WHERE Код = @Код", connection);

                command.Parameters.AddWithValue("@Код", complaint.Код);
                command.Parameters.AddWithValue("@КодРаботы", complaint.КодРаботы);
                command.Parameters.AddWithValue("@Описание", complaint.Описание);
                command.Parameters.AddWithValue("@Дата", complaint.Дата);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteComplaint(int id)
        {
            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("DELETE FROM Жалоба WHERE Код = @Код", connection);
                command.Parameters.AddWithValue("@Код", id);

                command.ExecuteNonQuery();
            }
        }

        public List<Complaint> GetComplaintsByWorkId(int workId)
        {
            var complaints = new List<Complaint>();

            using (var connection = _context.GetConnection())
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Жалоба WHERE КодРаботы = @workId", connection);
                command.Parameters.AddWithValue("@workId", workId);

                //MessageBox.Show($"Executing query: SELECT * FROM Жалоба WHERE КодРаботы = {workId}");

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
