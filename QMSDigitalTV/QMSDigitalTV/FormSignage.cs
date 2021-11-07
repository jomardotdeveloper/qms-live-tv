using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Media;
using WebSocketSharp;
using QMSDigitalTV.Digital;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QMSDigitalTV
{
    public partial class FormSignage : Form
    {
        private WebSocket WbClient;
        const string host = "ws://127.0.0.1:8090";
        delegate void LoadDelegate();

        /* TO HANDLE THE SSL CERTIFICATE ISSUE WITH THE API, WILL REMOVE UPON DEPLOYING THE REST API */
        private HttpClientHandler Handler;
        private HttpClient Client;
        private List<Transaction> Tokens;


        private string Window1;
        private string Window2;
        private string Window3;

        private List<Transaction> Window1Tokens;
        private List<Transaction> Window2Tokens;
        private List<Transaction> Window3Tokens;


        private List<int> WindowIDS;
        private List<int> WindowIDSsorted;


        List<Transaction> Priority;
        List<Transaction> Regular;
        List<Transaction> PriorityServing;
        List<Transaction> RegularServing;

        Timer t = null;
        private void StartTimer()
        {
            t = new System.Windows.Forms.Timer();
            t.Interval = 1000;
            t.Tick += new EventHandler(t_Tick);
            t.Enabled = true;
            
        }

        void t_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString();
        }

        public FormSignage()
        {
            InitializeComponent();
            lblTime.Text = DateTime.Now.ToString();
            //StartTimer();
            //GetAllTokenForTheDay();
            //LoadWindows();
            //Load();
            LoadTokensV2();
            Load();
            WbClient = new WebSocket(host);
            WbClient.Connect();


            if (WbClient.IsAlive)
            {
                WbClient.OnMessage += (ss, ee) => WebsocketResponse(ee.Data.ToString());
            }
            MessageBox.Show(WbClient.IsAlive.ToString());
        }

        private void WebsocketResponse(string message)
        {
            JObject json = JObject.Parse(message);
            JObject jsonMessage = JObject.Parse(json["message"].ToString());
            
            if(jsonMessage["message"].ToString() == "nextCustomer")
            {
                LoadTokensV2();
                if (this.win_1.InvokeRequired)
                {
                    LoadDelegate d = new LoadDelegate(Load);
                    this.Invoke(d, new object[] { });
                }
                else
                {
                    Load();
                }

                PlayMusic();
            }else if (jsonMessage["message"].ToString() == "ring")
            {
                PlayMusic();
            }
            //MessageBox.Show(json["message"]["message"].ToString());
            //if(json["message"].ToString() == "nextCustomer" || json["message"].ToString() == "newCustomer")
            //{
            //    GetAllTokenForTheDay();
            //    LoadWindows();

            //    if (this.win_1.InvokeRequired)
            //    {
            //        LoadDelegate d = new LoadDelegate(Load);
            //        this.Invoke(d, new object[] { });
            //    }
            //    else
            //    {
            //        Load();
            //    }

            //    PlayMusic();
            //}
        }


        private int imageNum = 1;
        private void LoadNextImage()
        {
            if (imageNum == 25)
            {
                imageNum = 1;
            }
            string RunningPath = AppDomain.CurrentDomain.BaseDirectory;
            string path = "{0}Resources\\" + imageNum.ToString() + ".jpg";
            
            PBSlider.ImageLocation = string.Format(@path, Path.GetFullPath(Path.Combine(RunningPath, @"..\..\")));
            imageNum++;
            
        }

        public int GetBranchID()
        {
            var projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string FilePath = Path.Combine(projectPath, "Resources");

            String line;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(FilePath + @"\config.txt");
                //Read the first line of text
                line = sr.ReadLine();

                string[] branch_id = line.Split('=');

                return Convert.ToInt32(branch_id[1]);
                //close the file
                sr.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception: " + e.Message);
            }
            return 0;
        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            LoadNextImage();
        }

        private void FormSignage_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void LoadTokensV2()
        {
            this.Handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            this.Client = new HttpClient(this.Handler);
            this.Client.BaseAddress = new Uri(Endpoints.BaseUrl);

            string param = GetBranchID().ToString();
            var response = this.Client.GetAsync(Endpoints.TokenUrl + param).Result;
            JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            if (String.IsNullOrWhiteSpace(json["1"].ToString()))
            {
                Window1 = null;
            }
            else
            {
                JObject data = JObject.Parse(json["1"].ToString());
                Window1 = data["token"].ToString();
            }

            if (String.IsNullOrWhiteSpace(json["2"].ToString()))
            {
                Window2 = null;
            }
            else
            {
                JObject data = JObject.Parse(json["2"].ToString());
                Window2 = data["token"].ToString();
            }


            if (String.IsNullOrWhiteSpace(json["3"].ToString()))
            {
                Window3 = null;
            }
            else
            {
                JObject data = JObject.Parse(json["3"].ToString());
                Window3 = data["token"].ToString();
            }


        }

        private void GetAllTokenForTheDay()
        {
            this.Handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            this.Client = new HttpClient(this.Handler);
            this.Client.BaseAddress = new Uri(Endpoints.BaseUrl);

            string param = "?id=" + GetBranchID().ToString();
            var response = this.Client.GetAsync(Endpoints.TokenUrl + param).Result;
            JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            JArray all = JArray.FromObject(json["transactions"]);

            this.Tokens = new List<Transaction>();
            Priority = new List<Transaction>();
            Regular = new List<Transaction>();
            PriorityServing = new List<Transaction>();
            RegularServing = new List<Transaction>();


            foreach (JObject t in all)
            {
                if (!String.IsNullOrEmpty(t["transaction_serve_datetime"].ToString()))
                {
                    if (t["customer_type_name"].ToString() == "Priority")
                    {
                        PriorityServing.Add(new Transaction(t["transaction_token"].ToString(), true, Convert.ToInt32(t["windows_id"])));
                    }
                    else
                    {
                        RegularServing.Add(new Transaction(t["transaction_token"].ToString(), false, Convert.ToInt32(t["windows_id"])));
                    }
                }
                else
                {
                    if (t["customer_type_name"].ToString() == "Priority")
                    {
                        Priority.Add(new Transaction(t["transaction_token"].ToString(), true, Convert.ToInt32(t["windows_id"])));
                    }
                    else
                    {
                        Regular.Add(new Transaction(t["transaction_token"].ToString(), false, Convert.ToInt32(t["windows_id"])));
                    }
                }
                
            }
            
            
            
        }

        private void LoadWindows()
        {
            this.WindowIDS = new List<int>();

            string param = "?id=" + GetBranchID().ToString();
            var response = this.Client.GetAsync(Endpoints.WindowsUrl + param).Result;
            JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            JArray all = JArray.FromObject(json["windows"]);

            foreach (JObject t in all)
            {
                this.WindowIDS.Add(Convert.ToInt32(t["windows_id"]));
            }

            this.WindowIDS.Sort();
            this.Window1Tokens = new List<Transaction>();
            this.Window2Tokens = new List<Transaction>();
            this.Window3Tokens = new List<Transaction>();


            foreach(Transaction tr in PriorityServing)
            {
                if (tr.Window == this.WindowIDS[0])
                {
                    this.Window1Tokens.Add(tr);
                }
                else if (tr.Window == this.WindowIDS[1])
                {
                    this.Window2Tokens.Add(tr);
                }
                else if (tr.Window == this.WindowIDS[2])
                {
                    this.Window3Tokens.Add(tr);
                }
            }

            foreach (Transaction tr in RegularServing)
            {
                if (tr.Window == this.WindowIDS[0])
                {
                    this.Window1Tokens.Add(tr);
                }
                else if (tr.Window == this.WindowIDS[1])
                {
                    this.Window2Tokens.Add(tr);
                }
                else if (tr.Window == this.WindowIDS[2])
                {
                    this.Window3Tokens.Add(tr);
                }
            }

            foreach (Transaction tr in Priority)
            {
                if (tr.Window == this.WindowIDS[0])
                {
                    this.Window1Tokens.Add(tr);
                }else if(tr.Window == this.WindowIDS[1])
                {
                    this.Window2Tokens.Add(tr);
                }else if(tr.Window == this.WindowIDS[2])
                {
                    this.Window3Tokens.Add(tr);
                }
            }

            foreach (Transaction tr in Regular)
            {
                if (tr.Window == this.WindowIDS[0])
                {
                    this.Window1Tokens.Add(tr);
                }
                else if (tr.Window == this.WindowIDS[1])
                {
                    this.Window2Tokens.Add(tr);
                }
                else if (tr.Window == this.WindowIDS[2])
                {
                    this.Window3Tokens.Add(tr);
                }
            }
        }

        private void Load()
        {

            if(this.Window1 != null)
            {
                win_1.Text = this.Window1;
            }
            else
            {
                win_1.Text = "None";
            }

            if (this.Window2 != null)
            {
                win_2.Text = this.Window2;
            }
            else
            {
                win_2.Text = "None";
            }

            if (this.Window3 != null)
            {
                win_3.Text = this.Window3;
            }
            else
            {
                win_3.Text = "None";
            }
            
        }


        private int GetWindow1ID()
        {
            int n = 1000;

            foreach(int p in this.WindowIDS)
            {
                if(n > p)
                {
                    n = p;
                }
            }

            return n;
        }

        private int GetWindow2ID()
        {
            foreach(int i in this.WindowIDS)
            {
                if(GetWindow1ID() != i && GetWindow3ID() != i)
                {
                    return i; 
                }
            }
            return 0;
        }

        private int GetWindow3ID()
        {
            int n = -1;

            foreach (int p in this.WindowIDS)
            {
                if (n < p)
                {
                    n = p;
                }
            }

            return n;
        }

        private void PlayMusic()
        {
            string RunningPath = AppDomain.CurrentDomain.BaseDirectory;
            string path = "{0}Resources\\Doorbell sound effect.wav";

            string pathLoc = string.Format(@path, Path.GetFullPath(Path.Combine(RunningPath, @"..\..\")));

            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = pathLoc;
            player.Play();
        }
    }
}
