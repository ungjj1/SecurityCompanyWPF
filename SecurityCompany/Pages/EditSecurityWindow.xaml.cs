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
using System.Windows.Shapes;

namespace SecurityCompany.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditSecurityWindow.xaml
    /// </summary>
    public partial class EditSecurityWindow : Window
    {
        public Security currentSecurity;
        private bool isNew;
        public static bool isEditWindowOpen {  get; private set; }
        public EditSecurityWindow(Security security)
        {
            InitializeComponent();
            this.currentSecurity = security;
            isEditWindowOpen = true;
            isNew = currentSecurity == null;

            this.Title = isNew ? "Добавление охранника" : "Редактирование охранника";
            TitleWindow.Content = this.Title;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BrowsePhotoButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
