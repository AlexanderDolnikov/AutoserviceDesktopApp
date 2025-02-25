using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;
using System.Data.SqlClient;

namespace AutoserviceApp.Views
{
    public partial class OrdersView : UserControl
    {
        private readonly OrderRepository _orderRepository;
        private readonly WorkRepository _workRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ComplaintRepository _complaintRepository;
        private readonly CarRepository _carRepository;
        private readonly DatabaseContext _context;
        private List<Order> _orders;
        private Order _selectedOrder;
        private Work _selectedWork;

        public OrdersView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _orderRepository = new OrderRepository(_context);
            _workRepository = new WorkRepository(_context);
            _clientRepository = new ClientRepository(_context);
            _complaintRepository = new ComplaintRepository(_context);
            _carRepository = new CarRepository(_context);

            LoadOrders();
            LoadClients();
            LoadCars();
        }

        private void LoadOrders()
        {
            _orders = _orderRepository.GetAllOrders();
            OrdersList.Items.Clear();

            foreach (var order in _orders)
            {
                var client = _clientRepository.GetClientById(order.КодКлиента);
                string clientName = client != null ? client.Фамилия : "Неизвестный";

                StackPanel orderPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Background = new SolidColorBrush(Colors.GhostWhite),
                    Margin = new Thickness(5),
                };

                TextBlock orderInfo = new TextBlock
                {
                    Text = $"Клиент: {clientName}, Даты: {order.ДатаНачала:dd.MM.yyyy} - {order.ДатаОкончания:dd.MM.yyyy}",
                    Width = 300,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(7),
                };

                Button detailsButton = new Button
                {
                    Content = "Подробнее",
                    Background = new SolidColorBrush(Color.FromRgb(170, 212, 19)),
                    Foreground = new SolidColorBrush(Colors.Black),
                    Padding = new Thickness(5),
                    FontWeight = FontWeights.Bold
                };
                detailsButton.Click += (sender, e) => ShowOrderDetails(order.Код);

                orderPanel.Children.Add(orderInfo);
                orderPanel.Children.Add(detailsButton);
                OrdersList.Items.Add(orderPanel);
            }
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты начала и окончания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ClientDropdown.SelectedItem == null || CarDropdown.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента и автомобиль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newOrder = new Order
            {
                ДатаНачала = StartDatePicker.SelectedDate.Value,
                ДатаОкончания = EndDatePicker.SelectedDate.Value,
                КодКлиента = (int)ClientDropdown.SelectedValue,
                КодАвтомобиля = (int)CarDropdown.SelectedValue
            };

