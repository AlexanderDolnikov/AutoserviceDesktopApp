using System.Collections.Generic;
using System.Windows.Controls;
using AutoserviceApp.Interfaces;
using AutoserviceApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess.Repositories;

namespace AutoserviceApp.Views
{
    public partial class DetailsView : UserControl
    {
        private readonly DatabaseContext _context;
        private readonly DetailRepository _detailRepository;
        private readonly WorkDetailRepository _workDetailRepository;
        private List<Detail> _details;
        private Detail _selectedDetail;

        public DetailsView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _detailRepository = new DetailRepository(_context);
            _workDetailRepository = new WorkDetailRepository(_context);

            LoadDetails();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }

        private void LoadDetails()
        {
            _details = _detailRepository.GetAllDetails();
            DetailsListBox.ItemsSource = _details;
        }

        private void DetailsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DetailsListBox.SelectedItem is Detail selectedDetail)
            {
                _selectedDetail = selectedDetail;
                DetailNameTextBox.Text = selectedDetail.Название;
                DetailCostTextBox.Text = selectedDetail.Стоимость.ToString();
                DetailManufacturerTextBox.Text = selectedDetail.Производитель;
            }
        }

        private void AddDetail_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DetailNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(DetailCostTextBox.Text) ||
                string.IsNullOrWhiteSpace(DetailManufacturerTextBox.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(DetailCostTextBox.Text, out decimal cost) || cost <= 0)
            {
                MessageBox.Show("Некорректное значение стоимости!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newDetail = new Detail
            {
                Название = DetailNameTextBox.Text.Trim(),
                Стоимость = cost,
                Производитель = DetailManufacturerTextBox.Text.Trim()
            };

            _detailRepository.AddDetail(newDetail);
            LoadDetails();
            DetailNameTextBox.Clear();
            DetailCostTextBox.Clear();
            DetailManufacturerTextBox.Clear();
        }

        private void EditDetail_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDetail == null) return;

            _selectedDetail.Название = DetailNameTextBox.Text.Trim();
            _selectedDetail.Производитель = DetailManufacturerTextBox.Text.Trim();

            if (!decimal.TryParse(DetailCostTextBox.Text, out decimal cost) || cost <= 0)
            {
                MessageBox.Show("Некорректное значение стоимости!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _selectedDetail.Стоимость = cost;

            _detailRepository.UpdateDetail(_selectedDetail);
            LoadDetails();
        }

        private void DeleteDetail_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Detail selectedDetail)
            {
                // Проверка на существование связанных записей
                bool hasWorkDetails = _workDetailRepository.GetAllWorkDetails().Any(d => d.КодДетали == selectedDetail.Код);

                if (hasWorkDetails)
                {
                    var result = MessageBox.Show(
                        "Эта деталь используется в работах. Все связанные данные будут удалены.\nВы уверены, что хотите удалить её?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
                }

                try
                {
                    _detailRepository.DeleteDetail(selectedDetail.Код);
                    MessageBox.Show("Деталь успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDetails();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
