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

            LoadMastersData();
            LoadWorkTypesData();
            LoadIncomeData();
        }

        private void LoadMastersData()
        {
            var masterStats = _dbViewsRepository.GetMasterWithWorkAmounts();
            if (masterStats.Count == 0)
            {
                MessageBox.Show("Нет данных по мастерам.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            double[] values = masterStats.Select(m => (double)m.КоличествоРабот).ToArray();
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
            MastersChart.Plot.SetAxisLimits(yMin: 0);
            MastersChart.Plot.Title("Работы мастеров");
            MastersChart.Plot.YLabel("Количество работ");

            // Обновляем график
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

            // Очищаем и строим круговую диаграмму
            WorkTypesChart.Plot.Clear();
            WorkTypesChart.Plot.AddPie(values);
            WorkTypesChart.Plot.Title("Работы по видам");
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

            string[] months = incomeStats.Select(i => i.Месяц).ToArray();
            double[] income = incomeStats.Select(i => (double)i.ОбщийДоход).ToArray();

            // Очищаем и строим график дохода
            IncomeChart.Plot.Clear();
            IncomeChart.Plot.AddScatter(Enumerable.Range(0, months.Length).Select(i => (double)i).ToArray(), income);
            IncomeChart.Plot.XTicks(Enumerable.Range(0, months.Length).Select(i => (double)i).ToArray(), months);
            IncomeChart.Plot.Title("Доход по месяцам");
            IncomeChart.Plot.YLabel("Доход");
            IncomeChart.Render();
        }
    }
}
