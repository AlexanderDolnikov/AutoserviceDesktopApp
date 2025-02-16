using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using AutoserviceApp.DataAccess.Repositories;
using AutoserviceApp.DataAccess;
using AutoserviceApp.Models;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

namespace AutoserviceApp
{
    public partial class MainWindow : Window
    {
        private readonly DetailRepository _detailRepository;
        private readonly WorkDetailRepository _workDetailRepository;
        private readonly WorkRepository _workRepository;
        private readonly ComplaintRepository _complaintRepository;
        private readonly UserRepository _userRepository;

        private User _currentUser;

        public MainWindow()
        {
            var loginWindow = new LoginWindow();

            if (loginWindow.ShowDialog() == true)
            {
                InitializeComponent();

                _currentUser = loginWindow.CurrentUser;
                var context = new DatabaseContext();

                _detailRepository = new DetailRepository(context);
                _workDetailRepository = new WorkDetailRepository(context);
                _workRepository = new WorkRepository(context);
                _complaintRepository = new ComplaintRepository(context);
                _userRepository = new UserRepository(context);

                LoadDetails();
                LoadWorkDetails();
                LoadWorks();
                LoadComplaints();
                LoadUsers();

                ApplyRoleRestrictions();
            }
            else
            {
                Application.Current.Shutdown();
            } 
        }


        private readonly Dictionary<string, List<string>> roleTabs = new Dictionary<string, List<string>>
        {
            { "Гость", new List<string> { "Детали" } },
            { "Клиент", new List<string> { "Детали", "Жалобы", "Работы" } },
            { "Сотрудник", new List<string> { "Детали", "ДетальРаботы", "Работы", "Жалобы", "Пользователи" } }
        };

        private void ApplyRoleRestrictions()
        {
            if (!roleTabs.ContainsKey(_currentUser.Role))
                return;

            List<string> allowedTabs = roleTabs[_currentUser.Role];

            foreach (TabItem item in MainTabControl.Items)
            {
                item.Visibility = allowedTabs.Contains(item.Header.ToString()) ? Visibility.Visible : Visibility.Collapsed;
            }

            ShowWelcomeScreen();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void LoadDetails()
        {
            var details = _detailRepository.GetAllDetails();
            DetailsGrid.ItemsSource = details;
        }

        private void LoadWorkDetails()
        {
            var workDetails = _workDetailRepository.GetAllWorkDetails();
            WorkDetailsGrid.ItemsSource = workDetails;
        }

        private void LoadWorks()
        {
            var works = _workRepository.GetAllWorks();
            WorksGrid.ItemsSource = works;
        }

        private void LoadComplaints()
        {
            var complaints = _complaintRepository.GetAllComplaints();
            ComplaintsGrid.ItemsSource = complaints;
        }
        private void LoadUsers()
        {
            var users = _userRepository.GetAllUsers();
            UsersGrid.ItemsSource = users;
        }
        private void ShowWelcomeScreen_Click(object sender, RoutedEventArgs e)
        {
            ShowWelcomeScreen();
        }

        private void ShowWelcomeScreen()
        {
            MainTabControl.Visibility = Visibility.Collapsed; // Скрываем вкладки
            
            string roleDescription = _currentUser.Role switch
            {
                "Гость" => "Вы можете просматривать список мастеров и последние выполненные работы.",
                "Клиент" => "Вы можете просматривать свои заказы и работы, а также оставлять жалобы.",
                "Сотрудник" => "Вы имеете полный доступ ко всем данным, включая управление пользователями.",
                _ => "Неизвестная роль"
            };

            WelcomeText.Text = $"Добро пожаловать в приложение нашего Автосервиса.\n\n" +
                               $"Вы вошли как: {_currentUser.Role}.\n\n" +
                               $"Вам доступен следующий функционал:\n{roleDescription}";

            WelcomeScreen.Visibility = Visibility.Visible;

        }
        private void CloseWelcomeScreen_Click(object sender, RoutedEventArgs e)
        {
            WelcomeScreen.Visibility = Visibility.Collapsed;
            MainTabControl.Visibility = Visibility.Visible;

            // Открываем первую доступную вкладку
            var allowedTabs = roleTabs[_currentUser.Role];
            foreach (TabItem item in MainTabControl.Items)
            {
                if (allowedTabs.Contains(item.Header.ToString()))
                {
                    MainTabControl.SelectedItem = item;
                    break;
                }
            }
        }


        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            string helpText = "Сделано студентом группы ПО-31 - Дольников Александр";

            MessageBox.Show(helpText, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void GenerateReport1_Click(object sender, RoutedEventArgs e)
        {
            var details = _detailRepository.GetAllDetails();

            using (var wordDoc = WordprocessingDocument.Create("DetailTableReport.docx", WordprocessingDocumentType.Document))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                body.AppendChild(new Paragraph(new Run(new Text("Отчет по таблице 'Деталь'"))));

                foreach (var detail in details)
                {
                    body.AppendChild(new Paragraph(new Run(new Text(
                        $"Код: {detail.Код}, Название: {detail.Название}, Стоимость: {detail.Стоимость}, Производитель: {detail.Производитель}"
                    ))));
                }

                mainPart.Document.Save();
            }

            MessageBox.Show("Отчет по таблице 'Деталь' успешно создан!", "Отчет");
        }
        private void GenerateReport2_Click(object sender, RoutedEventArgs e)
        {
            var popularDetails = _detailRepository.GetQuery2Result();

            using (var wordDoc = WordprocessingDocument.Create("PopularDetailsReport.docx", WordprocessingDocumentType.Document))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                body.AppendChild(new Paragraph(new Run(new Text("Отчет по популярным деталям"))));

                foreach (var detail in popularDetails)
                {
                    body.AppendChild(new Paragraph(new Run(new Text(
                        $"Название: {detail.Название}, Количество заказов: {detail.КоличествоЗаказов}"
                    ))));
                }

                mainPart.Document.Save();
            }

            MessageBox.Show("Отчет по популярным деталям успешно создан!", "Отчет");
        }
        private void GenerateReport3_Click(object sender, RoutedEventArgs e)
        {
            var groupedDetails = _detailRepository.GetQuery3Result();

            using (var wordDoc = WordprocessingDocument.Create("GroupedDetailsReport.docx", WordprocessingDocumentType.Document))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                body.AppendChild(new Paragraph(new Run(new Text("Отчет по группировке деталей"))));

                foreach (var detail in groupedDetails)
                {
                    body.AppendChild(new Paragraph(new Run(new Text(
                        $"Название: {detail.DetailName}, Общее количество: {detail.TotalQuantity}, Общая стоимость: {detail.TotalCost}"
                    ))));
                }

                body.AppendChild(new Paragraph(new Run(new Text(
                    $"Итог: Всего деталей — {groupedDetails.Sum(d => d.TotalQuantity)}, Общая стоимость — {groupedDetails.Sum(d => d.TotalCost)}"
                ))));

                mainPart.Document.Save();
            }

