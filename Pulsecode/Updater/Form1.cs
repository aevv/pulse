using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Updater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static FileInfo file;
        public static FileInfo hashFile;
        //   List<string> files = new List<string>();
        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                //foreach (String s in openFileDialog1.FileNames)
                //{
                //    listBox1.Items.Add(Path.GetFileName(s));
                //    files.Add(s);
                //}
                FileInfo f = new FileInfo(openFileDialog1.FileName);
                file = f;
                textBox2.Text = file.Name;
            }
        }

        //  public static event Action<ClientHandler> clientDC;

        public static Boolean stop = false;
        //        public static List<byte[]> fileData = new List<byte[]>();
        public static byte[] filedata;
        public static List<ClientHandler> clients = new List<ClientHandler>();
        //  public static List<Pair<String, byte[], string>> Pairs = new List<Pair<String, byte[], string>>();
        public static string hash;
        public static string zipHash;
        private void button1_Click(object sender, EventArgs e)
        {

            ushort port = 0;
            bool success = UInt16.TryParse(textBox1.Text, out port);
            if (!success)
            {
                MessageBox.Show("Please enter a valid port");
                return;
            }
            if (file == null || hashFile == null)
            {
                MessageBox.Show("Please select files");
                return;
            }
            stop = false;
            filedata = File.ReadAllBytes(file.FullName);
            hash = Utils.calcHash(hashFile.FullName);
            zipHash = Utils.calcHash(file.FullName);
            //Thread.CurrentThread.int
            forceUpdate = checkBox1.Checked;
            clients.Clear();
            Thread t = new Thread(new ParameterizedThreadStart(new Worker().start));
            t.Start((int)port);
            currentWorker = t;
            toggleButtons(true);
            toolStripStatusLabel1.Text = "Started server on " + port;
        }
        public static Form1 form;
        public static Thread currentWorker;
        public static void clientDCed(ClientHandler c)
        {
            clients.Remove(c);
            //  form.label2.Text = "Connected:" + clients.Count;
            form.Invoke(new changeText(form.changeLabel), "Connected:" + clients.Count);
            stopRoutine();
        }
        public static void stopRoutine()
        {
            if (clients.Count == 0 && stop)
            {
                //  if (currentWorker != null)
                //   {
                currentWorker.Abort();
                Worker.listener.Stop(); //important
                //    currentWorker = null;
                //   }
                form.Invoke(new invokeToggle(form.toggleButtons), false);
            }
        }
        public delegate void invokeToggle(bool arg);
        public void toggleButtons(bool start)
        {
            if (start)
            {
                button1.Enabled = false;
                button2.Enabled = true;
                button3.Enabled = false;
                button4.Enabled = false;
                checkBox1.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = true;
                button4.Enabled = true;
                checkBox1.Enabled = true;
            }
        }
        public delegate void changeText(string text);
        public void changeLabel(string text)
        {
            label2.Text = text;
        }
        public static bool forceUpdate = false;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog2.ShowDialog();
            if (dr == DialogResult.OK)
            {
                hashFile = new FileInfo(openFileDialog2.FileName);
                textBox3.Text = hashFile.Name;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Stopping server";
            stop = true;
            stopRoutine();
        }
        public static void calcAllHash()
        {
            foreach (string s in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories))
            {
               
                Console.WriteLine(s);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(currentWorker != null) 
            currentWorker.Abort();
            if(Worker.listener != null)
            Worker.listener.Stop();
        }
    }
}
