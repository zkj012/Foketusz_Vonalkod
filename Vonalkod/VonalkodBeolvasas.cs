using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/* További teendők
    törlés (ha törölhető és nincs alatta más (vagy más nem törölhető? Ez utóbbi esetben az alatta levőek törlése is)
    törlés vonalkód megjelenítése: főtanúsítványnál nincs; alatta levőek kezelése
    Új főtanúsítvány:munkához rendelés
    Kétszer zárt lakások kezelése (beolvasáskor, íráskor)
    Munkatárgyak kezelése
    Hibák kezelése (zárt lakás, kétszer zárt)
    Auditlog írása
    Lakástanúsítványok lakáshoz rendelése (kiválasztás, módosítás; lakásszám megerősítés)
    Épületbesorolás megjelenítése, cseréje, ha szükséges
    Mentés nélküli továbblépés
*/


namespace Vonalkod
{

    public partial class VonalkodBeolvasas : Form
    {
        List<VonalkodTartomany> vktart;
        Dictionary<string, string> NyTip = new Dictionary<string, string>();
        Dictionary<string, TreeNode> VkTree = new Dictionary<string, TreeNode>();
        Dictionary<string, Tan> VkTan = new Dictionary<string, Tan>();
        Tan ta;
        Tan t1;
        List<Tan> t2;
        List<Tan> t3;
        List<Lakas> Lakasok;
        const string KetszerZartSzoveg = "; Kétszer zárt";
        Color KetszerzartBetuszin = Color.DarkOrchid;

        public VonalkodBeolvasas()
        {
            InitializeComponent();
        }

        private void VonalkodBeolvasas_Load(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;
                using (KotorEntities ke = new KotorEntities())
                {
                    vktart = (from v in ke.VonalkodTartomany_t
                              orderby v.Eleje
                              select new VonalkodTartomany()
                              {
                                  NyomtatvanyTipusKod = v.NyomtatvanytipusKod,
                                  Eleje = v.Eleje,
                                  Vege = v.Vege
                              }).ToList();

                    /*                foreach(var w in vktart)
                                    {
                                        Console.WriteLine("{0},{1},{2}", w.Eleje, w.Vege, w.NyomtatvanyTipusKod);
                                    }
                    */
                    foreach (var v in (from n in ke.NyomtatvanyTipus_m select new { n.Kod, n.Nev }).ToList())
                    {
                        NyTip.Add(v.Kod, v.Nev);
                    }
                }

                lblUser.Text = LoginHelper.LoggedOnUser.username + " (" + LoginHelper.LoggedOnUser.regionev + ")";

                chkKemenysepro.Checked = LoginHelper.LoggedOnUser.kemenysepro;

                tvVonalkodok.HideSelection = false;
                tvVonalkodok.DrawMode = TreeViewDrawMode.OwnerDrawText;
                tvVonalkodok.DrawNode += (o, ev) =>
                {
                    if (!ev.Node.TreeView.Focused && ev.Node == ev.Node.TreeView.SelectedNode)
                    {
                        Font treeFont = ev.Node.NodeFont ?? ev.Node.TreeView.Font;
                        // Font treeFont = ev.Node.NodeFont;
                        ev.Graphics.FillRectangle(Brushes.DodgerBlue, ev.Bounds);
                        ControlPaint.DrawFocusRectangle(ev.Graphics, ev.Bounds, SystemColors.HighlightText, SystemColors.Highlight);
                        //TextRenderer.DrawText(ev.Graphics, ev.Node.Text, treeFont, ev.Bounds, SystemColors.HighlightText, TextFormatFlags.GlyphOverhangPadding);
                        TextRenderer.DrawText(ev.Graphics, ev.Node.Text, treeFont, ev.Bounds, ev.Node.ForeColor, TextFormatFlags.GlyphOverhangPadding);
                    }
                    else
                        ev.DrawDefault = true;
                };
            }
            finally
            {
                this.Enabled = true;
            }
        }

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

