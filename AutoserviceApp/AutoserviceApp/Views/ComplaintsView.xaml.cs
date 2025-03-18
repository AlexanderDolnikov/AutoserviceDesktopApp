using AutoserviceApp.DataAccess.Models;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoserviceApp.ViewModels;
using AutoserviceApp.Interfaces;
using AutoserviceApp.Helpers;
using AutoserviceApp.Models;

namespace AutoserviceApp.Views
{
    public partial class ComplaintsView : UserControl, IRefreshable
    {

        private readonly DatabaseContext _context;

        private readonly ComplaintRepository _complaintRepository;

        private OrderWithInfo _selectedOrder;
        private WorkWithInfo _selectedWork;

        private int _selectedComplaintIndex;

        public ComplaintsView()
        {
            InitializeComponent();

            _context = new DatabaseContext();

            _complaintRepository = new ComplaintRepository(_context);
        }

        private void ComplaintsView_OnLoad(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                _selectedWork = viewModel.SelectedWork;
                _selectedOrder = viewModel.SelectedOrder;

                RefreshData();

                ComplaintsHeader.Text = $"Жалобы по работе: \n" +
                                         $"{_selectedWork.Описание} \n" +
                                         $"Клиент: {_selectedOrder.ФамилияКлиента}";
            }
        }

        public void RefreshData()
        {
            if (_selectedWork is not null)
            {
                LoadComplaints(_selectedWork.Код);
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(ComplaintTextBox, ComplaintDatePicker);
        }

        /* - - - - - */

        private void ComplaintsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComplaintsListBox.SelectedItem is Complaint selectedComplaint)
            {
                _selectedComplaintIndex = ComplaintsListBox.SelectedIndex;

                ComplaintTextBox.Text = selectedComplaint.Описание;
                ComplaintDatePicker.SelectedDate = selectedComplaint.Дата;
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

            _complaintRepository.Add(newComplaint);
            LoadComplaints(_selectedWork.Код);

            SetFocusOnFirstInput();
        }

        private void EditComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (ComplaintsListBox.SelectedItem is Complaint selectedComplaint)
            {
                selectedComplaint.Описание = ComplaintTextBox.Text;
                selectedComplaint.Дата = ComplaintDatePicker.SelectedDate ?? DateTime.Now;

                _complaintRepository.Update(selectedComplaint);
                LoadComplaints(_selectedWork.Код);

                ComplaintsListBox.SelectedIndex = _selectedComplaintIndex;
            }
        }

        private void DeleteComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Complaint selectedComplaint)
            {
                _complaintRepository.Delete(selectedComplaint.Код);
                LoadComplaints(_selectedWork.Код);

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
