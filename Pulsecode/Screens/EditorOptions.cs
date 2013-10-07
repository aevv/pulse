using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Pulse.Mechanics;
using Ionic.Zip;

namespace Pulse.Screens
{
    public partial class EditorOptions : Form
    {
        EditorScreen parent;
        double selectedBookmark;
        public EditorOptions(EditorScreen parent)
        {
            this.parent = parent;
            InitializeComponent();
            parent.CurrentSong.remakeLists(parent.Chart);
            popTiming();
            timingBox.SelectedIndex = 0;
            artistBox.Text = parent.CurrentSong.Artist;
            titleBox.Text = parent.CurrentSong.SongName;
            creatorBox.Text = parent.CurrentSong.Creator;
            bgBox.Text = parent.Chart.BgName;
            fileBox.Text = parent.CurrentSong.FileName;
            leadBox.Text = "" + (parent.Chart.LeadInTime * 1000);
            tagsBox.Text = "";
            foreach (string s in parent.Chart.Tags)
            {
                tagsBox.Text += s + " ";
            }
            foreach (var pair in parent.CurrentSong.Charts)
            {
                difficultiesCombo.Items.Add(pair.Value.Name);
            }
            difficultiesCombo.SelectedIndex = 0;
            foreach (double d in parent.CurrentSong.Bookmarks)
            {
                bookmarkBox.Items.Add(d);
            }
            if (bookmarkBox.Items.Count > 0)
            {
                bookmarkBox.SelectedIndex = 0;
            }
        }

