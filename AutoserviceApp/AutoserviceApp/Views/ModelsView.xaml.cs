using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using AutoserviceApp.DataAccess.Repositories;
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
using AutoserviceApp.Interfaces;
using AutoserviceApp.Helpers;


namespace AutoserviceApp.Views
{
    public partial class ModelsView : UserControl, IRefreshable
    {
        private readonly DatabaseContext _context;
        private readonly ModelRepository _modelRepository;
        private readonly CarRepository _carRepository;
        private List<Model> _models;
        private Model _selectedModel;
        private int _selectedModelIndex;

        public ModelsView()
        {
            InitializeComponent();
            _context = new DatabaseContext();
            _modelRepository = new ModelRepository(_context);
            _carRepository = new CarRepository(_context);

            LoadModels();
        }

        public void RefreshData() => LoadModels();

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollHelper.HandleMouseWheel(sender, e);
        }

        private void SetFocusOnFirstInput(object sender = null, RoutedEventArgs? e = null)
        {
            ViewFocusHelper.SetFocusAndClearItemsValues(ModelNameTextBox);
        }

        /* - - - - - */
        private void LoadModels()
        {
            _models = _modelRepository.GetAllModels();
            ModelsListBox.ItemsSource = _models;
        }

        private void ModelsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelsListBox.SelectedItem is Model selectedModel)
            {
                _selectedModel = selectedModel;
                _selectedModelIndex = ModelsListBox.SelectedIndex;

                ModelNameTextBox.Text = selectedModel.Название;
            }
        }

        private void AddModel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ModelNameTextBox.Text))
            {
                MessageBox.Show("Введите название модели!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newModel = new Model
            {
                Название = ModelNameTextBox.Text.Trim()
            };

            try
            {
                _modelRepository.AddModel(newModel);
                LoadModels();

                SetFocusOnFirstInput();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint 'UQ__Модель"))
                {
                    MessageBox.Show($"Ошибка: Такая модель уже существует!");
                }
                else
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditModel_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedModel == null) return;

            if (string.IsNullOrWhiteSpace(ModelNameTextBox.Text))
            {
                MessageBox.Show("Введите название модели!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _selectedModel.Название = ModelNameTextBox.Text.Trim();
            
            try
            {
                _modelRepository.UpdateModel(_selectedModel);
                LoadModels();

                ModelsListBox.SelectedIndex = _selectedModelIndex;
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint 'UQ__Модель"))
                {
                    MessageBox.Show($"Ошибка: Такая модель уже существует!");
                }
                else
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteModel_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Model selectedModel)
            {
                // Проверяем, есть ли автомобили, связанные с этой моделью
                bool hasCars = _carRepository.GetAllCars().Any(car => car.КодМодели == selectedModel.Код);

                if (hasCars)
                {
                    var result = MessageBox.Show(
                        "Вы уверены, что хотите удалить эту модель? Связанные автомобили также будут удалены.",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
                }

                _modelRepository.DeleteModel(selectedModel.Код);
                LoadModels();

                SetFocusOnFirstInput();
            }
        }

    }
}