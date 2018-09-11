using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ReadWriteCsv;
namespace Google_maps_downloader
{
    public partial class Form1 : Form
    {
        Bitmap img;

        public Form1()
        {
            InitializeComponent();
            this.webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
            this.webBrowser1.ScriptErrorsSuppressed = true;

        }

        private void go_Button_Click(object sender, EventArgs e)
        {
            Navigate(address_Textbox.Text);
        }

        private void Navigate(String address)
        {
            if (String.IsNullOrEmpty(address)) return;
            if (address.Equals("about:blank")) return;
            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
            {
                address = "http://" + address;
            }
            try
            {
                webBrowser1.Navigate(address);
            }
            catch (System.UriFormatException)
            {
                return;
            }

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.webBrowser1.Document.MouseMove += new HtmlElementEventHandler(this.document_OnMouseMove);
            MessageBox.Show("页面加载完成");
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            address_Textbox.Text = webBrowser1.Url.ToString();

        }

        public string parseLonlat(string googleUrl)
        {
            string lonlat = "";

            string pattern = @"@-?\d+.\d+,-?\d+.\d+";

            foreach (Match match in Regex.Matches(googleUrl, pattern))
                lonlat = match.Value;

            return lonlat;
        }

        public bool saveFile(string lonlat, string classes, Bitmap img)
        {
            try
            {
                if (!Directory.Exists(classes))
                {
                    Directory.CreateDirectory(classes);
                }

                string guid = Guid.NewGuid().ToString().Replace("-", "");
                string imgFilename = classes + @"\" + guid + ".tif";
                img.Save(imgFilename);
                using (CsvFileWriter writer = new CsvFileWriter("classes.csv",true))
                {
                    CsvRow csvRow = new CsvRow();
                    csvRow.Add(lonlat);
                    csvRow.Add(classes);
                    csvRow.Add(imgFilename);

                    writer.WriteRow(csvRow);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            

        }

        private void document_OnMouseMove(object sender, HtmlElementEventArgs e)
        {
            img = new Bitmap(256, 256);
            Point sP = this.webBrowser1.PointToScreen(this.webBrowser1.Location);
            sP.X += 435;
            sP.Y += 200;
            Graphics g = Graphics.FromImage(img);

            g.CopyFromScreen(sP, Point.Empty, this.webBrowser1.ClientSize);

            pictureBox1.Image = img;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                this.Hide();
                string classes = this.listBox1.SelectedItem.ToString();
                string lonlat = parseLonlat(webBrowser1.Url.ToString()).Replace("\"", "");
                bool result = saveFile(lonlat, classes, img);
                if (result)
                {
                    MessageBox.Show("保存成功");

                }
                else
                {
                    MessageBox.Show("保存失败");
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.StackTrace);
            }
            finally
            {
                this.Show();
            }
        }

    }
}
