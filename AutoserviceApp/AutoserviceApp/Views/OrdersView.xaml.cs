using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.Models; // Если Order находится в Models


namespace AutoserviceApp.Views
{
    public partial class OrdersView : UserControl
    {
        public OrdersView()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            var clients = new Dictionary<int, string>
            {
                { 1, "Иванов И.И." },
                { 2, "Петров П.П." }
            };

            List<Order> orders = new List<Order>
            {
                new Order { Код = 1, ДатаНачала = DateTime.Parse("2024-01-10"), ДатаОкончания = DateTime.Parse("2024-01-20"), КодКлиента = 1 },
                new Order { Код = 2, ДатаНачала = DateTime.Parse("2024-02-05"), ДатаОкончания = DateTime.Parse("2024-02-12"), КодКлиента = 2 }
            };

            OrdersList.ItemsSource = orders.Select(o => new
            {
                o.Код,
                o.ДатаНачала,
                o.ДатаОкончания,
                Клиент = clients.ContainsKey(o.КодКлиента) ? clients[o.КодКлиента] : "Неизвестный клиент"
            }).ToList();
        }



        private void ShowOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int orderId = (int)button.Tag;

            MessageBox.Show($"Показать детали заказа {orderId}");
        }
    }
}