            MessageBox.Show("Отчет по группировке деталей успешно создан!", "Отчет");
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            ShowWelcomeScreen();
            // Закрываем текущее окно (главное)
            this.Hide();


            // Создаём и показываем окно входа
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                // Получаем нового пользователя и обновляем главное окно
                _currentUser = loginWindow.CurrentUser;
                ApplyRoleRestrictions();
                LoadDetails();
                LoadWorkDetails();
                LoadWorks();
                LoadComplaints();
            }
            else
            {
                // Если пользователь закрыл окно входа, завершаем приложение
                Application.Current.Shutdown();
            }

            // Показываем главное окно снова
            this.Show();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                WelcomeScreen.Visibility = Visibility.Collapsed;
                MainTabControl.Visibility = Visibility.Visible;

                var selectedTab = (sender as TabControl)?.SelectedIndex;

                switch (selectedTab)
                {
                    case 0: // Вкладка "Детали"
                        LoadDetails();
                        break;
                    case 1: // Вкладка "ДетальРаботы"
                        LoadWorkDetails();
                        break;
                    case 2: // Вкладка "Работа"
                        LoadWorks();
                        break;
                    case 3: // Вкладка "Жалобы"
                        LoadComplaints();
                        break;
                }
            }
        }

        private void NumericOnlyTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
        private bool ValidateTextBox(TextBox textBox, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox.Focus();
                return false;
            }
            return true;
        }

        private bool ValidatePasswordBox(PasswordBox passwordBox, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                passwordBox.Focus();
                return false;
            }
            return true;
        }

        private void DetailFilterPriceInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FilterPriceFromInput.Text))
            {
                FilterPriceFromInput.Text = "0";
            }
        }

        /* - - - - - - Users: - - - - - - */

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role != "Сотрудник")
            {
                MessageBox.Show("Вы не можете добавлять пользователей!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string login = UserLoginInput.Text;
            string password = UserPasswordInput.Password;
            string role = (UserRoleInput.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (!ValidateTextBox(UserLoginInput, "Введите логин!") ||
                !ValidatePasswordBox(UserPasswordInput, "Введите пароль!") ||
                UserRoleInput.SelectedItem == null)
            {
                return;
            }


            try
            {
                _userRepository.AddUser(login, password, role);
                LoadUsers();
                MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /* - - - - - - Details: - - - - - - */
        private void DetailsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DetailsGrid.SelectedItem is Detail selectedDetail)
            {
                // Переносим данные из выбранной строки в поля ввода
                DetailNameInput.Text = selectedDetail.Название;
                DetailPriceInput.Text = selectedDetail.Стоимость.ToString();
                DetailManufacturerInput.Text = selectedDetail.Производитель;
            }
        }

        private void AddDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newDetail = new Detail
                {
                    Название = DetailNameInput.Text,
                    Стоимость = decimal.Parse(DetailPriceInput.Text),
                    Производитель = DetailManufacturerInput.Text
                };

                _detailRepository.AddDetail(newDetail);
                LoadDetails();

                MessageBox.Show("Деталь успешно добавлена!");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Код ошибки SQL для нарушения FOREIGN KEY
                {
                    MessageBox.Show("Ошибка: Нельзя добавить деталь, так как нарушено ограничение FOREIGN KEY. Проверьте связанные данные.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private void EditDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DetailsGrid.SelectedItem is Detail selectedDetail)
                {
                    selectedDetail.Название = DetailNameInput.Text;
                    selectedDetail.Стоимость = decimal.Parse(DetailPriceInput.Text);
                    selectedDetail.Производитель = DetailManufacturerInput.Text;

                    _detailRepository.UpdateDetail(selectedDetail);
                    LoadDetails();

                    MessageBox.Show("Деталь успешно обновлена!");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Код ошибки FOREIGN KEY
                {
                    MessageBox.Show("Ошибка: Нарушено ограничение FOREIGN KEY. Проверьте связанные данные.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private void DeleteDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DetailsGrid.SelectedItem is Detail selectedDetail)
                {
                    _detailRepository.DeleteDetail(selectedDetail.Код);
                    LoadDetails();

                    MessageBox.Show("Деталь успешно удалена!");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Код ошибки FOREIGN KEY
                {
                    MessageBox.Show("Ошибка: Невозможно удалить деталь, так как она используется в других таблицах.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }


        private void DetailFilterPriceFromInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(FilterPriceFromInput.Text, @"\D"))
            {
                FilterPriceFromInput.Text = "0";
                FilterPriceFromInput.CaretIndex = FilterPriceFromInput.Text.Length; // Устанавливаем курсор в конец
            }
        }
        private void FilterPriceFromAndWord_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(FilterPriceFromInput.Text, out var priceFrom))
            {
                var keyword = FilterKeywordInput.Text;

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    MessageBox.Show("Введите ключевое слово для поиска.");
                    return;
                }

                var filteredDetails = _detailRepository.GetDetailsWithPriceAboveAndKeyword(priceFrom, keyword);
                DetailsGrid.ItemsSource = filteredDetails;
            }
            else
            {
                MessageBox.Show("Введите корректное значение минимальной стоимости.");
            }
        }
        private void FilterPriceFrom_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(FilterPriceFromInput.Text, out var priceFrom))
            {
                var filteredDetails = _detailRepository.GetDetailsWithPriceAbove(priceFrom);
                DetailsGrid.ItemsSource = filteredDetails;
            }
            else
            {
                MessageBox.Show("Введите корректное значение минимальной стоимости.");
            }
        }

        private void GroupDetails_Click(object sender, RoutedEventArgs e)
        {
            var groupedDetails = _detailRepository.GetGroupedDetails();
            DetailsGrid.ItemsSource = groupedDetails;
        }


        private void SearchDetailByName_Click(object sender, RoutedEventArgs e)
        {
            var name = DetailSearchInput.Text;
            var results = _detailRepository.SearchDetailsByName(name);
            DetailsGrid.ItemsSource = results;
        }

        private void SearchDetailByPrice_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(DetailSearchInput.Text, out var price))
            {
                var results = _detailRepository.SearchDetailsByPrice(price);
                DetailsGrid.ItemsSource = results;
            }
            else
            {
                MessageBox.Show("Введите корректное значение стоимости.");
            }
        }

        private void SearchDetailByManufacturer_Click(object sender, RoutedEventArgs e)
        {
            var manufacturer = DetailSearchInput.Text;
            var results = _detailRepository.SearchDetailsByManufacturer(manufacturer);
            DetailsGrid.ItemsSource = results;
        }
        private void SortDetailsAscending_Click(object sender, RoutedEventArgs e)
        {
            var sortedDetails = _detailRepository.SortDetailsByPriceAscending();
            DetailsGrid.ItemsSource = sortedDetails;
        }
        private void SortDetailsDescending_Click(object sender, RoutedEventArgs e)
        {
            var sortedDetails = _detailRepository.SortDetailsByPriceDescending();
            DetailsGrid.ItemsSource = sortedDetails;
        }

        private void ShowPopularDetails_Click(object sender, RoutedEventArgs e)
        {
            var popularDetails = _detailRepository.GetPopularDetails();
            DetailsGrid.ItemsSource = popularDetails;
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("MainTabControl") is TabControl tabControl)
            {
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    string selectedTabHeader = selectedTab.Header.ToString();

                    if (selectedTabHeader == "Детали")
                    {
                        FilterPriceFromInput.Text = "0";
                        DetailSearchInput.Clear();
                        LoadDetails();
                    }
                    else if (selectedTabHeader == "Работа")
                    {
                        WorkComplaintInput.Clear();
                        LoadWorks();
                    }
                }
            }
        }




        /* - - - - - - WorkDetail: - - - - - - */
        private void WorkDetailsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkDetailsGrid.SelectedItem is WorkDetail selectedWorkDetail)
            {
                WorkDetailWorkIdInput.Text = selectedWorkDetail.КодРаботы.ToString();
                WorkDetailDetailIdInput.Text = selectedWorkDetail.КодДетали.ToString();
                WorkDetailQuantityInput.Text = selectedWorkDetail.Количество.ToString();
            }
        }

        private void AddWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newWorkDetail = new WorkDetail
                {
                    КодРаботы = int.Parse(WorkDetailWorkIdInput.Text),
                    КодДетали = int.Parse(WorkDetailDetailIdInput.Text),
                    Количество = int.Parse(WorkDetailQuantityInput.Text)
                };

                _workDetailRepository.AddWorkDetail(newWorkDetail);
                LoadWorkDetails();

                MessageBox.Show("Деталь работы успешно добавлена!");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Код ошибки FOREIGN KEY
                {
                    MessageBox.Show("Ошибка: Нарушено ограничение FOREIGN KEY. Проверьте связанные данные.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private void EditWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WorkDetailsGrid.SelectedItem is WorkDetail selectedWorkDetail)
                {
                    selectedWorkDetail.КодРаботы = int.Parse(WorkDetailWorkIdInput.Text);
                    selectedWorkDetail.КодДетали = int.Parse(WorkDetailDetailIdInput.Text);
                    selectedWorkDetail.Количество = int.Parse(WorkDetailQuantityInput.Text);

                    _workDetailRepository.UpdateWorkDetail(selectedWorkDetail);
                    LoadWorkDetails();

                    MessageBox.Show("Деталь работы успешно обновлена!");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Код ошибки FOREIGN KEY
                {
                    MessageBox.Show("Ошибка: Нарушено ограничение FOREIGN KEY. Проверьте связанные данные.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private void DeleteWorkDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WorkDetailsGrid.SelectedItem is WorkDetail selectedWorkDetail)
                {
                    _workDetailRepository.DeleteWorkDetail(selectedWorkDetail.Код);
                    LoadWorkDetails();

                    MessageBox.Show("Деталь работы успешно удалена!");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    MessageBox.Show("Ошибка: Невозможно удалить запись, так как она связана с другими таблицами.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }


        /* - - - - - - Work: - - - - - - */
        private void WorksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorksGrid.SelectedItem is Work selectedWork)
            {
                WorkOrderIdInput.Text = selectedWork.КодЗаказа.ToString();
                WorkMasterIdInput.Text = selectedWork.КодМастера.ToString();
                WorkDescriptionInput.Text = selectedWork.Описание;
                WorkCostInput.Text = selectedWork.Стоимость.ToString();
                WorkTypeIdInput.Text = selectedWork.КодВидаРаботы.ToString();
            }
        }

        private void AddWork_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newWork = new Work
                {
                    КодЗаказа = int.Parse(WorkOrderIdInput.Text),
                    КодМастера = int.Parse(WorkMasterIdInput.Text),
                    Описание = WorkDescriptionInput.Text,
                    Стоимость = decimal.Parse(WorkCostInput.Text),
                    КодВидаРаботы = int.Parse(WorkTypeIdInput.Text)
                };

                _workRepository.AddWork(newWork);
                LoadWorks();

                MessageBox.Show("Работа успешно добавлена!");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Код ошибки FOREIGN KEY
                {
                    MessageBox.Show("Ошибка: Нарушено ограничение FOREIGN KEY. Проверьте связанные данные.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private void EditWork_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WorksGrid.SelectedItem is Work selectedWork)
                {
                    selectedWork.КодЗаказа = int.Parse(WorkOrderIdInput.Text);
                    selectedWork.КодМастера = int.Parse(WorkMasterIdInput.Text);
                    selectedWork.Описание = WorkDescriptionInput.Text;
                    selectedWork.Стоимость = decimal.Parse(WorkCostInput.Text);
                    selectedWork.КодВидаРаботы = int.Parse(WorkTypeIdInput.Text);

                    _workRepository.UpdateWork(selectedWork);
                    LoadWorks();

                    MessageBox.Show("Работа успешно обновлена!");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Код ошибки FOREIGN KEY
                {
                    MessageBox.Show("Ошибка: Нарушено ограничение FOREIGN KEY. Проверьте связанные данные.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private void DeleteWork_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WorksGrid.SelectedItem is Work selectedWork)
                {
                    _workRepository.DeleteWork(selectedWork.Код);
                    LoadWorks();

                    MessageBox.Show("Работа успешно удалена!");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    MessageBox.Show("Ошибка: Невозможно удалить работу, так как она используется в других таблицах.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private void ShowComplaintsByWork_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WorkComplaintInput.Text, out var workId))
            {
                var complaints = _complaintRepository.GetComplaintsByWorkId(workId);

                if (complaints.Count == 0)
                {
                    //MessageBox.Show("Жалобы не найдены для данной работы.");
                }

                // Очистка таблицы перед обновлением
                WorksGrid.ItemsSource = null;
                WorksGrid.Items.Clear();

                // Присвоение новых данных
                WorksGrid.ItemsSource = complaints;
            }
            else
            {
                MessageBox.Show("Введите корректный ID работы.");
            }
        }


        private void ShowWorkDetailsCost_Click(object sender, RoutedEventArgs e)
        {
            var worksWithCosts = _workDetailRepository.GetWorkDetailsCosts();
            WorksGrid.ItemsSource = worksWithCosts;
        }


        /* - - - - - - Complaints: - - - - - - */
        private void ComplaintsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComplaintsGrid.SelectedItem is Complaint selectedComplaint)
            {
                ComplaintWorkIdInput.Text = selectedComplaint.КодРаботы.ToString();
                ComplaintDescriptionInput.Text = selectedComplaint.Описание;
                ComplaintDateInput.Text = selectedComplaint.Дата.ToString("yyyy-MM-dd");
            }
        }

        private void AddComplaint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newComplaint = new Complaint
                {
                    КодРаботы = int.Parse(ComplaintWorkIdInput.Text),
                    Описание = ComplaintDescriptionInput.Text,
                    Дата = DateTime.Parse(ComplaintDateInput.Text)
                };

                _complaintRepository.AddComplaint(newComplaint);
                LoadComplaints();

                MessageBox.Show("Жалоба успешно добавлена!");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Код ошибки FOREIGN KEY
                {
                    MessageBox.Show("Ошибка: Нарушено ограничение FOREIGN KEY. Проверьте связанные данные.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
            catch (System.FormatException ex)
            {
                MessageBox.Show($"Введена неверная дата! \nВведите дату в формате yyyy-MM-dd, например:\n2024-01-31");
            }
        }

        private void EditComplaint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComplaintsGrid.SelectedItem is Complaint selectedComplaint)
                {
                    selectedComplaint.КодРаботы = int.Parse(ComplaintWorkIdInput.Text);
                    selectedComplaint.Описание = ComplaintDescriptionInput.Text;
                    selectedComplaint.Дата = DateTime.Parse(ComplaintDateInput.Text);

                    _complaintRepository.UpdateComplaint(selectedComplaint);
                    LoadComplaints();

                    MessageBox.Show("Жалоба успешно обновлена!");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Код ошибки FOREIGN KEY
                {
                    MessageBox.Show("Ошибка: Нарушено ограничение FOREIGN KEY. Проверьте связанные данные.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private void DeleteComplaint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComplaintsGrid.SelectedItem is Complaint selectedComplaint)
                {
                    _complaintRepository.DeleteComplaint(selectedComplaint.Код);
                    LoadComplaints();

                    MessageBox.Show("Жалоба успешно удалена!");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    MessageBox.Show("Ошибка: Невозможно удалить жалобу, так как она связана с другими записями.");
                }
                else
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }


    }
}
