using System.Collections.Generic;
using System.Windows.Controls;
using AutoserviceApp.Interfaces;
using AutoserviceApp.Models;

namespace AutoserviceApp.Views
{
    public partial class DetailsView : UserControl, IRefreshable
    {
        public DetailsView()
        {
            InitializeComponent();
            LoadDetails();
        }

        public void RefreshData()
        {
            LoadDetails();
        }

        private void LoadDetails()
        {
            List<Detail> details = new List<Detail>
            {
                new Detail { Код = 1, Название = "Масляный фильтр", Производитель = "Bosch", Стоимость = 500 },
                new Detail { Код = 2, Название = "Тормозные колодки", Производитель = "Brembo", Стоимость = 3000 }
            };

            DetailsList.ItemsSource = details;
        }
    }
}
