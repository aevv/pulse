using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.IO;
using System.Diagnostics;

namespace Pulse.UI
{
    public partial class EditorHelp : Form
    {
        public EditorHelp()
        {
            InitializeComponent();
            webBrowser1.Navigate("p.ulse.net/help/Pulse Documentation.html");
        }
        public static Uri RelativeFormat(String s)
        {
            return new Uri(Path.GetFullPath(s));
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //Console.WriteLine(e.Node.Name);
            switch (e.Node.Name)
            {
                case "Node1":
                    break;
                case "Node2":
                    break;
                case "Node4":
                    break;
                case "Node6":
                    break;
            }
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string[] last = e.Url.ToString().Split('/');
            toolStripStatusLabel1.Text = "Navigating to " + last[last.Length - 1];
        }

        private void openInBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(webBrowser1.Url.ToString());
        }

        private void EditorHelp_Load(object sender, EventArgs e)
        {

        }
    }
}
