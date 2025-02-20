using System.Collections.Generic;
using System.Windows.Controls;
using AutoserviceApp.Models;

namespace AutoserviceApp.Views
{
    public partial class DetailsView : UserControl
    {
        public DetailsView()
        {
            InitializeComponent();
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
