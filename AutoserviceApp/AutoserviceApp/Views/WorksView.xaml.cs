using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.Models;

namespace AutoserviceApp.Views
{
    public partial class WorksView : UserControl
    {
        public WorksView()
        {
            InitializeComponent();
            LoadWorks();
        }

        private void LoadWorks()
        {
            List<Work> works = new List<Work>
            {
                new Work { Код = 1, Описание = "Замена масла", Стоимость = 500 },
                new Work { Код = 2, Описание = "Ремонт подвески", Стоимость = 3000 }
            };

            WorksList.ItemsSource = works;
        }

        private void ShowComplaints_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int workId = (int)button.Tag;
            MessageBox.Show($"Показать жалобы по работе {workId}");
        }

        private void ShowWorkDetails_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int workId = (int)button.Tag;
            MessageBox.Show($"Показать детали работы {workId}");
        }
    }
}
