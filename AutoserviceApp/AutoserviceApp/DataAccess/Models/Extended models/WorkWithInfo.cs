namespace AutoserviceApp.DataAccess.Models
{
    public class WorkWithInfo
    {
        public int Код { get; set; }
        public int КодЗаказа { get; set; }
        public string Описание { get; set; }
        public decimal Стоимость { get; set; }
        public int КодМастера { get; set; }
        public string ФамилияМастера { get; set; }
        public int КодВидаРаботы { get; set; }
        public string НазваниеВидаРаботы { get; set; }

    }
}
