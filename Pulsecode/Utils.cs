using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Net;
using System.Collections.Specialized;
using System.Threading;

namespace Pulse
{
    /// <summary>
    /// A misc class for stuff that doesn't really belong anywhere else
    /// </summary>
    class Utils
    {
        public static Dictionary<string, string> chatCommands = new Dictionary<string, string>();
        static Utils()
        {
            chatCommands.Add("lua", "a scripting languages implemented into pulse for testing purposes. syntax is /lua <code>");
            chatCommands.Add("pm", "syntax /pm <user> <message>. sends <message> to <user>");
            chatCommands.Add("j", "syntax /j <channel>, joins <channel> but still being worked on");
            chatCommands.Add("r", "syntax /r <text>, sends <text> to the irc server without any modifications");
            chatCommands.Add("close", "closes the current tab, you must have at least 1 open though");
            string help = "list of commands: ";
            foreach (var i in chatCommands)
            {
                help += i.Key + ", ";
            }
            chatCommands.Add("help", help + "help use /help <command> to get more info about <command>");
        }
    
        public static string calcHash(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// Perhaps use this in the future for all mouse intersection calls for more concise code & more readibility, plus ability to manipulate result if not focused
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool contains(Rectangle r)
        {
            if (Game.game.Focused)
            {
                Point p = new Point(Game.game.Mouse.X, Game.game.Mouse.Y);
                return r.Contains(p);
            }
            return false;
        }
        public static string hashString(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower(Config.cultureEnglish);
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="nvc">additional form params</param>
        /// <param name="files">filename, post name, content-type</param>
        /// <returns></returns>
        public static string HttpUploadFile(string url, NameValueCollection nvc, params string[] files)
        {
            Console.WriteLine(string.Format("Uploading to {0}", url));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
            wr.Proxy = null;
            Stream rs = wr.GetRequestStream();
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            if (nvc != null)
            {
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, nvc[key]);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
            }

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            for (int x = 0; x < files.Length; x += 3)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                Console.WriteLine(string.Format(headerTemplate, files[x + 1], files[x], files[x + 2]));
                string header = string.Format(headerTemplate, files[x+1], files[x], files[x+2]);
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);

                FileStream fileStream = new FileStream(files[x], FileMode.Open, FileAccess.Read);
                long total = new FileInfo(files[x]).Length;
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                double counter = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {

                    rs.Write(buffer, 0, bytesRead);
                    counter += bytesRead;
                    Console.WriteLine((counter / (double)total) * 100 + "%");
                }
                fileStream.Close();                              
                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);  
            }
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                string resp = reader2.ReadToEnd();
                Console.WriteLine(string.Format("File uploaded, server response is: {0}", resp));
                return resp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading file " + ex.Message);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
            return "";
        }
        //return modified x from reswidth (used for widths)
        public static int getMX(int Base)
        {
            int diff = 1024 - Base;
            return Config.ResWidth - diff;
        }
        // Returns a System.Drawing.Bitmap with the contents of the current framebuffer

        //this can be used for vids O:
        public static Bitmap GrabScreenshot(Game game)
        {
            if (GraphicsContext.CurrentContext == null)
                throw new GraphicsContextMissingException();

            Bitmap bmp = new Bitmap(game.ClientSize.Width, game.ClientSize.Height);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(game.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, game.ClientSize.Width, game.ClientSize.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY); //removing would increase performance significantly 
            return bmp;
        }
        //for ptextbox
        

    }
    public class UploadClass {
        Thread t;
        public void HttpUploadFileAsync(string url, NameValueCollection nvc, params string[] files)
        {
            List<object> o = new List<object>();
            o.Add(url);
            o.Add(nvc);
            o.AddRange(files);
            t = new Thread(new ParameterizedThreadStart(uploadWorker));
            t.Start(o);
            t.IsBackground = true;
        }
        public int additionalArg;
        public event Action<string, double> uploadPercentChange;
        public event Action<string, int> uploadDone; 
        private void uploadWorker(object o)
        {
            Stream rs = null;
            WebResponse wresp = null;
            HttpWebRequest wr = null;
            try
            {
                List<object> obj = (List<object>)o;
                string url = (string)obj[0];
                NameValueCollection nvc = (NameValueCollection)obj[1];
                List<string> files = new List<string>();
                for (int i = 2; i < obj.Count; i++)
                {
                    files.Add((string)obj[i]);
                }
                Console.WriteLine(string.Format("Uploading to {0}", url));
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                wr = (HttpWebRequest)WebRequest.Create(url);
                wr.ContentType = "multipart/form-data; boundary=" + boundary;
                wr.Method = "POST";
                wr.KeepAlive = true;
                wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
                wr.Proxy = null;
                rs = wr.GetRequestStream();
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                if (nvc != null)
                {
                    foreach (string key in nvc.Keys)
                    {
                        rs.Write(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, key, nvc[key]);
                        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                        rs.Write(formitembytes, 0, formitembytes.Length);
                    }
                }

                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                for (int x = 0; x < files.Count; x += 3)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    Console.WriteLine(string.Format(headerTemplate, files[x + 1], files[x], files[x + 2]));
                    string header = string.Format(headerTemplate, files[x + 1], files[x], files[x + 2]);
                    byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                    rs.Write(headerbytes, 0, headerbytes.Length);

                    FileStream fileStream = new FileStream(files[x], FileMode.Open, FileAccess.Read);
                    long total = new FileInfo(files[x]).Length;
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;
                    double counter = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {

                        rs.Write(buffer, 0, bytesRead);
                        counter += bytesRead;
                        if (uploadPercentChange != null)
                        {
                            uploadPercentChange.Invoke(files[x], (counter / (double)total) * 100);
                        }
                        Console.WriteLine((counter / (double)total) * 100 + "%");
                    }
                    fileStream.Close();
                    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    rs.Write(trailer, 0, trailer.Length);
                }
                rs.Close();


                try
                {
                    wresp = wr.GetResponse();
                    Stream stream2 = wresp.GetResponseStream();
                    StreamReader reader2 = new StreamReader(stream2);
                    string resp = reader2.ReadToEnd();
                    Console.WriteLine(string.Format("File uploaded, server response is: {0}", resp));
                    if (uploadDone != null)
                    {
                        uploadDone.Invoke(resp, additionalArg);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error uploading file " + ex.Message);
                    if (wresp != null)
                    {
                        wresp.Close();
                        wresp = null;
                    }
                }
                finally
                {
                    wr = null;
                }
            }
            catch (ThreadAbortException)
            {
                if (wr != null)
                {
                    wr = null;
                }
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                if (rs != null)
                {
                    rs.Close();
                    rs = null;
                }             
            }
        }

        public void abort()
        {
            if (t != null)
            {
                t.Abort();
            }
        }
    }
}
