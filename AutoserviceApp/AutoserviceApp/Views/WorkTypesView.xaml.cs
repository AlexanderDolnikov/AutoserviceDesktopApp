using System.Windows;
using System.Windows.Controls;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.Interfaces;
using AutoserviceApp.Helpers;

namespace AutoserviceApp.Views
{
    public partial class WorkTypesView : UserControl, IRefreshable
    {
        private readonly DatabaseContext _context;
        private readonly WorkTypeRepository _workTypeRepository;
        private readonly WorkRepository _workRepository;
        private List<WorkType> _workTypes;
        private WorkType _selectedWorkType;
        private int _selectedWorkTypeIndex;

        public WorkTypesView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _workTypeRepository = new WorkTypeRepository(_context);
            _workRepository = new WorkRepository(_context);

            RefreshData();
        }

        public void RefreshData() => LoadWorkTypes();

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(WorkTypeNameTextBox);
        }

        /* - - - - - */

        private void LoadWorkTypes()
        {
            _workTypes = _workTypeRepository.GetAll();
            WorkTypesListBox.ItemsSource = _workTypes;
        }

        private void WorkTypesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkTypesListBox.SelectedItem is WorkType selectedWorkType)
            {
                _selectedWorkType = selectedWorkType;
                _selectedWorkTypeIndex = WorkTypesListBox.SelectedIndex;

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

            try
            {
                _workTypeRepository.Add(newWorkType);
                LoadWorkTypes();

                SetFocusOnFirstInput();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint 'UQ__ВидРабот"))
                {
                    MessageBox.Show($"Ошибка: Вид работы с таким названием уже существует!");
                }
                else
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }            
        }

        private void EditWorkType_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWorkType == null) return;

            _selectedWorkType.Название = WorkTypeNameTextBox.Text.Trim();
            _workTypeRepository.Update(_selectedWorkType);
            LoadWorkTypes();

            WorkTypesListBox.SelectedIndex = _selectedWorkTypeIndex;
        }

        private void DeleteWorkType_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is WorkType selectedWorkType)
            {
                bool hasWorks = _workRepository.GetAll().Any(w => w.КодВидаРаботы == selectedWorkType.Код);

                if (hasWorks)
                {
                    var result = MessageBox.Show(
                        "Этот вид работы используется в работах. Удаление приведет к удалению связанных данных.\nВы уверены, что хотите удалить его?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
                }

                _workTypeRepository.Delete(selectedWorkType.Код);
                LoadWorkTypes();

                SetFocusOnFirstInput();
            }
        }
    }
}

