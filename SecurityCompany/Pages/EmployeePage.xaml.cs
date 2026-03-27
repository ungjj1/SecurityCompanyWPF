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
using System.Xml;

namespace SecurityCompany.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmployeePage.xaml
    /// </summary>
    public partial class EmployeePage : Page
    {
        private User currentUser;
        private List<Security> allSecurity;
        private static bool isEditWindowOpen = false;
        public EmployeePage(User currentUser)
        {
            InitializeComponent();
            this.currentUser = currentUser;
            
            if(currentUser.Role != "Управляющий")
            {
                TopPanel.Visibility = Visibility.Collapsed;
            }

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                allSecurity = App.db.Security.ToList();

                RefreshSecurityList();
            }
            catch (Exception ex)
            {
               MessageBox.Show("Ошибка при загрузке данных", $"{ex}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RefreshSecurityList()
        {
            try
            {
                if (allSecurity == null) return;

                // Начинаем с полного списка
                IEnumerable<Security> filteredEmployee = allSecurity;

                // Применяем поиск
                string searchText = TBSearch.Text?.ToLower() ?? "";
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filteredEmployee = filteredEmployee.Where(e =>
                        (e.FirstName != null && e.FirstName.ToLower().Contains(searchText)) ||
                        (e.SecondName != null && e.SecondName.ToLower().Contains(searchText)) ||
                        (e.Post != null && e.Post.ToLower().Contains(searchText)) ||
                        (e.Status != null && e.Status.ToLower().Contains(searchText))
                    );
                }

                EmployyeList.ItemsSource = filteredEmployee.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении списка", $"{ex}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var security = border?.DataContext as Security;

            if (security != null)
            {
                if (currentUser.Role == "Управляющий")
                {
                    OpenEditSecurityWindow(security);
                }
                else
                {
                    MessageBox.Show("У вас нет прав для редактирования товаров", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void OpenEditSecurityWindow(Security security)
        {
            try
            {
                if (isEditWindowOpen || EditSecurityWindow.isEditWindowOpen)
                {
                    MessageBox.Show("Окно редактирования уже открыто. Сначала закройте его.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                isEditWindowOpen = true;

                // Создаем и открываем окно редактирования
                var editWindow = new EditSecurityWindow(security);
                editWindow.Owner = Window.GetWindow(this);
                editWindow.Closed += (s, args) =>
                {
                    isEditWindowOpen = false;
                    try
                    {
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                       MessageBox.Show("Ошибка при обновлении данных", $"{ex}", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
                editWindow.ShowDialog();

                try
                {
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при обновлении данных", $"{ex}", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                isEditWindowOpen = false;
                MessageBox.Show("Ошибка при открытии окна редактирования", $"{ex}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FireEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранного сотрудника из кнопки
                var button = sender as Button;
                var security = button?.DataContext as Security;

                if (security == null)
                {
                    MessageBox.Show("Пожалуйста, выберите сотрудника для увольнения.",
                                  "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, не уволен ли уже сотрудник
                if (security.Status == "Уволен")
                {
                    MessageBox.Show($"Сотрудник {security.SecondName} {security.FirstName} уже уволен.",
                                  "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Подтверждение увольнения
                MessageBoxResult result = MessageBox.Show(
                    $"Вы действительно хотите уволить сотрудника?\n\n" +
                    $"ФИО: {security.SecondName} {security.FirstName}\n" +
                    $"Должность: {security.Post}\n\n" +
                    $"Это действие нельзя отменить!",
                    "Подтверждение увольнения",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Меняем статус сотрудника
                    security.Status = "Уволен";

                    // Сохраняем изменения в базе данных
                    App.db.SaveChanges();

                    // Обновляем список сотрудников
                    RefreshSecurityList();

                    MessageBox.Show($"Сотрудник {security.SecondName} {security.FirstName} успешно уволен.",
                                  "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при увольнении сотрудника: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshSecurityList();
        }

        private void BtnCreateEmployee_Click(object sender, RoutedEventArgs e)
        {
            OpenEditSecurityWindow(null);
        }
    }
}
