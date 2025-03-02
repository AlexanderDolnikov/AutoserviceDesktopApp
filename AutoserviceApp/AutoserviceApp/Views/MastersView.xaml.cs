using System.Collections.Generic;
using System.Windows.Controls;
using AutoserviceApp.Interfaces;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoserviceApp.Views
{
    public partial class MastersView : UserControl
    {
        private readonly DatabaseContext _context;
        private readonly MasterRepository _masterRepository;
        private readonly WorkRepository _workRepository;
        private List<Master> _masters;
        private Master _selectedMaster;

        public MastersView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _masterRepository = new MasterRepository(_context);
            _workRepository = new WorkRepository(_context);

            LoadMasters();
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

        private void LoadMasters()
        {
            _masters = _masterRepository.GetAllMasters();
            MastersListBox.ItemsSource = _masters;
        }

        private void MastersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MastersListBox.SelectedItem is Master selectedMaster)
            {
                _selectedMaster = selectedMaster;
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

            _masterRepository.AddMaster(newMaster);
            LoadMasters();
            MasterNameTextBox.Clear();
            MasterSurnameTextBox.Clear();
            MasterPhoneTextBox.Clear();
            MasterSpecializationTextBox.Clear();
        }

        private void EditMaster_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMaster == null) return;

            _selectedMaster.Имя = MasterNameTextBox.Text.Trim();
            _selectedMaster.Фамилия = MasterSurnameTextBox.Text.Trim();
            _selectedMaster.Телефон = MasterPhoneTextBox.Text.Trim();
            _selectedMaster.Специализация = MasterSpecializationTextBox.Text.Trim();

            _masterRepository.UpdateMaster(_selectedMaster);
            LoadMasters();
        }

        private void DeleteMaster_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Master selectedMaster)
            {
                // Проверка на существование связанных записей
                bool hasWorks = _workRepository.GetAllWorks().Any(w => w.КодМастера == selectedMaster.Код);

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
                    _masterRepository.DeleteMaster(selectedMaster.Код);
                    MessageBox.Show("Мастер успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadMasters();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

