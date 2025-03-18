using AutoserviceApp.DataAccess;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace AutoserviceApp.Views
{    
    public partial class ReportsView : UserControl
    {
        private readonly DatabaseContext _context;

        private readonly DBViewsRepository _dbViewsRepository;
        private readonly DBProceduresAndFunctionsRepository _dbProceduresAndFunctionsRepository;
        private readonly OrderRepository _orderRepository;

        public ReportsView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            
            _dbViewsRepository = new DBViewsRepository(_context);
            _dbProceduresAndFunctionsRepository = new DBProceduresAndFunctionsRepository(_context);
            _orderRepository = new OrderRepository(_context);
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

            var ordersForPeriod = _dbProceduresAndFunctionsRepository.GetOrdersForPeriod(startDate, endDate);

            if (ordersForPeriod.Count() == 0)
            {
                MessageBox.Show("За выбранный период нет заказов.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

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
