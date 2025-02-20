using System.Collections.Generic;
using System.Windows.Controls;
using AutoserviceApp.Models;

namespace AutoserviceApp.Views
{
    public partial class MastersView : UserControl
    {
        public MastersView()
        {
            InitializeComponent();
            LoadMasters();
        }

        private void LoadMasters()
        {
            List<Master> masters = new List<Master>
            {
                new Master { Код = 1, Имя = "Иван", Фамилия = "Иванов", Специализация = "Моторист" },
                new Master { Код = 2, Имя = "Петр", Фамилия = "Петров", Специализация = "Кузовной ремонт" }
            };

            MastersList.ItemsSource = masters;
        }
    }
}
