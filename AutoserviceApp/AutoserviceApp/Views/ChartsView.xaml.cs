using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.DataAccess;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Models;
using ScottPlot;

namespace AutoserviceApp.Views
{
    public partial class ChartsView : UserControl
    {
        private readonly DatabaseContext _context;
        private readonly DBViewsRepository _dbViewsRepository;
        private readonly DBProceduresAndFunctionsRepository _dbProceduresAndFunctionsRepository;
        private readonly MasterRepository _masterRepository;

        public ChartsView()
        {
            InitializeComponent();

            _context = new DatabaseContext();
            _dbViewsRepository = new DBViewsRepository(_context);
            _dbProceduresAndFunctionsRepository = new DBProceduresAndFunctionsRepository(_context);
            _masterRepository = new MasterRepository(_context);

            MastersChart.Configuration.LockHorizontalAxis = true;
            MastersChart.Configuration.LockVerticalAxis = true;
            WorkTypesChart.Configuration.LockHorizontalAxis = true;
            WorkTypesChart.Configuration.LockVerticalAxis = true;
            IncomeChart.Configuration.LockHorizontalAxis = true;
            IncomeChart.Configuration.LockVerticalAxis = true;

            ApplyStyles(MastersChart);
            ApplyStyles(WorkTypesChart);
            ApplyStyles(IncomeChart);

            LoadMastersData();
            LoadWorkTypesData();
            LoadIncomeData();
        }

        void ApplyStyles(WpfPlot chart)
        {
            chart.Plot.Style(
                figureBackground: System.Drawing.ColorTranslator.FromHtml("#2c4055"),
                dataBackground: System.Drawing.ColorTranslator.FromHtml("#34495e"),
                grid: System.Drawing.ColorTranslator.FromHtml("#506070"),
                axisLabel: System.Drawing.Color.White,
                titleLabel: System.Drawing.Color.White,
                tick: System.Drawing.Color.LightGray
            );
            chart.Refresh();
        }

        private void LoadMastersData()
        {
            var masterStats = _dbViewsRepository.GetMasterWithWorkAmounts();
            if (masterStats.Count == 0)
            {
                MessageBox.Show("Нет данных по мастерам.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            double[] values = masterStats.Select(m => (double)m.КоличествоРабот).OrderBy(m => m).ToArray();
            double[] positions = Enumerable.Range(0, values.Length).Select(i => (double)i).ToArray();
            string[] labels = masterStats
                .Select(m =>
                {
                    var master = _masterRepository.GetById(m.КодМастера);
                    return master != null ? $"{master.Фамилия} {master.Имя}" : "Неизвестный мастер";
                })
                .ToArray();

            MastersChart.Plot.Clear();
            MastersChart.Plot.AddBar(values, positions);
            MastersChart.Plot.XTicks(positions, labels);
            MastersChart.Plot.XLabel("Мастера");
            MastersChart.Plot.XAxis.TickLabelStyle(rotation: 45, fontSize: 14, fontBold: true);
            MastersChart.Plot.YAxis.TickLabelStyle(fontSize: 14);
            MastersChart.Plot.SetAxisLimits(yMin: 0);
            MastersChart.Plot.Title("Работы мастеров");
            MastersChart.Plot.YLabel("Количество работ");
            MastersChart.Refresh();
        }

        private void LoadWorkTypesData()
        {
            var workTypesData = _dbProceduresAndFunctionsRepository.GetWorkTypesStats();
            if (workTypesData.Count == 0)
            {
                MessageBox.Show("Нет данных по видам работ.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string[] labels = workTypesData.Select(w => w.НазваниеВидаРаботы).ToArray();
            double[] values = workTypesData.Select(w => (double)w.КоличествоРабот).ToArray();

            WorkTypesChart.Plot.Clear();
            WorkTypesChart.Plot.Title("Работы по видам работ");

            var pie = WorkTypesChart.Plot.AddPie(values);
            pie.SliceLabels = labels;
            pie.ShowPercentages = true;
            pie.ShowLabels = true;
            pie.CenterFont.Size = 12;
            pie.SliceFont.Size = 13;

            // Вынес текст за пределы круга
            pie.Explode = true;

            pie.CenterFont.Color = System.Drawing.Color.White;
            WorkTypesChart.Render();
        }

        private void LoadIncomeData()
        {
            var incomeStats = _dbViewsRepository.GetMonthlyIncome();

            if (incomeStats.Count == 0)
            {
                MessageBox.Show("Нет данных по доходности.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var monthsList = incomeStats
                .Select(i => DateTime.ParseExact(i.Месяц, "yyyy-MM", null))
                .ToList();

            var allMonths = Enumerable.Range(0, (monthsList.Max() - monthsList.Min()).Days / 30 + 1)
                .Select(offset => monthsList.Min().AddMonths(offset).ToString("yyyy-MM"))
                .ToArray();

            // Заполняем доходы, подставляя 0 для отсутствующих месяцев
            var incomeDict = incomeStats.ToDictionary(i => i.Месяц, i => (double)i.ОбщийДоход);

            double[] income = allMonths
                .Select(m => incomeDict.GetValueOrDefault(m, 0))
                .ToArray();

            IncomeChart.Plot.Clear();

            var xs = Enumerable.Range(0, allMonths.Length)
                .Select(i => (double)i)
                .ToArray();

            IncomeChart.Plot.AddScatter(xs, income, System.Drawing.Color.Cyan, lineWidth: 2, markerSize: 8, markerShape: ScottPlot.MarkerShape.filledCircle);

            IncomeChart.Plot.XTicks(xs, allMonths);
            IncomeChart.Plot.Title("Доход по месяцам");
            IncomeChart.Plot.YLabel("Доход");
            IncomeChart.Plot.XLabel("Месяцы");
            IncomeChart.Plot.XAxis.TickLabelStyle(fontSize: 14);
            IncomeChart.Plot.YAxis.TickLabelStyle(fontSize: 14);
            IncomeChart.Render();
        }

    }
}
