using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;
using System.Data.SqlClient;
using AutoserviceApp.Interfaces;
using AutoserviceApp.DataAccess.Models;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace AutoserviceApp.Views
{
    public partial class OrdersView : UserControl, IRefreshable
    {
        private readonly OrderRepository _orderRepository;
        private readonly WorkRepository _workRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ComplaintRepository _complaintRepository;
        private readonly CarRepository _carRepository;
        private readonly DetailRepository _detailRepository;
        private readonly WorkDetailRepository _workDetailRepository;
        private readonly MasterRepository _masterRepository;
        private readonly WorkTypeRepository _workTypeRepository;
        private readonly DatabaseContext _context;
        private List<Order> _orders;
        private Order _selectedOrder;
        private WorkWithInfo _selectedWork;

        public OrdersView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _orderRepository = new OrderRepository(_context);
            _workRepository = new WorkRepository(_context);
            _clientRepository = new ClientRepository(_context);
            _complaintRepository = new ComplaintRepository(_context);
            _detailRepository = new DetailRepository(_context);
            _carRepository = new CarRepository(_context);
            _workDetailRepository = new WorkDetailRepository(_context);
            _masterRepository = new MasterRepository(_context);
            _workTypeRepository = new WorkTypeRepository(_context);

            LoadOrders();
            LoadClients();
            LoadCars();
            LoadDetails();
            LoadMasters();
            LoadWorkTypes();
        }
        public void RefreshData()
        {
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

        private void LoadMasters()
        {
            var masters = _masterRepository.GetAllMasters();
            MasterDropdown.ItemsSource = masters;
        }

        private void LoadWorkTypes()
        {
            var workTypes = _workTypeRepository.GetAllWorkTypes();
            WorkTypeDropdown.ItemsSource = workTypes;
        }


        /* - - - Заказы - - - */

        private void LoadOrders()
        {
            _orders = _orderRepository.GetAllOrders();
            OrdersList.Items.Clear();

            foreach (var order in _orders)
            {
                var client = _clientRepository.GetClientById(order.КодКлиента);
                var car = _carRepository.GetCarById(order.КодАвтомобиля);

                string clientName = client != null ? client.Фамилия : "Неизвестный";
                string carNumber = car != null ? car.НомернойЗнак : "Неизвестно";

                StackPanel orderPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Background = new SolidColorBrush(Colors.GhostWhite),
                    Margin = new Thickness(5),
                };

                TextBlock orderInfo = new TextBlock
                {
                    Text = $"Клиент: {clientName}, Авто: {carNumber}, Даты: {order.ДатаНачала:dd.MM.yyyy} - {order.ДатаОкончания:dd.MM.yyyy}",
                    Width = 400,
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

        private void ShowOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Order selectedOrder)
            {
                ShowOrderDetails(selectedOrder.Код);
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

            // Сохраняем текущий заказ
            _selectedOrder = order;
            
            // Загружаем работы по заказу
            WorksListBox.ItemsSource = GetWorksByOrderId(orderId);

            // Прячем кнопку добавления
            AddOrderButton.Visibility = Visibility.Collapsed;

            // Показываем кнопку сохранения изменений
            SaveChangesButton.Visibility = Visibility.Visible;

            // Переключаемся на детали заказа
            OrdersListPanel.Visibility = Visibility.Collapsed;
            OrderDetailsPanel.Visibility = Visibility.Visible;
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
            if (((Button)sender).DataContext is Order selectedOrder)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ {selectedOrder.Код}?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _orderRepository.DeleteOrder(selectedOrder.Код);
                        LoadOrders();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SaveOrder_Click(object sender, RoutedEventArgs e)
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


        /* - - - Работы - - - */
        private void WorksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorksListBox.SelectedItem is WorkWithInfo selectedWork)
            {
                WorkNameTextBox.Text = selectedWork.Описание;
                WorkCostTextBox.Text = selectedWork.Стоимость.ToString();

                MasterDropdown.SelectedValue = selectedWork.КодМастера;
                WorkTypeDropdown.SelectedValue = selectedWork.КодВидаРаботы;
            }
        }

        private void LoadWorks(int orderId)
        {
            WorksListBox.ItemsSource = GetWorksByOrderId(orderId);
        }
        private List<WorkWithInfo> GetWorksByOrderId(int orderId)
        {
            var works = _workRepository.GetAllWorks()
                .Where(works => works.КодЗаказа == _selectedOrder.Код)
                .Select(works => new WorkWithInfo
                {
                    Код = works.Код,
                    КодЗаказа = _selectedOrder.Код,
                    Описание = works.Описание,
                    Стоимость = works.Стоимость,
                    КодМастера = works.КодМастера,
                    ФамилияМастера = _masterRepository.GetMasterNameById(works.КодМастера),
                    КодВидаРаботы = works.КодВидаРаботы,
                    НазваниеВидаРаботы = _workTypeRepository.GetWorkTypeNameById(works.КодВидаРаботы)
                })
                .ToList();

            return works;
        }

        private void AddWork_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null) return;
            if (MasterDropdown.SelectedValue == null || WorkTypeDropdown.SelectedValue == null)
            {
                MessageBox.Show("Выберите мастера и вид работы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newWork = new Work
            {
                КодЗаказа = _selectedOrder.Код,
                КодМастера = (int)MasterDropdown.SelectedValue,
                КодВидаРаботы = (int)WorkTypeDropdown.SelectedValue,
                Описание = WorkNameTextBox.Text.Trim(),
                Стоимость = decimal.Parse(WorkCostTextBox.Text)
            };

            _workRepository.AddWork(newWork);
            LoadWorks(_selectedOrder.Код);
        }

        private void EditWork_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null || WorksListBox.SelectedItem == null) return;

            if (MasterDropdown.SelectedValue == null || WorkTypeDropdown.SelectedValue == null)
            {
                MessageBox.Show("Выберите мастера и вид работы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(WorkCostTextBox.Text, out decimal cost))
            {
                MessageBox.Show("Некорректное значение стоимости!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (WorksListBox.SelectedItem is WorkWithInfo selectedWorkWithInfo)
            {
                Work updatedWork = new Work
                {
                    Код = selectedWorkWithInfo.Код,
                    КодЗаказа = selectedWorkWithInfo.КодЗаказа,
                    Описание = WorkNameTextBox.Text.Trim(),
                    Стоимость = cost,
                    КодМастера = (int)MasterDropdown.SelectedValue,
                    КодВидаРаботы = (int)WorkTypeDropdown.SelectedValue
                };

                // Обновляем и загружаем работы
                _workRepository.UpdateWork(updatedWork);
                LoadWorks(_selectedOrder.Код);
            }
        }

        private void DeleteWork_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkWithInfo selectedWork)
            {
                // Подтверждение удаления
                var result = MessageBox.Show($"Вы уверены, что хотите удалить работу '{selectedWork.Описание}'?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _workRepository.DeleteWork(selectedWork.Код);
                        LoadWorks(_selectedOrder.Код);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        /* - - - ДетальРаботы - - - */
        private void WorkDetailsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkDetailsListBox.SelectedItem is WorkDetailsWithDetailAmount selectedWorkDetailsWithDetailAmount)
            {
                DetailDropdown.Text = selectedWorkDetailsWithDetailAmount.НазваниеДетали.ToString();
                WorkQuantityTextBox.Text = selectedWorkDetailsWithDetailAmount.Количество.ToString();
            }
        }

        private void ShowWorkDetails_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkWithInfo selectedWork)
            {
                _selectedWork = selectedWork;

                // Загружаем детали работы
                LoadWorkDetails(selectedWork.Код);

                // Показываем вкладку "Детали работы"
                OrderDetailsPanel.Visibility = Visibility.Collapsed;
                WorkDetailsPanel.Visibility = Visibility.Visible;
            }
        }

        private void LoadWorkDetails(int workId)
        {
            var details = _workDetailRepository.GetAllWorkDetails()
                .Where(detail => detail.КодРаботы == workId)
                .Select(detail => new WorkDetailsWithDetailAmount
                {
                    Код = detail.Код,
                    НазваниеДетали = _detailRepository.GetDetailNameById(detail.КодДетали),
                    Количество = detail.Количество
                })
                .ToList();

            WorkDetailsListBox.ItemsSource = details;
        }

        private void AddWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWork == null || DetailDropdown.SelectedValue == null) return;

            var newWorkDetail = new WorkDetail
            {
                КодРаботы = _selectedWork.Код,
                КодДетали = (int)DetailDropdown.SelectedValue,
                Количество = int.Parse(WorkQuantityTextBox.Text)
            };

            _workDetailRepository.AddWorkDetail(newWorkDetail);
            LoadWorkDetails(_selectedWork.Код);
        }

        private void EditWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            if (WorkDetailsListBox.SelectedItem is WorkDetailsWithDetailAmount selectedDetail)
            {
                WorkDetail updatedDetail = new WorkDetail
                {
                    Код = selectedDetail.Код,
                    КодРаботы = _selectedWork.Код,
                    КодДетали = (int)DetailDropdown.SelectedValue,
                    Количество = int.Parse(WorkQuantityTextBox.Text)
                };

                _workDetailRepository.UpdateWorkDetail(updatedDetail);
                LoadWorkDetails(_selectedWork.Код);
            }
        }

        private void DeleteWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkDetailsWithDetailAmount selectedWorkDetailWithDetailAmount)
            {
                _workDetailRepository.DeleteWorkDetail(selectedWorkDetailWithDetailAmount.Код);
                LoadWorkDetails(_selectedWork.Код);
            }
            else
            {
                MessageBox.Show($"работа: {_selectedWork.Код}, выбрано: ???");
            }
        }



        /* - - - Жалобы - - -*/
        private void ComplaintsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComplaintsListBox.SelectedItem is Complaint selectedComplaint)
            {
                ComplaintTextBox.Text = selectedComplaint.Описание;
                ComplaintDatePicker.SelectedDate = selectedComplaint.Дата;
            }
        }

        private void ShowComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkWithInfo selectedWork)
            {
                _selectedWork = selectedWork;

                // Загружаем жалобы по работе
                LoadComplaints(selectedWork.Код);

                // Показываем вкладку "Жалобы по работе"
                OrderDetailsPanel.Visibility = Visibility.Collapsed;
                ComplaintDetailsPanel.Visibility = Visibility.Visible;
            }
        }

        private void LoadComplaints(int workId)
        {
            ComplaintsListBox.ItemsSource = _complaintRepository.GetComplaintsByWorkId(workId);
        }

        private void AddComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWork == null) return;

            var newComplaint = new Complaint
            {
                КодРаботы = _selectedWork.Код,
                Описание = ComplaintTextBox.Text,
                Дата = ComplaintDatePicker.SelectedDate ?? DateTime.Now
            };

            _complaintRepository.AddComplaint(newComplaint);
            LoadComplaints(_selectedWork.Код);
        }

        private void EditComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (ComplaintsListBox.SelectedItem is Complaint selectedComplaint)
            {
                selectedComplaint.Описание = ComplaintTextBox.Text;
                selectedComplaint.Дата = ComplaintDatePicker.SelectedDate ?? DateTime.Now;

                _complaintRepository.UpdateComplaint(selectedComplaint);
                LoadComplaints(_selectedWork.Код);
            }
        }

        private void DeleteComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Complaint selectedComplaint)
            {
                _complaintRepository.DeleteComplaint(selectedComplaint.Код);
                LoadComplaints(_selectedWork.Код);
            }
        }


        /* - - - Детали - - -*/
        private void LoadDetails()
        {
            DetailDropdown.ItemsSource = _detailRepository.GetAllDetails();
        }


        /* - - - - - - - */
        private void BackToOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся к списку ДеталейРаботы
            WorkDetailsPanel.Visibility = Visibility.Collapsed;
            ComplaintDetailsPanel.Visibility = Visibility.Collapsed;
            OrderDetailsPanel.Visibility = Visibility.Visible;
        }
        private void BackToOrders_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаемся к списку заказов
            OrderDetailsPanel.Visibility = Visibility.Collapsed;
            OrdersListPanel.Visibility = Visibility.Visible;
        }
    }
}
