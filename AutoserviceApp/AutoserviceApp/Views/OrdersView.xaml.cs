using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Interfaces;
using AutoserviceApp.DataAccess.Models;
using AutoserviceApp.Helpers;
using AutoserviceApp.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AutoserviceApp.Views
{
    public enum SortMode
    {
        Ascending,   // Дата ↑
        Descending,  // Дата ↓
        Default      // По умолчанию (без сортировки)
    }

    public partial class OrdersView : UserControl, IRefreshable
    {
        private readonly DatabaseContext _context;

        private readonly OrderRepository _orderRepository;
        private readonly ClientRepository _clientRepository;
        private readonly CarRepository _carRepository;
        private readonly WorkRepository _workRepository;
        private readonly ModelRepository _modelRepository;

        private List<OrderWithInfo> _orders;
        private OrderWithInfo _selectedOrder;
        private int _selectedOrderIndex;

        private SortMode _currentSortMode = SortMode.Default;

        public OrdersView()
        {
            InitializeComponent();
            
            _context = new DatabaseContext();

            _orderRepository = new OrderRepository(_context);
            _clientRepository = new ClientRepository(_context);
            _carRepository = new CarRepository(_context);
            _workRepository = new WorkRepository(_context);
            _modelRepository = new ModelRepository(_context);

            RefreshData();
        }

        public void RefreshData()
        {
            LoadOrders();
            LoadClients();
            LoadCars();
        }
        private void LoadClients()
        {
            var clients = _clientRepository.GetAll()
                .Select(c => new
                {
                    c.Код,
                    DisplayText = $"{c.Фамилия} {c.Имя} ({c.Телефон})"
                })
                .ToList();

            ClientDropdown.ItemsSource = clients;
            ClientDropdown.DisplayMemberPath = "DisplayText";
            ClientDropdown.SelectedValuePath = "Код";
        }

        private void LoadCars()
        {
            var cars = _carRepository.GetAll()
                .Select(c => new
                {
                    c.Код,
                    DisplayText = $"{c.НомернойЗнак} ({_modelRepository.GetById(c.КодМодели)?.Название ?? "Неизвестно"})"
                })
                .ToList();

            CarDropdown.ItemsSource = cars;
            CarDropdown.DisplayMemberPath = "DisplayText";
            CarDropdown.SelectedValuePath = "Код";
        }


        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(StartDatePicker, EndDatePicker, ClientDropdown, CarDropdown);
        }

        /* - - - Заказы - - - */
        private void LoadOrders()
        {
            var orders = _orderRepository.GetAllOrdersWithInfo();
            OrdersListBox.ItemsSource = orders;
            _orders = orders;
        }

        private void OrdersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersListBox.SelectedItem is OrderWithInfo selectedOrder)
            {
                _selectedOrderIndex = OrdersListBox.SelectedIndex;

                StartDatePicker.SelectedDate = selectedOrder.ДатаНачала;
                EndDatePicker.SelectedDate = selectedOrder.ДатаОкончания;
                ClientDropdown.SelectedValue = selectedOrder.КодКлиента;
                CarDropdown.SelectedValue = selectedOrder.КодАвтомобиля;
            }
        }

        private void SortOrders_Click(object sender, RoutedEventArgs e)
        {
            _currentSortMode = _currentSortMode switch
            {
                SortMode.Default => SortMode.Ascending,
                SortMode.Ascending => SortMode.Descending,
                _ => SortMode.Default
            };

            ApplySorting();
        }

        private void ApplySorting()
        {
            if (_orders == null || !_orders.Any())
            {
                OrdersListBox.ItemsSource = new List<Order>(); // или просто return;
                return;
            }

            switch (_currentSortMode)
            {
                case SortMode.Default:
                    SortOrdersButton.Content = "Сортировки нет";
                    OrdersListBox.ItemsSource = _orders;
                    break;
                case SortMode.Ascending:
                    SortOrdersButton.Content = "Дата начала ↑";
                    OrdersListBox.ItemsSource = _orders.OrderBy(o => o.ДатаНачала).ToList();
                    break;
                case SortMode.Descending:
                    SortOrdersButton.Content = "Дата начала ↓";
                    OrdersListBox.ItemsSource = _orders.OrderByDescending(o => o.ДатаНачала).ToList();
                    break;
            }
        }


        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату начала заказа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ClientDropdown.SelectedItem == null || CarDropdown.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента и автомобиль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newOrder = new Order
            {
                ДатаНачала = (DateTime)StartDatePicker.SelectedDate,
                ДатаОкончания = EndDatePicker.SelectedDate.HasValue ? (DateTime)EndDatePicker.SelectedDate.Value : (DateTime?)null,
                КодКлиента = (int)ClientDropdown.SelectedValue,
                КодАвтомобиля = (int)CarDropdown.SelectedValue
            };

            _orderRepository.Add(newOrder);
            LoadOrders();

            SetFocusOnFirstInput();
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersListBox.SelectedItem == null) return;

            if (StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите Дату начала заказа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ClientDropdown.SelectedValue == null || CarDropdown.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента и мастера!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (OrdersListBox.SelectedItem is OrderWithInfo selectedOrderWithInfo)
            {
                Order updatedOrder = new Order
                {
                    Код = selectedOrderWithInfo.Код,
                    ДатаНачала = (DateTime)StartDatePicker.SelectedDate,
                    ДатаОкончания = EndDatePicker.SelectedDate.HasValue ? (DateTime)EndDatePicker.SelectedDate.Value : (DateTime?)null,
                    КодКлиента = (int)ClientDropdown.SelectedValue,
                    КодАвтомобиля = (int)CarDropdown.SelectedValue
                };

                _orderRepository.Update(updatedOrder);
                LoadOrders();

                OrdersListBox.SelectedIndex = _selectedOrderIndex;
            }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is OrderWithInfo selectedOrder)
            {
                // Проверяем, есть ли у заказа работы
                bool hasWorks = _workRepository.GetAll().Any(w => w.КодЗаказа == selectedOrder.Код);

                if (hasWorks)
                {
                    var result = MessageBox.Show(
                        "Этот заказ содержит работы. Все связанные данные будут удалены.\nВы уверены, что хотите удалить его?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
                }

                try
                {
                    _orderRepository.Delete(selectedOrder.Код);
                    MessageBox.Show("Заказ удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders();

                    SetFocusOnFirstInput();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        /* - - - Работы - - - */
        private void ShowWorks_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is OrderWithInfo selectedOrder)
            {
                // Сохраняем текущий заказ
                _selectedOrder = selectedOrder;

                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.SelectedOrder = _selectedOrder;
                    viewModel.SwitchView("Работы");
                }
            }
        }
           
    }
}
