using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GlowHokeyServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            label2.Visible = false;
            Porttxt.Visible = false;
            button1.Visible = false;
            label1.Visible = true;
            panel1.Refresh();

            TCPThread tcp = new TCPThread(Convert.ToInt32(Porttxt.Text));
            Thread th = new Thread(tcp.handle);
            th.Start();
//            Program.runServer(Convert.ToInt32(Porttxt.Text));
            
        }
    }
}