            _orderRepository.AddOrder(newOrder);
            LoadOrders();
        }


        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersList.SelectedItem is Order selectedOrder)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ {selectedOrder.Код}?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _orderRepository.DeleteOrder(selectedOrder.Код);
                    LoadOrders();
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для удаления.");
            }
        }


        private void ShowOrderDetails(int orderId)
        {
            var order = _orderRepository.GetOrderById(orderId);
            if (order == null) return;

            // Заполняем поля заказа
            EditStartDatePicker.SelectedDate = order.ДатаНачала;
            EditEndDatePicker.SelectedDate = order.ДатаОкончания;
            EditClientDropdown.SelectedValue = order.КодКлиента;
            EditCarDropdown.SelectedValue = order.КодАвтомобиля;

            // Загружаем работы по заказу
            WorksListBox.ItemsSource = _workRepository.GetWorksByOrderId(orderId);

            // Сохраняем текущий заказ
            _selectedOrder = order;

            // Прячем кнопку добавления
            AddOrderButton.Visibility = Visibility.Collapsed;

            // Показываем кнопку сохранения изменений
            SaveChangesButton.Visibility = Visibility.Visible;

            // Переключаемся на детали заказа
            OrdersListPanel.Visibility = Visibility.Collapsed;
            OrderDetailsPanel.Visibility = Visibility.Visible;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null)
            {
                MessageBox.Show("Нет заказа для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (EditStartDatePicker.SelectedDate == null || EditEndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты начала и окончания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditClientDropdown.SelectedItem == null || EditCarDropdown.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента и автомобиль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Обновляем данные заказа
            _selectedOrder.ДатаНачала = EditStartDatePicker.SelectedDate.Value;
            _selectedOrder.ДатаОкончания = EditEndDatePicker.SelectedDate.Value;
            _selectedOrder.КодКлиента = (int)EditClientDropdown.SelectedValue;
            _selectedOrder.КодАвтомобиля = (int)EditCarDropdown.SelectedValue;

            // Обновляем заказ в БД
            _orderRepository.UpdateOrder(_selectedOrder);

            // Переключаемся обратно на список заказов
            OrderDetailsPanel.Visibility = Visibility.Collapsed;
            OrdersListPanel.Visibility = Visibility.Visible;

            // Обновляем список заказов
            LoadOrders();
        }

        private void LoadClients()
        {
            var clients = _clientRepository.GetAllClients();
            ClientDropdown.ItemsSource = clients;
            EditClientDropdown.ItemsSource = clients; // Добавляем для редактирования
        }

        private void LoadCars()
        {
            var cars = _carRepository.GetAllCars();
            CarDropdown.ItemsSource = cars;
            EditCarDropdown.ItemsSource = cars; // Добавляем для редактирования
        }

        // Открыть детали работы
        private void ShowWorkDetails_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Work selectedWork)
            {
                _selectedWork = selectedWork;

                // Заполняем поля
                WorkDescriptionTextBox.Text = selectedWork.Описание;
                WorkCostTextBox.Text = selectedWork.Стоимость.ToString();

                // Скрываем детали заказа, показываем детали работы
                OrderDetailsPanel.Visibility = Visibility.Collapsed;
                WorkDetailsPanel.Visibility = Visibility.Visible;
            }
        }

        // Сохранение изменений в работе
        private void SaveWorkChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWork == null) return;

            _selectedWork.Описание = WorkDescriptionTextBox.Text;
            if (decimal.TryParse(WorkCostTextBox.Text, out decimal newCost))
            {
                _selectedWork.Стоимость = newCost;
                _workRepository.UpdateWork(_selectedWork);
            }

            // Возвращаемся к деталям заказа
            WorkDetailsPanel.Visibility = Visibility.Collapsed;
            OrderDetailsPanel.Visibility = Visibility.Visible;
        }

        // Открыть жалобы по работе
        private void ShowComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Work selectedWork)
            {
                _selectedWork = selectedWork;

                // Загружаем жалобы по работе
                ComplaintsListBox.ItemsSource = _complaintRepository.GetComplaintsByWorkId(selectedWork.Код);

                // Скрываем детали заказа, показываем жалобы
                OrderDetailsPanel.Visibility = Visibility.Collapsed;
                ComplaintDetailsPanel.Visibility = Visibility.Visible;
            }
        }

        // Вернуться из деталей работы / жалобы к деталям заказа
        private void BackToOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            WorkDetailsPanel.Visibility = Visibility.Collapsed;
            ComplaintDetailsPanel.Visibility = Visibility.Collapsed;
            OrderDetailsPanel.Visibility = Visibility.Visible;
        }

        // Добавление новой жалобы
        private void AddComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWork == null) return;

            //var addComplaintWindow = new AddComplaintWindow(_selectedWork.Код);
            //if (addComplaintWindow.ShowDialog() == true)
            //{
            //    // Перезагружаем список жалоб
            //    ComplaintsListBox.ItemsSource = _complaintRepository.GetComplaintsByWorkId(_selectedWork.Код);
            //}

            MessageBox.Show("Загрушка 1");
        }



        private void BackToOrders_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся к списку заказов
            OrderDetailsPanel.Visibility = Visibility.Collapsed;
            OrdersListPanel.Visibility = Visibility.Visible;
        }
    }
}