        private void tbVk_Leave(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;
                if (tbVk.Text != "")
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Console.WriteLine("Vonalkódolvasás: {0} (időpont: {1})", tbVk.Text, DateTime.Now.ToString("HH:mm:ss tt"));

                    using (KotorEntities ke = new KotorEntities())
                    {
                        if(GetTanTipus(tbVk.Text) != "TORLES" && GetTanTipus(tbVk.Text) != "KETSZERZART")
                        {
                            tbLastVk.Text = tbVk.Text;
                        }

                        if (GetTanTipus(tbVk.Text) == "Ismeretlen")
                        {
                            MessageBox.Show("Ismeretlen nyomtatványtípus, érvénytelen vonalkód");
                        }
                        else if (tbFo.Text == "")
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
                                      nyomtatvanytipuskod = v.NyomtatvanyTipusKod,
                                      nyomtatvanytipus = (from x in ke.NyomtatvanyTipus_m where x.Kod == v.NyomtatvanyTipusKod select new { x.Nev }).FirstOrDefault().Nev,
                                      NemMozgathato = (from w in ke.MunkaTargyaHiba_t where w.FigyelmeztetoVonalkod == v.Vonalkod select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0,
                                      KetszerzartRogzitve = (from w in ke.MunkaTargyaHiba_t where w.FigyelmeztetoVonalkod == v.Vonalkod && w.HibaTipusKod.Substring(0,1) == "8" select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0
                                  }).FirstOrDefault();
                            if(ta != null)
                            {
                                ta.Ketszerzart = ta.KetszerzartRogzitve;
                            }

                            if ((ta == null || ta.FotanId == null) && GetTanTipus(tbVk.Text) == "FOTANUSITVANY")
                            {
                                MessageBox.Show("Új, adatbázisban nem szereplő főtanúsítvány (adatbázisba illesztés)");
                                // címet kell beírni, ha kéménysöprő; ha adminisztrátor, akkor munkához kell, hogy rendelje.

                                if(chkKemenysepro.Checked)
                                {
                                    CimInput cimfrm = new CimInput();
                                    cimfrm.ShowDialog(this);
                                    tbCim.Text = CimHelper.CimSzovegKemenysepro;
                                }
                                else
                                {
                                    MessageBox.Show("A tanúsítványt először munkához kell csatolni a Kotor Munka/Vonalkodbeolvasas menüpontjában!");
                                }

                                ke.Tanusitvany_t.Add(new Tanusitvany_t()
                                {
                                    Vonalkod = tbVk.Text,
                                    NyomtatvanyTipusKod = GetTanTipus(tbVk.Text),
                                    BeolvasasIdopontja = System.DateTime.Now,
                                    Megjegyzes = "Cím: "+tbCim.Text, // kéményseprőmód esetén beírt cím, egyébként kiválasztott munka
                                    kacs_Megjegyzes = "Vonalkódolvasás-kéményseprő mód",
                                    Technikai = true
                                });
                                //try { ke.SaveChanges(); } catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                                ke.SaveChanges();

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
                                          nyomtatvanytipuskod = v.NyomtatvanyTipusKod,
                                          nyomtatvanytipus = (from x in ke.NyomtatvanyTipus_m where x.Kod == v.NyomtatvanyTipusKod select new { x.Nev }).FirstOrDefault().Nev,
                                          NemMozgathato = (from w in ke.MunkaTargyaHiba_t where w.FigyelmeztetoVonalkod == v.Vonalkod select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0,
                                          KetszerzartRogzitve = (from w in ke.MunkaTargyaHiba_t where w.FigyelmeztetoVonalkod == v.Vonalkod && w.HibaTipusKod.Substring(0,1) == "8" select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0
                                      }).FirstOrDefault();
                                if (ta != null)
                                {
                                    ta.Ketszerzart = ta.KetszerzartRogzitve;
                                }
                            }

                            if ((ta == null || ta.FotanId == null) && GetTanTipus(tbVk.Text) != "FOTANUSITVANY")
                            {
                                MessageBox.Show("Nincs aktuális főtanúsítvány; először főtanúsítványt kell beolvasni!!!");
                                // új vonalkódot kell beolvasni, ha ide elért
                            }
                            else
                            {
                                #region Adatstruktúra fölépítése a memóriában
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
                                          Sor1 = v.Munka_t.MunkaVegezesKezdes,
                                          Sor2 = v.Munka_t.MunkaVegezesKezdesSor2,
                                          oid = v.Oid,
                                          FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                                          FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod),
                                          db = vkInDb.IsmertVk,
                                          nyomtatvanytipuskod = v.NyomtatvanyTipusKod,
                                          nyomtatvanytipus = (from x in ke.NyomtatvanyTipus_m where x.Kod == v.NyomtatvanyTipusKod select new { x.Nev }).FirstOrDefault().Nev,
                                          KetszerzartRogzitve = (from w in ke.MunkaTargyaHiba_t where w.MunkaTargya_cs.MunkaId == v.MunkaId && w.MunkaTargya_cs.Eid == v.Munka_t.Rendeles_t.Eid && w.HibaTipusKod.Substring(0, 1) == "8" select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0,
                                          NemMozgathato = (from w in ke.MunkaTargyaHiba_t where w.FigyelmeztetoVonalkod == v.Vonalkod select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0,
                                          NemTorolheto = (from m in ke.MunkaTargya_cs where m.TanusitvanyId == ta.FotanId select new {vx = m.TanusitvanyId }).Count() != 0,
                                          lepcsohaz = v.Oid == null ? null : (from w in ke.ORE_t where w.Oid == v.Oid select new { lh = w.Lepcsohaz }).FirstOrDefault().lh,
                                          emelet = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select new { em = w.Emelet }).FirstOrDefault().em,
                                          emeletjel = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select new { emj = w.EmeletJelKod }).FirstOrDefault().emj,
                                          ajto = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select new { aj = w.Ajto }).FirstOrDefault().aj,
                                          ajtotores = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select new { ajt = w.Ajtotores }).FirstOrDefault().ajt
                                      }).FirstOrDefault();

                                if (t1.MunkaId != null)
                                {
                                    // rendelésszám, cím (munka->rendelés->eid->cím)
                                    var rend = (from v in ke.Munka_t
                                                where (v.MunkaId == t1.MunkaId)
                                                select new { v.RendelesId, v.Rendeles_t.Rendelesszam, v.Rendeles_t.RendelesStatuszKod, v.Rendeles_t.RendelesStatusz_m.Nev, v.Rendeles_t.Eid }).FirstOrDefault();
                                    tbRendeles.Text = (rend == null ? "" : rend.Rendelesszam + " (" + rend.Nev + ")");

                                    var epcim = (from v in ke.vEpuletElsCim where v.Eid == rend.Eid select new { v.CimSzoveg }).FirstOrDefault();
                                    tbCim.Text = (epcim == null ? "" : epcim.CimSzoveg);

                                    // lakáslista kitöltése
                                    Lakasok = (from o in ke.ORE_t
                                               where o.Eid == rend.Eid
                                               select new Lakas()
                                               {
                                                   oid = o.Oid,
                                                   lepcsohaz = o.Lepcsohaz,
                                                   emelet = o.Emelet,
                                                   emeletjelkod = o.EmeletJelKod,
                                                   ajto = o.Ajto,
                                                   ajtotores = o.Ajtotores,
                                                   torolve = o.Torolve,
                                                   megjegyzes = o.Megjegyzes
                                               }).ToList();

                                    if (t1.Sor1 != null)
                                    {
                                        lblSor1.Visible = true;
                                        tbSor1.Visible = true;
                                        tbSor1.Text = string.Format("{0:yyyy.MM.dd}", t1.Sor1);
                                    }

                                    if (t1.Sor2 != null)
                                    {
                                        lblSor2.Visible = true;
                                        tbSor2.Visible = true;
                                        tbSor2.Text = string.Format("{0:yyyy.MM.dd}", t1.Sor2);
                                        lbl99.Visible = true;
                                        pb99.Visible = true;
                                    }
                                    else if (chkSor2.Checked == true)
                                    {
                                        lbl99.Visible = true;
                                        pb99.Visible = true;
                                    }
                                }


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
                                          nyomtatvanytipuskod = v.NyomtatvanyTipusKod,
                                          nyomtatvanytipus = (from x in ke.NyomtatvanyTipus_m where x.Kod == v.NyomtatvanyTipusKod select new { x.Nev }).FirstOrDefault().Nev,
                                          NemMozgathato = (from w in ke.MunkaTargyaHiba_t where w.FigyelmeztetoVonalkod == v.Vonalkod select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0,
                                          NemTorolheto = (from m in ke.MunkaTargya_cs where m.TanusitvanyId == v.TanusitvanyId select new { vx = m.TanusitvanyId }).Count() != 0,
                                          KetszerzartRogzitve = (from w in ke.MunkaTargyaHiba_t where w.MunkaTargya_cs.MunkaId == v.MunkaId && w.MunkaTargya_cs.Oid == v.Oid && w.HibaTipusKod.Substring(0, 1) == "8" select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0
                                      }).ToList();
                                // ORE adatok kitöltése a t2-ben
                                foreach (Tan t in t2)
                                {
                                    t.Ketszerzart = t.KetszerzartRogzitve;
                                    if (t.oid != null)
                                    {
                                        Lakas l = (from x in Lakasok where x.oid == t.oid select new Lakas() { lepcsohaz = x.lepcsohaz, emelet = t.emelet, emeletjelkod = t.emeletjel, ajto = x.ajto, ajtotores = x.ajtotores }).FirstOrDefault();
                                        if (l != null)
                                        {
                                            t.lepcsohaz = l.lepcsohaz;
                                            t.emelet = l.emelet;
                                            t.emeletjel = l.emeletjelkod;
                                            t.ajto = l.ajto;
                                            t.ajtotores = l.ajtotores;
                                        }
                                    }
                                }

                                // a későbbi hozzáadások miatt a Lakasok lista vonalkód mezőit is ki kell tölteni
                                if (Lakasok != null)
                                {
                                    foreach (Lakas l in Lakasok)
                                    {
                                        l.vonalkod = (from t in t2 where t.oid == l.oid && t.nyomtatvanytipuskod == "LAKASTANUSITVANY" select new { t.Vonalkod }).FirstOrDefault() == null ? "" : (from t in t2 where t.oid == l.oid select new { t.Vonalkod }).FirstOrDefault().ToString();
                                    }
                                }

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
                                          nyomtatvanytipuskod = v.NyomtatvanyTipusKod,
                                          nyomtatvanytipus = (from x in ke.NyomtatvanyTipus_m where x.Kod == v.NyomtatvanyTipusKod select new { x.Nev }).FirstOrDefault().Nev,
                                          NemMozgathato = (from w in ke.MunkaTargyaHiba_t where w.FigyelmeztetoVonalkod == v.Vonalkod select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0,
                                          NemTorolheto = (from m in ke.MunkaTargya_cs where m.TanusitvanyId == v.TanusitvanyId select new { vx = m.TanusitvanyId }).Count() != 0
                                      }).ToList();

                                #region Treeview teljes fölépítése    
                                // teljes fa újraépítése
                                tvVonalkodok.Nodes.Clear();
                                tvVonalkodok.BeginUpdate();
                                // Dictionary<Tan,TreeNode> tn = new Dictionary<Tan,TreeNode>();

                                TreeNode tf = tvVonalkodok.Nodes.Add(t1.Vonalkod + " (" + NyTip[t1.nyomtatvanytipuskod] + ")" + (t1.oid == null ? "" : " " + getLakCim(t1.lepcsohaz, t1.emelet, t1.ajto, t1.ajtotores)) + (t1.Ketszerzart == true ? KetszerZartSzoveg : ""));
                                tf.ForeColor = (t1.Ketszerzart == true ? KetszerzartBetuszin : Color.Empty);

                                VkTan.Add(t1.Vonalkod, t1);
                                VkTree.Add(t1.Vonalkod, tf);

                                foreach (Tan tl in t2)
                                {
                                    TreeNode tln = tf.Nodes.Add(tl.Vonalkod + " (" + NyTip[tl.nyomtatvanytipuskod] + ")" + (tl.oid == null ? "" : " " + getLakCim(tl.lepcsohaz, tl.emelet, tl.ajto, tl.ajtotores)) + (tl.Ketszerzart == true ? KetszerZartSzoveg : ""));
                                    tln.ForeColor = (tl.Ketszerzart == true ? KetszerzartBetuszin : Color.Empty);

                                    VkTan.Add(tl.Vonalkod, tl);
                                    VkTree.Add(tl.Vonalkod, tln);

                                    foreach (Tan te in (from t in t3
                                                        where t.SzuloId == tl.TanId
                                                        select t))
                                    {
                                        VkTree.Add(te.Vonalkod, tln.Nodes.Add(te.Vonalkod + " (" + NyTip[te.nyomtatvanytipuskod] + ")" + (te.oid == null ? "" : " " + getLakCim(t1.lepcsohaz, t1.emelet, t1.ajto, t1.ajtotores))));
                                        VkTan.Add(te.Vonalkod, te);
                                    }
                                }

                                tbFo.Text = t1.Vonalkod;

                                if (ta.nyomtatvanytipuskod == "LAKASTANUSITVANY")
                                {
                                    tbLak.Text = ta.Vonalkod;
                                }
                                else if (ta.SzuloNyomtTipKod == "LAKASTANUSITVANY")
                                {
                                    tbLak.Text = ta.SzuloVk;
                                }

                                tvVonalkodok.ExpandAll();
                                tvVonalkodok.SelectedNode = VkTree[ta.Vonalkod];
                                tvVonalkodok.SelectedNode.EnsureVisible();

                                tvVonalkodok.EndUpdate();
                                #endregion
                                #endregion
                            }
                        }
                        #endregion
                        else
                        #region Van alapértelmezett főtanúsítvány
                        {
                            if (t1.Vonalkod == tbVk.Text || t2.Find(x => x.Vonalkod == tbVk.Text) != null || t3.Find(y => y.Vonalkod == tbVk.Text) != null || GetTanTipus(tbVk.Text) == "KETSZERZART" || GetTanTipus(tbVk.Text) == "TORLES")
                            #region Memóriában szerepel a beolvasott vonalkód vagy kétszer zárt (99, 9999) vagy törlés (0000)
                            {
                                switch (GetTanTipus(tbVk.Text))
                                {
                                    case "KETSZERZART":
                                    {
                                        if(tbLak.Text != "")
                                        {
                                            var kz = t2.Find(x => x.Vonalkod == tbLak.Text);
                                            var ml = t3.Find(y => y.SzuloVk == kz.Vonalkod && y.nyomtatvanytipuskod == "ZARTLAKASERTESITO");

                                            if (kz.KetszerzartRogzitve == false) // csak akkor állítható, ha még nincs az adatbázisban rögzítve a kétszer zárt állapot
                                            {
                                                if(!kz.Ketszerzart && ml != null) // eddig nem volt kétszer zárt->csak második látogatás értesítővel együtt lehet kétszer zárt
                                                {
                                                    kz.Ketszerzart = !kz.Ketszerzart;
                                                }
                                                else if(kz.Ketszerzart) // kétszer zárt állapotot lehet törölni
                                                {
                                                    kz.Ketszerzart = !kz.Ketszerzart;
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Nem lehet kétszer zárt, ha nincs második értesítő!");
                                                }

                                                VkTan[tbLak.Text].Ketszerzart = kz.Ketszerzart;

                                                tvVonalkodok.BeginUpdate();
                                                if (kz.Ketszerzart)
                                                {
                                                    VkTree[tbLak.Text].Text = VkTree[tbLak.Text].Text + KetszerZartSzoveg;
                                                    VkTree[tbLak.Text].ForeColor = KetszerzartBetuszin;
                                                }
                                                else
                                                {
                                                    VkTree[tbLak.Text].Text = VkTree[tbLak.Text].Text.Replace(KetszerZartSzoveg, "");
                                                    VkTree[tbLak.Text].ForeColor = Color.Empty;
                                                }
                                                tvVonalkodok.EndUpdate();
                                            }
                                            else
                                            {
                                                MessageBox.Show("A kétszer zárt állapot a munkatárgyánál rögzített, innen nem lehet megszüntetni (a 8-as hibát kell törölni)");
                                            }
                                        }
                                        else // főtanúsítvány-épület kétszer zárt
                                        {
                                            if(t1.KetszerzartRogzitve == false)
                                            {
                                                var ml = t2.Find(y => y.SzuloVk == tbFo.Text && y.nyomtatvanytipuskod == "ZARTLAKASERTESITO");

                                                if (!VkTan[tbFo.Text].Ketszerzart && ml != null) // eddig nem volkt készr zárt->csak második látogatás értesítővel együtt lehet kétszer zárt
                                                {
                                                    VkTan[tbFo.Text].Ketszerzart = !VkTan[tbFo.Text].Ketszerzart;
                                                }
                                                else if (VkTan[tbFo.Text].Ketszerzart) // kétszer zárt állapotot lehet törölni
                                                {
                                                    VkTan[tbFo.Text].Ketszerzart = !VkTan[tbFo.Text].Ketszerzart;
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Nem lehet kétszer zárt, ha nincs második értesítő!");
                                                }

                                                tvVonalkodok.BeginUpdate();
                                                if (VkTan[tbFo.Text].Ketszerzart)
                                                {
                                                    VkTree[tbFo.Text].Text = VkTree[tbFo.Text].Text + KetszerZartSzoveg;
                                                    VkTree[tbFo.Text].ForeColor = KetszerzartBetuszin;
                                                }
                                                else
                                                {
                                                    VkTree[tbFo.Text].Text = VkTree[tbFo.Text].Text.Replace(KetszerZartSzoveg, "");
                                                    VkTree[tbFo.Text].ForeColor = Color.Empty;
                                                }
                                                tvVonalkodok.EndUpdate();
                                            }
                                        }
                                        break;
                                    }
                                    case "TORLES":
                                    {
                                        string SelectedVonalkod = VkTree.FirstOrDefault(x => x.Value == tvVonalkodok.SelectedNode).Key;

                                        var ch = (from Tan v in VkTan.Values where v.SzuloVk == VkTan[SelectedVonalkod].Vonalkod && (v.NemTorolheto == true || v.NemMozgathato == true) select v);
                                        if (VkTan[SelectedVonalkod].NemMozgathato == false && VkTan[SelectedVonalkod].NemTorolheto == false && ch.Count() == 0)
                                        {
                                            ch = (from Tan v in VkTan.Values where v.SzuloVk == VkTan[SelectedVonalkod].Vonalkod select v).ToList();
                                            foreach (Tan x in ch)
                                            {
                                                if (x.db == vkInDb.IsmertVk)
                                                {
                                                    MessageBox.Show(string.Format("Adatbázisból törlendő tanúsítvány: {0} ({1})", x.Vonalkod, x.TanId.ToString()));
                                                }
                                                else
                                                {
                                                    t3.Remove(t3.Find(r => r.Vonalkod == x.Vonalkod));
                                                    t2.Remove(t2.Find(r => r.Vonalkod == x.Vonalkod));

                                                    tvVonalkodok.BeginUpdate();
                                                    tvVonalkodok.Nodes.Remove(VkTree[x.Vonalkod]);
                                                    tvVonalkodok.EndUpdate();

                                                    VkTree.Remove(x.Vonalkod);
                                                    VkTan[x.Vonalkod].Torlendo = true;
                                                    VkTan.Remove(x.Vonalkod);
                                                }
                                            }

                                            if (VkTan[SelectedVonalkod].db == vkInDb.IsmertVk)
                                            {
                                                MessageBox.Show(string.Format("Adatbázisból törlendő tanúsítvány: {0} ({1})", VkTan[tbLastVk.Text].Vonalkod, VkTan[tbLastVk.Text].TanId.ToString()));
                                            }
                                            else
                                            {
                                                t3.Remove(t3.Find(r => r.Vonalkod == tbLastVk.Text));
                                                t2.Remove(t2.Find(r => r.Vonalkod == tbLastVk.Text));

                                                tvVonalkodok.BeginUpdate();
                                                tvVonalkodok.Nodes.Remove(VkTree[SelectedVonalkod]);
                                                tvVonalkodok.EndUpdate();

                                                VkTree.Remove(SelectedVonalkod);
                                                VkTan[SelectedVonalkod].Torlendo = true;
                                                VkTan.Remove(SelectedVonalkod);
                                            }
                                        }
                                        break;
                                    }

                                    case "FOTANUSITVANY":
                                    {
                                        tbLak.Text = "";
                                        tvVonalkodok.SelectedNode = VkTree[tbVk.Text];
                                        tvVonalkodok.SelectedNode.EnsureVisible();
                                        break;
                                    }

                                    case "LAKASTANUSITVANY":
                                    {
                                        tvVonalkodok.BeginUpdate();
                                        tbLak.Text = tbVk.Text;
                                        tvVonalkodok.SelectedNode = VkTree[tbVk.Text];
                                        tvVonalkodok.SelectedNode.EnsureVisible();
                                        tvVonalkodok.EndUpdate();
                                        break;
                                    }

                                    default:
                                    {
                                        var tp = t2.Find(x => x.Vonalkod == VkTan[tbVk.Text].SzuloVk);
                                        if (tp != null) // lakástanúsítvány a szülő
                                        {
                                            if (tp.Vonalkod == tbLak.Text)
                                            {
                                                tvVonalkodok.BeginUpdate();
                                                tvVonalkodok.SelectedNode = VkTree[tbVk.Text];
                                                tvVonalkodok.SelectedNode.EnsureVisible();
                                                tvVonalkodok.EndUpdate();
                                            }
                                            else
                                            {
                                                if (VkTan[tbVk.Text].NemMozgathato == true)
                                                {
                                                    MessageBox.Show("A " + tbVk.Text + " számú " + NyTip[VkTan[tbVk.Text].nyomtatvanytipuskod] + " nem mozgatható, van hozzá rögzített hiba");
                                                }
                                                else
                                                {
                                                    if (MessageBox.Show("Biztos, hogy " + (tbLak.Text == "" ? "az épülethez" : "más lakáshoz") + " tartozik a most beolvasott vonalkódú nyomtatvány?", "Nyomtatványáthelyezés", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                                    {
                                                        VkTan[tbVk.Text].SzuloId = tbLak.Text == "" ? VkTan[tbFo.Text].TanId : VkTan[tbLak.Text].TanId;
                                                        VkTan[tbVk.Text].SzuloVk = tbLak.Text == "" ? VkTan[tbFo.Text].Vonalkod : VkTan[tbLak.Text].Vonalkod;
                                                        VkTan[tbVk.Text].SzuloNyomtTipKod = tbLak.Text == "" ? VkTan[tbFo.Text].nyomtatvanytipuskod : VkTan[tbLak.Text].nyomtatvanytipuskod;
                                                        VkTan[tbVk.Text].SzuloIdModositott = true;
                                                        VkTan[tbVk.Text].oid = tbLak.Text == "" ? VkTan[tbFo.Text].oid : VkTan[tbLak.Text].oid;

                                                        if (tbLak.Text == "") // lakás->főtanúsítvány mozgatás
                                                        {
                                                            t2.Add(VkTan[tbVk.Text]);
                                                            t3.Remove(t3.Find(r => r.Vonalkod == tbVk.Text));
                                                        }
                                                        else // más lakáshoz tartozik, nincs szintváltás
                                                        {
                                                            Tan v = (from w in t3 where w.Vonalkod == tbVk.Text select w).FirstOrDefault();
                                                            if(v!=null)
                                                            {
                                                                v.SzuloId = tbLak.Text == "" ? VkTan[tbFo.Text].TanId : VkTan[tbLak.Text].TanId;
                                                                v.SzuloIdModositott = true;
                                                            }
                                                        }

                                                        tvVonalkodok.BeginUpdate();

                                                        VkTree[tbVk.Text].Remove();
                                                        VkTree.Remove(tbVk.Text);
                                                        TreeNode tln = VkTree[tbLak.Text == "" ? tbFo.Text : tbLak.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (" + NyTip[VkTan[tbVk.Text].nyomtatvanytipuskod] + ")");
                                                        VkTree.Add(tbVk.Text, tln);

                                                        tvVonalkodok.SelectedNode = tln;
                                                        tvVonalkodok.SelectedNode.EnsureVisible();

                                                        tvVonalkodok.EndUpdate();
                                                    }
                                                }
                                            }
                                        }
                                        else if (tp == null && t1.TanId == VkTan[tbVk.Text].SzuloId && tbLak.Text != "") // főtanúsítvány->lakástanúsítvány mozgatás
                                        {
                                            if(VkTan[tbVk.Text].NemMozgathato)
                                            {
                                                MessageBox.Show(string.Format("A {0} nem mozgatható, a munkatárgyaknál rögzített hiba van.", GetTanTipus(tbVk.Text)));
                                            }

                                            else if (MessageBox.Show("Biztos, hogy " + (tbLak.Text == "" ? "az épülethez" : "más lakáshoz") + " tartozik a most beolvasott vonalkódú nyomtatvány?", "Nyomtatványáthelyezés", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                            {
                                                VkTan[tbVk.Text].SzuloId = VkTan[tbLak.Text].TanId;
                                                VkTan[tbVk.Text].SzuloVk = VkTan[tbLak.Text].Vonalkod;
                                                VkTan[tbVk.Text].SzuloNyomtTipKod = VkTan[tbLak.Text].nyomtatvanytipuskod;
                                                VkTan[tbVk.Text].SzuloIdModositott = true;
                                                VkTan[tbVk.Text].oid = VkTan[tbLak.Text].oid;

                                                t3.Add(VkTan[tbVk.Text]);
                                                t2.Remove(t2.Find(r => r.Vonalkod == tbVk.Text));

                                                tvVonalkodok.BeginUpdate();

                                                VkTree[tbVk.Text].Remove();
                                                VkTree.Remove(tbVk.Text);
                                                TreeNode tln = VkTree[tbLak.Text == "" ? tbFo.Text : tbLak.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (" + NyTip[VkTan[tbVk.Text].nyomtatvanytipuskod] + ")");
                                                VkTree.Add(tbVk.Text, tln);

                                                tvVonalkodok.SelectedNode = tln;
                                                tvVonalkodok.SelectedNode.EnsureVisible();

                                                tvVonalkodok.EndUpdate();
                                            }
                                        }

                                        break;
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
                                            MessageBox.Show("Új főtanúsítvány, az előző rögzítését be kell fejezni");
                                            break;
                                        }
                                        case "LAKASTANUSITVANY":
                                        {
                                            Tan tluj = new Tan();
                                            tluj.Vonalkod = tbVk.Text;
                                            tluj.SzuloId = t1.TanId;
                                            tluj.SzuloVk = t1.Vonalkod;
                                            tluj.SzuloNyomtTipKod = t1.nyomtatvanytipuskod;
                                            tluj.MunkaId = t1.MunkaId;
                                            tluj.FotanId = t1.TanId;
                                            tluj.FotanVk = t1.Vonalkod;
                                            tluj.nyomtatvanytipuskod = "LAKASTANUSITVANY";
                                            tluj.db = vkInDb.UjVk;
                                            tluj.Sor1 = t1.Sor1;
                                            tluj.Sor2 = t1.Sor2;
                                            tluj.BeolvasasIdopontja = DateTime.Now;

                                            tbLak.Text = tluj.Vonalkod;

                                            VkTan.Add(tluj.Vonalkod, tluj);
                                            t2.Add(tluj);

                                            tvVonalkodok.BeginUpdate();
                                            TreeNode tln = VkTree[tbFo.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (Új " + NyTip[tluj.nyomtatvanytipuskod] + ")");
                                            VkTree.Add(tbVk.Text, tln);

                                            tvVonalkodok.SelectedNode = tln;
                                            tvVonalkodok.SelectedNode.EnsureVisible();

                                            tvVonalkodok.EndUpdate();

                                            break;
                                        }

                                        default:
                                        {
                                            Tan tluj = new Tan();
                                            tluj.Vonalkod = tbVk.Text;

                                            if (tbLak.Text == "")
                                            {
                                                // főtanúsítványhoz kell hozzáadni

                                                tluj.SzuloId = t1.TanId;
                                                tluj.SzuloVk = t1.Vonalkod;
                                                tluj.SzuloNyomtTipKod = t1.nyomtatvanytipuskod;
                                                tluj.MunkaId = t1.MunkaId;
                                                tluj.FotanId = t1.TanId;
                                                tluj.FotanVk = t1.Vonalkod;
                                                tluj.nyomtatvanytipuskod = GetTanTipus(tbVk.Text);
                                                tluj.db = vkInDb.UjVk;
                                                tluj.Sor1 = t1.Sor1;
                                                tluj.Sor2 = t1.Sor2;
                                                tluj.BeolvasasIdopontja = DateTime.Now;

                                                //tbLak.Text = tluj.Vonalkod;

                                                VkTan.Add(tluj.Vonalkod, tluj);
                                                t2.Add(tluj);

                                                tvVonalkodok.BeginUpdate();

                                                TreeNode tln = VkTree[tbFo.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (Új " + NyTip[tluj.nyomtatvanytipuskod] + ")");
                                                VkTree.Add(tbVk.Text, tln);

                                                tvVonalkodok.SelectedNode = tln;
                                                tvVonalkodok.SelectedNode.EnsureVisible();

                                                tvVonalkodok.EndUpdate();
                                            }
                                            else
                                            {
                                                // lakástanúsítványhoz kell hozzáadni

                                                tluj.SzuloId = VkTan[tbLak.Text].TanId;
                                                tluj.SzuloVk = VkTan[tbLak.Text].Vonalkod;
                                                tluj.SzuloNyomtTipKod = VkTan[tbLak.Text].nyomtatvanytipuskod;
                                                tluj.MunkaId = VkTan[tbLak.Text].MunkaId;
                                                tluj.FotanId = VkTan[tbLak.Text].FotanId;
                                                tluj.FotanVk = VkTan[tbLak.Text].FotanVk;
                                                tluj.nyomtatvanytipuskod = GetTanTipus(tbVk.Text);
                                                tluj.db = vkInDb.UjVk;
                                                tluj.Sor1 = t1.Sor1;
                                                tluj.Sor2 = t1.Sor2;

                                                VkTan.Add(tluj.Vonalkod, tluj);
                                                t3.Add(tluj);

                                                tvVonalkodok.BeginUpdate();

                                                TreeNode tln = VkTree[tbLak.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (Új " + NyTip[tluj.nyomtatvanytipuskod] + ")");
                                                VkTree.Add(tbVk.Text, tln);

                                                tvVonalkodok.SelectedNode = tln;
                                                tvVonalkodok.SelectedNode.EnsureVisible();

                                                tvVonalkodok.EndUpdate();
                                            }
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
                Cursor.Current = Cursors.Default;
                tbVk.Text = "";
                tbVk.Select();
            }
            finally
            {
                this.Enabled = true;
            }
        }

        string GetTanTipus(string vonalkod)
        {
            string retval;

            switch (vonalkod)
            {
                case "99":
                case "9999":
                    {
                        retval = "KETSZERZART";
                        break;
                    }
                case "00":
                case "0000":
                    {
                        retval = "TORLES";
                        break;
                    }
                default:
                    {
                        var nyt = (from v in vktart
                                   where string.Compare(vonalkod, v.Eleje) >= 0 && string.Compare(vonalkod, v.Vege) <= 0 && vonalkod.Length == v.Eleje.Length && vonalkod.Length == v.Vege.Length
                                   select new VonalkodTartomany() { NyomtatvanyTipusKod = v.NyomtatvanyTipusKod }).FirstOrDefault();
                        retval = nyt == null ? "Ismeretlen" : nyt.NyomtatvanyTipusKod;
                        break;
                    }

            }
            return retval;
        }

        /*       public VonalkodTartomany GetVonalkodTartomanyByVonalkod(string vonalkod)
                {
                    if(string.IsNullOrEmpty(vonalkod) || vonalkod.Length < FixedMetadata.VK_PREFIX_HOSSZ) 
                        return null;

                    long vonalkodNumber = 0;
                    if(!long.TryParse(vonalkod, out vonalkodNumber))
                        return null;

                    VonalkodTartomany resultTartomany = null;
                    var tartomanyList = GetQuery(null).Where(m => m.Eleje.Length == vonalkod.Length && m.Vege.Length == vonalkod.Length && m.Eleje.Substring(0, FixedMetadata.VK_PREFIX_HOSSZ).Equals(vonalkod.Substring(0, FixedMetadata.VK_PREFIX_HOSSZ))).ToList();

                    foreach(var vonalkodTartomany in tartomanyList) {
                        long tartomanyStart = 0;
                        long tartomanyEnd = 0;

                        if(!long.TryParse(vonalkodTartomany.Eleje, out tartomanyStart) || !long.TryParse(vonalkodTartomany.Vege, out tartomanyEnd))
                            continue;

                        if (vonalkodNumber >= tartomanyStart && vonalkodNumber <= tartomanyEnd) {
                            resultTartomany = vonalkodTartomany;
                            break;
                        }
                    }

                    return resultTartomany;
                }
         */

        private void btnBeolvas_Click(object sender, EventArgs e)
            {
            try
            {
                this.Enabled = false;
                btnBeolvas.Select();
            }
            finally
            {
                this.Enabled = true;
            }
            tbVk.Select();
        }

        private void tvVonalkodok_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Console.Write("Treeview-Afterselect (selectednode: {0}; argumentnode: {1}) \n\r", tvVonalkodok.SelectedNode == null ? "" : tvVonalkodok.SelectedNode.Text, e.Node == null ? "" : e.Node.Text);
            e.Node.BackColor = System.Drawing.SystemColors.ButtonFace;
            string Vonalkod = VkTree.FirstOrDefault(x => x.Value == e.Node).Key;

            switch(GetTanTipus(Vonalkod))
            {
                case "FOTANUSITVANY":
                {
                    tbLak.Text = "";
                    tbLepcsohaz.Text = VkTan[Vonalkod].lepcsohaz;
                    tbEmelet.Text = VkTan[Vonalkod].emelet;
                    // tbEmeletjel.Text = VkTan[Vonalkod].emeletjel;
                    tbAjto.Text = VkTan[Vonalkod].ajto;
                    tbAjtotores.Text = VkTan[Vonalkod].ajtotores;
                    tbOid.Text = VkTan[Vonalkod].oid.ToString();
                    break;
                }
                case "LAKASTANUSITVANY":
                {
                    tbLak.Text = Vonalkod;
                    tbLepcsohaz.Text = VkTan[Vonalkod].lepcsohaz;
                    tbEmelet.Text = VkTan[Vonalkod].emelet;
                    // tbEmeletjel.Text = VkTan[Vonalkod].emeletjel;
                    tbAjto.Text = VkTan[Vonalkod].ajto;
                    tbAjtotores.Text = VkTan[Vonalkod].ajtotores;
                    tbOid.Text = VkTan[Vonalkod].oid.ToString();
                    break;
                }
                default:
                {
                    if(VkTan[Vonalkod].SzuloNyomtTipKod =="FOTANUSITVANY")
                    {
                        tbLak.Text = "";
                    }
                    else
                    {
                        tbLak.Text = VkTan[Vonalkod].SzuloVk;
                    }
                    tbLepcsohaz.Text = VkTan[VkTan[Vonalkod].SzuloVk].lepcsohaz;
                    tbEmelet.Text = VkTan[VkTan[Vonalkod].SzuloVk].emelet;
                    // tbEmeletjel.Text = VkTan[VkTan[Vonalkod].SzuloVk].emeletjel;
                    tbAjto.Text = VkTan[VkTan[Vonalkod].SzuloVk].ajto;
                    tbAjtotores.Text = VkTan[VkTan[Vonalkod].SzuloVk].ajtotores;
                    tbOid.Text = VkTan[VkTan[Vonalkod].SzuloVk].oid.ToString();
                    break;
                }
            }

            var ch = (from Tan v in VkTan.Values where v.SzuloVk == VkTan[Vonalkod].Vonalkod && (v.NemTorolheto == true || v.NemMozgathato == true) select v);
            if (VkTan[Vonalkod].NemMozgathato == false && VkTan[Vonalkod].NemTorolheto == false && ch.Count() == 0 && (GetTanTipus(Vonalkod) != "FOTANUSITVANY"))
            {
                lblTorles.Visible = true;
                pbTorles.Visible = true;
            }
            else
            {
                lblTorles.Visible = false;
                pbTorles.Visible = false;
            }

            if (VkTan[Vonalkod].KetszerzartRogzitve == true || ((t1.Sor2 == null) && (chkSor2.Checked != true)))
            {
                pb99.Visible = false;
                lbl99.Visible = false;
            }
            else
            {
                pb99.Visible = true;
                lbl99.Visible = true;
            }
        }

        private string getLakCim(string lepcsohaz, string emelet, string ajto, string ajtotores)
        {
            return (((lepcsohaz == "") || (lepcsohaz == null) ? "" : lepcsohaz + " lépcsőház ") + ((emelet == "") || (emelet == null) ? "" : emelet + " ") + ((ajto == "") || (ajto == null) ? "" : ajto + ((ajtotores == "") || (ajtotores == null) ? "" : "/")) + ((ajtotores == "") || (ajtotores == null) ? "" : ajtotores) + (((ajto != null) && (ajto != "")) || ((ajtotores != null) && (ajtotores != "")) ? ". ajtó" : "")).Trim();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;
                using (KotorEntities ke = new KotorEntities())
                {

                    if (t2 != null)
                    {
                        foreach (Tan v in (from w in t2 where w.db == vkInDb.UjVk select w))
                        {
                            ke.Tanusitvany_t.Add(new Tanusitvany_t()
                            {
                                MunkaId = v.MunkaId,
                                Technikai = v.MunkaId == null ? true : false,
                                Vonalkod = v.Vonalkod,
                                SzulotanusitvanyId = v.SzuloId,
                                NyomtatvanyTipusKod = v.nyomtatvanytipuskod,
                                Oid = v.oid,
                                BeolvasasIdopontja = v.BeolvasasIdopontja
                            });
                        }
                        ke.SaveChanges();


                        //foreach (Tan v in (from w in t2 where w.TanId == null select new Tan() { TanId = w.TanId, SzuloVk = w.SzuloVk, Vonalkod = w.Vonalkod })) // új lakástanúsítványok-TanId visszakeresése
                        foreach (Tan v in (from w in t2 where w.TanId == null select w)) // új lakástanúsítványok-TanId visszakeresése
                        {
                            Tan y = (from w in ke.Tanusitvany_t where w.Vonalkod == v.Vonalkod select new Tan() { TanId = w.TanusitvanyId }).FirstOrDefault();
                            v.TanId = y.TanId;
                        }

                        foreach (Tan v in (from w in t2 where w.SzuloVk != "" && w.SzuloId == null select w)) // hiányzó szuloid-k beírása
                        {
                            Tan y = (from w in ke.Tanusitvany_t where w.Vonalkod == v.SzuloVk select new Tan() { TanId = w.TanusitvanyId }).FirstOrDefault();
                            v.SzuloId = y.TanId;
                        }

                        foreach (Tan v in (from w in t2 where w.SzuloIdModositott == true select w))
                        {
                            var x = (from w in ke.Tanusitvany_t where w.TanusitvanyId == v.TanId select w).First();
                            x.SzulotanusitvanyId = v.SzuloId;
                        }
                    }

                    if (t3 != null)
                    {
                        foreach (Tan v in (from w in t3 where w.SzuloVk != "" && w.SzuloId == null select w)) // hiányzó szuloid-k beírása
                        {
                            Tan y = (from w in ke.Tanusitvany_t where w.Vonalkod == v.SzuloVk select new Tan() { TanId = w.TanusitvanyId }).FirstOrDefault();
                            v.SzuloId = y.TanId;
                        }

                        foreach (Tan v in (from w in t3 where w.db == vkInDb.UjVk select w))
                        {
                            ke.Tanusitvany_t.Add(new Tanusitvany_t()
                            {
                                MunkaId = v.MunkaId,
                                Vonalkod = v.Vonalkod,
                                Technikai = v.MunkaId == null ? true : false,
                                SzulotanusitvanyId = v.SzuloId,
                                NyomtatvanyTipusKod = v.nyomtatvanytipuskod,
                                Oid = v.oid,
                                BeolvasasIdopontja = v.BeolvasasIdopontja
                            });
                        }

                        foreach (Tan v in (from w in t3 where w.SzuloIdModositott == true select w))
                        {
                            var x = (from w in ke.Tanusitvany_t where w.TanusitvanyId == v.TanId select w).First();
                            x.SzulotanusitvanyId = v.SzuloId;
                        }
                        ke.SaveChanges();
                    }
                    
                    Utilities.ResetAllControls(this);

                    lbl99.Visible = false;
                    pb99.Visible = false;
                    lblTorles.Visible = false;
                    pbTorles.Visible = false;
                    VkTan.Clear();
                    VkTree.Clear();
                }
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private void chkSor2_CheckedChanged(object sender, EventArgs e)
        {
            pb99.Visible = chkSor2.Checked;
            lbl99.Visible = chkSor2.Checked;
        }

        private void btnLakasadat_Click(object sender, EventArgs e)
        {

        }

        private void chkEvesEllenorzes_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
