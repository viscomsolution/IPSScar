using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TGMTcs
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string en = TGMTcrypto.EncryptTextAES("aaabbbcccaaaaaaaaaaaaaaaaaaaaaaaaabbbbbbbbbbbz", "eeee");
            string de = TGMTcrypto.DecryptTextAES(en, "eeee");

            int a = 5;
        }
    }
}
