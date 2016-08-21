using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;


// Coded by Mehmet Kıllıoğlu
// https://github.com/mehmetkillioglu/ieeeDownloader

namespace ieeeXploreDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string appPath = Path.GetDirectoryName(Application.ExecutablePath);
        string link = "http://ieeexplore.ieee.org/stamp/stamp.jsp?tp=&arnumber="; // Initialize variables
        string link2 = "http://ieeexplore.ieee.org/xpl/articleDetails.jsp?arnumber=";
        string folderName;
        string proxyLink;
        string fileSaveName;
        int proxyPort;
        string proxyUsername;
        string proxyPassword;
        string articleTitle;
        string saveLocation;
        public void updateVariables() // This function will update variables from program settings
        {
                proxyLink = textBox1.Text;
                proxyPort = Int32.Parse(textBox2.Text);
                proxyUsername = textBox3.Text;
                proxyPassword = textBox4.Text;
                saveLocation = textBox6.Text;
                if (checkBox2.Checked) // If checked, file name will be %articleTitle%.pdf, otherwise %articleNumber%.pdf
                {
                    fileSaveName = textBox7.Text;
                }
                else
                {
                    fileSaveName = textBox5.Text;
                }
        }
        private void Form1_Load(object sender, EventArgs e) // Update textboxes from program settings
        {
            
            textBox1.Text = Properties.Settings.Default["proxyLink"].ToString();
            textBox2.Text = Properties.Settings.Default["proxyPort"].ToString();
            textBox3.Text = Properties.Settings.Default["proxyUsername"].ToString();
            textBox4.Text = Properties.Settings.Default["proxyPassword"].ToString();
            textBox6.Text = Properties.Settings.Default["saveLocation"].ToString();
            textBox1.Text.Trim();
            textBox2.Text.Trim();
            updateVariables();
            toolStripStatusLabel2.Text = "Ready...";
            statusStrip1.Update();
        }

        private void button3_Click(object sender, EventArgs e) // Save textbox changes to program settings
        {
            Properties.Settings.Default["proxyLink"] = textBox1.Text.Trim();
            Properties.Settings.Default["proxyPort"] = Int32.Parse(textBox2.Text.Trim());
            Properties.Settings.Default["proxyUsername"] = textBox3.Text;
            Properties.Settings.Default["proxyPassword"] = textBox4.Text;
            Properties.Settings.Default["saveLocation"] = textBox6.Text;
            Properties.Settings.Default.Save();
            updateVariables();
            toolStripStatusLabel2.Text = "Settings Saved!";
            statusStrip1.Update();
        }
        

        private void button1_Click(object sender, EventArgs e) // Select new download path
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if( result == DialogResult.OK )
            {
                folderName = folderBrowserDialog1.SelectedPath;
                textBox6.Text = folderName.ToString(); // Update textbox
            }
        }
        HttpWebResponse myHttpWebResponse;
        HttpWebResponse myHttpWebResponse2;
        HttpWebRequest request;
        HttpWebRequest request2;
        Stream remoteStream = null;
        Stream localStream = null;
        private void button2_Click(object sender, EventArgs e) // Main download code
        {

            if (myHttpWebResponse != null) myHttpWebResponse.Close(); // Clear 
            if (myHttpWebResponse2 != null) myHttpWebResponse2.Close();
            if (remoteStream != null) remoteStream.Close();
            if (localStream != null) localStream.Close();
            if (textBox5.Text.All(Char.IsNumber)){

            toolStripStatusLabel2.Text = "Downloading!";
            statusStrip1.Update();
            updateVariables();
            
            
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()); // Check file name
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            fileSaveName = r.Replace(fileSaveName, "");
            request = (HttpWebRequest)WebRequest.Create(link + textBox5.Text); // Create first request
            if (checkBox1.Checked) // Check Use Proxy setting
            {
            var proxyURI = new Uri(string.Format("{0}:{1}", proxyLink, proxyPort));
            ICredentials credentials = new NetworkCredential(proxyUsername, proxyPassword);
            WebProxy webProxy = new WebProxy(proxyURI, true, null, credentials); // Initialize webProxy
            request.Proxy = webProxy;
            }
            request.AllowAutoRedirect = true; // This is required because ieee Xplore redirects itself
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36"; // Header info
            CookieContainer myCookies = new CookieContainer(); // IEEE Xplore uses cookies to access pdf
            request.CookieContainer = myCookies;
            myHttpWebResponse = (HttpWebResponse)request.GetResponse(); // Get response
            Stream receiveStream = myHttpWebResponse.GetResponseStream();
            StreamReader readStream = null;
            readStream = new StreamReader(receiveStream, Encoding.GetEncoding(myHttpWebResponse.CharacterSet)); // Read response to stream


            string data = readStream.ReadToEnd();
            List<string> list = new List<string>(); // Create a new list to hold all links from first source code. Our PDF link will appear as last link. 

            Regex regex = new Regex("(?:href|src)=[\"|']?(.*?)[\"|'|>]+", RegexOptions.Singleline | RegexOptions.CultureInvariant); // Get all links
            if (regex.IsMatch(data))
            {
                foreach (Match match in regex.Matches(data))
                {
                    list.Add(match.Groups[1].Value);
                }
            }
            int linkCount = list.Count;
            String pdfLink = list[linkCount - 1].ToString(); // Save PDF link 

            request2 = (HttpWebRequest)WebRequest.Create(pdfLink); // This request will end up with PDF File
            request2.AllowAutoRedirect = true; // This is required because ieee Xplore redirects itself
            if (checkBox1.Checked) // Check Use Proxy setting
            {
                var proxyURI = new Uri(string.Format("{0}:{1}", proxyLink, proxyPort));
                ICredentials credentials = new NetworkCredential(proxyUsername, proxyPassword);
                WebProxy webProxy = new WebProxy(proxyURI, true, null, credentials);
                request2.Proxy = webProxy;
            }
            request2.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36";
            request2.CookieContainer = myCookies;
            myHttpWebResponse2 = (HttpWebResponse)request2.GetResponse();
            remoteStream = null;
            localStream = null;
            int bytesProcessed = 0;
            var fileName = saveLocation + "/" + fileSaveName + ".pdf"; // Set file path 
            try
            {
                if (myHttpWebResponse2 != null)
                {
                    remoteStream = myHttpWebResponse2.GetResponseStream();

                    localStream = File.Create(fileName); // Create File
                    byte[] buffer = new byte[1024]; // Allocate a 1k buffer
                    int bytesRead;
                    do
                    {
                        bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                        localStream.Write(buffer, 0, bytesRead); // Write stream to file
                        bytesProcessed += bytesRead;
                    }
                    while (bytesRead > 0);
                }
            }
            finally
            {
                if (myHttpWebResponse != null) myHttpWebResponse.Close(); // Clear 
                if (myHttpWebResponse2 != null) myHttpWebResponse2.Close();
                if (remoteStream != null)
                    remoteStream.Close();
                if (localStream != null)
                    localStream.Close();
            }
            toolStripStatusLabel2.Text = "Downloaded!"; // Status update
            statusStrip1.Update();
            }
            else{
                MessageBox.Show("Article number is not valid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox6.Text == "")
            {
                MessageBox.Show("Save location is not valid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                System.Diagnostics.Process.Start(@textBox6.Text); // Open save location in Explorer
            }

        }
        public void findArticleNumber()
        {
            if (textBox8.Text != "")
            {
                Uri myUri = new Uri(textBox8.Text);
                int param1 = Int32.Parse((HttpUtility.ParseQueryString(myUri.Query).Get("arnumber")));
                textBox5.Text = param1.ToString();
            }
        }
        private void button5_Click(object sender, EventArgs e) // Get Article Title
        {
            findArticleNumber();
            if (textBox5.Text != "")
            {
                toolStripStatusLabel2.Text = "Getting Title!";
                statusStrip1.Update();
                updateVariables();
                HttpWebRequest request3 = (HttpWebRequest)WebRequest.Create(link2 + textBox5.Text); // Create request to article details page
                HttpWebResponse myHttpWebResponse3 = (HttpWebResponse)request3.GetResponse(); // Get Response
                Stream receiveStream = myHttpWebResponse3.GetResponseStream();
                StreamReader readStream = null;
                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(myHttpWebResponse3.CharacterSet)); // Read stream

                string data3 = readStream.ReadToEnd();
                Match DescriptionMatch = Regex.Match(data3, "<meta name=\"citation_title\" content=\"([^<]*)\">", RegexOptions.IgnoreCase | RegexOptions.Multiline); // Find title from meta data called cititation_title
                articleTitle = DescriptionMatch.Groups[1].Value;
                string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                articleTitle = r.Replace(articleTitle, "");

                textBox7.Text = articleTitle;
                checkBox2.Checked = true;
                toolStripStatusLabel2.Text = "Ready!";
                statusStrip1.Update();
            }
            else
            {
                MessageBox.Show("Article number is not valid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["proxyLink"] = ""; // Clear all settings
            Properties.Settings.Default["proxyPort"] = 0;
            Properties.Settings.Default["proxyUsername"] = "";
            Properties.Settings.Default["proxyPassword"] = "";
            Properties.Settings.Default["saveLocation"] = "";
            Properties.Settings.Default.Save();
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            updateVariables();
            toolStripStatusLabel2.Text = "Settings Cleared!";
            statusStrip1.Update();
        }

    }
}
