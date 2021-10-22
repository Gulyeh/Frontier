using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Frontier.Windows.Analyze_Window
{
    /// <summary>
    /// Logika interakcji dla klasy Analiza.xaml
    /// </summary>
    public partial class Analyze : Page
    {
        public ObservableCollection<KeyValuePair<string, int>> ValueList = new ObservableCollection<KeyValuePair<string, int>>();
        public ObservableCollection<KeyValuePair<string, int>> ValueList1 = new ObservableCollection<KeyValuePair<string, int>>();

        public Analyze()
        {
            InitializeComponent();
            FillChart();
        }
        private void FillChart()
        {
            ValueList.Add(new KeyValuePair<string, int>("Styczeń", 10));
            ValueList.Add(new KeyValuePair<string, int>("Luty", 20));
            ValueList.Add(new KeyValuePair<string, int>("Marzec", 30));
            ValueList.Add(new KeyValuePair<string, int>("Kwiecień", 20));
            ValueList.Add(new KeyValuePair<string, int>("Maj", 10));
            ValueList.Add(new KeyValuePair<string, int>("Czerwiec", 10));
            ValueList.Add(new KeyValuePair<string, int>("Lipiec", 20));
            ValueList.Add(new KeyValuePair<string, int>("Sierpień", 5));
            ValueList.Add(new KeyValuePair<string, int>("Wrzesień", 3));
            ValueList.Add(new KeyValuePair<string, int>("Październik", 8));
            ValueList.Add(new KeyValuePair<string, int>("Listopad", 40));
            ValueList.Add(new KeyValuePair<string, int>("Grudzień", 70));

            ValueList1.Add(new KeyValuePair<string, int>("Styczeń", 4222322));
            ValueList1.Add(new KeyValuePair<string, int>("Luty", 34433));
            ValueList1.Add(new KeyValuePair<string, int>("Marzec", 22323232));
            ValueList1.Add(new KeyValuePair<string, int>("Kwiecień", 834343));
            ValueList1.Add(new KeyValuePair<string, int>("Maj", 1033));
            ValueList1.Add(new KeyValuePair<string, int>("Czerwiec", 1534343));
            ValueList1.Add(new KeyValuePair<string, int>("Lipiec", 204545));
            ValueList1.Add(new KeyValuePair<string, int>("Sierpień", 54454));
            ValueList1.Add(new KeyValuePair<string, int>("Wrzesień", 3445));
            ValueList1.Add(new KeyValuePair<string, int>("Październik", 84545));
            ValueList1.Add(new KeyValuePair<string, int>("Listopad", 204545));
            ValueList1.Add(new KeyValuePair<string, int>("Grudzień", 222340));

            Chart1.ItemsSource = ValueList;
            Chart2.ItemsSource = ValueList1;
            Chart3.ItemsSource = ValueList;
            Chart4.ItemsSource = ValueList1;
        }
    }
}
