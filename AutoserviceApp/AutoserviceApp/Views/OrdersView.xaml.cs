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
using DocumentFormat.OpenXml.Bibliography;
using System.Reflection.PortableExecutable;
using System.Windows.Input;

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
        private OrderWithInfo _selectedOrder;
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
        }

        private void LoadCars()
        {
            var cars = _carRepository.GetAllCars();
            CarDropdown.ItemsSource = cars;
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

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
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
                    ФамилияКлиента = _clientRepository.GetClientById(order.КодКлиента)?.Фамилия ?? "Неизвестно",
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
                    MessageBox.Show("Заказ успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadOrders();
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

                // Загружаем работы по заказу
                LoadWorks(selectedOrder.Код);

                WorkHeader.Text = $"Работы по заказу: \nКлиент: {selectedOrder.ФамилияКлиента}, \nАвто: {selectedOrder.НомернойЗнакАвтомобиля}, \nДаты: {selectedOrder.ДатаНачала:dd.MM.yyyy} - {selectedOrder.ДатаОкончания:dd.MM.yyyy}";

                OrdersPanel.Visibility = Visibility.Collapsed;
                WorksPanel.Visibility = Visibility.Visible;
            }
        }

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
                // Проверяем, есть ли у работы жалобы или детали
                bool hasComplaints = _complaintRepository.GetComplaintsByWorkId(selectedWork.Код).Any();
                bool hasWorkDetails = _workDetailRepository.GetAllWorkDetails().Any(d => d.КодРаботы == selectedWork.Код);

                if (hasComplaints || hasWorkDetails)
                {
                    var result = MessageBox.Show(
                        "Эта работа содержит жалобы или детали. Все связанные данные будут удалены.\nВы уверены, что хотите удалить её?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
                }

                try
                {
                    _workRepository.DeleteWork(selectedWork.Код);
                    MessageBox.Show("Работа успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadWorks(_selectedOrder.Код);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

                WorkDetailsHeader.Text = $"Детали работы: \nОписание: {selectedWork.Описание} \nАвто: {_selectedOrder.НомернойЗнакАвтомобиля}, \nКлиент: {_selectedOrder.ФамилияКлиента}";

                // Показываем вкладку "Детали работы"
                WorksPanel.Visibility = Visibility.Collapsed;
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

                ComplaintsHeader.Text = $"Жалобы по работе: \n{selectedWork.Описание} \nКлиент: {_selectedOrder.ФамилияКлиента}";

                // Показываем вкладку "Жалобы по работе"
                WorksPanel.Visibility = Visibility.Collapsed;
                ComplaintsPanel.Visibility = Visibility.Visible;
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
        private void BackToWorks_Click(object sender, RoutedEventArgs e)
        {
            OrdersPanel.Visibility = Visibility.Collapsed;
            WorkDetailsPanel.Visibility = Visibility.Collapsed;
            ComplaintsPanel.Visibility = Visibility.Collapsed;
            WorksPanel.Visibility = Visibility.Visible;
        }
        private void BackToOrders_Click(object sender, RoutedEventArgs e)
        {
            WorksPanel.Visibility = Visibility.Collapsed;
            WorkDetailsPanel.Visibility = Visibility.Collapsed;
            ComplaintsPanel.Visibility = Visibility.Collapsed;
            OrdersPanel.Visibility = Visibility.Visible;
        }
    }
}
