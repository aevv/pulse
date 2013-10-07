using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Threading;
namespace UpdateClient
{
    class WebsiteParse
    {
        static Dictionary<string, string> fileHashes = new Dictionary<string, string>();
        static Thread current;
       const string baseUrl = "http://exp.ulse.net";
        public static void parseInit()
        {
            //   string s = Uri.EscapeDataString("help/pulse file.zip");
            // Uri uri = new Uri("http://p.ulse.net\\help\\pulse file.zip");
            // Console.WriteLine(s);
            try
            {
                WebClient wc = new WebClient();
                wc.Proxy = null;  //<-- THIS CODE MAKES DOWNLOADSTRING 12309013912X FASTER
                wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore); //weird caching

                string toparse = wc.DownloadString(baseUrl + "/Release/hashes.html"); //why the fuck does this block for so long
                current = Thread.CurrentThread;
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                wc.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(wc_DownloadFileCompleted);
                // Console.WriteLine(toparse);
                StringReader sr = new StringReader(toparse);
                string thisline;
                while ((thisline = sr.ReadLine()) != null)
                {
                    if (!thisline.Contains('|'))
                    {
                        continue;
                    }
                    else
                    {
                        string[] split = thisline.Split('|');
                        Console.WriteLine("File Name:{0} Hash:{1}", split[0], split[1]); //file name first, then hash
                        fileHashes.Add(split[0], split[1]);
                    }
                }
                Form1.form.Invoke(new Form1.showMessage(Form1.form.showMessageImpl), "");
                foreach (var pair in fileHashes)
                {
                    if (!File.Exists(pair.Key))
                    {
                        string s = pair.Key.Replace('\\', '/');
                        try
                        {
                            #region FUCK CONVENIENCE METHODS SO MUCH

                            /* string[] dirNames = pair.Key.Split('\\');
                        for (int i = 0; i < dirNames.Length; i++)
                        {
                            if (i == dirNames.Length - 1)
                            {
                                break;
                            }
                            string dirString = "";
                            for (int j = 0; j <= i; j++)
                            {
                                dirString += dirNames[j];
                            }
                            if (!Directory.Exists(dirString))
                            {
                                
                                Directory.CreateDirectory(dirString);
                            }
                        }*/
                            #endregion
                            string[] dirNames = pair.Key.Split('\\');
                            if (dirNames.Length > 1) //not just one file
                            {
                                FileInfo temp = new FileInfo(pair.Key);
                                if (!Directory.Exists(temp.DirectoryName))
                                {
                                    Directory.CreateDirectory(temp.DirectoryName);
                                }
                            }
                            //      Console.WriteLine("http://p.ulse.net/" + Uri.EscapeDataString(s));
                            string pathtoWrite = Path.GetFullPath(pair.Key);
                            //   Console.WriteLine(pathtoWrite);
                            currentDownloading = pair.Key;
                            invokeText("Downloading " + currentDownloading);
                            wc.DownloadFileAsync(new Uri(baseUrl + "/Release/" + Uri.EscapeDataString(s)), pathtoWrite);

                            Thread.Sleep(1000 * 60 * 60); //sry if your download takes 1 hour
                            //alex thats poor, is there no way to just check if its still downloading?
                            //wc.IsBusy maybe? 
                            //doubt it, async is on a worker thread
                            throw new Exception("Timed out");
                        }
                        catch (ThreadInterruptedException ex)
                        {
                            continue;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Could not download " + s + " " + e);
                        }
                        // wc.DownloadFile()
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(Path.GetFullPath(pair.Key));
                        if (IsFileLocked(fi))
                        {
                            MessageBox.Show("Please close " + pair.Key + ", update aborted.");
                            Environment.Exit(0);
                        }
                        if (fi.IsReadOnly)
                        {
                            //File.SetAttributes(fi.FullName, FileAttributes.Normal);
                            fi.IsReadOnly = false;
                        }
                        string hash = Program.calcHash(Path.GetFullPath(pair.Key));
                        if (hash != pair.Value)
                        {
                            string s = pair.Key.Replace('\\', '/');
                            try
                            {
                                // Console.WriteLine("http://p.ulse.net/" + Uri.EscapeDataString(s));
                                currentDownloading = pair.Key;
                                invokeText("Downloading " + currentDownloading);
                                wc.DownloadFileAsync(new Uri(baseUrl + "/Release/" + Uri.EscapeDataString(s)), pair.Key);

                                Thread.Sleep(1000 * 60 * 60); //sry if your download takes 1 hour
                                throw new Exception("Timed out");
                            }
                            catch (ThreadInterruptedException ex)
                            {
                                continue;
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show("Could not download " + s + " " + e);
                            }
                        }
                        else
                        {
                            invokeText("Skipped " + pair.Key);
                        }
                    }
                }
                invokeText("All up to date!");
                Form1.started = false;
            }
            catch (Exception www)
            {
                MessageBox.Show(www.ToString(), "update failed");
                Environment.Exit(-1);
            }
        }
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        static string currentDownloading = "";
        //   Delegate d1 = new Form1.changeLabel(Form1.form.changeLabelImp);
        public static void invokeText(string s)
        {
            Form1.form.Invoke(new Form1.changeLabel(Form1.form.changeLabelImp), s);
        }
        public static void invokeProg(int i)
        {
            Form1.form.Invoke(new Form1.setProgress(Form1.form.setProg), i);
        }
        static void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

            if (e.Error != null)//(!string.IsNullOrWhiteSpace(e.Error.ToString()))
            {
                MessageBox.Show(e.Error.ToString());
            }
            else
            {
                
                invokeText("Finished downloading " + currentDownloading);
            }
            current.Interrupt();
        }

        static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            invokeText(currentDownloading + " : " + e.ProgressPercentage + "%");
            invokeProg(e.ProgressPercentage);
            Console.WriteLine(e.BytesReceived);
        }
        public static void calcAllHash(string baseDir)
        {
            string dirstring = baseDir;// @"C:\Users\Alex\Desktop\9292pulse\Pulsecode\bin\Release";
            foreach (string s in Directory.EnumerateFiles(dirstring, "*.*", SearchOption.AllDirectories))
            {
                if (s == System.Reflection.Assembly.GetExecutingAssembly().Location) //|| s.Contains("pdb"))
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
                    Console.WriteLine(parse.Remove(0,parse.Split('\\')[0].Length + 1) + "|" + Program.calcHash(s));
                }
                catch { }
            }
        }
    }
}
