using System.Windows.Controls;
using AutoserviceApp.Interfaces;
using AutoserviceApp.Models;
using System.Windows;
using AutoserviceApp.DataAccess;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Helpers;

namespace AutoserviceApp.Views
{
    public partial class DetailsView : UserControl, IRefreshable
    {
        private readonly DatabaseContext _context;
        private readonly DetailRepository _detailRepository;
        private readonly WorkDetailRepository _workDetailRepository;
        private List<Detail> _details;
        private Detail _selectedDetail;
        private int _selectedDetailIndex;

        public DetailsView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _detailRepository = new DetailRepository(_context);
            _workDetailRepository = new WorkDetailRepository(_context);

            RefreshData();
        }

        public void RefreshData() => LoadDetails();

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(DetailNameTextBox, DetailCostTextBox, DetailManufacturerTextBox);
        }

        /* - - - - - */

        private void LoadDetails()
        {
            _details = _detailRepository.GetAll();
            DetailsListBox.ItemsSource = _details;
        }

        private void DetailsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DetailsListBox.SelectedItem is Detail selectedDetail)
            {
                _selectedDetail = selectedDetail;
                _selectedDetailIndex = DetailsListBox.SelectedIndex;

                DetailNameTextBox.Text = selectedDetail.Название;
                DetailCostTextBox.Text = selectedDetail.Стоимость.ToString();
                DetailManufacturerTextBox.Text = selectedDetail.Производитель;
            }
        }

        private void SearchDetails_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchDetailsTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                DetailsListBox.ItemsSource = _details;
                return;
            }

            var filteredDetails = _details
                .Where(d => d.Название.ToLower().Contains(searchText))
                .ToList();

            DetailsListBox.ItemsSource = filteredDetails;
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
                MessageBox.Show("Некорректное значение стоимости! Введите стоимость > 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newDetail = new Detail
            {
                Название = DetailNameTextBox.Text.Trim(),
                Стоимость = cost,
                Производитель = DetailManufacturerTextBox.Text.Trim()
            };

            _detailRepository.Add(newDetail);
            LoadDetails();

            SetFocusOnFirstInput();
        }

        private void EditDetail_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDetail == null) return;

            _selectedDetail.Название = DetailNameTextBox.Text.Trim();
            _selectedDetail.Производитель = DetailManufacturerTextBox.Text.Trim();

            if (!decimal.TryParse(DetailCostTextBox.Text, out decimal cost) || cost <= 0)
            {
                MessageBox.Show("Некорректное значение стоимости! Введите стоимость > 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _selectedDetail.Стоимость = cost;

            _detailRepository.Update(_selectedDetail);
            LoadDetails();

            DetailsListBox.SelectedIndex = _selectedDetailIndex;
        }

        private void DeleteDetail_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Detail selectedDetail)
            {
                // Проверка на существование связанных записей
                bool hasWorkDetails = _workDetailRepository.GetAll().Any(d => d.КодДетали == selectedDetail.Код);

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
                    _detailRepository.Delete(selectedDetail.Код);
                    MessageBox.Show("Деталь успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDetails();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                SetFocusOnFirstInput();
            }
        }
    }
}
