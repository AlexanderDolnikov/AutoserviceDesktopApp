using System.Windows.Controls;
using AutoserviceApp.Interfaces;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;
using AutoserviceApp.DataAccess.Repositories;
using System.Windows;
using AutoserviceApp.Helpers;

namespace AutoserviceApp.Views
{
    public partial class MastersView : UserControl, IRefreshable
    {
        private readonly DatabaseContext _context;
        private readonly MasterRepository _masterRepository;
        private readonly WorkRepository _workRepository;
        private List<Master> _masters;
        private Master _selectedMaster;
        private int _selectedMasterIndex;

        public MastersView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _masterRepository = new MasterRepository(_context);
            _workRepository = new WorkRepository(_context);

            RefreshData();
        }

        public void RefreshData() => LoadMasters();

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(MasterNameTextBox, MasterSurnameTextBox, MasterPhoneTextBox, MasterSpecializationTextBox);
        }

        /* - - - - - */

        private void LoadMasters()
        {
            _masters = _masterRepository.GetAll();
            MastersListBox.ItemsSource = _masters;
        }

        private void MastersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MastersListBox.SelectedItem is Master selectedMaster)
            {
                _selectedMaster = selectedMaster;
                _selectedMasterIndex = MastersListBox.SelectedIndex;

                MasterNameTextBox.Text = selectedMaster.Имя;
                MasterSurnameTextBox.Text = selectedMaster.Фамилия;
                MasterPhoneTextBox.Text = selectedMaster.Телефон;
                MasterSpecializationTextBox.Text = selectedMaster.Специализация;
            }
        }

        private void AddMaster_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MasterNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(MasterSurnameTextBox.Text))
            {
                MessageBox.Show("Введите имя и фамилию мастера!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newMaster = new Master
            {
                Имя = MasterNameTextBox.Text.Trim(),
                Фамилия = MasterSurnameTextBox.Text.Trim(),
                Телефон = MasterPhoneTextBox.Text.Trim(),
                Специализация = MasterSpecializationTextBox.Text.Trim()
            };

            _masterRepository.Add(newMaster);
            LoadMasters();

            SetFocusOnFirstInput();
        }

        private void EditMaster_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMaster == null) return;

            _selectedMaster.Имя = MasterNameTextBox.Text.Trim();
            _selectedMaster.Фамилия = MasterSurnameTextBox.Text.Trim();
            _selectedMaster.Телефон = MasterPhoneTextBox.Text.Trim();
            _selectedMaster.Специализация = MasterSpecializationTextBox.Text.Trim();

            _masterRepository.Update(_selectedMaster);
            LoadMasters();

            MastersListBox.SelectedIndex = _selectedMasterIndex;
        }

        private void DeleteMaster_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Master selectedMaster)
            {
                // Проверка на существование связанных записей
                bool hasWorks = _workRepository.GetAll().Any(w => w.КодМастера == selectedMaster.Код);

                if (hasWorks)
                {
                    var result = MessageBox.Show(
                        "Этот мастер связан с работами. Все связанные данные будут удалены.\nВы уверены, что хотите удалить его?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
                }

                try
                {
                    _masterRepository.Delete(selectedMaster.Код);
                    MessageBox.Show("Мастер успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadMasters();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                SetFocusOnFirstInput();
            }
        }
    }
}

