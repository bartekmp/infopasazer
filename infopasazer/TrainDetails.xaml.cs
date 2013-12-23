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
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace infopasazer
{
    /// <summary>
    /// Interaction logic for TrainDetails.xaml
    /// </summary>
    public partial class TrainDetails : Window
    {
        private HtmlDocument doc = new HtmlDocument();
        private HttpClient client = new HttpClient();
        private String pageUrl = String.Empty;
        private List<TrainData> checkpointList = new List<TrainData>();

        private static String[] pastColours = { "#80cc80", "#cccc33", "#cc8d00", "#cc4d4d" };
        private static String[] futureColours = { "#a0ffa0", "#ffff40", "#ffb000", "#ff6060" };
        private const String currentColour = "#0000ff";

        public List<TrainData> CheckpointList
        {
            get
            {
                return checkpointList;
            }
        }
        public class TrainData
        {
            public String Train { get; set; }
            public String Date { get; set; }
            public String Relation { get; set; }
            public String Station { get; set; }
            public String ExpectedArrival { get; set; }
            public String ArrivalDelay { get; set; }
            public String ExpectedDepature { get; set; }
            public String DepatureDelay { get; set; }
            public String currentStation { get; set; }
        }


        public TrainDetails(String url = "", String name = "")
        {
            InitializeComponent();
            pageUrl = url;
            this.Title += " " + name;
            LoadResults();
            Parse();
        }

        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;

            this.Activate();

            this.Focus();
        }

        private void LoadResults()
        {
            /*Load a html code from given URL*/
            var url = "http://infopasazer.intercity.pl/" + pageUrl;
            try
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Unable to connect to database!");
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                doc.LoadHtml(responseBody);
            }
            catch (Exception ex)
            {
                error.Content = ex.Message;
                return;
            }

        }

        private void Parse()
        {
            var document = doc.DocumentNode;
            var tableSource = (from n in document.Descendants()
                         where n.Name == "table" &&
                         n.Attributes.Contains("class") &&
                         n.Attributes["class"].Value == "contacts"
                         select n).FirstOrDefault();

            var extractTable = (from n in tableSource.Descendants()
                                where n.Name == "tr" && !n.Attributes.Contains("class")
                                select n);

            foreach(var Node in extractTable)
            {
                var node = Node.ChildNodes;
                string current = string.Empty;

                if(Node.Attributes.Contains("bgcolor"))
                {
                    if (pastColours.Contains(Node.Attributes["bgcolor"].Value) == true)
                    {
                        current = "-1";
                    }
                    if(futureColours.Contains(Node.Attributes["bgcolor"].Value) == true)
                    {
                        current = "1";
                    }
                }

                for(int i = 0; i < node.Count; ++i)
                {
                    var x = node[i];
                    if(x.Attributes.Contains("class") && x.Attributes["class"].Value == "mid")
                    {
                        current = "0";
                    }
                    
                }
                node[2].InnerHtml = node[2].InnerHtml.Replace("<br>", " - ");
                //Regex for replacing multiple whitespaces with one space
                CheckpointList.Add(new TrainData 
                    {
                        Train = Regex.Replace(node[0].InnerText, @"\s+", " "),
                        Date = Regex.Replace(node[1].InnerText, @"\s+", " "),
                        Relation = Regex.Replace(node[2].InnerText, @"\s+", " "),
                        Station = Regex.Replace(node[3].InnerText, @"\s+", " "),
                        ExpectedArrival = Regex.Replace(node[4].InnerText, @"\s+", " "),
                        ArrivalDelay = Regex.Replace(node[5].InnerText, @"\s+", " "),
                        ExpectedDepature = Regex.Replace(node[6].InnerText, @"\s+", " "),
                        DepatureDelay = Regex.Replace(node[7].InnerText, @"\s+", " "),
                        currentStation = current
                    });
            }

            SetList();
        }

        private void SetList()
        {
            ResizeGridViewColumn();
            TrainStationList.ItemsSource = CheckpointList; // data binding
        }

        private void ResizeGridViewColumn() //resize list grid
        {
            var columns = TrainStationList.View as GridView;
            foreach (var column in columns.Columns)
            {
                if (double.IsNaN(column.Width))
                {
                    column.Width = column.ActualWidth;
                }

                column.Width = double.NaN;
            }
        }
    }
}
