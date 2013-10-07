using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Tamir.SharpSsh;

namespace Updater
{
    public partial class hashcalc : Form
    {
       
        static List<string> blacklist = new List<string>();
        public hashcalc()
        {
            string initialDirectory = "";
            string blackListValue = "";
            InitializeComponent();
            if (!File.Exists("setup.ini"))
            {
                File.WriteAllText("setup.ini", "#The initial directory to use\r\nInitDir=\r\n#Blacklist of file names, comma separated, case insensitive\r\nBlackList=");
                MessageBox.Show("setup.ini has been created in the current working directory");
            }
            else
            {
                using (StreamReader sr = new StreamReader("setup.ini"))
                {
                    string thisLine = "";
                    while ((thisLine = sr.ReadLine()) != null)
                    {
                        if (thisLine.StartsWith("#"))
                        {
                            continue;
                        }
                        string[] splitted = thisLine.Split('=');
                        string value = splitted[1];
                        switch (splitted[0])
                        {
                            case "InitDir":
                                initialDirectory = value;

                                break;
                            case "BlackList" :
                                blackListValue = value;
                                break;
                        }
                    }
                }
                string[] files = blackListValue.ToLower().Split(',');//csv
                blacklist.AddRange(files);
                
                folderBrowserDialog1.SelectedPath = initialDirectory;
                setHashPath(initialDirectory);

            }

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        public static Dictionary<string, string> calcAllHash(string baseDir)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string dirstring = baseDir;// @"C:\Users\Alex\Desktop\9292pulse\Pulsecode\bin\Release";
            foreach (string s in Directory.EnumerateFiles(dirstring, "*.*", SearchOption.AllDirectories))
            {
                Console.WriteLine(s);
                if (s == System.Reflection.Assembly.GetExecutingAssembly().Location || blacklist.Contains(Path.GetFileName(s).ToLower())) //|| s.Contains("pdb"))
                {
                    continue;
                }
                System.Uri uri1 = new Uri(s);

                System.Uri uri2 = new Uri(dirstring);
                Uri relativeUri = uri2.MakeRelativeUri(uri1);


                string parse = Uri.UnescapeDataString(relativeUri.ToString());
                parse = parse.Replace('/', '\\');

                try
                {
                    result.Add(parse.Remove(0, parse.Split('\\')[0].Length + 1), Utils.calcHash(s));
                }
                catch (Exception) { }

            }
            return result;
        }
        string dirToHash;
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = folderBrowserDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                setHashPath(folderBrowserDialog1.SelectedPath);
            }
        }
        public void setHashPath(string path)
        {
            dirToHash = path;
            directoryBox.Text = dirToHash;
            if (dirToHash != null)
            {
                hashDisplay.Clear();
                theFiles = null;
                theFiles = calcAllHash(dirToHash);
                foreach (var s in theFiles)
                {
                    hashDisplay.AppendText(s.Key + "|" + s.Value + "\n");
                }
            }
        }
        Dictionary<string, string> theFiles;
        private void button2_Click(object sender, EventArgs e)
        {

        }
        const string root = "/var/www/Release/";//"/usr/www/wwh/public/ulse/p/Release/";
        const string hashesLoc = "http://exp.ulse.net/Release/hashes.html";
        private void button3_Click(object sender, EventArgs e)
        {
            if (theFiles == null)
            {
                MessageBox.Show("Please select a folder");
                return;
            }
            WebClient wc = new WebClient();
            wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            Dictionary<string, string> serverVer = new Dictionary<string, string>();
            wc.Proxy = null;
            try
            {
                string toparse = wc.DownloadString(hashesLoc);

                using (StringReader sr = new StringReader(toparse))
                {
                    string thisLine;
                    while ((thisLine = sr.ReadLine()) != null)
                    {
                        if (thisLine.Contains('|'))
                        {
                            string[] splitted = thisLine.Split('|');
                            serverVer.Add(splitted[0], splitted[1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error parsing website, " + ex);
                //return;
                Console.WriteLine("Error parsing website, will overwrite everything.");
            }
            using (StreamWriter fs = new StreamWriter("hashes.html", false))
            {
                foreach (var s in theFiles)
                {
                    fs.WriteLine(s.Key + "|" + s.Value);
                }
            }
            var sftp = new Sftp(hostName.Text, User.Text, pass.Text);
            sftp.OnTransferProgress += new FileTransferEvent(sftp_OnTransferProgress);
            sftp.Connect();
            foreach (var file in theFiles)
            {
                if (serverVer.ContainsKey(file.Key))
                {
                    if (serverVer[file.Key] == file.Value)
                    {
                        Console.WriteLine("skipped " + file.Key);
                        continue;
                    }
                }
                if (file.Key.Contains('\\'))
                {
                    //FileInfo fi = new FileInfo(dirToHash + "\\" + file.Key);
                    //file.Key.Split("");
                    string[] splitted = file.Key.Split('\\');
                    string dirr = root;
                    for (int i = 0; i < splitted.Length - 1; i++)
                    {
                        dirr += splitted[i] + "/";
                        bool mkdir = false;
                        try
                        {
                            sftp.GetFileList(dirr);
                        }
                        catch (Exception)
                        {
                            mkdir = true;
                            Console.WriteLine("Making directory " + dirr);
                        }
                        if (mkdir)
                        {
                            try
                            {

                                sftp.Mkdir(dirr);
                            }
                            catch (Exception exc)
                            {
                                Console.WriteLine("exception with " + dirr + " while making the dir " + exc);
                            }
                        }
                    }

                }
                string changed = file.Key.Replace('\\', '/');
                sftp.Put(dirToHash + "\\" + file.Key, root + changed);
                Console.WriteLine("Sent {0} to {1}", dirToHash + "\\" + file.Key, root + changed);
            }
            sftp.Put("hashes.html", root + "hashes.html");
            Console.WriteLine("Send hashes.html");
            //      foreach (string s in sftp.GetFileList("/"))
            //      {
            //    Console.WriteLine(s);
            //     }
            // sftp.
            //      sftp.Put("test.html","/var/www/test.html");
            sftp.Close();
            /*   FtpWebRequest fRequest = (FtpWebRequest)WebRequest.Create("ftp://proxima-centauri.dreamhost.com/");
               //fRequest.Method = WebRequestMethods.Ftp
               Ftp
               fRequest.Credentials = new NetworkCredential("aevv", "matteh11F");
               Stream ftpStream = fRequest.GetRequestStream();
               */
        }

        void sftp_OnTransferProgress(string src, string dst, int transferredBytes, int totalBytes, string message)
        {
            Console.WriteLine(transferredBytes + "/" + totalBytes);
        }

        private void hashcalc_Load(object sender, EventArgs e)
        {

        }
    }
}
