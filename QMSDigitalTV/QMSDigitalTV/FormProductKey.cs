using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using QMSDigitalTV.Digital;

namespace QMSDigitalTV
{
    public partial class FormProductKey : Form
    {
        /* TO HANDLE THE SSL CERTIFICATE ISSUE WITH THE API, WILL REMOVE UPON DEPLOYING THE REST API */
        private HttpClientHandler Handler;
        private HttpClient Client;
        string FilePath;

        public FormProductKey()
        {
            InitializeComponent();

            var projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            this.FilePath = Path.Combine(projectPath, "Resources");

            if (!File.Exists(this.FilePath + @"\config.txt"))
            {
                FileStream fs = File.Create(this.FilePath + @"\config.txt");
            }
        }

        public bool IsInstalled()
        {
            return new FileInfo(this.FilePath + @"\config.txt").Length != 0;
        }

        private void WriteFile()
        {
            this.Handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            this.Client = new HttpClient(this.Handler);
            this.Client.BaseAddress = new Uri(Endpoints.BaseUrl);

            string param = "?product_key=" + this.txtProductKey.Text;
            var response = this.Client.GetAsync(Endpoints.BranchUrl + param).Result;
            JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            JArray array = JArray.FromObject(json["branches"]);


            if (array.Count < 1)
            {
                MessageBox.Show("Product key is invalid!");
            }
            else
            {
                String line = "branch_id=" + array[0]["branch_id"];
                try
                {
                    //Pass the filepath and filename to the StreamWriter Constructor
                    StreamWriter sw = new StreamWriter(this.FilePath + @"\config.txt");
                    //Write a line of text
                    sw.WriteLine(line);
                    MessageBox.Show("Your kiosk has been succesfuly installed. Please close the application and open it again. Thank you. ");
                    //Close the file
                    sw.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception: " + e.Message);
                }

            }

        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void FormProductKey_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            WriteFile();
        }
    }
}
