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

namespace AutoserviceApp.Helpers
{
    public static class ExcelExportHelper
    {
        private static readonly DatabaseContext _context = new DatabaseContext();
        private static readonly ClientRepository _clientRepository = new ClientRepository(_context);
        private static readonly CarRepository _carRepository = new CarRepository(_context);
        private static readonly ModelRepository _modelRepository = new ModelRepository(_context);

        public static void ExportOrdersToExcel(List<OrderWithInfo> ordersWithInfo)
        {
            if (ordersWithInfo == null || !ordersWithInfo.Any())
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
                    ordersWithInfo = ordersWithInfo.OrderByDescending(o => o.ДатаНачала).ToList();

                    // Определяем диапазон дат
                    string startDate = ordersWithInfo.Last().ДатаНачала.ToShortDateString();
                    string endDate = ordersWithInfo.First().ДатаНачала.ToShortDateString();
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

                    foreach (var order in ordersWithInfo)
                    {
                        worksheet.Cells[row, 1].Value = order.ДатаНачала.ToShortDateString();
                        worksheet.Cells[row, 2].Value = order.ДатаОкончания.ToShortDateString();

                        var client = _clientRepository.GetById(order.КодКлиента);
                        worksheet.Cells[row, 3].Value = client?.Фамилия ?? "Неизвестно";
                        worksheet.Cells[row, 4].Value = client?.Имя ?? "Неизвестно";
                        worksheet.Cells[row, 5].Value = client?.Телефон ?? "Неизвестно";

                        var car = _carRepository.GetById(order.КодАвтомобиля);
                        worksheet.Cells[row, 6].Value = car?.НомернойЗнак ?? "Неизвестно";
                        worksheet.Cells[row, 7].Value = _modelRepository.GetById(car?.КодМодели ?? 0)?.Название ?? "Неизвестно";

                        row++;
                    }

                    // Тонкие границы внутри таблицы
                    using (var tableBorders = worksheet.Cells[$"A5:G{row-1}"])
                    {
                        tableBorders.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        tableBorders.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    using (var tableBorders = worksheet.Cells[$"B5:B{row - 1}"])
                    {
                        tableBorders.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }
                    using (var tableBorders = worksheet.Cells[$"E5:E{row - 1}"])
                    {
                        tableBorders.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    // Жирная граница вокруг всей таблицы
                    using (var fullTableBorders = worksheet.Cells[$"A3:G{row-1}"])
                    {
                        fullTableBorders.Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    }

                    // Итоговые данные
                    worksheet.Cells[row + 1, 1].Value = "Итоги:";
                    worksheet.Cells[row + 1, 1].Style.Font.Bold = true;
                    worksheet.Cells[row + 1, 1, row + 1, 2].Merge = true;
                    worksheet.Cells[row + 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row + 1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));
                    worksheet.Cells[row + 1, 1].Style.Font.Size = 14;

                    worksheet.Cells[row + 2, 1].Value = $"Всего заказов: {ordersWithInfo.Count}";
                    worksheet.Cells[row + 2, 1].Style.Font.Size = 12;

                    worksheet.Cells.AutoFitColumns();

                    // Сохранение файла
                    string fileName = $"Заказы_{DateTime.Now:yyyy-MM-dd}.xlsx";
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel файлы (*.xlsx)|*.xlsx",
                        Title = "Сохранить отчет",
                        FileName = fileName
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, package.GetAsByteArray());
                        MessageBox.Show("Отчет успешно сохранен!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
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
