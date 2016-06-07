using System;
using System.Collections;
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
        Dictionary<String, String> NyTip = new Dictionary<String, String>();
        Dictionary<String, TreeNode> VkTree = new Dictionary<String, TreeNode>();
        Dictionary<String, Tan> VkTan = new Dictionary<String, Tan>();
        Dictionary<String, String> EmeletjelLista = new Dictionary<String, String>();
        Tan ta;
        Tan t1;
        List<Tan> t2;
        List<Tan> t3;
        List<Lakas> Lakasok;
        const string KetszerZartSzoveg = "; Kétszer zárt";
        Color KetszerzartBetuszin = Color.DarkOrchid;
        bool GridEnabledPreviousStatus;
        bool UjlakasPrevStatus;

        enum AdatBevitel
        {
            VonalkodOlvasas,
            LakasadatKereses,
            Lakasadatmodositas,
            Ujlakas
        }

        private readonly IComparer<Lakas> lakasComparer = new LakasComparer();

        AdatBevitel AdatbevitelStatusz; // a vonalkód mező fókusz visszatérést szabályozza pl. lakásadatfrissítéskor

        public VonalkodBeolvasas()
        {
            InitializeComponent();
        }

        private void VonalkodBeolvasas_Load(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;

                AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas;
                tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug

                chkKemenysepro.Checked = LoginHelper.LoggedOnUser.kemenysepro;

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

                    foreach (var v in (from n in ke.NyomtatvanyTipus_m select new { n.Kod, n.Nev }).ToList())
                    {
                        NyTip.Add(v.Kod, v.Nev);
                    }

                    EmeletjelLista.Add("<NA>", "<Nincs emelet>");
                    foreach (var w in (from em in ke.EmeletJel_m select new { em.Kod, em.RovidNev }))
                    {
                        EmeletjelLista.Add(w.Kod, w.RovidNev);
                    }

                    cbEmeletjel.DataSource = EmeletjelLista.ToList();
                    cbEmeletjel.DisplayMember = "Value";
                    cbEmeletjel.ValueMember = "Key";

                    btnLakasadatModositas.Enabled = !chkKemenysepro.Checked;
                    tbAjto.Enabled = !chkKemenysepro.Checked;
                    tbAjtotores.Enabled = !chkKemenysepro.Checked;
                    cbEmeletjel.Enabled = !chkKemenysepro.Checked;
                    tbLepcsohaz.Enabled = !chkKemenysepro.Checked;
                    btnLakasadatModositas.Enabled = !chkKemenysepro.Checked;
                    tbMegj.Enabled = !chkKemenysepro.Checked;
                    chkEvesEllenorzes.Enabled = !chkKemenysepro.Checked;
                    grLakasLista.Visible = false; // üresen nem látható
                    grLakasLista.Enabled = false; // választani nem lehet, ha van a vonalkódhoz oid
                    GridEnabledPreviousStatus = false;
                    grLakasLista.AutoGenerateColumns = false;
                    grLakasLista.MultiSelect = false;

                    grLakasLista.Columns.Add(new DataGridViewTextBoxColumn { Name = "lh", DataPropertyName = "lepcsohaz", HeaderText = "Lépcsőház", ReadOnly = true });
                    grLakasLista.Columns.Add(new DataGridViewTextBoxColumn { Name = "em", DataPropertyName = "emelet", HeaderText = "Emelet", ReadOnly = true });
                    grLakasLista.Columns.Add(new DataGridViewTextBoxColumn { Name = "aj", DataPropertyName = "ajto", HeaderText = "Ajtó", ReadOnly = true });
                    grLakasLista.Columns.Add(new DataGridViewTextBoxColumn { Name = "ajt", DataPropertyName = "ajtotores", HeaderText = "Ajtótörés", ReadOnly = true });
                    grLakasLista.Columns.Add(new DataGridViewTextBoxColumn { Name = "vk", DataPropertyName = "vonalkod", HeaderText = "Vonalkód", ReadOnly = true });
                    grLakasLista.Columns.Add(new DataGridViewTextBoxColumn { Name = "oid", DataPropertyName = "oid", HeaderText = "ÖREId", ReadOnly = true });
                    grLakasLista.Columns.Add(new DataGridViewTextBoxColumn { Name = "mj", DataPropertyName = "megjegyzes", HeaderText = "Megjegyzés", ReadOnly = true });
                    grLakasLista.Columns.Add(new DataGridViewTextBoxColumn { Name = "emj", DataPropertyName = "emeletjelkod", HeaderText = "Emeletjel", ReadOnly = true, Visible = true });
                    grLakasLista.Columns.Add(new DataGridViewCheckBoxColumn { Name = "EsetiRendeles", DataPropertyName = "NemTorolhetoEsetiRendelesMiatt", HeaderText = "Eseti rendelés", ReadOnly = true, Visible = true });
                    grLakasLista.Columns.Add(new DataGridViewCheckBoxColumn { Name = "torolve", DataPropertyName = "torolve", HeaderText = "Törölve", ReadOnly = true, Visible = true });
                    grLakasLista.Columns.Add(new DataGridViewCheckBoxColumn { Name = "torlendo", DataPropertyName = "torlendo", HeaderText = "Törlendő", ReadOnly = true, Visible = true });
                    grLakasLista.Columns.Add(new DataGridViewCheckBoxColumn { Name = "uj", DataPropertyName = "uj", HeaderText = "Új", ReadOnly = true, Visible = true });
                }

                lblUser.Text = LoginHelper.LoggedOnUser.username + " (" + LoginHelper.LoggedOnUser.regionev + ")";

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
                    {
                        ev.DrawDefault = true;
                    }
                };

                tbVk.Select();
                lblStatus.Text = "Vonalkódbeolvasásra vár";
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private void tbVk_Leave(object sender, EventArgs e)
        {
            Console.WriteLine("tbVk_Leave");
            try
            {
                this.Enabled = false;
                if (tbVk.Text != "")
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Console.WriteLine("Vonalkódolvasás: {0} (időpont: {1})", tbVk.Text, DateTime.Now.ToString("HH:mm:ss tt"));

                    using (KotorEntities ke = new KotorEntities())
                    {
                        if (GetTanTipus(tbVk.Text) != "TORLES" && GetTanTipus(tbVk.Text) != "KETSZERZART")
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
                                      nyomtatvanytipus = v.NyomtatvanyTipus_m.Nev,
                                      NemMozgathato = ke.MunkaTargyaHiba_t.Where(w => w.FigyelmeztetoVonalkod == v.Vonalkod).Any(),
                                      KetszerzartRogzitve = ke.MunkaTargyaHiba_t.Where(w => w.FigyelmeztetoVonalkod == v.Vonalkod && w.HibaTipusKod.Substring(0, 1) == "8").Any(),
                                      Ketszerzart = v.Ketszerzart,
                                      EvesEllenorzes = v.EvesEllenorzes
                                  }).FirstOrDefault();
                            if (ta != null && ta.KetszerzartRogzitve == true)
                            {
                                ta.Ketszerzart = ta.KetszerzartRogzitve;
                            }

                            if ((ta == null || ta.FotanId == null) && GetTanTipus(tbVk.Text) == "FOTANUSITVANY")
                            {
                                MessageBox.Show("Új, adatbázisban nem szereplő főtanúsítvány (adatbázisba illesztés)");
                                // címet kell beírni, ha kéménysöprő; ha adminisztrátor, akkor munkához kell, hogy rendelje.

                                if (chkKemenysepro.Checked)
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
                                    Megjegyzes = "Cím: " + tbCim.Text, // kéményseprőmód esetén beírt cím, egyébként kiválasztott munka
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
                                          nyomtatvanytipus = v.NyomtatvanyTipus_m.Nev,
                                          NemMozgathato = ke.MunkaTargyaHiba_t.Where(w => w.FigyelmeztetoVonalkod == v.Vonalkod).Any(),
                                          KetszerzartRogzitve = ke.MunkaTargyaHiba_t.Where(w => w.MunkaTargya_cs.Tanusitvany_t.Vonalkod == v.Vonalkod && w.HibaTipusKod.Substring(0, 1) == "8").Any(),
                                          Ketszerzart = v.Ketszerzart,
                                          EvesEllenorzes = v.EvesEllenorzes
                                      }).FirstOrDefault();
                                if (ta != null && ta.KetszerzartRogzitve == true)
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
                                          nyomtatvanytipus = v.NyomtatvanyTipus_m.Nev,
                                          KetszerzartRogzitve = ke.MunkaTargyaHiba_t.Where(w => w.MunkaTargya_cs.MunkaId == v.MunkaId && w.MunkaTargya_cs.Eid == v.Munka_t.Rendeles_t.Eid && w.HibaTipusKod.Substring(0, 1) == "8").Any(),
                                          Ketszerzart = v.Ketszerzart,
                                          EvesEllenorzes = v.EvesEllenorzes,
                                          NemMozgathato = ke.MunkaTargyaHiba_t.Where(w => w.FigyelmeztetoVonalkod == v.Vonalkod).Any(),
                                          NemTorolheto = ke.MunkaTargya_cs.Where(m => m.TanusitvanyId == ta.FotanId).Any(),
                                          lepcsohaz = v.Oid == null ? null : (from w in ke.ORE_t where w.Oid == v.Oid select w.Lepcsohaz).FirstOrDefault(),
                                          emelet = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select w.Emelet).FirstOrDefault(),
                                          emeletjel = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select w.EmeletJelKod).FirstOrDefault(),
                                          ajto = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select w.Ajto).FirstOrDefault(),
                                          ajtotores = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select w.Ajtotores).FirstOrDefault(),
                                          OreMegjegyzes = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select w.Megjegyzes).FirstOrDefault()
                                      }).FirstOrDefault();
                                if (t1.MunkaId != null && t1.KetszerzartRogzitve == true)
                                {
                                    t1.Ketszerzart = t1.KetszerzartRogzitve;
                                }

                                if (t1.MunkaId == null && !chkKemenysepro.Checked)
                                {
                                    MessageBox.Show("Folytatás előtt a főtanúsítványt munkához kell rendelni!");

                                }
                                else if (t1.MunkaId != null)
                                {
                                    // rendelésszám, cím (munka->rendelés->eid->cím)
                                    if (t1.Sor2 != null)
                                    {
                                        chkSor2.Checked = true;
                                        chkSor2.Enabled = false;
                                    }

                                    var rend = (from v in ke.Munka_t
                                                where (v.MunkaId == t1.MunkaId)
                                                select new { v.RendelesId, v.Rendeles_t.Rendelesszam, v.Rendeles_t.RendelesStatuszKod, v.Rendeles_t.RendelesStatusz_m.Nev, v.Rendeles_t.Eid }).FirstOrDefault();
                                    tbRendeles.Text = (rend == null ? string.Empty : rend.Rendelesszam + " (" + rend.Nev + ")");

                                    var epcim = (from v in ke.vEpuletElsCim where v.Eid == rend.Eid select new { v.CimSzoveg }).FirstOrDefault();
                                    tbCim.Text = (epcim == null ? string.Empty : epcim.CimSzoveg);

                                    // lakáslista kitöltése
                                    Lakasok = (from o in ke.ORE_t
                                               where o.Eid == rend.Eid
                                               select new Lakas()
                                               {
                                                   oid = o.Oid,
                                                   lepcsohaz = o.Lepcsohaz,
                                                   orglepcsohaz = o.Lepcsohaz,
                                                   emelet = o.Emelet,
                                                   emeletjelkod = o.EmeletJelKod,
                                                   orgemeletjelkod = o.EmeletJelKod,
                                                   ajto = o.Ajto,
                                                   orgajto = o.Ajto,
                                                   ajtotores = o.Ajtotores,
                                                   orgajtotores = o.Ajtotores,
                                                   torolve = o.Torolve,
                                                   megjegyzes = o.Megjegyzes,
                                                   EvesEllenorzesIndokolt = false, // ezt be kell építeni az adatbázisba is!
                                                   torlendo = false,
                                                   uj = false,
                                                   Modositott = false,
                                                   NemTorolhetoEsetiRendelesMiatt = false
                                                   /* 
                                                       * Ez így nagyon lassú még mindig (ÖRE táblát kell karbantartani?)
                                                       * 
                                                       * NemTorolhetoEsetiRendelesMiatt = o.MunkaTargya_cs.Where(mt => mt.Munka_t.MunkaStatuszKod != "TOROLVE" && mt.Munka_t.Rendeles_t.MunkaTipusKod != "SORMUNKA" && mt.Munka_t.Rendeles_t.MunkaTipusKod != "SORMUNKA_MUSZ").Any()
                                                   || o.Rendeles_t.Where(r => r.MunkaTipusKod != "SORMUNKA" && r.MunkaTipusKod != "SORMUNKA_MUSZ").Any() */
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
                                          nyomtatvanytipus = v.NyomtatvanyTipus_m.Nev,
                                          NemMozgathato = ke.MunkaTargyaHiba_t.Where(w => w.FigyelmeztetoVonalkod == v.Vonalkod).Any(),
                                          NemTorolheto = ke.MunkaTargya_cs.Where(m => m.TanusitvanyId == v.TanusitvanyId).Any(), // ez nem elég v.Tanusitvany_t1.Any()-vel?
                                          KetszerzartRogzitve = ke.MunkaTargyaHiba_t.Where(w => w.MunkaTargya_cs.MunkaId == v.MunkaId && w.MunkaTargya_cs.Oid == v.Oid && w.HibaTipusKod.Substring(0, 1) == "8").Any(),
                                          Ketszerzart = v.Ketszerzart,
                                          EvesEllenorzes = v.EvesEllenorzes
                                      }).ToList();
                                // ORE adatok kitöltése a t2-ben
                                foreach (Tan t in t2)
                                {
                                    if (t.MunkaId != null && t.KetszerzartRogzitve == true)
                                    {
                                        t.Ketszerzart = t.KetszerzartRogzitve;
                                    }

                                    if (t.oid != null)
                                    {
                                        Lakas l = (from x in Lakasok where x.oid == t.oid select new Lakas() { lepcsohaz = x.lepcsohaz, emelet = x.emelet, emeletjelkod = x.emeletjelkod, ajto = x.ajto, ajtotores = x.ajtotores }).FirstOrDefault();
                                        if (l != null)
                                        {
                                            t.lepcsohaz = l.lepcsohaz;
                                            t.emelet = l.emelet;
                                            t.emeletjel = l.emeletjelkod;
                                            t.ajto = l.ajto;
                                            t.ajtotores = l.ajtotores;
                                            t.OreMegjegyzes = l.megjegyzes;
                                        }
                                    }
                                }

                                // a későbbi hozzáadások miatt a Lakasok lista vonalkód mezőit is ki kell tölteni
                                if (Lakasok != null)
                                {
                                    foreach (Lakas l in Lakasok)
                                    {
                                        l.vonalkod = (from t in t2 where t.oid == l.oid && t.nyomtatvanytipuskod == "LAKASTANUSITVANY" select t.Vonalkod).FirstOrDefault() == null ? "" : (from t in t2 where t.oid == l.oid select t.Vonalkod).FirstOrDefault();
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
                                          nyomtatvanytipus = (from x in ke.NyomtatvanyTipus_m where x.Kod == v.NyomtatvanyTipusKod select x.Nev).FirstOrDefault(),
                                          NemMozgathato = ke.MunkaTargyaHiba_t.Where(w => w.FigyelmeztetoVonalkod == v.Vonalkod).Any(),
                                          NemTorolheto = ke.MunkaTargya_cs.Where(m => m.TanusitvanyId == v.TanusitvanyId).Any()
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

                                // lehet, hogy az egészet a tree selectionchanged metódusába kellene rakni (tbfo, tblak kezelés)

                                tbFo.Text = t1.Vonalkod;

                                if (ta.nyomtatvanytipuskod == "LAKASTANUSITVANY")
                                {
                                    tbLak.Text = ta.Vonalkod;
                                }
                                else if (ta.SzuloNyomtTipKod == "LAKASTANUSITVANY")
                                {
                                    tbLak.Text = ta.SzuloVk;
                                }
                                else
                                {
                                    tbLak.Text = string.Empty; // fontos, hogy a default szöveg ne üres legyen a tbLak esetében; a gridet ott inicializálja
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
                                            if (tbLak.Text != "")
                                            {
                                                var kz = t2.Find(x => x.Vonalkod == tbLak.Text);
                                                var ml = t3.Find(y => y.SzuloVk == kz.Vonalkod && y.nyomtatvanytipuskod == "ZARTLAKASERTESITO");

                                                if (kz.KetszerzartRogzitve == false) // csak akkor állítható, ha még nincs az adatbázisban rögzítve a kétszer zárt állapot
                                                {
                                                    if (!kz.Ketszerzart && ml != null) // eddig nem volt kétszer zárt->csak második látogatás értesítővel együtt lehet kétszer zárt
                                                    {
                                                        kz.Ketszerzart = !kz.Ketszerzart;
                                                    }
                                                    else if (kz.Ketszerzart) // kétszer zárt állapotot lehet törölni
                                                    {
                                                        kz.Ketszerzart = !kz.Ketszerzart;
                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show("Nem lehet kétszer zárt, ha nincs második látogatás értesítő!");
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
                                                if (t1.KetszerzartRogzitve == false)
                                                {
                                                    var ml = t2.Find(y => y.SzuloVk == tbFo.Text && y.nyomtatvanytipuskod == "ZARTLAKASERTESITO");

                                                    if (!VkTan[tbFo.Text].Ketszerzart && ml != null) // eddig nem volkt kétszer zárt->csak második látogatás értesítővel együtt lehet kétszer zárt
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
                                    case "TORLES": // TODO: ezt át kell nézni (láncolat törlése; adatbázisból törlés)
                                        {
                                            string SelectedVonalkod = VkTree.FirstOrDefault(x => x.Value == tvVonalkodok.SelectedNode).Key;

                                            var ch = (from Tan v in VkTan.Values where v.SzuloVk == VkTan[SelectedVonalkod].Vonalkod && (v.NemTorolheto == true || v.NemMozgathato == true) select v);
                                            if (VkTan[SelectedVonalkod].NemMozgathato == false && VkTan[SelectedVonalkod].NemTorolheto == false && !ch.Any())
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
                                                    MessageBox.Show(string.Format("Adatbázisból törlendő tanúsítvány: {0} ({1})", VkTan[SelectedVonalkod].Vonalkod, VkTan[SelectedVonalkod].TanId.ToString()));
                                                }
                                                else
                                                {
                                                    t3.Remove(t3.Find(r => r.Vonalkod == SelectedVonalkod));
                                                    t2.Remove(t2.Find(r => r.Vonalkod == SelectedVonalkod));

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
                                            tvVonalkodok.SelectedNode = VkTree[tbLastVk.Text];
                                            tvVonalkodok.SelectedNode.EnsureVisible();
                                            break;
                                        }

                                    case "LAKASTANUSITVANY":
                                        {
                                            tvVonalkodok.BeginUpdate();
                                            tvVonalkodok.SelectedNode = VkTree[tbLastVk.Text];
                                            tvVonalkodok.SelectedNode.EnsureVisible();
                                            tvVonalkodok.EndUpdate();
                                            break;
                                        }

                                    default: // TODO: át kell nézni, hogy a tree-beli választás után jól működik-e a mozgatás (tblastvk használata kérdéses)
                                        {
                                            var tp = t2.Find(x => x.Vonalkod == VkTan[tbLastVk.Text].SzuloVk);
                                            if (tp != null) // lakástanúsítvány a szülő
                                            {
                                                if (tp.Vonalkod == tbLak.Text)
                                                {
                                                    tvVonalkodok.BeginUpdate();
                                                    tvVonalkodok.SelectedNode = VkTree[tbLastVk.Text];
                                                    tvVonalkodok.SelectedNode.EnsureVisible();
                                                    tvVonalkodok.EndUpdate();
                                                }
                                                else
                                                {
                                                    if (VkTan[tbLastVk.Text].NemMozgathato == true)
                                                    {
                                                        MessageBox.Show("A " + tbLastVk.Text + " számú " + NyTip[VkTan[tbLastVk.Text].nyomtatvanytipuskod] + " nem mozgatható, van hozzá rögzített hiba");
                                                    }
                                                    else
                                                    {
                                                        if (MessageBox.Show("Biztos, hogy " + (tbLak.Text == "" ? "az épülethez" : "más lakáshoz") + " tartozik a most beolvasott vonalkódú nyomtatvány?", "Nyomtatványáthelyezés", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                                        {
                                                            VkTan[tbVk.Text].SzuloId = tbLak.Text == string.Empty ? VkTan[tbFo.Text].TanId : VkTan[tbLak.Text].TanId;
                                                            VkTan[tbVk.Text].SzuloVk = tbLak.Text == string.Empty ? VkTan[tbFo.Text].Vonalkod : VkTan[tbLak.Text].Vonalkod;
                                                            VkTan[tbVk.Text].SzuloNyomtTipKod = tbLak.Text == string.Empty ? VkTan[tbFo.Text].nyomtatvanytipuskod : VkTan[tbLak.Text].nyomtatvanytipuskod;
                                                            VkTan[tbVk.Text].SzuloIdModositott = true;
                                                            VkTan[tbVk.Text].oid = tbLak.Text == string.Empty ? VkTan[tbFo.Text].oid : VkTan[tbLak.Text].oid;

                                                            if (tbLak.Text == string.Empty) // lakás->főtanúsítvány mozgatás
                                                            {
                                                                t2.Add(VkTan[tbVk.Text]);
                                                                t3.Remove(t3.Find(r => r.Vonalkod == tbVk.Text));
                                                            }
                                                            else // más lakáshoz tartozik, nincs szintváltás
                                                            {
                                                                Tan v = (from w in t3 where w.Vonalkod == tbVk.Text select w).FirstOrDefault();
                                                                if (v != null)
                                                                {
                                                                    v.SzuloId = tbLak.Text == string.Empty ? VkTan[tbFo.Text].TanId : VkTan[tbLak.Text].TanId;
                                                                    v.SzuloIdModositott = true;
                                                                }
                                                            }

                                                            tvVonalkodok.BeginUpdate();

                                                            VkTree[tbVk.Text].Remove();
                                                            VkTree.Remove(tbVk.Text);
                                                            TreeNode tln = VkTree[tbLak.Text == string.Empty ? tbFo.Text : tbLak.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (" + NyTip[VkTan[tbVk.Text].nyomtatvanytipuskod] + ")");
                                                            VkTree.Add(tbVk.Text, tln);

                                                            tvVonalkodok.SelectedNode = tln;
                                                            tvVonalkodok.SelectedNode.EnsureVisible();

                                                            tvVonalkodok.EndUpdate();
                                                        }
                                                    }
                                                }
                                            }
                                            else if (tp == null && t1.TanId == VkTan[tbVk.Text].SzuloId && tbLak.Text != string.Empty) // főtanúsítvány->lakástanúsítvány mozgatás
                                            {
                                                if (VkTan[tbVk.Text].NemMozgathato)
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
                                if (!(from v in ke.Tanusitvany_t where v.Vonalkod == tbVk.Text select v).Any())
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
                                                tluj.oid = null;

                                                // tbLak.Text = tluj.Vonalkod; // TODO tv-selectionchanged-be kell mozgatni

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

                                                if (tbLak.Text == string.Empty)
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
                                                    tluj.oid = t1.oid;

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
                                                    tluj.oid = VkTan[tbLak.Text].oid;

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
                if (AdatbevitelStatusz == AdatBevitel.VonalkodOlvasas)
                {
                    tbVk.Select();
                }

            }
            finally
            {
                this.Enabled = true;
            }
        }

        private void tvVonalkodok_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Console.WriteLine("Treeview-Afterselect (selectednode: {0}; argumentnode: {1})", tvVonalkodok.SelectedNode == null ? "" : tvVonalkodok.SelectedNode.Text, e.Node == null ? "" : e.Node.Text);

            if (e.Node != null)
            {
                tvVonalkodok.SelectedNode.EnsureVisible();
            }

            e.Node.BackColor = System.Drawing.SystemColors.ButtonFace;
            string Vonalkod = VkTree.FirstOrDefault(x => x.Value == e.Node).Key;

            switch (GetTanTipus(Vonalkod))
            {
                case "FOTANUSITVANY":
                    {
                        tbLak.Text = "";
                        /* TODO
                        tbLepcsohaz.Text = VkTan[Vonalkod].lepcsohaz;
                        if (VkTan[Vonalkod].emeletjel == null)
                        {
                            VkTan[Vonalkod].emeletjel = "<NA>";
                            VkTan[Vonalkod].emelet = "<Nincs emelet>";
                        }
                        cbEmeletjel.SelectedIndex = cbEmeletjel.FindStringExact(VkTan[Vonalkod].emelet);
                        tbAjto.Text = VkTan[Vonalkod].ajto;
                        tbAjtotores.Text = VkTan[Vonalkod].ajtotores;
                        tbOid.Text = VkTan[Vonalkod].oid.ToString();
                        tbMegj.Text = VkTan[Vonalkod].OreMegjegyzes;
                        */
                        break;
                    }
                case "LAKASTANUSITVANY":
                    {
                        tbLak.Text = Vonalkod;
                        /* TODO
                        tbLepcsohaz.Text = VkTan[Vonalkod].lepcsohaz;
                        if (VkTan[Vonalkod].emeletjel == null)
                        {
                            VkTan[Vonalkod].emeletjel = "<NA>";
                            VkTan[Vonalkod].emelet = "<Nincs emelet>";
                        }
                        cbEmeletjel.SelectedIndex = cbEmeletjel.FindStringExact(VkTan[Vonalkod].emelet);
                        tbAjto.Text = VkTan[Vonalkod].ajto;
                        tbAjtotores.Text = VkTan[Vonalkod].ajtotores;
                        tbOid.Text = VkTan[Vonalkod].oid.ToString();
                        tbMegj.Text = VkTan[Vonalkod].OreMegjegyzes;
    */
                        break;
                    }
                default:
                    {
                        if (VkTan[Vonalkod].SzuloNyomtTipKod == "FOTANUSITVANY")
                        {
                            tbLak.Text = "";
                        }
                        else
                        {
                            tbLak.Text = VkTan[Vonalkod].SzuloVk;
                        }
                        /* TODO
                        tbLepcsohaz.Text = VkTan[VkTan[Vonalkod].SzuloVk].lepcsohaz;
                        cbEmeletjel.SelectedIndex = cbEmeletjel.FindStringExact(VkTan[VkTan[Vonalkod].SzuloVk].emelet);
                        tbAjto.Text = VkTan[VkTan[Vonalkod].SzuloVk].ajto;
                        tbAjtotores.Text = VkTan[VkTan[Vonalkod].SzuloVk].ajtotores;
                        tbOid.Text = VkTan[VkTan[Vonalkod].SzuloVk].oid.ToString();
                        tbMegj.Text = VkTan[VkTan[Vonalkod].SzuloVk].OreMegjegyzes;
    */
                        break;
                    }
            }

            if (VkTan[Vonalkod].NemMozgathato == false && VkTan[Vonalkod].NemTorolheto == false && !(from Tan v in VkTan.Values where v.SzuloVk == VkTan[Vonalkod].Vonalkod && (v.NemTorolheto == true || v.NemMozgathato == true) select v).Any() && (GetTanTipus(Vonalkod) != "FOTANUSITVANY"))
            {
                lblTorles.Visible = true;
                pbTorles.Visible = true;
            }
            else
            {
                lblTorles.Visible = false;
                pbTorles.Visible = false;
            }

            if (VkTan[Vonalkod].KetszerzartRogzitve == true || ((t1.Sor2 == null) && (chkSor2.Checked == false)) || ((VkTan[Vonalkod].nyomtatvanytipuskod == "ZARTLAKASERTESITO") && (VkTan[VkTan[Vonalkod].SzuloVk].KetszerzartRogzitve == true)))
            {
                pb99.Visible = false;
                lbl99.Visible = false;
            }
            else
            {
                pb99.Visible = true;
                lbl99.Visible = true;
            }

            tbVk.Select();
            lblStatus.Text = "Vonalkódbeolvasásra vár";

            /* TODO            int? so = VkTan[Vonalkod].oid != null ? VkTan[Vonalkod].oid : VkTan[Vonalkod].SzuloVk == string.Empty ? (int?)null : VkTan[VkTan[Vonalkod].SzuloVk].oid;
                        if (so != null)
                        {
                            grLakasLista.ClearSelection();

                            bool rowExists = false;

                            for (int i = 0; i < grLakasLista.Rows.Count; i++)
                            {
                                if ((int)grLakasLista.Rows[i].Cells["oid"].Value == so)
                                {
                                    grLakasLista.Rows[i].Selected = true;

                                    rowExists = true;
                                    grLakasLista.FirstDisplayedScrollingRowIndex = i == 0 ? 0 : i - 1;
                                    break;
                                }
                            }

                            if (!rowExists && grLakasLista.RowCount > 0)
                            {
                                grLakasLista.FirstDisplayedScrollingRowIndex = 0;
                            }

                            // itt nem lehet lakást választani, csak az adatait szerkeszteni, ha nincs hozzá eseti megrendelő
                            if ((from l in Lakasok where l.oid == so select l).First().NemTorolhetoEsetiRendelesMiatt == false)
                            {
                                btnLakasadatModositas.Enabled = true;
                                btnLakasadatModositas.Visible = true;
                                lblStatus.Text = "";
                            }
                            else
                            {
                                btnLakasadatModositas.Enabled = false;
                                btnLakasadatModositas.Visible = false;
                                lblStatus.Text = "A lakás adatai nem módosíthatóak, eseti megrendelés van hozzá";
                            }
                        }*/
        }

        private void tbLak_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine("TbLak-textchanged");
            if (Lakasok != null && !chkKemenysepro.Checked)
            {
                grLakasLista.Visible = true;
                if (string.IsNullOrEmpty(tbLak.Text))
                {
                    // nincs érvényes lakástanúsítvány -> az összes adatot megjeleníti+nincs kiválasztott sor
                    UpdateGridDataSource(Lakasok);
                    grLakasLista.ClearSelection();
                    grLakasLista.AutoResizeColumns();
                    grLakasLista.Visible = !chkKemenysepro.Checked;

                    lblStatus.Text = string.Empty;
                }
                else if ((from Tan v in VkTan.Values where v.Vonalkod == tbLak.Text select v).Any()) // ha van ilyen vonalkód a memóriában
                {
                    if (VkTan[tbLak.Text].oid == null)
                    {
                        // nincs lakásadat, a vonalkód nélküliekből válogathat, szerkesztheti a kiválasztottat, ha ahhoz nincs eseti munka, vagy újat adhat hozzá
                        UpdateGridDataSource(Lakasok, x => String.IsNullOrEmpty(x.vonalkod));
                        grLakasLista.AutoResizeColumns();
                        grLakasLista.Visible = !chkKemenysepro.Checked;
                        grLakasLista.ClearSelection();
                        grLakasLista.Enabled = true;

                        lblStatus.Text = "A tanúsítványhoz nincs lakásadat, a meglevő szabad listából kell választani vagy újat kell létrehozni";
                        AdatbevitelStatusz = AdatBevitel.LakasadatKereses; // ez átgondolandó
                        tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug

                        btnUjLakas.Visible = true;

                        // keresés, kiváLasztás gomb = true
                        tbLepcsohaz.Select();

                    }
                    else if ((from l in Lakasok where l.oid == VkTan[tbLak.Text].oid select l).First().NemTorolhetoEsetiRendelesMiatt)
                    {
                        // ha van a lakáshoz egyedi rendelés, nem szerkeszthető a lakásadat
                        UpdateGridDataSource(Lakasok, x => x.vonalkod == tbLak.Text);
                        grLakasLista.AutoResizeColumns();
                        grLakasLista.Visible = !chkKemenysepro.Checked;
                        lblStatus.Text = "A lakáshoz van egyedi rendelés, az adatok nem szerkeszthetőek";

                        // TODO: grid selectionchange-be mozgatandó
                        btnLakasadatModositas.Visible = false;
                        btnUjLakas.Visible = false;

                    }
                    else
                    {
                        // szerkeszthető lakásadat, de nem választható; új nem hozható létre
                        UpdateGridDataSource(Lakasok, x => x.vonalkod == tbLak.Text);
                        grLakasLista.Enabled = false;

                        lblStatus.Text = "A lakás adatai szerkeszthetőek";

                        // TODO: grid selectionchange-be mozgatandó
                        btnLakasadatModositas.Visible = true;
                        btnUjLakas.Visible = false;
                    }
                }
                else if (!chkKemenysepro.Checked)
                {
                    // itt engedni kell új lakást hozzáadni, ha van lakástanúsítvány. Csak admin módban, csak ha van munkaid
                    UpdateGridDataSource(Lakasok, x => !String.IsNullOrEmpty(x.vonalkod));
                    grLakasLista.Refresh();
                    GridEnabledPreviousStatus = grLakasLista.Enabled;
                    grLakasLista.Enabled = true;
                    grLakasLista.ClearSelection();

                    // TODO: grid selectionchange-be mozgatandó
                    btnUjLakas.Visible = true;
                    btnLakasadatModositas.Visible = false;

                    AdatbevitelStatusz = AdatBevitel.LakasadatKereses;
                    tbVk.Select();
                    lblStatus.Text = "Vonalkódbeolvasásra vár";
                }
            }
            else if (!chkKemenysepro.Checked)
            {
                // TODO: grid selectionchange-be mozgatandó
                // üres a lakáslista->új lakást lehet hozzáadni
                btnUjLakas.Visible = true;
                AdatbevitelStatusz = AdatBevitel.LakasadatKereses;
                tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
                btnUjLakas.Visible = true;
                btnLakasadatModositas.Visible = false;
                tbVk.Select();
                lblStatus.Text = "Vonalkódbeolvasásra vár";
            }
        }

        private void grLakasLista_SelectionChanged(object sender, EventArgs e)
        {
            Console.WriteLine("GridSelectionChanged (adatbevitelstátusz: {0}; selected row-oid: {1}, number of selected rows: {2}, tblak: {3})", AdatbevitelStatusz, grLakasLista.SelectedRows.Count == 0 ? "Nincs kiválasztott sor" : grLakasLista.SelectedRows[0].Cells["oid"].Value.ToString(), grLakasLista.SelectedRows.Count.ToString(), tbLak.Text);

            if (grLakasLista.SelectedRows.Count == 0)
            {
                tbLepcsohaz.Text = string.Empty;
                cbEmeletjel.SelectedIndex = cbEmeletjel.FindStringExact("<Nincs emelet>");
                tbAjto.Text = string.Empty;
                tbAjtotores.Text = string.Empty;
                chkEvesEllenorzes.Checked = false;
                tbMegj.Text = string.Empty;
                tbOid.Text = string.Empty;
                btnLakasadatModositas.Visible = false;
            }
            else
            {
                int loid;
                if (!int.TryParse(grLakasLista.SelectedRows[0].Cells["oid"].Value.ToString(), out loid))
                {
                    // új lakás, még nincs oid
                }
                else
                {
                    var ls = (from l in Lakasok where l.oid == loid select l).First();
                    tbLepcsohaz.Text = ls.lepcsohaz;
                    cbEmeletjel.SelectedIndex = cbEmeletjel.FindStringExact(ls.emelet);
                    // cbEmeletjel.SelectedIndex = VkTan[tbLak.Text].emeletjel == null ? cbEmeletjel.Items.IndexOf("<NA>") : cbEmeletjel.Items.IndexOf(VkTan[tbLak.Text].emeletjel);
                    tbAjto.Text = ls.ajto;
                    tbAjtotores.Text = ls.ajtotores;
                    chkEvesEllenorzes.Checked = ls.EvesEllenorzesIndokolt;
                    tbMegj.Text = ls.megjegyzes;
                    tbOid.Text = loid.ToString();

                    if (!ls.NemTorolhetoEsetiRendelesMiatt) // módosítható-e a lakásadat
                    {
                        btnLakasadatModositas.Visible = true;
                    }

                    switch (AdatbevitelStatusz)
                    {
                        case AdatBevitel.LakasadatKereses:
                            btnLakasValasztas.Visible = true;
                            break;
                        default:
                            break;
                    }
                }
            }


            tbVk.Select();
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

        private string getLakCim(string lepcsohaz, string emelet, string ajto, string ajtotores)
        {
            return (((lepcsohaz == "") || (lepcsohaz == null) ? "" : lepcsohaz + " lépcsőház ") + ((emelet == "") || (emelet == null) ? "" : emelet + " ") + ((ajto == "") || (ajto == null) ? "" : ajto + ((ajtotores == "") || (ajtotores == null) ? "" : "/")) + ((ajtotores == "") || (ajtotores == null) ? "" : ajtotores) + (((ajto != null) && (ajto != "")) || ((ajtotores != null) && (ajtotores != "")) ? ". ajtó" : "")).Trim();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Console.WriteLine("btnSave_Click");
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
                                BeolvasasIdopontja = v.BeolvasasIdopontja,
                                Ketszerzart = v.Ketszerzart,
                                EvesEllenorzes = v.EvesEllenorzes
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

        private void chkEvesEllenorzes_CheckedChanged(object sender, EventArgs e)
        {
            if (tbLak.Text == "")
            {
                VkTan[tbFo.Text].EvesEllenorzes = chkEvesEllenorzes.Checked;
            }
            else
            {
                VkTan[tbLak.Text].EvesEllenorzes = chkEvesEllenorzes.Checked;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Biztos, hogy nem menti el a beolvasott adatokat?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Utilities.ResetAllControls(this);

                lbl99.Visible = false;
                pb99.Visible = false;
                lblTorles.Visible = false;
                pbTorles.Visible = false;
                VkTan.Clear();
                VkTree.Clear();
            }
        }

        private void chkKemenysepro_CheckedChanged(object sender, EventArgs e)
        { // ezt még ki kell egészíteni (ha láthatóak, akkor státuszfüggő a megjelenésük
            Console.WriteLine("chkKemenysepro_CheckedChanged");
            btnLakasadatModositas.Enabled = !chkKemenysepro.Checked;
            tbAjto.Enabled = !chkKemenysepro.Checked;
            tbAjtotores.Enabled = !chkKemenysepro.Checked;
            cbEmeletjel.Enabled = !chkKemenysepro.Checked;
            tbLepcsohaz.Enabled = !chkKemenysepro.Checked;
            btnLakasadatModositas.Enabled = !chkKemenysepro.Checked;
            tbMegj.Enabled = !chkKemenysepro.Checked;
            chkEvesEllenorzes.Enabled = !chkKemenysepro.Checked;
            btnCancel.Visible = !chkKemenysepro.Checked;
            if (grLakasLista.DataSource != null)
            {
                grLakasLista.Visible = !chkKemenysepro.Checked;
            }

        }

        private int? LakasKeresesGrid(string lh, string emj, string aj, string ajt)
        {
            int lhmatch = -1;
            int emjmatch = -1;
            int ajmatch = -1;
            int ajtmatch = -1;

            grLakasLista.ClearSelection();

            for (int i = 0; i < grLakasLista.Rows.Count; i++)
            {
                if ((string)grLakasLista.Rows[i].Cells["vk"].Value == "") // csak a vonalkód nélküliekben keres
                {
                    lhmatch = (string)grLakasLista.Rows[i].Cells["lh"].Value == lh ? i : lhmatch;
                    emjmatch = (string)grLakasLista.Rows[i].Cells["emj"].Value == emj ? i : lhmatch;
                    ajmatch = (string)grLakasLista.Rows[i].Cells["aj"].Value == aj ? i : lhmatch;
                    ajtmatch = (string)grLakasLista.Rows[i].Cells["ajt"].Value == ajt ? i : lhmatch;

                }
            }

            // if row does not exist, set current cell to first row.
            /*           if (!rowExists)
                       {
                           // grLakasLista.Rows[0].Selected = true;
                           grLakasLista.FirstDisplayedScrollingRowIndex = 0;
                       }*/
            return null;
        } // ez kell-e??? (vagy Robi módján meg lehet csinálni???)

        private void UpdateGridDataSource(IEnumerable<Lakas> originalData, params Func<Lakas, bool>[] filters)
        {
            var result = originalData;
            foreach (var filter in filters)
            {
                result = result.Where(filter);
            }

            result.OrderBy(x => x, lakasComparer);

            grLakasLista.DataSource = result.ToList();
            grLakasLista.Refresh();
        }

        private void btnLakasKereses_Click(object sender, EventArgs e)
        {
            // itt lehet keresni a lakások adatát
        }

        private void btnLakasadatSave_Click(object sender, EventArgs e)
        {
            Console.WriteLine("LakasadatSaveClick ({0})", AdatbevitelStatusz.ToString());
            switch (AdatbevitelStatusz)
            {
                case AdatBevitel.Lakasadatmodositas:
                    // módosított lakásadatok mentése

                    int loid;
                    if (int.TryParse(tbOid.Text, out loid))
                    {
                        var lm = (from l in Lakasok where l.oid == loid select l);
                        // lm.lepcsohaz = tbLepcsohaz.Text;
                        Console.WriteLine("{0},{1}", cbEmeletjel.SelectedItem.ToString(), cbEmeletjel.SelectedValue.ToString());

                    }
                    lblStatus.Text = "Vonalkód beolvasásra vár";
                    AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas;
                    tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug

                    btnLakasadatSave.Visible = false;
                    btnMegsem.Visible = false;
                    btnLakasadatModositas.Visible = true;
                    tvVonalkodok.Enabled = true;
                    tbVk.Select();
                    break;
                case AdatBevitel.Ujlakas:
                    lblStatus.Text = "Vonalkód beolvasásra vár";
                    AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas;
                    tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug

                    btnLakasadatSave.Visible = false;
                    btnMegsem.Visible = false;
                    btnLakasadatModositas.Visible = true;
                    tvVonalkodok.Enabled = true;
                    tbVk.Select();

                    // új lakás hozzáadása a listákhoz
                    // vonalkód adatok frissítése a lakásokkal
                    // treeview szöveg frissítése
                    // grid adatforrás frissítése
                    // grid kiválasztás


                    break;
                default:
                    break;
            }
        }

        private void btnLakasadatModositas_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbOid.Text))
            {
                MessageBox.Show("Először ki kell jelölni egy módosítandó lakást");
            }
            else
            {
                AdatbevitelStatusz = AdatBevitel.Lakasadatmodositas;
                tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
                btnLakasadatSave.Visible = true;
                btnLakasadatModositas.Visible = false;
                btnMegsem.Visible = true;
                tvVonalkodok.Enabled = false;
                GridEnabledPreviousStatus = grLakasLista.Enabled;
                grLakasLista.Enabled = false;
                lblStatus.Text = "Gridben kijelölt lakás adatainak módosítása";
                tbLepcsohaz.Select();
                UjlakasPrevStatus = btnUjLakas.Visible;
                btnUjLakas.Visible = false;

                btnLakasValasztas.Visible = false;
            }
        }

        private void btnUjLakas_Click(object sender, EventArgs e)
        {
            Console.WriteLine("btnUjlakas_Click");

            chkEvesEllenorzes.Enabled = true;

            AdatbevitelStatusz = AdatBevitel.Ujlakas;
            tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
            tbLepcsohaz.Text = String.Empty;
            cbEmeletjel.SelectedIndex = 0;
            tbAjto.Text = String.Empty;
            tbAjtotores.Text = String.Empty;
            tbMegj.Text = String.Empty;
            chkEvesEllenorzes.Checked = false;
            tbOid.Text = String.Empty;

            tbLepcsohaz.Select();
            tvVonalkodok.Enabled = false;
            GridEnabledPreviousStatus = grLakasLista.Enabled;
            grLakasLista.Enabled = false;
            grLakasLista.ClearSelection();

            btnLakasadatSave.Visible = true;
            btnMegsem.Visible = true;
            btnLakasadatModositas.Visible = false;

            lblStatus.Text = "Új lakás adatainak rögzítése";

            btnLakasValasztas.Visible = false;
        }

        private void btnMegsem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("btnMegsem ({0})", AdatbevitelStatusz);

            chkEvesEllenorzes.Enabled = false;

            switch (AdatbevitelStatusz)
            {
                case AdatBevitel.Lakasadatmodositas:
                    tvVonalkodok.Enabled = true;
                    grLakasLista.Enabled = GridEnabledPreviousStatus;
                    AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas;
                    tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug

                    btnMegsem.Visible = false;
                    btnLakasadatSave.Visible = false;
                    btnLakasadatModositas.Visible = true;
                    btnUjLakas.Visible = UjlakasPrevStatus;
                    lblStatus.Text = "Vonalkód beolvasásra vár";
                    break;

                case AdatBevitel.Ujlakas:
                    tvVonalkodok.Enabled = true;
                    grLakasLista.Enabled = true;
                    AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas; // itt lehet, hogy keresés kellene???
                    tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug

                    btnMegsem.Visible = false;
                    btnLakasadatSave.Visible = false;
                    btnUjLakas.Visible = true;
                    break;

                default:
                    grLakasLista.Enabled = GridEnabledPreviousStatus;
                    AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas;
                    tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug

                    break;
            }
        }

        private void btnLakasValasztas_Click(object sender, EventArgs e)
        {
            // vonalkód-lakás összerendelés
            // innen már nem új, hanem összerendelt lakás lesz
            // el kellene menteni a lakásdefiníciót (oid lesz)
        }

        private void tbLepcsohaz_TextChanged(object sender, EventArgs e)
        {
            if (AdatbevitelStatusz == AdatBevitel.LakasadatKereses)
            {
                LakasKereses();
            }
        }

        private void cbEmeletjel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AdatbevitelStatusz == AdatBevitel.LakasadatKereses)
            {
                LakasKereses();
            }
        }

        private void tbAjto_TextChanged(object sender, EventArgs e)
        {
            if (AdatbevitelStatusz == AdatBevitel.LakasadatKereses)
            {
                LakasKereses();
            }
        }

        private void tbAjtotores_TextChanged(object sender, EventArgs e)
        {
            if (AdatbevitelStatusz == AdatBevitel.LakasadatKereses)
            {
                LakasKereses();
            }
        }

        private void LakasKereses()
        {
            UpdateGridDataSource(Lakasok,
            x => (String.IsNullOrEmpty(tbLepcsohaz.Text) || x.lepcsohaz == tbLepcsohaz.Text)
            && (String.IsNullOrEmpty(tbAjto.Text) || x.ajto == tbAjto.Text)
            && (String.IsNullOrEmpty(tbAjtotores.Text) || x.ajtotores == tbAjtotores.Text));
        }
    }
}
