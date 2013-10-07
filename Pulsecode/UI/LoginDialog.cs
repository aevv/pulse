using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pulse.Client;
using Pulse;
using Pulse.Screens;
namespace Pulse.UI
{
    public partial class LoginDialog : Form
    {
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string user = textBox1.Text;
            string passhash = Utils.hashString(textBox2.Text);
            PacketWriter.sendLogin(Game.conn.Bw, user, passhash);
            Account.saveAcc(user,passhash);
            this.Dispose();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(null, null);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {



            ((MenuScreen)Game.game.screens["menuScreen"]).logOut();
            Game.pbox.setIrc(null);
            Account.currentAccount = null;
            if (Game.ircl != null)
            {
                Game.ircl.terminate();
                Game.ircl = null;
            }
            this.Dispose();
        }
    }
}
