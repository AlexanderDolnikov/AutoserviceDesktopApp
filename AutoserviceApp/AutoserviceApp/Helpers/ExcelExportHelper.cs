using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using AutoserviceApp.Models.DBViewsModels;
using Microsoft.Win32;
using AutoserviceApp.DataAccess.Models;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;

namespace AutoserviceApp.Helpers
{
    public static class ExcelExportHelper
    {
        private static readonly DatabaseContext _context = new DatabaseContext();
        private static readonly ClientRepository _clientRepository = new ClientRepository(_context);
        private static readonly CarRepository _carRepository = new CarRepository(_context);
        private static readonly ModelRepository _modelRepository = new ModelRepository(_context);
        private static readonly MasterRepository _masterRepository = new MasterRepository(_context);
        private static readonly DBProceduresAndFunctionsRepository _DBProceduresAndFunctionsRepository = new DBProceduresAndFunctionsRepository(_context);

        public static void ExportOrdersToExcel(List<Order> orders, bool customTimeFrame = false)
        {
            if (orders == null || !orders.Any())
            {
                MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Заказы");

                    orders = orders.OrderByDescending(o => o.ДатаНачала).ToList();

                    string startDate = orders.Last().ДатаНачала.ToShortDateString();
                    string endDate = orders.First().ДатаНачала.ToShortDateString();
                    string safeStartDate = orders.Last().ДатаНачала.ToString("yyyy-MM-dd");
                    string safeEndDate = orders.First().ДатаНачала.ToString("yyyy-MM-dd");

                    string reportTitle = $"Все заказы по датам: {startDate} - {endDate}";

                    worksheet.Cells["A1:H1"].Merge = true;
                    worksheet.Cells["A1"].Value = reportTitle;
                    worksheet.Cells["A1"].Style.Font.Size = 16;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));
                    worksheet.Row(1).Height = 30;

