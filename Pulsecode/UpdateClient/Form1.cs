using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
namespace UpdateClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //For the mobile skin, pretty hacky ;p
           // webBrowser1.Navigating -= this.webBrowser1_Navigating;
            //webBrowser1.Navigate("http://rep.ulse.net", null, null, "User-Agent: Mozilla/5.0 (SymbianOS/9.2; U; Series60/3.1 NokiaE71-1/110.07.127; Profile/MIDP-2.0 Configuration/CLDC-1.1 ) AppleWebKit/413 (KHTML, like Gecko) Safari/413");
            //webBrowser1.Navigating += this.webBrowser1_Navigating;
            webBrowser1.Navigate("http://exp.ulse.net/changelog", null, null, "User-Agent:pulse updater");
        }
        public delegate void disable();
        public static Form1 form;
        public void disB()
        {
            progressBar1.MarqueeAnimationSpeed = 0;
            progressBar1.Style = ProgressBarStyle.Blocks;
        }
        static Thread tt;
      public static bool started;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

             //   TcpClient client = new TcpClient("92.232.66.53", 6666);
              //  client.ReceiveTimeout = 500;
              //  UpdateHandler uh = new UpdateHandler(client);
                Thread t = new Thread(new ThreadStart(new ThreadStart(WebsiteParse.parseInit)));
                tt = t;
                t.Start();
                started = true;
                label2.Text = "Checking what to update...";
            }
            catch (Exception ef) { MessageBox.Show(ef.ToString()); }
            button1.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 1;
        }
        bool block;
        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
       //     webBrowser1.Navigating += this.webBrowser1_Navigating;
          //  progressBar1.MarqueeAnimationSpeed = 0;
         //   progressBar1.Style = ProgressBarStyle.Blocks;
        }
        public delegate void showMessage(string s);
        public void showMessageImpl(string s) //lazy lol
        {
            progressBar1.Style = ProgressBarStyle.Continuous;
        }
        string lasturl;
        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
         ///   progressBar1.Style = ProgressBarStyle.Marquee;
         ///   progressBar1.MarqueeAnimationSpeed = 1;
            //Console.WriteLine(e.TargetFrameName);

         /*   if (lasturl != e.Url.ToString())
            {
                e.Cancel = true;
                //Console.WriteLine(sender);
                lasturl = e.Url.ToString();
                //  webBrowser1.Navigate("http://rep.ulse.net", null, null, "User-Agent: Mozilla/5.0 (SymbianOS/9.2; U; Series60/3.1 NokiaE71-1/110.07.127; Profile/MIDP-2.0 Configuration/CLDC-1.1 ) AppleWebKit/413 (KHTML, like Gecko) Safari/413");    
                // webBrowser1.Navigating -= this.webBrowser1_Navigating;
                webBrowser1.Navigate(e.Url, null, null, "User-Agent: Mozilla/5.0 (SymbianOS/9.2; U; Series60/3.1 NokiaE71-1/110.07.127; Profile/MIDP-2.0 Configuration/CLDC-1.1 ) AppleWebKit/413 (KHTML, like Gecko) Safari/413");
            }
            else
            {
                lasturl = e.Url.ToString();
            }*/
        }
        public delegate void changeLabel(string s);
        public delegate void setProgress(int i);
        public void changeLabelImp(string s)
        {
            label2.Text = s;
        }
        public void setProg(int i)
        {
            progressBar1.Value = i;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (started)
            {
                if (MessageBox.Show("are you sure? if your download is not finished it may be corrupted", "alert", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                if (tt != null)
                {
                    tt.Abort();
                }
            }
        }

        private void label2_TextChanged(object sender, EventArgs e)
        {
     //       label2.Location = new Point(0, this.Height);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
              }
    }
}
