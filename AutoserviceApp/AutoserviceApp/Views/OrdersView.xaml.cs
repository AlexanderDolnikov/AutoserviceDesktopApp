using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Interfaces;
using AutoserviceApp.DataAccess.Models;
using AutoserviceApp.Helpers;
using AutoserviceApp.ViewModels;

namespace AutoserviceApp.Views
{
    public partial class OrdersView : UserControl, IRefreshable
    {
        private readonly DatabaseContext _context;

        private readonly OrderRepository _orderRepository;
        private readonly ClientRepository _clientRepository;
        private readonly CarRepository _carRepository;
        private readonly WorkRepository _workRepository;

        private List<Order> _orders;
        private OrderWithInfo _selectedOrder;
        private int _selectedOrderIndex;    

        public OrdersView()
        {
            InitializeComponent();
            
            _context = new DatabaseContext();

            _orderRepository = new OrderRepository(_context);
            _clientRepository = new ClientRepository(_context);
            _carRepository = new CarRepository(_context);
            _workRepository = new WorkRepository(_context);

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
            var clients = _clientRepository.GetAll();
            ClientDropdown.ItemsSource = clients;
        }

        private void LoadCars()
        {
            var cars = _carRepository.GetAllCars();
            CarDropdown.ItemsSource = cars;
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
            var orders = _orderRepository.GetAllOrders()
                .Select(order => new OrderWithInfo
                {
                    Код = order.Код,
                    ДатаНачала = order.ДатаНачала,
                    ДатаОкончания = order.ДатаОкончания ?? default(DateTime),
                    КодКлиента = order.КодКлиента,
                    ФамилияКлиента = _clientRepository.GetById(order.КодКлиента)?.Фамилия ?? "Неизвестно",
                    КодАвтомобиля = order.КодАвтомобиля,
                    НомернойЗнакАвтомобиля = _carRepository.GetCarById(order.КодАвтомобиля)?.НомернойЗнак ?? "Неизвестно",
                })
                .ToList();

            OrdersListBox.ItemsSource = orders;
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

            _orderRepository.AddOrder(newOrder);
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

                _orderRepository.UpdateOrder(updatedOrder);
                LoadOrders();

                OrdersListBox.SelectedIndex = _selectedOrderIndex;
            }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is OrderWithInfo selectedOrder)
            {
                // Проверяем, есть ли у заказа работы
                bool hasWorks = _workRepository.GetAllWorks().Any(w => w.КодЗаказа == selectedOrder.Код);

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
                    _orderRepository.DeleteOrder(selectedOrder.Код);
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
