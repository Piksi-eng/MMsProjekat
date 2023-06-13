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
    public partial class EdgeParamter : Form
    {
        public int nValue
        {
            get
            {
                return (Convert.ToInt32(TextBox.Text, 10));
            }
            set { TextBox.Text = value.ToString(); }
        }
        public EdgeParamter()
        {
            InitializeComponent();

            OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            CancleBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
