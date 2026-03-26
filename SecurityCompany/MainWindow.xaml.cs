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
       private User currentuser;
        public MainWindow(User user)
        {
            InitializeComponent();
            this.currentuser = user;
            TBUserName.Text = $"{user.Surname} {user.FirstName} {user.Patronymic}";
            TBRole.Text = $"{user.Role}";
        }

        private void BtnClient_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void BtnEmployees_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnObject_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
