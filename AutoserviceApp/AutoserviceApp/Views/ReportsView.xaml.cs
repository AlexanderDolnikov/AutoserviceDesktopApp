using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Helpers;
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

namespace AutoserviceApp.Views
{    public partial class ReportsView : UserControl
    {
        private readonly DBViewsRepository _dbViewsRepository;
        private readonly OrderRepository _orderRepository;

        public ReportsView()
        {
            InitializeComponent();
            _dbViewsRepository = new DBViewsRepository(new DataAccess.DatabaseContext());
            _orderRepository = new OrderRepository(new DataAccess.DatabaseContext());
        }

        // Отчет по заказам за период
        private void GenerateOrdersForPeriodReport_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = EndDatePicker.SelectedDate.Value;

            // Получаем все заказы и фильтруем их по датам
            var orders = _orderRepository.GetAll()
                .Where(o => o.ДатаНачала >= startDate && o.ДатаНачала <= endDate)
                .ToList();

            if (orders.Count == 0)
            {
                MessageBox.Show("За выбранный период нет заказов.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ordersForPeriod = _dbViewsRepository.GetOrdersForPeriod(startDate, endDate);

            // Вызываем новый метод, который использует хранимую процедуру
            ExcelExportHelper.ExportOrdersToExcel(ordersForPeriod, true);
        }


        // Отчет по всем заказам
        private void ExportAllOrders_Click(object sender, RoutedEventArgs e)
        {
            var orders = _orderRepository.GetAll();

            if (orders.Count == 0)
            {
                MessageBox.Show("Нет заказов для экспорта.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ExcelExportHelper.ExportOrdersToExcel(orders);
        }


        // Отчет по статистике мастеров
        private void GenerateMasterStatsReport_Click(object sender, RoutedEventArgs e)
        {
            var masterStats = _dbViewsRepository.GetMasterWithWorkAmounts();

            if (masterStats.Count == 0)
            {
                MessageBox.Show("Нет данных по мастерам.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ExcelExportHelper.ExportMasterWithWorkAmountsToExcel(masterStats);
        }

        // Отчет по доходу по месяцам
        private void GenerateMonthlyIncomeReport_Click(object sender, RoutedEventArgs e)
        {
            var incomeStats = _dbViewsRepository.GetMonthlyIncome();

            if (incomeStats.Count == 0)
            {
                MessageBox.Show("Нет данных о доходах.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ExcelExportHelper.ExportMonthlyIncomeToExcel(incomeStats);
        }
    }
}
