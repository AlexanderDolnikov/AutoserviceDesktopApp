using AutoserviceApp.DataAccess.Models;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoserviceApp.ViewModels;
using AutoserviceApp.Models;
using AutoserviceApp.Interfaces;
using AutoserviceApp.Helpers;

namespace AutoserviceApp.Views
{
    public partial class WorkDetailsView : UserControl, IRefreshable
    {
        private readonly DatabaseContext _context;

        private readonly WorkDetailRepository _workDetailRepository;
        private readonly DetailRepository _detailRepository;
        private readonly DBProceduresAndFunctionsRepository _DBProceduresAndFunctionsRepository;

        private OrderWithInfo _selectedOrder;
        private WorkWithInfo _selectedWork;

        private int _selectedWorkDetailIndex;

        public WorkDetailsView()
        {
            InitializeComponent();
            _context = new DatabaseContext();

            _workDetailRepository = new WorkDetailRepository(_context);
            _detailRepository = new DetailRepository(_context);
            _DBProceduresAndFunctionsRepository = new DBProceduresAndFunctionsRepository(_context);
        }
        private void WorkDetailsView_OnLoad(object sender, RoutedEventArgs e)
        {
            // Получаем выбранную работу из ViewModel
            if (DataContext is MainViewModel viewModel)
            {
                _selectedWork = viewModel.SelectedWork;
                _selectedOrder = viewModel.SelectedOrder;

                RefreshData();

                WorkDetailsHeader.Text = $"Детали работы: \n" +
                                         $"Описание: {_selectedWork.Описание} \n" +
                                         $"Авто: {_selectedOrder.НомернойЗнакАвтомобиля}, \n" +
                                         $"Клиент: {_selectedOrder.ФамилияКлиента}";
            }
        }


        public void RefreshData()
        {
            if (_selectedWork is not null)
            {
                LoadWorkDetails(_selectedWork.Код);
                LoadDetails();
            }
        }

        private void LoadDetails()
        {
            DetailDropdown.ItemsSource = _detailRepository.GetAll();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(DetailDropdown, WorkQuantityTextBox);
        }

        /* - - - - - */

        private void WorkDetailsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkDetailsListBox.SelectedItem is WorkDetailsWithInfo selectedWorkDetailsWithDetailAmount)
            {
                _selectedWorkDetailIndex = WorkDetailsListBox.SelectedIndex;

                DetailDropdown.Text = selectedWorkDetailsWithDetailAmount.НазваниеДетали.ToString();
                WorkQuantityTextBox.Text = selectedWorkDetailsWithDetailAmount.Количество.ToString();
            }
        }

        private void LoadWorkDetails(int workId)
        {
            var details = _workDetailRepository.GetAll()
                .Where(detail => detail.КодРаботы == workId)
                .Select(detail => new WorkDetailsWithInfo
                {
                    Код = detail.Код,
                    НазваниеДетали = _detailRepository.GetById(detail.КодДетали).Название,
                    Количество = detail.Количество
                })
                .ToList();

            WorkDetailsListBox.ItemsSource = details;
        }

        private void AddWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWork == null || DetailDropdown.SelectedValue == null) return;

            if (!int.TryParse(WorkQuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Ошибка: Введите корректное целое число (> 0) в поле 'Количество'.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newWorkDetail = new WorkDetail
            {
                КодРаботы = _selectedWork.Код,
                КодДетали = (int)DetailDropdown.SelectedValue,
                Количество = int.Parse(WorkQuantityTextBox.Text)
            };

            _workDetailRepository.Add(newWorkDetail);
            LoadWorkDetails(_selectedWork.Код);

            SetFocusOnFirstInput();
        }

        private void EditWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(WorkQuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Ошибка: Введите корректное целое число (> 0) в поле 'Количество'.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (WorkDetailsListBox.SelectedItem is WorkDetailsWithInfo selectedDetail)
            {
                WorkDetail updatedDetail = new WorkDetail
                {
                    Код = selectedDetail.Код,
                    КодРаботы = _selectedWork.Код,
                    КодДетали = (int)DetailDropdown.SelectedValue,
                    Количество = int.Parse(WorkQuantityTextBox.Text)
                };

                _workDetailRepository.Update(updatedDetail);
                LoadWorkDetails(_selectedWork.Код);

                WorkDetailsListBox.SelectedIndex = _selectedWorkDetailIndex;
            }
        }

        private void DeleteWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkDetailsWithInfo selectedWorkDetailWithDetailAmount)
            {
                _workDetailRepository.Delete(selectedWorkDetailWithDetailAmount.Код);
                LoadWorkDetails(_selectedWork.Код);

                SetFocusOnFirstInput();
            }
        }

        /* - - - - - */

        private void MergeAllDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWork == null)
            {
                MessageBox.Show("Ошибка: работа не выбрана. Сначала выберите работу", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Внимание! В результате данной операции дубликаты деталей в данной работе будут объединены в одну запись по каждой детали.\n" +
                "Количество деталей будет просуммировано, повторяющиеся записи будут удалены.\n\n" +
                "Вы уверены, что хотите продолжить?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            int updatedCount = _DBProceduresAndFunctionsRepository.MergeWorkDetailsByWorkId(_selectedWork.Код);
            MessageBox.Show($"Объединено дубликатов: {updatedCount}", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshData();
        }


        /* - - - - - */
        private void BackToWorks_Click(object sender = null, RoutedEventArgs e = null)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SwitchView("Работы");
            }
        }
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.XButton1 == MouseButtonState.Pressed)
            {
                BackToWorks_Click();
            }
        }

    }
}