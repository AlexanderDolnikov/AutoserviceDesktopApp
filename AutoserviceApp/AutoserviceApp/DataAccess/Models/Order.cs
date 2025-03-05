namespace AutoserviceApp.Models
{
    public class Order
    {
        public int Код { get; set; }
        public DateTime ДатаНачала { get; set; }
        public DateTime? ДатаОкончания { get; set; }
        public int КодКлиента { get; set; }
        public int КодАвтомобиля { get; set; }
    }
}

