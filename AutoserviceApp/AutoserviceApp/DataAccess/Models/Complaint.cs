namespace AutoserviceApp.Models
{
    public class Complaint
    {
        public int Код { get; set; }
        public int КодРаботы { get; set; }
        public string Описание { get; set; }
        public DateTime Дата { get; set; }
    }
}