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
    public partial class VonalkodBeolvasas : Form
    {
        List<VonalkodTartomany> vktart;
        Dictionary<string, string> NyTip = new Dictionary<string,string>();
        Dictionary<string, TreeNode> VkTree = new Dictionary<string, TreeNode>();
        Dictionary<string, Tan> VkTan = new Dictionary<string, Tan>();
        Tan ta;
        Tan t1;
        List<Tan> t2;
        List<Tan> t3;

        public VonalkodBeolvasas()
        {
            InitializeComponent();
        }

        private void VonalkodBeolvasas_Load(object sender, EventArgs e)
        {
            // tanúsítványtípus-vonalkódtartomány inicializálása
            // minden vonalkódnál meg kell kérdezni, hogy milyen típusú, ha az új vonalkód select NyomtatvanytipusKod from VonalkodTartomany_t where <VONALKÓD> between Eleje and Vege
            using (KotorEntities ke = new KotorEntities())
            {
                vktart = (from v in ke.VonalkodTartomany_t
                          select new VonalkodTartomany()
                          {
                              NyomtatvanyTipusKod = v.NyomtatvanytipusKod,
                              Eleje = v.Eleje,
                              Vege = v.Vege
                          }).ToList();

                foreach (var v in (from n in ke.NyomtatvanyTipus_m select new { n.Kod, n.Nev }).ToList())
                {
                    NyTip.Add(v.Kod, v.Nev);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            // csak próbálkozások


            tbVk.Text = "01150000137719";
            //tbVk.Text = "201410262961";
            //tbVk.Text = "1214139089";
            tbVk.Refresh();

            using (KotorEntities ke = new KotorEntities())
            {
                var ta = (from v in ke.Tanusitvany_t
                            where v.Vonalkod == tbVk.Text
                            select new Tan()
                            {
                                Vonalkod=v.Vonalkod,
                                TanId = v.TanusitvanyId,
                                SzuloId = v.SzulotanusitvanyId,
                                MunkaId = v.MunkaId,
                                oid = v.Oid,
                                FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                                FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod)
                            }).First();

                var t1 = (from v in ke.Tanusitvany_t
                         where (v.TanusitvanyId == ta.FotanId )
                         select new Tan()
                         {
                                Vonalkod=v.Vonalkod,
                                TanId = v.TanusitvanyId,
                                SzuloId = v.SzulotanusitvanyId,
                                MunkaId = v.MunkaId,
                                oid = v.Oid,
                                FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                                FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod)
                         }).First();

                var t2 = (from v in ke.Tanusitvany_t
                         where (v.SzulotanusitvanyId == ta.FotanId)
                         select new Tan()
                         {
                             Vonalkod = v.Vonalkod,
                             TanId = v.TanusitvanyId,
                             SzuloId = v.SzulotanusitvanyId,
                             MunkaId = v.MunkaId,
                             oid = v.Oid,
                             FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                             FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod)
                         }).ToList();

                var t3 = (from v in ke.Tanusitvany_t
                         where (v.Tanusitvany_t2.SzulotanusitvanyId == ta.FotanId )
                         select new Tan()
                         {
                                Vonalkod=v.Vonalkod,
                                TanId = v.TanusitvanyId,
                                SzuloId = v.SzulotanusitvanyId,
                                MunkaId = v.MunkaId,
                                oid = v.Oid,
                                FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                                FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod)
                         }).ToList();

                
                tvVonalkodok.BeginUpdate();
                tvVonalkodok.Nodes.Clear();
                // Dictionary<Tan,TreeNode> tn = new Dictionary<Tan,TreeNode>();

                TreeNode tf = tvVonalkodok.Nodes.Add(t1.Vonalkod);
                // tn.Add(t1, tf);

                foreach(Tan tl in t2)
                {
                    TreeNode tln = tf.Nodes.Add(tl.Vonalkod);
                    // tn.Add(tl, tln);
                    // var t3flt = from t in t3 where t.SzuloId == tl.TanId select new Tan();
                    /* foreach (Tan tx in t3)
                    {
                        if(tx.SzuloId == tl.TanId)
                        {
                            Console.WriteLine(tx.Vonalkod);
                            tln.Nodes.Add(tx.Vonalkod);
                        }
                    }*/

                    foreach (Tan te in (from t in t3 where t.SzuloId == tl.TanId select new Tan()
                    {
                        Vonalkod = t.Vonalkod,
                        TanId = t.TanId,
                        SzuloId = t.SzuloId,
                        MunkaId = t.MunkaId,
                        oid = t.oid,
                        FotanId = t.FotanId,
                        FotanVk = t.FotanVk
                    }))
                    {
                        Console.WriteLine(te.Vonalkod);
                        tln.Nodes.Add(te.Vonalkod);
                    }
                }

                tvVonalkodok.EndUpdate();
                tvVonalkodok.ExpandAll();


                /*
                if(ta != null)
                {
                    var tsz = ta;
                    while (tsz.SzuloId != null)
                    {
                        tsz = (from v in ke.Tanusitvany_t
                              where v.TanusitvanyId == tsz.SzuloId
                              select new Tan()
                              {
                                TanId = v.TanusitvanyId,
                                SzuloId = v.SzulotanusitvanyId
                              }).First();
                    }
                    ta.FotanId = tsz.TanId;
                }
*/



                /*var vk = from ft in Tanusitvany_t
                         join lt in Tanusitvany_t on ft.TanusitvanyId equals lt.SzulotanusitvanyId into tmpLt
                         from lt in tmpLt.DefaultIfEmpty()
                         join et in Tanusitvany_t on lt.TanusitvanyId equals et.SzulotanusitvanyId into tmpEt
                         from et in tmpEt.DefaultIfEmpty()
                         let FotanId = ft.TanusitvanyId
                         let FotanVk = ft.Vonalkod
                         let LtId = (int?)lt.TanusitvanyId
                         let LtVk = lt.Vonalkod
                         let LtTipus = lt.NyomtatvanyTipusKod
                         let EtId = (int?)et.TanusitvanyId
                         let EtVk = et.Vonalkod
                         let EtTipus = et.NyomtatvanyTipusKod

                         where ft.Vonalkod == "01150000137719"
                         select new
                         {
                             FotanId,
                             FotanVk,
                             ft.NyomtatvanyTipusKod,
                             LtId,
                             LtVk,
                             LtTipus,
                             EtId,
                             EtVk,
                             EtTipus

                         };*/

            }
        }

        private void tbVk_Leave(object sender, EventArgs e)
        {

            if (tbVk.Text != "")
            {
                using (KotorEntities ke = new KotorEntities())
                {
                    if (GetTanTipus(tbVk.Text) == "Ismeretlen" && tbVk.Text != "99")
                    {
                        MessageBox.Show("Ismeretlen nyomtatványtípus, érvénytelen vonalkód");
                    }
                    else
                    {
                        if (tbFo.Text == "")
                        #region Nincs aktuális főtanúsítvány
                        {
                            ta = (from v in ke.Tanusitvany_t
                                  where v.Vonalkod == tbVk.Text
                                  select new Tan()
                                  {
                                      Vonalkod = v.Vonalkod,
                                      TanId = v.TanusitvanyId,
                                      SzuloId = v.SzulotanusitvanyId,
                                      SzuloVk = v.Tanusitvany_t2.Vonalkod,
                                      SzuloNyomtTipKod = v.Tanusitvany_t2.NyomtatvanyTipusKod,
                                      MunkaId = v.MunkaId,
                                      oid = v.Oid,
                                      FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                                      FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod),
                                      nyomtatvanytipuskod = v.NyomtatvanyTipusKod
                                  }).FirstOrDefault();
                            if (ta == null || ta.FotanId == null)
                            {
                                MessageBox.Show("Nincs főtanúsítvány vagy nem adatbázisban szereplő vonalkód (lehet biankó főtanúsítvány is)");
                            }
                            else
                            {
                                t1 = (from v in ke.Tanusitvany_t
                                      where (v.TanusitvanyId == ta.FotanId)
                                      select new Tan()
                                      {
                                          Vonalkod = v.Vonalkod,
                                          TanId = v.TanusitvanyId,
                                          SzuloId = v.SzulotanusitvanyId,
                                          SzuloVk = v.SzulotanusitvanyId == null ? "" : v.Tanusitvany_t2.Vonalkod,
                                          SzuloNyomtTipKod = v.SzulotanusitvanyId == null ? "" : v.Tanusitvany_t2.NyomtatvanyTipusKod,
                                          MunkaId = v.MunkaId,
                                          oid = v.Oid,
                                          FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                                          FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod),
                                          db = vkInDb.IsmertVk,
                                          nyomtatvanytipuskod = v.NyomtatvanyTipusKod
                                      }).FirstOrDefault();


                                t2 = (from v in ke.Tanusitvany_t
                                      where (v.SzulotanusitvanyId == ta.FotanId)
                                      select new Tan()
                                      {
                                          Vonalkod = v.Vonalkod,
                                          TanId = v.TanusitvanyId,
                                          SzuloId = v.SzulotanusitvanyId,
                                          SzuloVk = v.Tanusitvany_t2.Vonalkod,
                                          SzuloNyomtTipKod = v.Tanusitvany_t2.NyomtatvanyTipusKod,
                                          MunkaId = v.MunkaId,
                                          oid = v.Oid,
                                          FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                                          FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod),
                                          db = vkInDb.IsmertVk,
                                          nyomtatvanytipuskod = v.NyomtatvanyTipusKod
                                      }).ToList();

                                t3 = (from v in ke.Tanusitvany_t
                                      where (v.Tanusitvany_t2.SzulotanusitvanyId == ta.FotanId)
                                      select new Tan()
                                      {
                                          Vonalkod = v.Vonalkod,
                                          TanId = v.TanusitvanyId,
                                          SzuloId = v.SzulotanusitvanyId,
                                          SzuloVk = v.Tanusitvany_t2.Vonalkod,
                                          SzuloNyomtTipKod = v.Tanusitvany_t2.NyomtatvanyTipusKod,
                                          MunkaId = v.MunkaId,
                                          oid = v.Oid,
                                          FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                                          FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod),
                                          db = vkInDb.IsmertVk,
                                          nyomtatvanytipuskod = v.NyomtatvanyTipusKod
                                      }).ToList();

                                // teljes fa újraépítése
                                tvVonalkodok.Nodes.Clear();
                                tvVonalkodok.BeginUpdate();
                                // Dictionary<Tan,TreeNode> tn = new Dictionary<Tan,TreeNode>();

                                TreeNode tf = tvVonalkodok.Nodes.Add(t1.Vonalkod + " (" + NyTip[t1.nyomtatvanytipuskod] + ")");
                                VkTan.Add(t1.Vonalkod, t1);
                                VkTree.Add(t1.Vonalkod, tf);

                                foreach (Tan tl in t2)
                                {
                                    TreeNode tln = tf.Nodes.Add(tl.Vonalkod + " (" + NyTip[tl.nyomtatvanytipuskod] + ")");
                                    VkTan.Add(tl.Vonalkod, tl);
                                    VkTree.Add(tl.Vonalkod, tln);

                                    foreach (Tan te in (from t in t3
                                                        where t.SzuloId == tl.TanId
                                                        select new Tan()
                                                        {
                                                            Vonalkod = t.Vonalkod,
                                                            TanId = t.TanId,
                                                            SzuloId = t.SzuloId,
                                                            MunkaId = t.MunkaId,
                                                            oid = t.oid,
                                                            FotanId = t.FotanId,
                                                            FotanVk = t.FotanVk,
                                                            nyomtatvanytipuskod = t.nyomtatvanytipuskod
                                                        }))
                                    {
                                        VkTree.Add(te.Vonalkod, tln.Nodes.Add(te.Vonalkod + " (" + NyTip[te.nyomtatvanytipuskod] + ")"));
                                        VkTan.Add(te.Vonalkod, te);
                                    }
                                }

                                tvVonalkodok.EndUpdate();
                                tvVonalkodok.ExpandAll();
                                tbFo.Text = t1.Vonalkod;

                                if (ta.nyomtatvanytipuskod == "LAKASTANUSITVANY")
                                {
                                    tbLak.Text = ta.Vonalkod;
                                }
                                else if(ta.SzuloNyomtTipKod == "LAKASTANUSITVANY")
                                {
                                    tbLak.Text = ta.SzuloVk;
                                }

                                tvVonalkodok.SelectedNode = VkTree[ta.Vonalkod];
                                VkTree[ta.Vonalkod].EnsureVisible();
                                
                            }
                        }
                        #endregion
                        else
                        #region Van alapértelmezett főtanúsítvány
                        {
                            if (t1.Vonalkod == tbVk.Text || t2.Find(x => x.Vonalkod == tbVk.Text) != null || t3.Find(y => y.Vonalkod == tbVk.Text) != null || tbVk.Text == "99")
                            #region Memóriában szerepel a beolvasott vonalkód vagy kétszer zárt (99)
                            {
                                if (tbVk.Text == "99")
                                {
                                    MessageBox.Show("Kétszer zárt");
                                }
                                else
                                {
                                    switch (VkTan[tbVk.Text].nyomtatvanytipuskod)
                                    {
                                        case "FOTANUSITVANY":
                                        {
                                            tbLak.Text = "";
                                            tvVonalkodok.SelectedNode = VkTree[tbVk.Text];
                                            VkTree[tbVk.Text].EnsureVisible();
                                            break;
                                        }
                                        case "LAKASTANUSITVANY":
                                        {
                                            tbLak.Text = tbVk.Text;
                                            tvVonalkodok.SelectedNode = VkTree[tbVk.Text];
                                            VkTree[tbVk.Text].EnsureVisible();
                                            break;
                                        }
                                        default:
                                        {
                                            var tp = t2.Find(x => x.TanId == VkTan[tbVk.Text].SzuloId);
                                            if(tp != null) // lakástanúsítvány a szülő
                                            {
                                                if(tp.Vonalkod == tbLak.Text)
                                                {
                                                    tvVonalkodok.SelectedNode = VkTree[tbVk.Text];
                                                    VkTree[tbVk.Text].EnsureVisible();
                                                }
                                                else
                                                {
                                                    if(MessageBox.Show("Biztos, hogy más lakáshoz tartozik a most beolvasott vonalkódú nyomtatvány?","Nyomtatványáthelyezés",MessageBoxButtons.YesNo) == DialogResult.Yes)
                                                    { 
                                                        // nem helyezhető át,ha már munkatargyahiba van hozzá rögzítve
                                                        // oid is módosítandó

                                                        VkTan[tbVk.Text].SzuloId = tbLak.Text == "" ? VkTan[tbFo.Text].TanId : VkTan[tbLak.Text].TanId;
                                                        VkTan[tbVk.Text].SzuloVk = tbLak.Text == "" ? VkTan[tbFo.Text].Vonalkod : VkTan[tbLak.Text].Vonalkod;
                                                        VkTan[tbVk.Text].SzuloNyomtTipKod = tbLak.Text == "" ? VkTan[tbFo.Text].nyomtatvanytipus : VkTan[tbLak.Text].nyomtatvanytipuskod;
                                                        VkTan[tbVk.Text].Modositott = true;

                                                        tvVonalkodok.BeginUpdate();
                                                        
                                                        VkTree[tbVk.Text].Remove();
                                                        VkTree.Remove(tbVk.Text);
                                                        TreeNode tln;
                                                        if(tbLak.Text == "")
                                                        {
                                                            // főtanúsítványhoz rendelés
                                                            tln = VkTree[tbFo.Text].Nodes.Add(VkTan[tbFo.Text].Vonalkod + " (" + NyTip[VkTan[tbFo.Text].nyomtatvanytipuskod] + ")");
                                                        }
                                                        else
                                                        {
                                                            // lakáshoz rendelés
                                                            tln = VkTree[tbLak.Text].Nodes.Add(VkTan[tbLak.Text].Vonalkod + " (" + NyTip[VkTan[tbLak.Text].nyomtatvanytipuskod] + ")");
                                                        }
                                                        
                                                        VkTree.Add(tbVk.Text,tln);

                                                        tvVonalkodok.SelectedNode = tln;
                                                        tvVonalkodok.SelectedNode.EnsureVisible();

                                                        tvVonalkodok.EndUpdate();
                                                    }
                                                }
                                            }

                                            if(tp == null && t1.TanId == VkTan[tbVk.Text].SzuloId)
                                            {
                                                // ez még megírandó
                                                // tp = t1.Find(x => x.TanId == VkTan[tbVk.Text].SzuloId);
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                            #endregion
                            else
                            #region Memóriában nem szereplő vonalkód
                            {
                                Tan tcheck = (from v in ke.Tanusitvany_t
                                              where v.Vonalkod == tbVk.Text
                                              select new Tan()
                                              {
                                                  Vonalkod = v.Vonalkod,
                                                  TanId = v.TanusitvanyId,
                                                  SzuloId = v.SzulotanusitvanyId,
                                                  MunkaId = v.MunkaId,
                                                  oid = v.Oid,
                                                  FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                                                  FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod),
                                                  nyomtatvanytipuskod = v.NyomtatvanyTipusKod
                                              }).FirstOrDefault();
                                if (tcheck == null)
                                #region Sem adatbázisban, sem memóriában nem szereplő vonalkód 
                                {
                                    switch (GetTanTipus(tbVk.Text))
                                    {
                                        case "FOTANUSITVANY":
                                        {
                                            break;
                                        }
                                        default:
                                        {
                                            break;
                                        }
                                    }
                                }
                                #endregion
                                else
                                #region Adatbázisban szereplő, memóriába nem beolvasott vonalkód (másik munkához tartozik)
                                {
                                    MessageBox.Show("Más munkához tartozó vonalkód");
                                }
                                #endregion
                            }
                            #endregion
                        }
                        #endregion
                    }
                }
                tbVk.Text = "";
                tbVk.Select();
            }
        }

        string GetTanTipus(string vonalkod)
        {
            string nyt = (from v in vktart
                       where string.Compare(vonalkod, v.Eleje) >= 0 && string.Compare(vonalkod, v.Vege) <= 0
                       select new VonalkodTartomany() { NyomtatvanyTipusKod = v.NyomtatvanyTipusKod}).FirstOrDefault().NyomtatvanyTipusKod;
            return nyt == null ? "Ismeretlen" : nyt;
        }
    }
    class Tan
    {
        public string Vonalkod;
        public int TanId;
        public int? SzuloId;
        public string SzuloVk;
        public string SzuloNyomtTipKod;
        public int? MunkaId;
        public int? oid;
        public int? FotanId;
        public string FotanVk;
        public string lepcsohaz;
        public string emeletjel;
        public string emelet;
        public string ajto;
        public string ajtotores;
        public string nyomtatvanytipuskod;
        public string nyomtatvanytipus;
        public vkInDb db;
        public string epuletcim;
        public bool Ketszerzart;
        public bool Modositott;
    }

    class VonalkodTartomany
    {
        public string NyomtatvanyTipusKod;
        public string Eleje;
        public string Vege;
        // public string NyomtatvanyNev;
    }

    enum vkInDb
    {
        UjVk,
        IsmertVk   
    }
}
