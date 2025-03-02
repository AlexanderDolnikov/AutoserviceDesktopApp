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
using System.Collections.Generic;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;

namespace AutoserviceApp.Views
{
    public partial class CarsView : UserControl
    {
        private readonly CarRepository _carRepository;
        private readonly ModelRepository _modelRepository;
        private readonly OrderRepository _orderRepository;
        private readonly DatabaseContext _context;
        private List<CarWithInfo> _cars;
        private CarWithInfo _selectedCar;

        public CarsView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _carRepository = new CarRepository(_context);
            _modelRepository = new ModelRepository(_context);
            _orderRepository = new OrderRepository(_context);

            LoadModels();
            LoadCars();
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
        private void LoadModels()
        {
            ModelDropdown.ItemsSource = _modelRepository.GetAllModels();
        }

        private void LoadCars()
        {
            _cars = _carRepository.GetAllCars()
                .Select(car => new CarWithInfo
                {
                    Код = car.Код,
                    КодМодели = car.КодМодели,
                    НазваниеМодели = _modelRepository.GetModelNameById(car.КодМодели),
                    НомернойЗнак = car.НомернойЗнак,
                    ГодВыпуска = car.ГодВыпуска
                })
                .ToList();

            CarsListBox.ItemsSource = _cars;
        }

        private void CarsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CarsListBox.SelectedItem is CarWithInfo selectedCar)
            {
                _selectedCar = selectedCar;
                ModelDropdown.SelectedValue = selectedCar.КодМодели;
                CarNumberTextBox.Text = selectedCar.НомернойЗнак;
                CarYearTextBox.Text = selectedCar.ГодВыпуска.ToString();
            }
        }

        private void AddCar_Click(object sender, RoutedEventArgs e)
        {
            if (ModelDropdown.SelectedValue == null)
            {
                MessageBox.Show("Выберите модель автомобиля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CarNumberTextBox.Text) || string.IsNullOrWhiteSpace(CarYearTextBox.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newCar = new Car
            {
                КодМодели = (int)ModelDropdown.SelectedValue,
                НомернойЗнак = CarNumberTextBox.Text,
                ГодВыпуска = int.Parse(CarYearTextBox.Text)
            };

            _carRepository.AddCar(newCar);
            LoadCars();
        }

        private void EditCar_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCar == null)
            {
                MessageBox.Show("Выберите автомобиль для изменения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ModelDropdown.SelectedValue == null)
            {
                MessageBox.Show("Выберите модель автомобиля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CarNumberTextBox.Text) || string.IsNullOrWhiteSpace(CarYearTextBox.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var updatedCar = new Car
            {
                Код = _selectedCar.Код,
                КодМодели = (int)ModelDropdown.SelectedValue,
                НомернойЗнак = CarNumberTextBox.Text,
                ГодВыпуска = int.Parse(CarYearTextBox.Text)
            };

            _carRepository.UpdateCar(updatedCar);
            LoadCars();
        }

        private void DeleteCar_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is CarWithInfo selectedCar)
            {
                // Проверяем, есть ли заказы, связанные с этим автомобилем
                bool hasOrders = _orderRepository.GetAllOrders().Any(order => order.КодАвтомобиля == selectedCar.Код);

                if (hasOrders)
                {
                    var result = MessageBox.Show(
                        "Этот автомобиль используется в заказах. Удаление приведет к удалению всех связанных заказов.\nВы уверены, что хотите продолжить?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
                }

                _carRepository.DeleteCar(selectedCar.Код);
                LoadCars();
            }
        }

    }
}