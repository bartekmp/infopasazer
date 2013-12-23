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
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections.ObjectModel;

namespace infopasazer
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        public HtmlDocument document = new HtmlDocument();
        public ObservableCollection<CityLink> cityUrlList = new ObservableCollection<CityLink>();

        public ObservableCollection<CityLink> CitiesList
        {
            get
            {
                return cityUrlList;
            }
        }

        public Popup(HtmlDocument doc)
        {
            DataContext = this;
            InitializeComponent();
            document = doc;
            Data.DName = String.Empty;
            Data.DUrl = String.Empty;
            Parse();
            
        }

        public class CityLink
        {
            public string url;
            public string name;
            public string Url { get; set; }
            public string Name {get; set;}
            public CityLink(string u = "", string n = "")
            {
                Url = u;
                Name = n;
            }
        }

        public void Parse()
        {
            var node = document.DocumentNode.LastChild;

            var selectTableRow = (from x in node.Descendants()
                                    where x.Name == "td"
                                    select x.Descendants());

            var extractValues = (from x in (from y in selectTableRow select y.FirstOrDefault())
                                     where x.Name == "a"
                                     select x);

            var extractPairs = (from i in (from x in extractValues select x)
                            select new {i.Attributes["href"].Value, i.InnerText });

            foreach(var i in extractPairs)
            {
                cityUrlList.Add(new CityLink(i.Value, i.InnerText));
            }
            
            stationChoiceList.ItemsSource = cityUrlList;

            counter.Content = cityUrlList.Count.ToString();            
        }

        private void Choose(object sender, RoutedEventArgs e)
        {

            if (cityUrlList.Count > 0)
            {
                var sel = stationChoiceList.SelectedValue;
                if (sel == null)
                    sel = stationChoiceList.Items[0];
                Data.DName = ((CityLink)sel).Name;
                Data.DUrl = ((CityLink)sel).Url;
            }
            else
            {
                Data.DName = "Error";
            }
            this.Close();
        }

        private void abortedChoice(object sender, EventArgs e)
        {
            if(Data.DName == String.Empty)
                Data.DName = "Error";
        }

        private void Choosing(object sender, MouseButtonEventArgs e)
        {
            Choose(sender, (RoutedEventArgs)e);
        }
    }
}
