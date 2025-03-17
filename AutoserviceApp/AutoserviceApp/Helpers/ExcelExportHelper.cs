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

                    // Сортируем заказы по убыванию даты начала
                    orders = orders.OrderByDescending(o => o.ДатаНачала).ToList();

                    // Определяем диапазон дат
                    string startDate = orders.Last().ДатаНачала.ToShortDateString();
                    string endDate = orders.First().ДатаНачала.ToShortDateString();
                    string safeStartDate = orders.Last().ДатаНачала.ToString("yyyy-MM-dd");
                    string safeEndDate = orders.First().ДатаНачала.ToString("yyyy-MM-dd");

                    string reportTitle = $"Все заказы по датам: {startDate} - {endDate}";

                    // Заголовок отчета
                    worksheet.Cells["A1:G1"].Merge = true;
                    worksheet.Cells["A1"].Value = reportTitle;
                    worksheet.Cells["A1"].Style.Font.Size = 16;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));
                    worksheet.Row(1).Height = 30;

                    // Тонкие границы внутри pаголовков секций
                    using (var tableBorders = worksheet.Cells[$"A3:G4"])
                    {
                        tableBorders.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    // Заголовки секций (с мержем)
                    worksheet.Cells["A3:B3"].Merge = true;
                    worksheet.Cells["A3"].Value = "Заказ";
                    worksheet.Cells["C3:E3"].Merge = true;
                    worksheet.Cells["C3"].Value = "Клиент";
                    worksheet.Cells["F3:G3"].Merge = true;
                    worksheet.Cells["F3"].Value = "Автомобиль";

                    // Заголовки 2 уровня
                    worksheet.Cells[4, 1].Value = "Дата начала";
                    worksheet.Cells[4, 2].Value = "Дата окончания";
                    worksheet.Cells[4, 3].Value = "Фамилия";
                    worksheet.Cells[4, 4].Value = "Имя";
                    worksheet.Cells[4, 5].Value = "Телефон";
                    worksheet.Cells[4, 6].Value = "Номерной знак";
                    worksheet.Cells[4, 7].Value = "Модель";

                    // Стили заголовков
                    using (var range = worksheet.Cells["A3:G4"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    // Заполняем данные
                    int row = 5;

                    foreach (var order in orders)
                    {
                        worksheet.Cells[row, 1].Value = order.ДатаНачала.ToShortDateString();
                        worksheet.Cells[row, 2].Value = order.ДатаОкончания.HasValue ? order.ДатаОкончания.Value.ToShortDateString() : "В процессе";

                        var client = _clientRepository.GetById(order.КодКлиента);
                        worksheet.Cells[row, 3].Value = client?.Фамилия ?? "Неизвестно";
                        worksheet.Cells[row, 4].Value = client?.Имя ?? "Неизвестно";
                        worksheet.Cells[row, 5].Value = client?.Телефон ?? "Неизвестно";

                        var car = _carRepository.GetById(order.КодАвтомобиля);
                        worksheet.Cells[row, 6].Value = car?.НомернойЗнак ?? "Неизвестно";
                        worksheet.Cells[row, 7].Value = _modelRepository.GetById(car?.КодМодели ?? 0)?.Название ?? "Неизвестно";

                        row++;
                    }

                    // Клетки таблицы + заголовки
                    using (var tableBorders = worksheet.Cells[$"A5:G{row-1}"])
                    {
                        tableBorders.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    // Итоговые данные
                    worksheet.Cells[row + 1, 1].Value = "Итоги:";
                    worksheet.Cells[row + 1, 1].Style.Font.Bold = true;
                    worksheet.Cells[row + 1, 1, row + 1, 2].Merge = true;
                    worksheet.Cells[row + 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row + 1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));
                    worksheet.Cells[row + 1, 1].Style.Font.Size = 14;

                    worksheet.Cells[row + 2, 1].Value = $"Всего заказов: {orders.Count}";
                    worksheet.Cells[row + 2, 1].Style.Font.Size = 12;

                    worksheet.Cells.AutoFitColumns();

                    // Сохранение файла
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
                    worksheet.Cells[row + 1, 1, row + 1, 2].Merge = true;
                    worksheet.Cells[row + 1, 1].Value = "Итоги:";
                    worksheet.Cells[row + 1, 1].Style.Font.Bold = true;
                    worksheet.Cells[row + 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row + 1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));

                    worksheet.Cells[row + 2, 1].Value = "Максимум работ у мастера:";
                    worksheet.Cells[row + 2, 2].Value = masterStats.Max(m => m.КоличествоРабот);
                    worksheet.Cells[row + 3, 1].Value = "Минимум работ у мастера:";
                    worksheet.Cells[row + 3, 2].Value = masterStats.Min(m => m.КоличествоРабот);
                    worksheet.Cells[row + 4, 1].Value = "Среднее количество работ у мастеров:";
                    worksheet.Cells[row + 4, 2].Value = Math.Round(masterStats.Average(m => m.КоличествоРабот), 3);

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
                    var minMonth = incomeStats.Min(i => i.Месяц);
                    var maxMonth = incomeStats.Max(i => i.Месяц);

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
                    int monthsCount = 0;

                    // Заполняем данные, включая пропущенные месяца
                    for (var date = maxDate; date >= minDate; date = date.AddMonths(-1))
                    {
                        var month = date.ToString("yyyy-MM");
                        var incomeData = incomeStats.FirstOrDefault(i => i.Месяц == month);

                        worksheet.Cells[row, 1].Value = month;
                        worksheet.Cells[row, 2].Value = incomeData?.КоличествоЗаказов ?? 0;
                        worksheet.Cells[row, 3].Value = incomeData?.ОбщийДоход ?? 0;
                        row++;
                        monthsCount++;
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

                    worksheet.Cells[row + 2, 1].Value = "Всего месяцев работы:";
                    worksheet.Cells[row + 2, 2].Value = monthsCount;
                    worksheet.Cells[row + 3, 1].Value = "Суммарный доход:";
                    worksheet.Cells[row + 3, 2].Value = incomeStats.Sum(i => i.ОбщийДоход);
                    worksheet.Cells[row + 4, 1].Value = "Средний доход в месяц:";
                    worksheet.Cells[row + 4, 2].Value = Math.Round(incomeStats.Sum(i => i.ОбщийДоход) / monthsCount, 2);
                    worksheet.Cells[row + 5, 1].Value = "Макс. заказов в месяц:";
                    worksheet.Cells[row + 5, 2].Value = incomeStats.Max(i => i.КоличествоЗаказов);
                    worksheet.Cells[row + 6, 1].Value = "Мин. заказов в месяц:";
                    worksheet.Cells[row + 6, 2].Value = incomeStats.Min(i => i.КоличествоЗаказов);
                    worksheet.Cells[row + 7, 1].Value = "Среднее количество заказов:";
                    worksheet.Cells[row + 7, 2].Value = Math.Round(incomeStats.Average(i => i.КоличествоЗаказов), 2);

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
