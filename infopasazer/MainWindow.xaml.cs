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
using System.Net.Http;
using System.Xml.Linq;
using System.Web.RegularExpressions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;


namespace infopasazer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private List<TrainDetail> stationListArr = new List<TrainDetail>();
        private List<TrainDetail> stationListDep = new List<TrainDetail>();
        private HtmlDocument doc = new HtmlDocument();
        private HttpClient client = new HttpClient();

        public class TrainDetail
        {
            public string TrainNumber { get; set; }
            public string Provider { get; set; }
            public string Date { get; set; }
            public string Relation { get; set; }
            public string EstimatedArrival { get; set; }
            public string Delay { get; set; }
            public string TrainUrl { get; set; }
            public string DelayType { get; set; }
        }

        public List<TrainDetail> StationList
        {
            get
            {
                if(arrivalsRadio.IsChecked==true)
                    return stationListArr;
                if (depaturesRadio.IsChecked == true)
                    return stationListDep;

                return new List<TrainDetail>();
            }
        }        

        private void performSearch(object sender, RoutedEventArgs e)
        {
            error.Content = String.Empty;
            var btn = sender as Button;
            var queryKeyWord = textBox.Text;
            if(queryKeyWord == String.Empty || String.IsNullOrWhiteSpace(queryKeyWord) || queryKeyWord.First() == ' ')
            {
                error.Content = "Musisz wpisać poprawną nazwę!";
                return;
            }

            var queryURL = "http://infopasazer.intercity.pl/index_set.php?stacja=";
            var queryURLStation = queryURL + queryKeyWord;
            
            if(!checkForInternetConnection())
            {
                error.Content = "Brak połączenia z internetem!";
                return;
            }
            try
            {
                HttpResponseMessage response = client.GetAsync(queryURLStation).Result;
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Błąd połączenia z bazą!");
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                doc.LoadHtml(responseBody);
            }
            catch(Exception ex)
            {
                error.Content = ex.Message;
                return;
            }
            /*New popup window to select desired station*/
            var popup = new Popup(doc);
            popup.ShowDialog();

            /*Start parsing retrieved data*/
            if(Data.DName == "Error")
            {
                error.Content = "Nie znaleziono takiej stacji!";
            }
            else
            {
                LoadResults();
                Parse();
            }
        }

        private void LoadResults()
        {
            /*Load a html code from given URL*/
            var url = "http://infopasazer.intercity.pl/"+Data.DUrl;
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
            /*Clearing visible ListView and datalists*/
            stationListArr.Clear();
            stationListDep.Clear();
            ArrivalsList.ItemsSource = null;
            ArrivalsList.Items.Clear();

            ///////////////////////////////////
            /*Actual parsing of html document*/
            ///////////////////////////////////

            /*Temporary lists to store raw data*/
            var listOfArrivalData = new List<List<string>>();
            var listOfDepatureData = new List<List<string>>();

            var documentBody = doc.DocumentNode.LastChild.LastChild.LastChild;

            var trainTable = (from x in documentBody.Descendants()
                                     where x.Attributes.Contains("class") && 
                                     x.Attributes["class"].Value == "contacts"
                                     select x);

            /*Two tables are the results of that expression - table of arrivals and table of depatures*/
            var arrivalsTableFull = trainTable.First();
            var depaturesTableFull = trainTable.Last();

            /*Extracting acutal html <table>s*/
            var arrivalsTable = (from x in arrivalsTableFull.ChildNodes
                                 where !x.Attributes.Contains("class")
                                 select x);

            var depaturesTable = (from x in depaturesTableFull.ChildNodes
                                  where !x.Attributes.Contains("class")
                                  select x);

            //ARRIVALS
            //extracting rows from table
            foreach(var n in arrivalsTable)
            {
                if(n.Name == "tr")
                {
                    var train = (from x in n.ChildNodes
                                 select x.InnerHtml);
                    listOfArrivalData.Add(train.ToList());
                }
            }

            foreach(var n in listOfArrivalData)
            {
                string tempUrl = string.Empty;
                for(int i = 0; i < n.Count; ++i)
                {
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(n[i]);

                    if(i==0)
                    {
                        tempUrl = document.DocumentNode.FirstChild.Attributes["href"].Value;
                    }
                                           
                    document.DocumentNode.InnerHtml=document.DocumentNode.InnerHtml.Replace("<br>", " - ");
                    n[i] = document.DocumentNode.InnerText; // removing html tags
                    
                }

                /*Finding out type of delay*/
                var delayString = n[5].Substring(0, n[5].IndexOf(' '));
                var dValue = Convert.ToInt32(delayString);
                string dType = "0";

                if (dValue >= 5 && dValue < 20)
                    dType = "1";
                if (dValue >= 20 && dValue < 60)
                    dType = "2";
                if (dValue >= 60)
                    dType = "3";

                //Regex for removing redundant whitespaces
                stationListArr.Add(new TrainDetail
                    {
                        TrainNumber = Regex.Replace(n[0].Replace("-", " "), @"\s+", " "),
                        Provider = Regex.Replace(n[1], @"\s+", " "),
                        Date = Regex.Replace(n[2], @"\s+", " "),
                        Relation = Regex.Replace(n[3], @"\s+", " "),
                        EstimatedArrival = Regex.Replace(n[4], @"\s+", " "),
                        Delay = Regex.Replace(n[5], @"\s+", " "),
                        TrainUrl = tempUrl, 
                        DelayType = dType
                    }); // new list node
            }

            //DEPATURES
            //extracting rows from table
            foreach (var n in depaturesTable)
            {
                if (n.Name == "tr")
                {
                    var train = (from x in n.ChildNodes
                                 select x.InnerHtml);
                    listOfDepatureData.Add(train.ToList());
                }
            }

            foreach (var n in listOfDepatureData)
            {
                string tempUrl = string.Empty;
                for (int i = 0; i < n.Count; ++i)
                {
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(n[i]);

                    if (i == 0)
                    {
                        tempUrl = document.DocumentNode.FirstChild.Attributes["href"].Value;
                    }

                    document.DocumentNode.InnerHtml = document.DocumentNode.InnerHtml.Replace("<br>", " - ");
                    n[i] = document.DocumentNode.InnerText; // removing html tags
                }

                /*Finding out type of delay*/
                var delayString = n[5].Substring(0, n[5].IndexOf(' '));
                var dValue = Convert.ToInt32(delayString);
                string dType = "0";

                /*Setting delay type, which has impact on colour of listview row*/
                if (dValue >= 5 && dValue < 20)
                    dType = "1";
                if (dValue >= 20 && dValue < 60)
                    dType = "2";
                if (dValue >= 60)
                    dType = "3";

                //Regex for removing redundant whitespaces
                stationListDep.Add(new TrainDetail
                {
                            TrainNumber = Regex.Replace(n[0].Replace("-", " "), @"\s+", " "),
                            Provider = Regex.Replace(n[1], @"\s+", " "),
                            Date = Regex.Replace(n[2], @"\s+", " "),
                            Relation = Regex.Replace(n[3], @"\s+", " "),
                            EstimatedArrival = Regex.Replace(n[4], @"\s+", " "),
                            Delay = Regex.Replace(n[5], @"\s+", " "),
                            TrainUrl = tempUrl,
                            DelayType = dType
                        }); // new list node
            }

            /*Set data binding, update column width*/
            SetList();
        }

        private void SetList()
        {
            ResizeGridViewColumn();
            ArrivalsList.ItemsSource = StationList; // data binding
        }

        private void ResizeGridViewColumn() //resize list grid
        {
            var columns = ArrivalsList.View as GridView;
            foreach (var column in columns.Columns)
            {
                if (double.IsNaN(column.Width))
                {
                    column.Width = column.ActualWidth;
                }

                column.Width = double.NaN;
            }
        }

        private void updateList(object sender, RoutedEventArgs e) //set new width of list and refresh binding
        {
            ArrivalsList.ItemsSource = null;
            ArrivalsList.Items.Clear();
            ResizeGridViewColumn();
            ArrivalsList.ItemsSource = StationList;
        }

        private void listItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*Identifying clicked element*/
            var track = ((ListViewItem)sender).Content as TrainDetail; // casting element of ListView to desired type

            /*Open new window with details about train*/
            var trainDetails = new TrainDetails(track.TrainUrl, track.TrainNumber);           

            trainDetails.Show();

        }

        private void clearQueryBox(object sender, MouseEventArgs e) //clear content of query box
        {
            textBox.Text = "";
            textBox.Clear();
        }

        private Boolean checkForInternetConnection()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nic in nics)
            {
                if ((nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel) &&
                    nic.OperationalStatus == OperationalStatus.Up && !nic.Name.Contains("VirtualBox"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
