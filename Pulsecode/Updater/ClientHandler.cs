using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;
namespace Updater
{
    public class ClientHandler
    {
        TcpClient client;
        public ClientHandler(TcpClient cc)
        {
            client = cc;
        }
        public event Action<ClientHandler> clientDC;
        public void start()
        {
            Console.WriteLine("client started");
            try
            {
                client.NoDelay = true;
                using (BinaryWriter bw = new BinaryWriter(client.GetStream()))
                {
                    using (BinaryReader br = new BinaryReader(client.GetStream()))
                    {
                        bool requestOnly = br.ReadBoolean(); // Just requesting version #, implement later
                        if (!requestOnly)
                        {
                            bw.Write(Form1.forceUpdate);
                            bw.Write(Form1.hash); //write the hash of the file, have client store this for comparison
                            bool same = br.ReadBoolean();
                            Console.WriteLine(same);
                            if (!same || Form1.forceUpdate)
                            {
                                bw.Write(Form1.zipHash);
                                MemoryStream ms = new MemoryStream(Form1.filedata);
                                byte[] buf = new byte[4096];
                                int size = 0;
                                while ((size = ms.Read(buf, 0, buf.Length)) != 0)
                                {
                                    bw.Write(buf, 0, size);
                                }
                                bw.Flush();
                            }
                        }
                        #region obselete
                        /* 
                        if (br.ReadInt32() == 31337) //validation
                        {
                            bw.Write((int)-11); //starting write
                            bw.Write((int)Form1.pairs.Count); //write the count
                            foreach (var p in Form1.pairs)
                            {
                                bw.Write(p.key); //writing file name
                                byte result = br.ReadByte();
                                if (result == 0) //file does not exist
                                {
                                    bw.Write((long)p.value.LongLength); //file length
                                    //   bw.Write(p.value); //maybe use memory stream?
                                    MemoryStream ms = new MemoryStream(p.value);
                                    int size = 0;
                                    byte[] buf = new byte[4096];
                                    while ((size = ms.Read(buf, 0, buf.Length)) != 0)
                                    {
                                        bw.Write(buf);
                                    }
                                    byte confirm = br.ReadByte();
                                    if (confirm == 1) //successful download
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        Console.WriteLine("unsuccessful d/l");
                                        break;
                                    }
                                }
                                else if (result == 1) //shit, file exists
                                {
                                    bw.Write(p.extra); //writing hash
                                    byte hashCheckResult = br.ReadByte();
                                    if (hashCheckResult == 0) //identical hashes, so skip
                                    {
                                        continue;
                                    }
                                    else if (hashCheckResult == 1)
                                    {
                                        bw.Write((long)p.value.LongLength); //file length
                                        MemoryStream ms = new MemoryStream(p.value);
                                        //ms.Position
                                        int size = 0;
                                        byte[] buf = new byte[4096];
                                        //   int b = ms.ReadByte();
                                        //   int g = ms.ReadByte();
                                        //  int d = ms.ReadByte();
                                        while ((size = ms.Read(buf, 0, buf.Length)) != 0)
                                        {
                                            //   Console.WriteLine(buf[0]);
                                            bw.Write(buf);
                                            bw.Flush();
                                        }
                                        byte confirm = br.ReadByte();
                                        if (confirm == 1) //successful download
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            Console.WriteLine("unsuccessful d/l");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Unknown hash code " + hashCheckResult);
                                    }
                                }
                            }
                        }
                        else
                        {
                            client.Close();
                            //abort?
                        }*/
                        #endregion
                    }
                   
                } 
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex + " error happened at " + (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString());
                if (client != null)
                {
                    client.Close();
                }
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
                if (clientDC != null)
                {
                    clientDC.Invoke(this);
                }
            }

        }
    }
}
