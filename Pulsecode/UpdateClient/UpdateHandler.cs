/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Windows.Forms;
using Ionic.Zip;
namespace UpdateClient
{
    class UpdateHandler
    {
        TcpClient client;
        public UpdateHandler(TcpClient handl)
        {
            client = handl;
        }
        public void start()
        {
            Console.WriteLine("started");
            client.NoDelay = true;
            try
            {
                using (BinaryWriter bw = new BinaryWriter(client.GetStream()))
                {
                    using (BinaryReader br = new BinaryReader(client.GetStream()))
                    {
                        #region obselete
                        //bw.Write((int)31337);
                        //if (br.ReadInt32() == -11)
                        //{
                        //    int count = br.ReadInt32();
                        //    for (int i = 0; i < count; i++)
                        //    {
                        //        String temp = Path.GetFileName(br.ReadString()); //file name
                        //        if (File.Exists(temp))
                        //        {
                        //            bw.Write((byte)1);
                        //            string hash = br.ReadString();
                        //            String myhash = Program.calcHash(temp);
                        //            if (myhash.Equals(hash))
                        //            {
                        //                bw.Write((byte)0);
                        //                continue;
                        //            }
                        //            else
                        //            {
                        //                bw.Write((byte)1);
                        //                long fileLength = br.ReadInt64();
                        //                // byte[] buf = new byte[4096];
                        //                byte[] bb = new byte[fileLength];
                        //                  long read = 0;
                        //                FileStream fs = new FileStream(temp, FileMode.Create);
                        //                int size = 0;
                        //               // client.ReceiveTimeout;
                        //                while ((size = br.Read(bb, 0, bb.Length)) != 0)
                        //                {
                        //                    Console.WriteLine("one read"); 
                        //                    fs.Write(bb, 0, size);
                        //                    fs.Flush();
                        //                    read += size;
                        //                    if (read >= fileLength)
                        //                    {
                        //                        break;
                        //                    }

                        //                }

                        //                /*    while (read < fileLength)
                        //                    {
                        //                        byte[] readss = br.ReadBytes(4096);
                        //                        fs.Write(readss, 0, readss.Length);
                        //                        read += readss.Length;
                        //                    }
                        //                //      if (fileLength - read != 0)
                        //                //      {
                        //                ///          byte[] bytess = br.ReadBytes((int)(fileLength - read));
                        //                ///        fs.Write(bytess, 0, bytess.Length);
                        //                //     }
                        //                fs.Close();
                        //                bw.Write((byte)1); //successful download
                        //            }
                        //        }
                        //        else
                        //        {
                        //            bw.Write((byte)0); //0 if file does not exist, 1 if it does
                        //            long fileLength = br.ReadInt64();
                        //            // byte[] buf = new byte[4096];
                        //            byte[] bb = new byte[fileLength];
                        //            long read = 0;
                        //            FileStream fs = new FileStream(temp, FileMode.Create);
                        //            int size = 0;
                        //            KeyValuePair<string, string> test = new KeyValuePair<string, string>();

                        //            while ((size = br.Read(bb, 0, bb.Length)) != 0)
                        //            {
                        //                Console.WriteLine("one read");
                        //                fs.Write(bb, 0, size);
                        //                read += size;
                        //                if (read >= fileLength)
                        //                {
                        //                    break;
                        //                }
                        //            }


                        //      if (fileLength - read != 0)
                        //      {
                        ///          byte[] bytess = br.ReadBytes((int)(fileLength - read));
                        ///        fs.Write(bytess, 0, bytess.Length);
                        //     }
                        #endregion
                       
                        String hash = "";
                        bw.Write(false); //Full on request
                        bool forceUpdate = br.ReadBoolean(); //first reads for force
                        if (File.Exists("Pulse.exe"))
                        {
                            hash = Program.calcHash("Pulse.exe");
                        }
                        String serverHash = br.ReadString();
                        bool match = hash == serverHash;
                        bw.Write(match);
                        if (forceUpdate || !match)
                        {
                            string ziphash = br.ReadString();
                            using (FileStream fs = new FileStream("temp.zip", FileMode.Create))
                            {
                                int size = 0;
                                byte[] buf = new byte[4096];
                                while ((size = br.Read(buf, 0, buf.Length)) != 0)
                                {
                                    fs.Write(buf, 0, size);
                                }
                                fs.Flush();
                            }
                            if (ziphash != Program.calcHash("temp.zip"))
                            {
                                MessageBox.Show("Corrupted zip, please re-download");
                            }
                            else
                            {

                                ZipFile zipFile = new ZipFile("temp.zip");
                                foreach (ZipEntry ent in zipFile)
                                {
                                    ent.Extract(ExtractExistingFileAction.OverwriteSilently);
                                }
                                zipFile.Dispose();
                                File.Delete("temp.zip");
                                Form1.form.Invoke(new Form1.showMessage(Form1.form.showMessageImpl), "Download and extraction finished");
                               // MessageBox.Show("Download and extraction finished");
                            }
                        }
                        else
                        {
                            Form1.form.Invoke(new Form1.showMessage(Form1.form.showMessageImpl), "Nothing to update here.");
                            //Console.  WriteLine("blockingtest"); //lol doesn't work oh well
                        }
                    }
                }
            }
            catch (Exception exx)
            {
                Console.WriteLine(exx);
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }
            Form1.form.Invoke(new Form1.disable(Form1.form.disB));
        }
    }
}
*/