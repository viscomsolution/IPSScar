using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TGMTcs
{
    public class TGMTcontrol
    {
        public static void SetHint(TextBox textBox, string hint)
        {
            bool waterMarkActive = true;
            textBox.ForeColor = Color.Gray;
            textBox.Text = hint;

            textBox.GotFocus += (source, e) =>
            {
                if (waterMarkActive)
                {
                    waterMarkActive = false;
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                }
            };

            textBox.LostFocus += (source, e) =>
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    waterMarkActive = true;
                    textBox.Text = hint;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }
    }
}
