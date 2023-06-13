using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMSProject
{
    public partial class Gama : Form
    {
        public Gama()
        {
            InitializeComponent();

            OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
        public double red
        {
            get
            {
                return (Convert.ToDouble(RedTxtBox.Text));
            }
            set { RedTxtBox.Text = value.ToString(); }
        }

        public double green
        {
            get
            {
                return (Convert.ToDouble(GreenTxtBox.Text));
            }
            set { GreenTxtBox.Text = value.ToString(); }
        }

        public double blue
        {
            get
            {
                return (Convert.ToDouble(BlueTxtBox.Text));
            }
            set { BlueTxtBox.Text = value.ToString(); }
        }
    }
}
