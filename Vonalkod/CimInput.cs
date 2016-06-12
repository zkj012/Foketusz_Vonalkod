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
        private KotorEntities entities;

        public CimInput()
        {
            InitializeComponent();
        }

        private void CimInput_Load(object sender, EventArgs e)
        {
            entities = new KotorEntities();
            LoadIrszComboBox();
            // LoadEmeletJelComboBox();
        }

        private void LoadIrszComboBox()
        {

            var irszDataSource = entities.Kozterulet_t.Select(x => x.IRSZ).Distinct().OrderBy(x => x).ToList();


            var autoCompleteList = new AutoCompleteStringCollection();
            foreach (var irsz in irszDataSource)
            {
                autoCompleteList.Add(irsz.ToString());
            }

            cbIrsz.DataSource = irszDataSource;
            cbIrsz.AutoCompleteCustomSource = autoCompleteList;

        }

        private void cbIrsz_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadNevComboBox();
        }

        private void LoadNevComboBox()
        {
            var nevek = entities.Kozterulet_t.Where(x => x.IRSZ == (int)cbIrsz.SelectedItem).Select(x => x.Nev).OrderBy(x => x).ToList();
            var autoCompleteList = new AutoCompleteStringCollection();
            foreach (var nev in nevek)
            {
                autoCompleteList.Add(nev);
            }
            cbNev.DataSource = nevek;
            cbNev.AutoCompleteCustomSource = autoCompleteList;
        }

        private void cbNev_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadJellegComboBox();
        }

        private void LoadJellegComboBox()
        {
            var jellegek = entities.Kozterulet_t.Where(x => x.IRSZ == (int)cbIrsz.SelectedItem && x.Nev == (string)cbNev.SelectedItem)
                .Select(x => x.KozteruletJelleg_m)
                .OrderBy(x => x.Nev)
                .ToList();

            var autoCompleteList = new AutoCompleteStringCollection();
            foreach (var jelleg in jellegek)
            {
                autoCompleteList.Add(jelleg.Nev);
            }

            cbJelleg.DataSource = jellegek;
            cbJelleg.AutoCompleteCustomSource = autoCompleteList;

        }

        private void CimInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (entities != null)
            {
                entities.Dispose();
            }
        }

/*        private void LoadEmeletJelComboBox()
        {
            var emeletJelek = entities.EmeletJel_m.OrderBy(x => x.RovidNev).ToList();
            var autoCompleteList = new AutoCompleteStringCollection();
            foreach (var jel in emeletJelek)
            {
                autoCompleteList.Add(jel.Nev);
            }
            cbEmelet.DataSource = emeletJelek;
            cbEmelet.AutoCompleteCustomSource = autoCompleteList;
        }
*/

        private void btnSave_Click(object sender, EventArgs e)
        {
            int i;
            CimHelper.IRSZ = (int)cbIrsz.SelectedItem;
            CimHelper.Nev = (string)cbNev.SelectedItem;
            CimHelper.JellegKod = (string)cbJelleg.SelectedValue;
            CimHelper.JellegNev = ((KozteruletJelleg_m)(cbJelleg.SelectedItem)).Nev;
            if(!int.TryParse(txtSzam1.Text,out i))
            {
                i = 0;
            }
            CimHelper.Szam1 = i == 0 ? null : (int?)i;
            CimHelper.Jel1 = txtJel1.Text;
            if (!int.TryParse(txtSzam2.Text, out i))
            {
                i = 0;
            }
            CimHelper.Szam2 = i == 0 ? null : (int?)i;
            CimHelper.Jel2 = txtJel2.Text;
            //            CimHelper.EmeletKod = (string)cbEmelet.SelectedValue;
            //            CimHelper.EmeletRovidNev = cbEmelet.SelectedValue == null ? "" : ((EmeletJel_m)cbEmelet.SelectedItem).RovidNev;
            CimHelper.Epulet = txtEpulet.Text;
            CimHelper.Lepcsohaz = txtLepcsohaz.Text;
            
            this.Close();
        }

        private void txtSzam1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtSzam2_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }
    }



    internal static class CimHelper
    {
        public static int IRSZ { get; set; }
        public static string Nev { get; set; }
        public static string JellegKod { get; set; }
        public static string JellegNev { get; set; }
        public static int? Szam1 { get; set; }
        public static string Jel1 { get; set; }
        public static int? Szam2 { get; set; }
        public static string Jel2 { get; set; }

        public static string Epulet { get; set; }
        //        public static string EmeletKod { get; set; }
        //        public static string EmeletRovidNev { get; set; }
        public static string Lepcsohaz { get; set; }

        public static string SzamJel1
        {
            get
            {
                string sz = $"{Szam1?.ToString()}";
                sz += string.IsNullOrEmpty(Jel1) ? "" : $"/{Jel1}";
                return sz;
            }
        }

        public static string SzamJel2
        {
            get
            {
                string sz = $"{Szam2?.ToString()}";
                sz += string.IsNullOrEmpty(Jel2) ? "" : $"/{Jel2}";
                return sz;
            }
        }

        public static string SzamEsJel
        {
            get
            {
                if (String.IsNullOrEmpty(SzamJel2))
                {
                    return SzamJel1;
                }
                else
                {
                    return $"{SzamJel1} - {SzamJel2}";
                }
            }
        }

        public static string CimSzovegKemenysepro
        {
            get
            {
                string Epcim = $"{IRSZ} Budapest, {Nev} {JellegNev} {SzamEsJel}";
                Epcim += string.IsNullOrEmpty(Epulet) ? "" : $" {Epulet} épület";
                Epcim += string.IsNullOrEmpty(Lepcsohaz) ? "" : $" {Lepcsohaz} lépcsőház";

                return Epcim;
            }
        }        
    }



}
