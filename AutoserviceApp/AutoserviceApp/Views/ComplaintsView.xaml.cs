using System;
using System.Collections.Generic;
using System.Windows.Controls;
using AutoserviceApp.Models;

namespace AutoserviceApp.Views
{
    public partial class ComplaintsView : UserControl
    {
        public ComplaintsView()
        {
            InitializeComponent();
            LoadComplaints();
        }

        private void LoadComplaints()
        {
            List<Complaint> complaints = new List<Complaint>
            {
                new Complaint { Код = 1, Описание = "Плохое качество ремонта", Дата = DateTime.Today },
                new Complaint { Код = 2, Описание = "Долго выполняли работу", Дата = DateTime.Today.AddDays(-5) }
            };

            ComplaintsList.ItemsSource = complaints;
        }
    }
}
