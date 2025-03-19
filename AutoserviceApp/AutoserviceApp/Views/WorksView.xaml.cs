using AutoserviceApp.DataAccess.Models;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using AutoserviceApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoserviceApp.Models;
using AutoserviceApp.Helpers;
using AutoserviceApp.Interfaces;

namespace AutoserviceApp.Views
{
    public partial class WorksView : UserControl, IRefreshable
    {
        private readonly DatabaseContext _context;

        private readonly WorkRepository _workRepository;
        private readonly WorkDetailRepository _workDetailRepository;
        private readonly MasterRepository _masterRepository;
        private readonly WorkTypeRepository _workTypeRepository;
        private readonly ComplaintRepository _complaintRepository;

        private OrderWithInfo _selectedOrder;
        private WorkWithInfo _selectedWork;

        private int _selectedWorkIndex;

        public WorksView()
        {
            InitializeComponent();
            _context = new DatabaseContext();

            _workRepository = new WorkRepository(_context);
            _workDetailRepository = new WorkDetailRepository(_context);
            _masterRepository = new MasterRepository(_context);
            _workTypeRepository = new WorkTypeRepository(_context);
            _complaintRepository = new ComplaintRepository(_context);
        }

        private void WorksView_OnLoad(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                _selectedOrder = viewModel.SelectedOrder;

                RefreshData();

                WorkHeader.Text = $"Работы по заказу: \n" +
                                  $"Клиент: {_selectedOrder.ФамилияКлиента}, \n" +
                                  $"Авто: {_selectedOrder.НомернойЗнакАвтомобиля}, \n" + 
                                  $"Даты: {_selectedOrder.ДатаНачала:dd.MM.yyyy} - {_selectedOrder.ДатаОкончания:dd.MM.yyyy}";
            }
        }

        public void RefreshData()
        {
            if (_selectedOrder is not null)
            {
                LoadWorks(_selectedOrder.Код);

                LoadMasters();
                LoadWorkTypes();
            }
        }
        private void LoadMasters()
        {
            var masters = _masterRepository.GetAll();
            MasterDropdown.ItemsSource = masters;
        }
        private void LoadWorkTypes()
        {
            var workTypes = _workTypeRepository.GetAll();
            WorkTypeDropdown.ItemsSource = workTypes;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(WorkNameTextBox, WorkCostTextBox, MasterDropdown, WorkTypeDropdown);
        }

        /* - - - - - */
        
        private void LoadWorks(int orderId)
        {
            WorksListBox.ItemsSource = GetWorksByOrderId(orderId);
        }
        private void WorksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorksListBox.SelectedItem is WorkWithInfo selectedWork)
            {
                _selectedWork = selectedWork;
                _selectedWorkIndex = WorksListBox.SelectedIndex;

                WorkNameTextBox.Text = selectedWork.Описание;
                WorkCostTextBox.Text = selectedWork.Стоимость.ToString();

                MasterDropdown.SelectedValue = selectedWork.КодМастера;
                WorkTypeDropdown.SelectedValue = selectedWork.КодВидаРаботы;
            }
        }

        private List<WorkWithInfo> GetWorksByOrderId(int orderId)
        {
            var works = _workRepository.GetAll()
                .Where(works => works.КодЗаказа == _selectedOrder.Код)
                .Select(works => new WorkWithInfo
                {
                    Код = works.Код,
                    КодЗаказа = _selectedOrder.Код,
                    Описание = works.Описание,
                    Стоимость = works.Стоимость,
                    КодМастера = works.КодМастера,
                    ФамилияМастера = _masterRepository.GetById(works.КодМастера).Фамилия,
                    КодВидаРаботы = works.КодВидаРаботы,
                    НазваниеВидаРаботы = _workTypeRepository.GetById(works.КодВидаРаботы).Название
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

            if (!decimal.TryParse(WorkCostTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Ошибка: Введите корректную стоимость (> 0) в поле 'Стоимость'.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            _workRepository.Add(newWork);
            LoadWorks(_selectedOrder.Код);

            SetFocusOnFirstInput();
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
                _workRepository.Update(updatedWork);
                LoadWorks(_selectedOrder.Код);

                WorksListBox.SelectedIndex = _selectedWorkIndex;
            }
        }

        private void DeleteWork_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkWithInfo selectedWork)
            {
                // Проверяем, есть ли у работы жалобы или детали
                bool hasComplaints = _complaintRepository.GetComplaintsByWorkId(selectedWork.Код).Any();
                bool hasWorkDetails = _workDetailRepository.GetAll().Any(d => d.КодРаботы == selectedWork.Код);

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
                    _workRepository.Delete(selectedWork.Код);
                    MessageBox.Show("Работа успешно удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadWorks(_selectedOrder.Код);

                    SetFocusOnFirstInput();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        /* - - - ДетальРаботы - - - */
        private void ShowWorkDetails_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkWithInfo selectedWork)
            {
                _selectedWork = selectedWork;

                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.SelectedWork = _selectedWork;
                    viewModel.SwitchView("ДеталиРаботы");
                }
            }
        }

        /* - - - Жалобы - - -*/
        private void ShowComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkWithInfo selectedWork)
            {
                _selectedWork = selectedWork;

                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.SelectedWork = _selectedWork;
                    viewModel.SwitchView("Жалобы");
                }
            }
        }

        /* - - - - - */

        private void BackToOrders_Click(object sender = null, RoutedEventArgs e = null)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SelectedWork = null;
                viewModel.SwitchView("Заказы");
            }
        }
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.XButton1 == MouseButtonState.Pressed)
            {
                BackToOrders_Click();

            }
        }

    }
}