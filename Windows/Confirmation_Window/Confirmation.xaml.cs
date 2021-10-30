using System.Windows;

namespace Frontier.Windows.Confirmation_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Potwierdzenie.xaml
    /// </summary>
    public partial class Confirmation : Window
    {
        private string Type { get; set; }
        public Confirmation(string type)
        {
            InitializeComponent();
            Type = type;
            SelectText();
        }

        private void SelectText()
        {
            switch (Type)
            {
                case "Warehouse":
                case "Contactors":
                    Info_Text.Text = "Czy na pewno chcesz usunąć zaznaczone rekordy?";
                    break;
                case "Logout":
                    Info_Text.Text = "Czy na pewno chcesz się wylogować?";
                    break;
                case "Save":
                    Info_Text.Text = "Czy na pewno chcesz zapisać?";
                    break;
                case "Delete":
                    Info_Text.Text = "Czy na pewno chcesz usunąc?";
                    break;
                default:
                    Info_Text.Text = "Error";
                    break;
            }
        }
        private void Yes_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void No_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