                    using (var tableBorders = worksheet.Cells["A3:H4"])
                    {
                        tableBorders.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    worksheet.Cells["A3:C3"].Merge = true;
                    worksheet.Cells["A3"].Value = "Заказ";
                    worksheet.Cells["D3:F3"].Merge = true;
                    worksheet.Cells["D3"].Value = "Клиент";
                    worksheet.Cells["G3:H3"].Merge = true;
                    worksheet.Cells["G3"].Value = "Автомобиль";

                    worksheet.Cells[4, 1].Value = "Стоимость";
                    worksheet.Cells[4, 2].Value = "Дата начала";
                    worksheet.Cells[4, 3].Value = "Дата окончания";
                    worksheet.Cells[4, 4].Value = "Фамилия";
                    worksheet.Cells[4, 5].Value = "Имя";
                    worksheet.Cells[4, 6].Value = "Телефон";
                    worksheet.Cells[4, 7].Value = "Номерной знак";
                    worksheet.Cells[4, 8].Value = "Модель";

                    using (var range = worksheet.Cells["A3:H4"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    int row = 5;
                    List<decimal> costs = new List<decimal>();

                    foreach (var order in orders)
                    {
                        decimal totalCost = _DBProceduresAndFunctionsRepository.GetTotalWorksCostForOrder(order.Код);
                        worksheet.Cells[row, 1].Value = totalCost;
                        costs.Add(totalCost);

                        worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // Выравнивание стоимости влево

                        worksheet.Cells[row, 2].Value = order.ДатаНачала.ToShortDateString();
                        worksheet.Cells[row, 3].Value = order.ДатаОкончания.HasValue ? order.ДатаОкончания.Value.ToShortDateString() : "В процессе";

                        var client = _clientRepository.GetById(order.КодКлиента);
                        worksheet.Cells[row, 4].Value = client?.Фамилия ?? "Неизвестно";
                        worksheet.Cells[row, 5].Value = client?.Имя ?? "Неизвестно";
                        worksheet.Cells[row, 6].Value = client?.Телефон ?? "Неизвестно";

                        var car = _carRepository.GetById(order.КодАвтомобиля);
                        worksheet.Cells[row, 7].Value = car?.НомернойЗнак ?? "Неизвестно";
                        worksheet.Cells[row, 8].Value = _modelRepository.GetById(car?.КодМодели ?? 0)?.Название ?? "Неизвестно";

                        row++;
                    }

                    using (var tableBorders = worksheet.Cells[$"A5:H{row - 1}"])
                    {
                        tableBorders.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    worksheet.Cells[row + 1, 1, row + 1, 3].Merge = true;
                    worksheet.Cells[row + 1, 1].Value = "Итоги:";
                    worksheet.Cells[row + 1, 1].Style.Font.Bold = true;
                    worksheet.Cells[row + 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row + 1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));
                    worksheet.Cells[row + 1, 1].Style.Font.Size = 14;

                    worksheet.Cells[row + 2, 1, row + 2, 2].Merge = true;
                    worksheet.Cells[row + 2, 1].Value = "Всего заказов:";
                    worksheet.Cells[row + 2, 3].Value = orders.Count();

                    worksheet.Cells[row + 3, 1, row + 3, 2].Merge = true;
                    worksheet.Cells[row + 3, 1].Value = "Суммарная стоимость:";
                    worksheet.Cells[row + 3, 3].Value = costs.Sum();

                    worksheet.Cells[row + 4, 1, row + 4, 2].Merge = true;
                    worksheet.Cells[row + 4, 1].Value = "Максимальная стоимость:";
                    worksheet.Cells[row + 4, 3].Value = costs.Max();

                    worksheet.Cells[row + 5, 1, row + 5, 2].Merge = true;
                    worksheet.Cells[row + 5, 1].Value = "Минимальная стоимость:";
                    worksheet.Cells[row + 5, 3].Value = costs.Min();

                    worksheet.Cells[row + 6, 1, row + 6, 2].Merge = true;
                    worksheet.Cells[row + 6, 1].Value = "Средняя стоимость:";
                    worksheet.Cells[row + 6, 3].Value = costs.Average();

                    worksheet.Cells.AutoFitColumns();

                    string fileName = (!customTimeFrame) ?
                        $"ОтчетПоВсемЗаказам_{DateTime.Now:yyyy-MM-dd}.xlsx" :
                        $"ОтчетПоЗаказамЗаПериод_({safeStartDate} - {safeEndDate})_{DateTime.Now:yyyy-MM-dd}.xlsx";

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel файлы (*.xlsx)|*.xlsx",
                        Title = "Сохранить отчет",
                        FileName = fileName
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, package.GetAsByteArray());
                        MessageBox.Show("Отчет сохранен!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void ExportMasterWithWorkAmountsToExcel(List<MasterWithWorkAmounts> masterStats)
        {
            if (masterStats == null || !masterStats.Any())
            {
                MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Статистика мастеров");

                    // Заголовок отчета
                    worksheet.Cells["A1:D1"].Merge = true;
                    worksheet.Cells["A1"].Value = "Статистика мастеров";
                    worksheet.Cells["A1"].Style.Font.Size = 16;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));
                    worksheet.Row(1).Height = 30;

                    // Заголовки таблицы
                    worksheet.Cells[3, 1].Value = "Фамилия";
                    worksheet.Cells[3, 2].Value = "Имя";
                    worksheet.Cells[3, 3].Value = "Телефон";
                    worksheet.Cells[3, 4].Value = "Количество работ";

                    using (var headerRange = worksheet.Cells["A3:D3"])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    // Заполняем данные, сортируем по убыванию количества работ
                    int row = 4;
                    foreach (var master in masterStats.OrderByDescending(m => m.КоличествоРабот))
                    {
                        var masterData = _masterRepository.GetById(master.КодМастера);

                        worksheet.Cells[row, 1].Value = masterData?.Фамилия ?? "Неизвестно";
                        worksheet.Cells[row, 2].Value = masterData?.Имя ?? "Неизвестно";
                        worksheet.Cells[row, 3].Value = masterData?.Телефон ?? "Неизвестно";
                        worksheet.Cells[row, 4].Value = master.КоличествоРабот;

                        row++;
                    }

                    // Клетки внутри таблицы + заголовки
                    using (var borderRange = worksheet.Cells[$"A3:D{row - 1}"])
                    {
                        borderRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    // Итоги
                    worksheet.Cells[row + 1, 1, row + 1, 3].Merge = true;
                    worksheet.Cells[row + 1, 1].Value = "Итоги:";
                    worksheet.Cells[row + 1, 1].Style.Font.Bold = true;
                    worksheet.Cells[row + 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row + 1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));

                    worksheet.Cells[row + 2, 1, row + 2, 2].Merge = true;
                    worksheet.Cells[row + 2, 1].Value = "Всего мастеров:";
                    worksheet.Cells[row + 2, 3].Value = masterStats.Count();

                    worksheet.Cells[row + 3, 1, row + 3, 2].Merge = true;
                    worksheet.Cells[row + 3, 1].Value = "Максимум работ:";
                    worksheet.Cells[row + 3, 3].Value = masterStats.Max(m => m.КоличествоРабот);

                    worksheet.Cells[row + 4, 1, row + 4, 2].Merge = true;
                    worksheet.Cells[row + 4, 1].Value = "Минимум работ:";
                    worksheet.Cells[row + 4, 3].Value = masterStats.Min(m => m.КоличествоРабот);
                    
                    worksheet.Cells[row + 5, 1, row + 5, 2].Merge = true;
                    worksheet.Cells[row + 5, 1].Value = "В среднем работ:";
                    worksheet.Cells[row + 5, 3].Value = Math.Round(masterStats.Average(m => m.КоличествоРабот), 3);

                    worksheet.Cells.AutoFitColumns();

                    // Сохранение файла
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel файлы (*.xlsx)|*.xlsx",
                        Title = "Сохранить отчет",
                        FileName = $"СтатистикаМастеров_{DateTime.Now:yyyy-MM-dd}.xlsx"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, package.GetAsByteArray());
                        MessageBox.Show("Отчет сохранен!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void ExportMonthlyIncomeToExcel(List<MonthlyIncome> incomeStats)
        {
            if (incomeStats == null || !incomeStats.Any())
            {
                MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (ExcelPackage package = new ExcelPackage())
                {
                    var minDate = incomeStats.Min(i => DateTime.Parse(i.Месяц));
                    var maxDate = incomeStats.Max(i => DateTime.Parse(i.Месяц));

                    // Генерация полного списка месяцев (включая пустые)
                    var fullMonths = new List<MonthlyIncome>();
                    for (var date = minDate; date <= maxDate; date = date.AddMonths(1))
                    {
                        var monthStr = date.ToString("yyyy-MM");
                        var existingData = incomeStats.FirstOrDefault(i => i.Месяц == monthStr);
                        if (existingData != null)
                        {
                            fullMonths.Add(existingData);
                        }
                        else
                        {
                            fullMonths.Add(new MonthlyIncome { Месяц = monthStr, КоличествоЗаказов = 0, ОбщийДоход = 0 });
                        }
                    }

                    var minMonth = fullMonths.Min(i => i.Месяц);
                    var maxMonth = fullMonths.Max(i => i.Месяц);

                    string reportTitle = $"Доход по месяцам: {minMonth} - {maxMonth}";

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(reportTitle);

                    // Заголовок
                    worksheet.Cells["A1:C1"].Merge = true;
                    worksheet.Cells["A1"].Value = reportTitle;
                    worksheet.Cells["A1"].Style.Font.Size = 16;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));
                    worksheet.Row(1).Height = 30;

                    // Заголовки таблицы
                    worksheet.Cells[3, 1].Value = "Месяц";
                    worksheet.Cells[3, 2].Value = "Количество заказов";
                    worksheet.Cells[3, 3].Value = "Доход";

                    using (var headerRange = worksheet.Cells["A3:C3"])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    int row = 4;

                    // Заполняем данные
                    foreach (var data in fullMonths.OrderByDescending(i => i.Месяц))
                    {
                        worksheet.Cells[row, 1].Value = data.Месяц;
                        worksheet.Cells[row, 2].Value = data.КоличествоЗаказов;
                        worksheet.Cells[row, 3].Value = data.ОбщийДоход;
                        row++;
                    }

                    // Клетки таблицы + заголовки
                    using (var borderRange = worksheet.Cells[$"A3:C{row - 1}"])
                    {
                        borderRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    // Итоги
                    worksheet.Cells[row + 1, 1, row + 1, 2].Merge = true;
                    worksheet.Cells[row + 1, 1].Value = "Итоги:";
                    worksheet.Cells[row + 1, 1].Style.Font.Bold = true;
                    worksheet.Cells[row + 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row + 1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));

                    // месяцы
                    worksheet.Cells[row + 2, 1].Value = "Всего месяцев работы:";
                    worksheet.Cells[row + 2, 2].Value = fullMonths.Count;

                    // доход
                    worksheet.Cells[row + 3, 1].Value = "Суммарный доход:";
                    worksheet.Cells[row + 3, 2].Value = fullMonths.Sum(i => i.ОбщийДоход);

                    worksheet.Cells[row + 4, 1].Value = "Максимальный доход:";
                    worksheet.Cells[row + 4, 2].Value = fullMonths.Max(i => i.ОбщийДоход);

                    worksheet.Cells[row + 5, 1].Value = "Средний доход:";
                    worksheet.Cells[row + 5, 2].Value = Math.Round(fullMonths.Average(i => i.ОбщийДоход), 2);

                    worksheet.Cells[row + 6, 1].Value = "Минимальный доход:";
                    worksheet.Cells[row + 6, 2].Value = fullMonths.Min(i => i.ОбщийДоход);

                    // количество заказов
                    worksheet.Cells[row + 7, 1].Value = "Макс. заказов:";
                    worksheet.Cells[row + 7, 2].Value = fullMonths.Max(i => i.КоличествоЗаказов);

                    worksheet.Cells[row + 8, 1].Value = "Среднее кол-во заказов:";
                    worksheet.Cells[row + 8, 2].Value = Math.Round(fullMonths.Average(i => i.КоличествоЗаказов), 2);

                    worksheet.Cells[row + 9, 1].Value = "Мин. заказов:";
                    worksheet.Cells[row + 9, 2].Value = fullMonths.Min(i => i.КоличествоЗаказов);

                    worksheet.Cells.AutoFitColumns();

                    // Сохранение файла
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel файлы (*.xlsx)|*.xlsx",
                        Title = "Сохранить отчет",
                        FileName = $"ДоходПоМесяцам_({minMonth} - {maxMonth})_{DateTime.Now:yyyy-MM-dd}.xlsx"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, package.GetAsByteArray());
                        MessageBox.Show("Отчет сохранен!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
