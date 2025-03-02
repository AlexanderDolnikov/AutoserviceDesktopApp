using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoserviceApp.Views
{
    public partial class WorkTypesView : UserControl
    {
        private readonly DatabaseContext _context;
        private readonly WorkTypeRepository _workTypeRepository;
        private readonly WorkRepository _workRepository;
        private List<WorkType> _workTypes;
        private WorkType _selectedWorkType;

        public WorkTypesView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _workTypeRepository = new WorkTypeRepository(_context);
            _workRepository = new WorkRepository(_context);

            LoadWorkTypes();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta / 3);
                e.Handled = true;
            }
        }

        private void LoadWorkTypes()
        {
            _workTypes = _workTypeRepository.GetAllWorkTypes();
            WorkTypesListBox.ItemsSource = _workTypes;
        }

        private void WorkTypesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkTypesListBox.SelectedItem is WorkType selectedWorkType)
            {
                _selectedWorkType = selectedWorkType;
                WorkTypeNameTextBox.Text = selectedWorkType.Название;
            }
        }

        private void AddWorkType_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WorkTypeNameTextBox.Text))
            {
                MessageBox.Show("Введите название вида работы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newWorkType = new WorkType
            {
                Название = WorkTypeNameTextBox.Text.Trim()
            };

            _workTypeRepository.AddWorkType(newWorkType);
            LoadWorkTypes();
            WorkTypeNameTextBox.Clear();
        }

        private void EditWorkType_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWorkType == null) return;

            _selectedWorkType.Название = WorkTypeNameTextBox.Text.Trim();
            _workTypeRepository.UpdateWorkType(_selectedWorkType);
            LoadWorkTypes();
        }

        private void DeleteWorkType_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkType selectedWorkType)
            {
                bool hasWorks = _workRepository.GetAllWorks().Any(w => w.КодВидаРаботы == selectedWorkType.Код);

                if (hasWorks)
                {
                    var result = MessageBox.Show(
                        "Этот вид работы используется в работах. Удаление приведет к удалению связанных данных.\nВы уверены, что хотите удалить его?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
                }

                _workTypeRepository.DeleteWorkType(selectedWorkType.Код);
                LoadWorkTypes();
            }
        }
    }
}