        private void EditorOptions_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            offsetBox.Text = "" + Math.Round(parent.CurrentOffset, 0);
        }
        int selectedTimingOffset;

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TimingSection t = null;
            bool chartS = false;
            string temp = (string)timingBox.SelectedItem;
            string[] split = temp.Split(',');
            foreach (var pair in parent.CurrentSong.Timings)
            {
                if (pair.Key == Convert.ToInt32(split[0]))
                {
                    t = pair.Value;
                }
            }
            foreach (TimingSection s in parent.Chart.Sections)
            {
                if (s.Offset == Convert.ToInt32(split[0]))
                {
                    t = s;
                    chartS = true;
                }
            }
            if (t != null)
            {
                offsetBox.Text = "" + t.Offset;
                bpmBox.Text = "" + 60 / (t.Snap / 1000);
                diffSpecBox.CheckedChanged -= new EventHandler(specChange);
                diffSpecBox.Checked = chartS;
                diffSpecBox.CheckedChanged += new EventHandler(specChange);
                bpmChange.CheckedChanged -= new EventHandler(timingLeave);
                bpmChange.Checked = t.ChangeSnap;
                bpmChange.CheckedChanged += new EventHandler(timingLeave);
                if (t.ChangeSnap)
                {
                    bpmBox.Enabled = true;
                }
                else
                {
                    bpmBox.Enabled = false;
                }

            }
            selectedTimingOffset = Convert.ToInt32(offsetBox.Text);
        }

        private void timingLeave(object sender, EventArgs e)
        {
            try
            {
                if (!offsetBox.Text.Equals("") && !bpmBox.Text.Equals("") && Convert.ToDouble(bpmBox.Text, Config.cultureEnglish) > 0 && Convert.ToInt32(offsetBox.Text) > -1)
                {
                    if (Convert.ToDouble(bpmBox.Text, Config.cultureEnglish) > 1000 && bpmChange.Checked)
                    {
                        MessageBox.Show("Bpm cannot be larger than 1000");
                        bpmBox.Text = "60";
                    }
                    try
                    {
                        try
                        {
                            if (diffSpecBox.Checked)
                            {
                                foreach (TimingSection s in parent.Chart.Sections)
                                {
                                    if (s.Offset == selectedTimingOffset)
                                    {
                                        s.Snap = (60 / (Convert.ToDouble(bpmBox.Text, Config.cultureEnglish))) * 1000;
                                        s.ChangeSnap = bpmChange.Checked;
                                        s.Offset = Convert.ToInt32(offsetBox.Text);
                                    }
                                }
                            }
                            else
                            {
                                parent.CurrentSong.Timings[Convert.ToInt32(offsetBox.Text)].Snap = (60 / (Convert.ToDouble(bpmBox.Text, Config.cultureEnglish))) * 1000;
                                parent.CurrentSong.Timings[Convert.ToInt32(offsetBox.Text)].ChangeSnap = bpmChange.Checked;
                            }
                        }
                        catch
                        {
                            parent.CurrentSong.Timings.Remove(selectedTimingOffset); 
                            parent.CurrentSong.Timings.Add(Convert.ToInt32(offsetBox.Text), new TimingSection(Convert.ToDouble(offsetBox.Text, Config.cultureEnglish), (60 / Convert.ToDouble(bpmBox.Text, Config.cultureEnglish)) * 1000, bpmChange.Checked, 1200));
                        }
                        if (bpmChange.Checked)
                        {
                            bpmBox.Enabled = true;
                        }
                        else
                        {
                            bpmBox.Enabled = false;
                        }
                        timingBox.Items.Clear();
                        parent.CurrentSong.remakeLists(parent.Chart);
                        popTiming();
                        timingBox.SelectedItem = "" + offsetBox.Text + "," + bpmBox.Text + "," + bpmChange.Checked;                        
                    }
                    catch (Exception ex) { ErrorLog.log(ex.Message + "\n" + ex.StackTrace + "\nOffBox: " + offsetBox.Text + ", bpmBox: " + bpmBox.Text + ", sel: " + timingBox.SelectedItem + "\n"); }
                }
                else
                {
                    MessageBox.Show("Wrong values entered, BPM must be above 0 and boxes cannot be empty");
                }
            }
            catch
            {
                MessageBox.Show("Wrong values entered, BPM must be above 0 and boxes cannot be empty");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            int id = 0;
            for (int x = 0; x < parent.CurrentSong.TimingsList.Count; x++)
            {
                if (parent.CurrentOffset > parent.CurrentSong.TimingsList[x].Offset)
                {
                    id = x;
                }
            }
            try
            {
                parent.CurrentSong.Timings.Add((int)parent.CurrentOffset, new TimingSection(Math.Floor(parent.CurrentOffset), parent.CurrentSong.TimingsList[id].Snap, false));
                timingBox.Items.Clear();
                parent.CurrentSong.remakeLists(parent.Chart);
                popTiming();

            }
            catch
            {

            }
        }

        private void dataLeave(object sender, EventArgs e)
        {
            try
            {
                parent.CurrentSong.Artist = artistBox.Text;
                parent.Chart.BgName = bgBox.Text;
                parent.CurrentSong.Creator = creatorBox.Text;
                parent.CurrentSong.FileName = fileBox.Text;
                parent.CurrentSong.SongName = titleBox.Text;
                string[] sp = tagsBox.Text.Split(' ');
                for (int x = 0; x < sp.Length; x++)
                {
                    if (!sp[x].Equals("") || !sp[x].Equals(" "))
                    {
                        parent.Chart.Tags.Add(sp[x]);
                    }
                }
                string s = leadBox.Text;
                for (int x = 0; x < s.Length; x++)
                {
                    if (s[x] == ',' || s[x] == '.')
                    {
                        s = s.Remove(x, 1);
                        x--;
                    }
                }
                parent.Chart.LeadInTime = Convert.ToDouble(s, Config.cultureEnglish) / 1000;
                leadBox.Text = s;
            }
            catch
            {

            }
        }

        private void closing(object sender, FormClosingEventArgs e)
        {
            timingLeave(null, null);
            dataLeave(null, null);
            bool found = false;
            foreach (TimingSection s in parent.CurrentSong.TimingsList)
            {
                if (s.ChangeSnap)
                {
                    found = true;
                }
            }
            if (!found)
            {
                MessageBox.Show("No valid BPM setting timing sections found, adding a 60 bpm 0 offset section");
                parent.CurrentSong.Timings.Add(0, new TimingSection(0, 1000, true));
            }
            parent.refreshGraphics();
            parent.opOpen = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                diffNameBox.Text = (string)difficultiesCombo.SelectedItem;
                judgeBox.Items.Clear();
                for (int x = 1; x < 11; x++)
                {
                    judgeBox.Items.Add(x);
                }
                judgeBox.SelectedIndex = parent.CurrentSong.Charts[difficultiesCombo.SelectedIndex].Judgement - 1;
            }
            catch
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (timingBox.SelectedItem != null)
            {
                string[] split = timingBox.SelectedItem.ToString().Split(',');
                parent.CurrentSong.Timings.Remove(Int32.Parse(split[0], System.Globalization.NumberStyles.AllowDecimalPoint));
                parent.CurrentSong.remakeLists(parent.Chart);
                timingBox.Items.Clear();
                popTiming();
                offsetBox.Text = "";
                timingBox.Text = "";
            }
        }

        private void diffNewButton_Click(object sender, EventArgs e)
        {
            parent.CurrentSong.Charts.Add(parent.CurrentSong.Charts.Count, new Chart(0, "NewChart"));
            difficultiesCombo.Items.Add(parent.CurrentSong.Charts[parent.CurrentSong.Charts.Count - 1].Name);
            difficultiesCombo.SelectedIndex = difficultiesCombo.Items.Count - 1;
        }

        private void diffUpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (difficultiesCombo.SelectedIndex < 0)
                    difficultiesCombo.SelectedIndex = 0;
                parent.CurrentSong.Charts[difficultiesCombo.SelectedIndex].Name = diffNameBox.Text;
                parent.CurrentSong.Charts[difficultiesCombo.SelectedIndex].Judgement = judgeBox.SelectedIndex + 1;
                int temp = difficultiesCombo.SelectedIndex;
                difficultiesCombo.Items.Clear();
                foreach (var pair in parent.CurrentSong.Charts)
                {
                    difficultiesCombo.Items.Add(pair.Value.Name);
                }
                difficultiesCombo.SelectedIndex = temp;
            }
            catch { }
        }

        private void diffDelButton_Click(object sender, EventArgs e)
        {
            if (parent.CurrentSong.Charts.Count > -1)
            {
                parent.CurrentSong.Charts.Remove(difficultiesCombo.SelectedIndex);
                int temp = difficultiesCombo.SelectedIndex;
                difficultiesCombo.Items.Clear();
                foreach (var pair in parent.CurrentSong.Charts)
                {
                    difficultiesCombo.Items.Add(pair.Value.Name);
                }
                if (temp > 0)
                {
                    difficultiesCombo.SelectedIndex = temp - 1;
                }
                else
                {
                    difficultiesCombo.SelectedIndex = temp - 0;
                }
                parent.CurrentSong.reIDCharts();
                parent.loadSong(parent.CurrentSong, difficultiesCombo.SelectedIndex);
            }
            else
            {
                MessageBox.Show("Cannot delete all difficulties");
            }
        }

        private void diffSwitchButton_Click(object sender, EventArgs e)
        {
            parent.loadSong(parent.CurrentSong, difficultiesCombo.SelectedIndex);
        }

        private void offsetBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void loadSongToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void popTiming()
        {
            foreach (TimingSection s in parent.CurrentSong.TimingsList)
            {
                if (s.Snap == 0.0)
                {
                    timingBox.Items.Add("" + s.Offset + "," + 0 + "," + s.ChangeSnap);
                }
                else
                {
                    timingBox.Items.Add("" + s.Offset + "," + 60 / (s.Snap / 1000) + "," + s.ChangeSnap);
                }
            }
        }

        private void specChange(object sender, EventArgs e)
        {
            if (diffSpecBox.Checked)
            {
                TimingSection s = null;
                try
                {
                    foreach (var pair in parent.CurrentSong.Timings)
                    {
                        if (pair.Key == selectedTimingOffset)
                        {
                            s = pair.Value;
                            parent.CurrentSong.Timings.Remove(selectedTimingOffset);
                            break;
                        }
                    }
                    parent.Chart.Sections.Add(s);
                    parent.CurrentSong.remakeLists(parent.Chart);
                    timingBox.Items.Clear();
                    popTiming();
                }
                catch { }
            }
            else
            {
                TimingSection s = null;
                foreach (TimingSection t in parent.Chart.Sections)
                {
                    if (t.Offset == selectedTimingOffset)
                    {
                        s = t;
                        parent.Chart.Sections.Remove(t);
                        break;
                    }
                }
                parent.CurrentSong.Timings.Add(selectedTimingOffset, s);
                parent.CurrentSong.remakeLists(parent.Chart);
                timingBox.Items.Clear();
                popTiming();
            }
        }
        private void openHelp()
        {
            new Pulse.UI.EditorHelp().ShowDialog();
        }
        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void bookmarkBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bmOffBox.Text = "" + bookmarkBox.SelectedItem;
            selectedBookmark = (double)bookmarkBox.SelectedItem;
        }

        private void bmCurOffButton_Click(object sender, EventArgs e)
        {
            bmOffBox.Text = "" + Math.Round(parent.CurrentOffset, 0);
            bmUpdate(null, null);
        }

        private void newBMButton_Click(object sender, EventArgs e)
        {
            bool toAdd = true;
            foreach (double d in parent.CurrentSong.Bookmarks)
            {
                if (Math.Round(parent.CurrentOffset, 0) == d)
                {
                    toAdd = false;
                }
            }
            if (toAdd)
            {
                parent.CurrentSong.Bookmarks.Add(Math.Round(parent.CurrentOffset, 0));
                bookmarkBox.Items.Add(Math.Round(parent.CurrentOffset, 0));
            }
            bmUpdate(null, null);
        }

        private void delBMButton_Click(object sender, EventArgs e)
        {
            try
            {
                double bm = (double)bookmarkBox.SelectedItem;
                parent.CurrentSong.Bookmarks.Remove(bm);
                if (bookmarkBox.SelectedIndex > 0)
                {
                    bookmarkBox.SelectedIndex--;
                }
                else
                {
                    bookmarkBox.SelectedIndex = 0;
                }
                bookmarkBox.Items.Remove(bm);
            }
            catch (Exception ex)
            {
                ErrorLog.log(ex);
            }
        }
        private void bmUpdate(object sender, EventArgs e)
        {
            for (int x = 0; x < bookmarkBox.Items.Count; x++)
            {
                double temp = (double)bookmarkBox.Items[x];
                if (temp == selectedBookmark)
                {
                    bookmarkBox.Items[x] = Convert.ToDouble(bmOffBox.Text);
                }
            }
            parent.CurrentSong.Bookmarks.Sort(delegate(double a, double b)
            {
                return a.CompareTo(b);
            });
            bookmarkBox.Items.Clear();
            int id = -1;
            for (int x = 0; x < parent.CurrentSong.Bookmarks.Count; x++)
            {
                bookmarkBox.Items.Add(parent.CurrentSong.Bookmarks[x]);
                if (parent.CurrentSong.Bookmarks[x] == selectedBookmark)
                {
                    id = x;
                }
            }
            bookmarkBox.SelectedIndex = id;
        }

        private void exportAspnzToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (parent.saveMap())
            {
                SaveFileDialog s = new SaveFileDialog();
                s.FileName = parent.CurrentSong.Artist + " - " + parent.CurrentSong.SongName + ".pnz";
                DialogResult res = s.ShowDialog();
                if (res == DialogResult.OK)
                {
                    using (ZipFile zip = new ZipFile(s.FileName))
                    {
                        zip.AddFile("songs\\" + parent.CurrentSong.Dir + "\\" + parent.CurrentSong.FileName, "");
                        foreach (var pair in parent.CurrentSong.Charts)
                        {
                            try
                            {
                                zip.AddFile("songs\\" + parent.CurrentSong.Dir + "\\" + pair.Value.BgName, "");
                            }
                            catch { }
                            zip.AddFile("songs\\" + parent.CurrentSong.Dir + "\\" + parent.CurrentSong.PncName + " - " + pair.Value.Name, "");
                        }
                        zip.Save();
                    }
                }
            }
        }

        private void leadBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
    }
}
