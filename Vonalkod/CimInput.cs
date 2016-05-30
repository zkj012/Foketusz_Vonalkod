using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vonalkod
{
    public partial class CimInput : Form
    {
        public CimInput()
        {
            InitializeComponent();
        }

        private void CimInput_Load(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            CimHelper.CimSzovegKemenysepro = txtCim.Text;
            this.Close();
        }
    }
    internal static class CimHelper
    {
        public static string CimSzovegKemenysepro;
        public static int? koztid;
    }

    

}
