using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Helpers;

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
        private int _selectedCarIndex;

        public CarsView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _carRepository = new CarRepository(_context);
            _modelRepository = new ModelRepository(_context);
            _orderRepository = new OrderRepository(_context);

            RefreshData();
        }

        public void RefreshData()
        {
            LoadModels();
            LoadCars();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(ModelDropdown, CarNumberTextBox, CarYearTextBox);
        }

        private void FilterByYearButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(MinYearTextBox.Text, out int minYear) || !int.TryParse(MaxYearTextBox.Text, out int maxYear))
            {
                MessageBox.Show("Введите корректные числовые значения для года выпуска.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (minYear > maxYear)
            {
                MessageBox.Show("Минимальный год не может быть больше максимального.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var filteredCars = _carRepository.GetAll()
                .Select(car => new CarWithInfo
                {
                    Код = car.Код,
                    КодМодели = car.КодМодели,
                    НазваниеМодели = _modelRepository.GetById(car.КодМодели).Название,
                    НомернойЗнак = car.НомернойЗнак,
                    ГодВыпуска = car.ГодВыпуска
                })
                .Where(c => c.ГодВыпуска >= minYear && c.ГодВыпуска <= maxYear)
                .ToList();

            CarsListBox.ItemsSource = filteredCars;
        }


        /* - - - - - */
        private void LoadModels()
        {
            ModelDropdown.ItemsSource = _modelRepository.GetAll();
        }
        private void CarsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CarsListBox.SelectedItem is CarWithInfo selectedCar)
            {
                _selectedCar = selectedCar;
                _selectedCarIndex = CarsListBox.SelectedIndex;

                ModelDropdown.SelectedValue = selectedCar.КодМодели;
                CarNumberTextBox.Text = selectedCar.НомернойЗнак;
                CarYearTextBox.Text = selectedCar.ГодВыпуска.ToString();
            }
        }

        private void LoadCars()
        {
            _cars = _carRepository.GetAll()
                .Select(car => new CarWithInfo
                {
                    Код = car.Код,
                    КодМодели = car.КодМодели,
                    НазваниеМодели = _modelRepository.GetById(car.КодМодели).Название,
                    НомернойЗнак = car.НомернойЗнак,
                    ГодВыпуска = car.ГодВыпуска
                })
                .ToList();

            CarsListBox.ItemsSource = _cars;
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

            // Валидация номера автомобиля - от 7 до 15 символов
            if (CarNumberTextBox.Text.Length < 7 || CarNumberTextBox.Text.Length > 15)
            {
                MessageBox.Show("Номерной знак должен содержать от 7 до 15 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация года выпуска
            if (!int.TryParse(CarYearTextBox.Text, out int carYear) || carYear < 1900 || carYear > DateTime.Now.Year)
            {
                MessageBox.Show($"Введите корректный год выпуска (от 1900 до {DateTime.Now.Year})!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newCar = new Car
            {
                КодМодели = (int)ModelDropdown.SelectedValue,
                НомернойЗнак = CarNumberTextBox.Text,
                ГодВыпуска = int.Parse(CarYearTextBox.Text)
            };

            try
            {
                _carRepository.Add(newCar);
                LoadCars();

                SetFocusOnFirstInput();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint 'UQ__Автомоби"))
                {
                    MessageBox.Show($"Ошибка: Автомобиль с таким Номерным знаком уже существует!");
                }
                else
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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

            // Валидация номера автомобиля - от 7 до 15 символов
            if (CarNumberTextBox.Text.Length < 7 || CarNumberTextBox.Text.Length > 15)
            {
                MessageBox.Show("Номерной знак должен содержать от 7 до 15 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация года выпуска
            if (!int.TryParse(CarYearTextBox.Text, out int carYear) || carYear < 1900 || carYear > DateTime.Now.Year)
            {
                MessageBox.Show($"Введите корректный год выпуска (от 1900 до {DateTime.Now.Year})!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var updatedCar = new Car
            {
                Код = _selectedCar.Код,
                КодМодели = (int)ModelDropdown.SelectedValue,
                НомернойЗнак = CarNumberTextBox.Text,
                ГодВыпуска = int.Parse(CarYearTextBox.Text)
            };


            try
            {
                _carRepository.Update(updatedCar);
                LoadCars();

                CarsListBox.SelectedIndex = _selectedCarIndex;
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint 'UQ__Автомоби"))
                {
                    MessageBox.Show($"Ошибка: Автомобиль с таким Номерным знаком уже существует!");
                }
                else
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCar_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is CarWithInfo selectedCar)
            {
                // Проверяем, есть ли заказы, связанные с этим автомобилем
                bool hasOrders = _orderRepository.GetAll().Any(order => order.КодАвтомобиля == selectedCar.Код);

                if (hasOrders)
                {
                    var result = MessageBox.Show(
                        "Этот автомобиль используется в заказах. Удаление приведет к удалению всех связанных заказов.\nВы уверены, что хотите продолжить?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
                }

                _carRepository.Delete(selectedCar.Код);
                LoadCars();

                SetFocusOnFirstInput();
            }
        }

    }
}