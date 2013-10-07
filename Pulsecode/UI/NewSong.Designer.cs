namespace Pulse.UI {
    partial class NewSong {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
                }
            base.Dispose(disposing);
            }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewSong));
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.difs = new Pulse.UI.WaterMarkTextBox();
            this.Creator = new Pulse.UI.WaterMarkTextBox();
            this.preview = new Pulse.UI.WaterMarkTextBox();
            this.Title = new Pulse.UI.WaterMarkTextBox();
            this.Artist = new Pulse.UI.WaterMarkTextBox();
            this.textBox1 = new Pulse.UI.WaterMarkTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(164, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(209, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Drag the desired mp3 and bg onto the form";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(156, 71);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(217, 132);
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(225, 447);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Create";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // difs
            // 
            this.difs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.difs.Location = new System.Drawing.Point(99, 400);
            this.difs.Name = "difs";
            this.difs.Size = new System.Drawing.Size(337, 20);
            this.difs.TabIndex = 6;
            this.difs.WaterMarkColor = System.Drawing.Color.Gray;
            this.difs.WaterMarkText = "Difficulties (comma separated)";
            // 
            // Creator
            // 
            this.Creator.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.Creator.Location = new System.Drawing.Point(99, 360);
            this.Creator.Name = "Creator";
            this.Creator.Size = new System.Drawing.Size(337, 20);
            this.Creator.TabIndex = 5;
            this.Creator.WaterMarkColor = System.Drawing.Color.Gray;
            this.Creator.WaterMarkText = "Creator";
            // 
            // preview
            // 
            this.preview.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.preview.Location = new System.Drawing.Point(99, 317);
            this.preview.Name = "preview";
            this.preview.Size = new System.Drawing.Size(337, 20);
            this.preview.TabIndex = 4;
            this.preview.WaterMarkColor = System.Drawing.Color.Gray;
            this.preview.WaterMarkText = "Preview point (in ms)";
            // 
            // Title
            // 
            this.Title.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.Title.Location = new System.Drawing.Point(99, 225);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size(337, 20);
            this.Title.TabIndex = 2;
            this.Title.WaterMarkColor = System.Drawing.Color.Gray;
            this.Title.WaterMarkText = "Title";
            // 
            // Artist
            // 
            this.Artist.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.Artist.Location = new System.Drawing.Point(99, 273);
            this.Artist.Name = "Artist";
            this.Artist.Size = new System.Drawing.Size(337, 20);
            this.Artist.TabIndex = 3;
            this.Artist.WaterMarkColor = System.Drawing.Color.Gray;
            this.Artist.WaterMarkText = "Artist";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(167, 45);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(182, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.WaterMarkColor = System.Drawing.Color.Gray;
            this.textBox1.WaterMarkText = "";
            // 
            // NewSong
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 482);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.difs);
            this.Controls.Add(this.Creator);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.preview);
            this.Controls.Add(this.Title);
            this.Controls.Add(this.Artist);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NewSong";
            this.Text = "Create a new notechart";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

            }

        #endregion

        private System.Windows.Forms.Label label1;
        private WaterMarkTextBox textBox1;
        private WaterMarkTextBox Artist;
        private WaterMarkTextBox Title;
        private WaterMarkTextBox preview;
        private System.Windows.Forms.PictureBox pictureBox1;
        private WaterMarkTextBox Creator;
        private WaterMarkTextBox difs;
        private System.Windows.Forms.Button button1;
        }
    }