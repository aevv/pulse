using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace Pulse.UI {
    public partial class NewSong : Form {
        public NewSong() {
            InitializeComponent();
            this.AllowDrop = true;

            // Add event handlers for the drag & drop functionality
            this.DragEnter += new DragEventHandler(Form_DragEnter);
            this.DragDrop += new DragEventHandler(Form_DragDrop);
            Console.WriteLine("loaded");
            }
        private void Form_Load(object sender, EventArgs e) {
            // Enable drag and drop for this form
            // (this can also be applied to any controls)

            }

        // This event occurs when the user drags over the form with 
        // the mouse during a drag drop operation 
        void Form_DragEnter(object sender, DragEventArgs e) {
            // Check if the Dataformat of the data can be accepted
            // (we only accept file drops from Explorer, etc.)
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy; // Okay
            else
                e.Effect = DragDropEffects.None; // Unknown data, ignore it

            }

        // Occurs when the user releases the mouse over the drop target 
        String bgPath;
        String mp3Path;
        void Form_DragDrop(object sender, DragEventArgs e) {
            // Extract the data from the DataObject-Container into a string list
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            // Do something with the data...

            // For example add all files into a simple label control:
            foreach(string File in FileList) {
                if (Path.GetExtension(File).ToLower(Config.cultureEnglish).Equals(".png") || Path.GetExtension(File).ToLower(Config.cultureEnglish).Equals(".jpg") || Path.GetExtension(File).ToLower(Config.cultureEnglish).Equals(".jpeg"))
                {
                    pictureBox1.Image = ResizeBitmap(new Bitmap(Bitmap.FromFile(File)), pictureBox1.Width, pictureBox1.Height);
                    bgPath = File;
                    break;
                }
                if (Path.GetExtension(File).ToLower(Config.cultureEnglish).Equals(".mp3"))
                {
                    mp3Path = File;
                    textBox1.Text = Path.GetFileName(File);
                    break;
                }
            }
            }
        public static Bitmap ResizeBitmap(Bitmap b, int nWidth, int nHeight) {
            Bitmap result = new Bitmap(nWidth, nHeight);
            using(Graphics g = Graphics.FromImage((Image)result))
                g.DrawImage(b, 0, 0, nWidth, nHeight);
            return result;
            }
        public static string generateFolder(string artist, string title) {
            string toCheck = "songs\\" + artist + " - " + title;
            if(Directory.Exists(toCheck)) {
                return toCheck + "(1)";
                } else {
                Directory.CreateDirectory(toCheck);
                return toCheck;
                }
            }
        private void button1_Click(object sender, EventArgs e) {
           
            if(string.IsNullOrWhiteSpace(mp3Path)) {
                MessageBox.Show("Please specify mp3!");
                return;
                }
            WaterMarkTextBox[] boxes = { Title, Artist, preview, Creator, difs };
            foreach(WaterMarkTextBox wm in boxes) {
                if(string.IsNullOrWhiteSpace(wm.Text)) {
                    MessageBox.Show("Please provide data for the " + wm.WaterMarkText + "!");

                    return;
                    }
                }
            string folder = generateFolder(Artist.Text, Title.Text);
            if (!string.IsNullOrWhiteSpace(bgPath))
            {
                File.Copy(bgPath, folder + "\\" + Path.GetFileName(bgPath));
            }
            File.Copy(mp3Path, folder + "\\" + Path.GetFileName(mp3Path));
            foreach (String diff in difs.Text.Split(',')) {
                String mdiff = diff.Trim();
                using (StreamWriter sw = new StreamWriter(folder + "\\" + Artist.Text + " - " + Title.Text + " - " + mdiff + ".pnc"))
                {
                    sw.WriteLine("pncv1");
                    sw.WriteLine("[song]");
                    sw.WriteLine("filename=" + Path.GetFileName(mp3Path));
                    sw.WriteLine("songname=" + Title.Text);
                    sw.WriteLine("artist=" + Artist.Text);
                    sw.WriteLine("bg=" + ((string.IsNullOrWhiteSpace(bgPath)) ? "" : Path.GetFileName(bgPath)));
                    sw.WriteLine("difficulty=" + mdiff);
                    sw.WriteLine("preview=" + preview.Text);
                    sw.WriteLine("creator=" + Creator.Text);
                    sw.WriteLine("[timing]");
                    sw.WriteLine("[editor]");
                    sw.WriteLine("[objects]");
                }
            }
          /*  using(StreamWriter sw = new StreamWriter(folder + "\\" + Artist.Text + " - " + Title.Text + ".pnc")) {
                sw.WriteLine("[song]");
                sw.WriteLine("filename=" + Path.GetFileName(mp3Path));
                sw.WriteLine("songname=" + Title.Text);
                sw.WriteLine("artist=" + Artist.Text);
                sw.WriteLine("bg=" + ((string.IsNullOrWhiteSpace(bgPath))? "" : Path.GetFileName(bgPath)));
                sw.WriteLine("preview=" + preview.Text);
                sw.WriteLine("creator=" + Creator.Text);
                sw.WriteLine("[timing]");
                foreach(String diff in difs.Text.Split(',')) {
                    sw.WriteLine("[" + diff.Trim() + "]");
                    }
                }*/
      
            MessageBox.Show("Successfully created song");
        //    button1.Enabled = false;
            this.Dispose();
            }
        }
    }
