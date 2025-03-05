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

        private OrderWithInfo _selectedOrder;
        private WorkWithInfo _selectedWork;

        private int _selectedWorkDetailIndex;

        public WorkDetailsView()
        {
            InitializeComponent();
            _context = new DatabaseContext();

            _workDetailRepository = new WorkDetailRepository(_context);
            _detailRepository = new DetailRepository(_context);
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
            DetailDropdown.ItemsSource = _detailRepository.GetAllDetails();
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
            var details = _workDetailRepository.GetAllWorkDetails()
                .Where(detail => detail.КодРаботы == workId)
                .Select(detail => new WorkDetailsWithInfo
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

            SetFocusOnFirstInput();
        }

        private void EditWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            if (WorkDetailsListBox.SelectedItem is WorkDetailsWithInfo selectedDetail)
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

                WorkDetailsListBox.SelectedIndex = _selectedWorkDetailIndex;
            }
        }

        private void DeleteWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkDetailsWithInfo selectedWorkDetailWithDetailAmount)
            {
                _workDetailRepository.DeleteWorkDetail(selectedWorkDetailWithDetailAmount.Код);
                LoadWorkDetails(_selectedWork.Код);

                SetFocusOnFirstInput();
            }
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
