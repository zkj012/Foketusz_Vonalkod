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
            LoadEmeletJelComboBox();
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

        private void LoadEmeletJelComboBox()
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


        private void btnSave_Click(object sender, EventArgs e)
        {
            CimHelper.IRSZ = (int)cbIrsz.SelectedItem;
            CimHelper.Nev = (string)cbNev.SelectedItem;
            CimHelper.JellegKod = (string)cbJelleg.SelectedValue;
            CimHelper.JellegNev = ((KozteruletJelleg_m)(cbJelleg.SelectedItem)).Nev;
            CimHelper.Szam1 = numSzam1.Value == 0 ? null : (int?)numSzam1.Value;
            CimHelper.Jel1 = txtJel1.Text;
            CimHelper.Szam2 = numSzam2.Value == 0 ? null : (int?)numSzam2.Value;
            CimHelper.Jel2 = txtJel2.Text;
            CimHelper.EmeletKod = (string)cbEmelet.SelectedValue;
            CimHelper.EmeletRovidNev = ((EmeletJel_m)cbEmelet.SelectedItem).RovidNev;
            CimHelper.Ajto = txtAjto.Text;
            
            this.Close();
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
        public static string EmeletKod { get; set; }
        public static string EmeletRovidNev { get; set; }
        public static string Ajto { get; set; }

        public static string SzamJel1
        {
            get
            {
                return $"{Szam1?.ToString()}{Jel1}";
            }
        }

        public static string SzamJel2
        {
            get
            {
                return $"{Szam2?.ToString()}{Jel2}";
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
                return $"{IRSZ} Budapest, {Nev} {JellegNev} {SzamEsJel} {EmeletRovidNev} {Ajto}";
                // Feltételezve, hogy ez mind budapest
            }
        }        
    }



}
