namespace AutoserviceApp.DataAccess.Models
{
    public class OrderWithInfo
    {
        public int Код { get; set; }
        public DateTime ДатаНачала { get; set; }
        public DateTime ДатаОкончания { get; set; }
        public int КодКлиента { get; set; }
        public string ФамилияКлиента { get; set; }
        public int КодАвтомобиля { get; set; }
        public string НомернойЗнакАвтомобиля { get; set; }
        public string ДатаОкончанияОтображение => ДатаОкончания != DateTime.MinValue ? ДатаОкончания.ToShortDateString() : "Не завершено";
    }
}
