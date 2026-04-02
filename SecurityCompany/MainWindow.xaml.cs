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

namespace SecurityCompany
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User currentUser;
        public MainWindow(User user)
        {
            InitializeComponent();
            this.currentUser = user;

            MainFrame.Navigate(new Pages.ObjectPage(currentUser));

            TBUserName.Text = $"{currentUser.Surname} {currentUser.FirstName} {currentUser.Patronymic}";
            TBRole.Text = $"{currentUser.Role}";

            if (currentUser != null && currentUser.Role == "Управляющий")
            {
                BtnCLient.Visibility = Visibility.Visible;
                BtnEmployees.Visibility = Visibility.Visible;
                BtnObject.Visibility = Visibility.Visible;
            }
            else if(currentUser != null && currentUser.Role == "Диспетчер")
            {
                BtnEmployees.Visibility = Visibility.Visible;
                BtnObject.Visibility = Visibility.Visible;
            }
            else
            {
                BtnObject.Visibility = Visibility.Visible;
            }
        }

        private void BtnClient_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ClientPage(currentUser));
            this.Title = "ГАРД - Клиенты";
        }

        private void BtnEmployees_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.EmployeePage(currentUser));
            this.Title = "ГАРД - Штат";
        }

        private void BtnObject_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ObjectPage(currentUser));
            this.Title = "ГАРД - Объекты";
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
