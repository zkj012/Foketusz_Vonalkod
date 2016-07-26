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
using System.Diagnostics;

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
        const String KetszerZartSzoveg = "; Kétszer zárt";
        Color KetszerzartBetuszin = Color.DarkOrchid;
        List<String> PirosSzurke = new List<String>();
        int ujlakasid = -1;
        int? eid;
        int? cid;

        Boolean PrevGridEnabledStatus;
        Boolean PrevbtnUjlakasStatus;
        Boolean PrevbtnLakasmodStatus;
        String PrevLblStatus;

        enum AdatBevitel // üzemmódok 
        {
            VonalkodOlvasas,
            LakasadatKereses,
            Lakasadatmodositas,
            Ujlakas
        }

        private readonly IComparer<Lakas> lakasComparer = new LakasComparer();

        AdatBevitel AdatbevitelStatusz; // a vonalkód mező fókusz visszatérést szabályozza pl. lakásadatfrissítéskor
        AdatBevitel PrevAdatbevitelStatusz;
        bool ujfo; // Új főtanúsítvány; a mégsem gombnál törölni kell az adatbázisból is a beírt értéket, ha az új


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
                tbBevitelStatusz.Text = AdatbevitelStatusz.ToString();
                tbLepcsohaz.Enabled = false;
                cbEmeletjel.Enabled = false;
                tbAjto.Enabled = false;
                tbAjtotores.Enabled = false;
                tbMegj.Enabled = false;
                chkEvesEllenorzes.Enabled = false;
                tbTulaj.Enabled = false;

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
                    tbTulaj.Enabled = !chkKemenysepro.Checked;
                    chkEvesEllenorzes.Enabled = !chkKemenysepro.Checked;
                    grLakasLista.Visible = false; // üresen nem látható
                    grLakasLista.Enabled = false; // választani nem lehet, ha van a vonalkódhoz oid
                    PrevGridEnabledStatus = false;
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
                    grLakasLista.Columns.Add(new DataGridViewTextBoxColumn { Name = "tulaj", DataPropertyName = "Tulaj", HeaderText = "Tulajdonos", ReadOnly = true, Visible = true });
                    grLakasLista.Columns.Add(new DataGridViewCheckBoxColumn { Name = "EsetiRendeles", DataPropertyName = "NemTorolhetoEsetiRendelesMiatt", HeaderText = "Eseti rendelés", ReadOnly = true, Visible = true });
                    //grLakasLista.Columns.Add(new DataGridViewCheckBoxColumn { Name = "torolve", DataPropertyName = "torolve", HeaderText = "Törölve", ReadOnly = true, Visible = true });
                    //grLakasLista.Columns.Add(new DataGridViewCheckBoxColumn { Name = "torlendo", DataPropertyName = "torlendo", HeaderText = "Törlendő", ReadOnly = true, Visible = true });
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
            try
            {
                this.Enabled = false;
                if (!string.IsNullOrEmpty(tbVk.Text))
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
                                      EvesEllenorzes = v.EvesEllenorzesIndokolt
                                  }).FirstOrDefault();
                            if (ta != null && ta.KetszerzartRogzitve == true)
                            {
                                ta.Ketszerzart = ta.KetszerzartRogzitve;
                            }

                            ujfo = false;

                            if ((ta == null || ta.FotanId == null) && GetTanTipus(tbVk.Text) == "FOTANUSITVANY")
                            {
                                ujfo = true;
                                if (chkKemenysepro.Checked)
                                {
                                    MessageBox.Show("Új, adatbázisban nem szereplő főtanúsítvány - a címet rögzíteni kell");
                                    // címet kell beírni, ha kéménysöprő; ha adminisztrátor, akkor munkához kell, hogy rendelje.
                                    CimInput cimfrm = new CimInput();
                                    cimfrm.ShowDialog(this);
                                    tbCim.Text = CimHelper.CimSzovegKemenysepro;
                                }
                                else
                                {
                                    MessageBox.Show("A tanúsítványt először munkához kell csatolni a Kotorban!");
                                    Process.Start("IExplore.exe", "http://kotor/Rendeles");
                                    // TODO: itt újra kell indítani a beolvasást, kiüríteni az eddigi adatokat
                                }

                                if (chkKemenysepro.Checked)
                                {
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
                                    ke.SaveChanges(); // TODO: hibakezelést meg kell valósítani

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
                                              EvesEllenorzes = v.EvesEllenorzesIndokolt
                                          }).FirstOrDefault();
                                    if (ta != null && ta.KetszerzartRogzitve == true)
                                    {
                                        ta.Ketszerzart = ta.KetszerzartRogzitve;
                                    }
                                }
                            }

                            if ((ta == null || ta.FotanId == null) && GetTanTipus(tbVk.Text) != "FOTANUSITVANY")
                            {
                                MessageBox.Show("Nincs aktuális főtanúsítvány; először főtanúsítványt kell beolvasni!!!");
                                // új vonalkódot kell beolvasni, ha ide elért
                            }
                            else if (!ujfo || chkKemenysepro.Checked)
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
                                          EvesEllenorzes = v.EvesEllenorzesIndokolt,
                                          TanMegjegyzes = v.Megjegyzes,
                                          NemMozgathato = ke.MunkaTargyaHiba_t.Where(w => w.FigyelmeztetoVonalkod == v.Vonalkod).Any(),
                                          NemTorolheto = ke.MunkaTargya_cs.Where(m => m.TanusitvanyId == ta.FotanId).Any(),
                                          lepcsohaz = v.Oid == null ? null : (from w in ke.ORE_t where w.Oid == v.Oid select w.Lepcsohaz).FirstOrDefault(),
                                          emelet = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select w.Emelet).FirstOrDefault(),
                                          emeletjel = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select w.EmeletJelKod).FirstOrDefault(),
                                          ajto = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select w.Ajto).FirstOrDefault(),
                                          ajtotores = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select w.Ajtotores).FirstOrDefault(),
                                          OreMegjegyzes = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select w.Megjegyzes).FirstOrDefault(),
                                          OreTulaj = v.Tulajdonos // ez így csalás, a tanúsítványhoz rögzített adatot hozza
                                          // OreTulaj= v.Oid == null ? "" : (from w in ke.vOreTulaj1 where w.Oid == v.Oid select w.OreTulajNev).FirstOrDefault() //ez így nagyon lassú
                                      }).FirstOrDefault();
                                if (t1.MunkaId != null && t1.KetszerzartRogzitve == true)
                                {
                                    t1.Ketszerzart = t1.KetszerzartRogzitve;
                                }

                                if (t1.MunkaId == null && !chkKemenysepro.Checked)
                                {
                                    MessageBox.Show("A tanúsítványt először munkához kell csatolni a Kotorban!");
                                    Process.Start("IExplore.exe", "http://kotor/Rendeles");
                                    // TODO: itt újra kell indítani a beolvasást, kiüríteni az eddigi adatokat
                                }
                                else if(t1.MunkaId == null && !string.IsNullOrEmpty(t1.TanMegjegyzes)) // kéményseprő mód, nincs munkaid de már van tanúsítvány a memóriában és van megjegyzés a tanúsítványhoz
                                {
                                    tbCim.Text = t1.TanMegjegyzes.Replace("Cím: ", "");
                                }
                                else if(t1.MunkaId != null)
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

                                    var epcim = (from v in ke.vEpuletElsCim where v.Eid == rend.Eid select new { v.CimSzoveg, v.Cid }).FirstOrDefault();
                                    tbCim.Text = (epcim == null ? string.Empty : epcim.CimSzoveg);
                                    eid = rend.Eid;
                                    cid = (epcim == null ? null : (int?)epcim.Cid);

                                    // lakáslista kitöltése
                                    Lakasok = (from o in ke.ORE_t
                                               where o.Eid == rend.Eid
                                               select new Lakas()
                                               {
                                                   oid = o.Oid,
                                                   lepcsohaz = string.IsNullOrEmpty(o.Lepcsohaz) ? null : o.Lepcsohaz,
                                                   orglepcsohaz = string.IsNullOrEmpty(o.Lepcsohaz) ? null : o.Lepcsohaz,
                                                   emelet = string.IsNullOrEmpty(o.Emelet) ? null : o.Emelet,
                                                   emeletjelkod = string.IsNullOrEmpty(o.EmeletJelKod) ? null : o.EmeletJelKod,
                                                   orgemeletjelkod = string.IsNullOrEmpty(o.EmeletJelKod) ? null : o.EmeletJelKod,
                                                   ajto = string.IsNullOrEmpty(o.Ajto) ? null : o.Ajto,
                                                   orgajto = string.IsNullOrEmpty(o.Ajto) ? null : o.Ajto,
                                                   ajtotores = string.IsNullOrEmpty(o.Ajtotores) ? null : o.Ajtotores,
                                                   orgajtotores = string.IsNullOrEmpty(o.Ajtotores) ? null : o.Ajtotores,
                                                   torolve = o.Torolve,
                                                   megjegyzes = string.IsNullOrEmpty(o.Megjegyzes) ? null : o.Megjegyzes,
                                                   orgmegjegyzes = string.IsNullOrEmpty(o.Megjegyzes) ? null : o.Megjegyzes,
                                                   EvesEllenorzesIndokolt = false,
                                                   torlendo = false,
                                                   uj = false,
                                                   Modositott = false,

                                                   //OrgTulaj = (from t in ke.vOreTulaj1 where t.Oid == o.Oid select t).Any() ? null : (from t in ke.vOreTulaj1 where t.Oid == o.Oid select t).FirstOrDefault().OreTulajNev,

                                                   NemTorolhetoEsetiRendelesMiatt = false
                                                   /* 
                                                       * Ez így nagyon lassú még mindig (ÖRE táblát kell karbantartani?)
                                                       * 
                                                       * NemTorolhetoEsetiRendelesMiatt = o.MunkaTargya_cs.Where(mt => mt.Munka_t.MunkaStatuszKod != "TOROLVE" && mt.Munka_t.Rendeles_t.MunkaTipusKod != "SORMUNKA" && mt.Munka_t.Rendeles_t.MunkaTipusKod != "SORMUNKA_MUSZ").Any()
                                                   || o.Rendeles_t.Where(r => r.MunkaTipusKod != "SORMUNKA" && r.MunkaTipusKod != "SORMUNKA_MUSZ").Any() */
                                                   
                                               }).ToList();

                                    foreach (var la in Lakasok)
                                    {
                                        la.Tulaj = la.OrgTulaj;
                                    }

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

                                if (!(t1.MunkaId == null && !chkKemenysepro.Checked))
                                {
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
                                              EvesEllenorzes = v.EvesEllenorzesIndokolt,
                                              OreTulaj = v.Tulajdonos // ez így csalás, a tanúsítványhoz rögzített adatot hozza
                                              // OreTulaj = v.Oid == null ? null : ke.vOreTulaj1.Where(t => t.Oid == v.Oid).Any() ? null : ke.vOreTulaj1.Where(t => t.Oid == v.Oid).First().OreTulajNev // ez így nagyon lassú
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
                                    // amíg a lakástulajdonosadat lekérdezése lassú, addig a tanúsítványhoz rögzített értéket jelenítjük meg
                                    if (Lakasok != null)
                                    {
                                        foreach (Lakas l in Lakasok)
                                        {
                                            l.vonalkod = (from t in t2 where t.oid == l.oid && t.nyomtatvanytipuskod == "LAKASTANUSITVANY" select t.Vonalkod).FirstOrDefault() == null ? "" : (from t in t2 where t.oid == l.oid select t.Vonalkod).FirstOrDefault();
                                            if (l.vonalkod == t1.Vonalkod)
                                            {
                                                l.Tulaj = t1.OreTulaj;
                                            }
                                            else
                                            {
                                                Tan q = (from to in t2 where to.Vonalkod == l.vonalkod select to).FirstOrDefault();
                                                if (q != null)
                                                {
                                                    l.Tulaj = q.OreTulaj;
                                                }
                                            }

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

                                    tvVonalkodok.EndUpdate();

                                    tvVonalkodok.BeginUpdate();

                                    tvVonalkodok.ExpandAll();
                                    tvVonalkodok.SelectedNode = VkTree[ta.Vonalkod];
                                    tvVonalkodok.SelectedNode.EnsureVisible();

                                    tvVonalkodok.EndUpdate();
                                    #endregion
                                    #endregion
                                }
                            }
                        }
                        #endregion
                        else
                        #region Van alapértelmezett főtanúsítvány
                        {
                            if (t1.Vonalkod == tbVk.Text || t2.Find(x => x.Vonalkod == tbVk.Text) != null || t3.Find(y => y.Vonalkod == tbVk.Text) != null || GetTanTipus(tbVk.Text) == "KETSZERZART" || GetTanTipus(tbVk.Text) == "TORLES" || PirosSzurke.Find(v0 => v0 == tbVk.Text) != null)
                            #region Memóriában szerepel a beolvasott vonalkód vagy kétszer zárt (99, 9999) vagy törlés (0000)
                            {
                                if(PirosSzurke.Find(v0 => v0 == tbLastVk.Text) != null)
                                {
                                    // előreolvasott figyelmeztetőt kell kezelni
                                    switch (GetTanTipus(tbVk.Text))
                                    {
                                        case "TORLES":
                                            PirosSzurke.Remove(PirosSzurke.Find(v0 => v0 == tbLastVk.Text));
                                            tbFigy.Text = string.Empty;
                                            foreach (var a in PirosSzurke)
                                            {
                                                tbFigy.Text += a + "\r\n";
                                            }
                                            break;
                                        default:
                                            MessageBox.Show("Ez a vonalkód már szerepel a beolvasott vonalkódok között");
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (GetTanTipus(tbVk.Text))
                                    {
                                        case "KETSZERZART":
                                            if (tbLak.Text != "" && tbLak.Text != "-")
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

                                                    if (!VkTan[tbFo.Text].Ketszerzart && ml != null) // eddig nem volt kétszer zárt->csak második látogatás értesítővel együtt lehet kétszer zárt
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
                                        case "TORLES": // TODO: ezt át kell nézni (láncolat törlése; adatbázisból törlés)
                                            string SelectedVonalkod = VkTree.FirstOrDefault(x => x.Value == tvVonalkodok.SelectedNode).Key;

                                            var ch = (from Tan v in VkTan.Values where v.SzuloVk == VkTan[SelectedVonalkod].Vonalkod && (v.NemTorolheto == true || v.NemMozgathato == true) select v);
                                            if (VkTan[SelectedVonalkod].NemMozgathato == false && VkTan[SelectedVonalkod].NemTorolheto == false && !ch.Any() && VkTan[SelectedVonalkod].db != vkInDb.IsmertVk) // adatbázisbeli tanúsítványt nem törlünk (a sebesség miatt nincs minden információ az adatbázisban)
                                            {
                                                ch = (from Tan v in VkTan.Values where v.SzuloVk == VkTan[SelectedVonalkod].Vonalkod select v).ToList();
                                                foreach (Tan x in ch)
                                                {
                                                    if (x.db == vkInDb.IsmertVk)
                                                    {
                                                        MessageBox.Show(string.Format("Adatbázisból törlendő tanúsítvány: {0} ({1}); NemTorolheto státusz: {2}", x.Vonalkod, x.TanId.ToString(),x.NemTorolheto.ToString()));
                                                        // TODO: adatbázisból törlés megvalósítása, ha minden infot be tudunk olvasni (pl. eseti megrendelés)
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
                                                    MessageBox.Show(string.Format("Adatbázisból törlendő tanúsítvány: {0} ({1}); Nemtorolheto statusz: {2}", VkTan[SelectedVonalkod].Vonalkod, VkTan[SelectedVonalkod].TanId.ToString(),VkTan[SelectedVonalkod].NemTorolheto.ToString()));
                                                    // TODO: adatbázisból törlés megvalósítása, ha minden infot be tudunk olvasni (pl. eseti megrendelés)
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

                                        case "FOTANUSITVANY":
                                            tvVonalkodok.SelectedNode = VkTree[tbLastVk.Text];
                                            tvVonalkodok.SelectedNode.EnsureVisible();
                                            tvVonalkodok.Enabled = true; // TODO: meg kell keresni azt, hogy hol nem állítja korrekt módon az értékeket; ez csak tüneti kezelés
                                            grLakasLista.Enabled = false; // TODO: meg kell keresni azt, hogy hol nem állítja korrekt módon az értékeket; ez csak tüneti kezelés
                                            break;

                                        case "LAKASTANUSITVANY":
                                            tvVonalkodok.BeginUpdate();
                                            tvVonalkodok.SelectedNode = VkTree[tbLastVk.Text];
                                            tvVonalkodok.SelectedNode.EnsureVisible();
                                            tvVonalkodok.EndUpdate();
                                            break;

                                        default: // TODO: át kell nézni, hogy a tree-beli választás után jól működik-e a mozgatás (tblastvk használata kérdéses)
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
                                                    if (MessageBox.Show("Biztos, hogy " + ((string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") ? "az épülethez" : "más lakáshoz") + " tartozik a "+tbLastVk.Text+" vonalkódú "+ NyTip[VkTan[tbLastVk.Text].nyomtatvanytipuskod]+"?\n\r("+VkTan[tbLastVk.Text].SzuloVk+" helyett " + ((string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") ? VkTan[tbFo.Text].Vonalkod : VkTan[tbLak.Text].Vonalkod)+")", "Nyomtatványáthelyezés", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                                    {
                                                            VkTan[tbVk.Text].SzuloId = (string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") ? VkTan[tbFo.Text].TanId : VkTan[tbLak.Text].TanId;
                                                            VkTan[tbVk.Text].SzuloVk = (string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") ? VkTan[tbFo.Text].Vonalkod : VkTan[tbLak.Text].Vonalkod;
                                                            VkTan[tbVk.Text].SzuloNyomtTipKod = (string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") ? VkTan[tbFo.Text].nyomtatvanytipuskod : VkTan[tbLak.Text].nyomtatvanytipuskod;
                                                            VkTan[tbVk.Text].SzuloIdModositott = true;
                                                            VkTan[tbVk.Text].oid = (string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") ? VkTan[tbFo.Text].oid : VkTan[tbLak.Text].oid;

                                                            if ((string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-")) // lakás->főtanúsítvány mozgatás
                                                            {
                                                                t2.Add(VkTan[tbVk.Text]);
                                                                t3.Remove(t3.Find(r => r.Vonalkod == tbVk.Text));
                                                            }
                                                            else // más lakáshoz tartozik, nincs szintváltás
                                                            {
                                                                Tan v = (from w in t3 where w.Vonalkod == tbVk.Text select w).FirstOrDefault();
                                                                if (v != null)
                                                                {
                                                                    v.SzuloId = (string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") ? VkTan[tbFo.Text].TanId : VkTan[tbLak.Text].TanId;
                                                                    v.SzuloIdModositott = true;
                                                                }
                                                            }

                                                            tvVonalkodok.BeginUpdate();

                                                            VkTree[tbVk.Text].Remove();
                                                            VkTree.Remove(tbVk.Text);
                                                            TreeNode tln = VkTree[string.IsNullOrEmpty(tbLak.Text) ? tbFo.Text : tbLak.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (" + NyTip[VkTan[tbVk.Text].nyomtatvanytipuskod] + ")");
                                                            VkTree.Add(tbVk.Text, tln);
                                                            tvVonalkodok.SelectedNode = tln;
                                                            tvVonalkodok.SelectedNode.EnsureVisible();

                                                            tvVonalkodok.EndUpdate();
                                                        }
                                                    }
                                                }
                                            }
                                            else if (tp == null && t1.TanId == VkTan[tbVk.Text].SzuloId && !string.IsNullOrEmpty(tbLak.Text)) // főtanúsítvány->lakástanúsítvány mozgatás
                                            {
                                                if (VkTan[tbVk.Text].NemMozgathato)
                                                {
                                                    MessageBox.Show(string.Format("A {0} nem mozgatható, a munkatárgyaknál rögzített hiba van.", GetTanTipus(tbVk.Text)));
                                                }

                                                else if (MessageBox.Show("Biztos, hogy " + ((string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") ? "az épülethez" : "más lakáshoz") + " tartozik a " + tbLastVk.Text + " vonalkódú " + NyTip[VkTan[tbLastVk.Text].nyomtatvanytipuskod] + "?\n\r(" + VkTan[tbLastVk.Text].SzuloVk + " helyett " + ((string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") ? VkTan[tbFo.Text].Vonalkod : VkTan[tbLak.Text].Vonalkod)+")", "Nyomtatványáthelyezés", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
                                                    TreeNode tln = VkTree[(string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") ? tbFo.Text : tbLak.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (" + NyTip[VkTan[tbVk.Text].nyomtatvanytipuskod] + ")");
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
                                            MessageBox.Show("Új főtanúsítvány, az előző rögzítését be kell fejezni");
                                            break;
                                        case "LAKASTANUSITVANY":
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

                                            VkTan.Add(tluj.Vonalkod, tluj);
                                            t2.Add(tluj);

                                            tvVonalkodok.BeginUpdate();
                                            TreeNode tln = VkTree[tbFo.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (Új " + NyTip[tluj.nyomtatvanytipuskod] + ")");
                                            VkTree.Add(tbVk.Text, tln);
                                                
                                            

                                            foreach(var v in PirosSzurke)
                                            {
                                                Tan tluj1 = new Tan();
                                                tluj1.Vonalkod = v;
                                                tluj1.SzuloId = tluj.TanId;
                                                tluj1.SzuloVk = tluj.Vonalkod;
                                                tluj1.SzuloNyomtTipKod = tluj.nyomtatvanytipuskod;
                                                tluj1.MunkaId = tluj.MunkaId;
                                                tluj1.FotanId = tluj.FotanId;
                                                tluj1.FotanVk = tluj.FotanVk;
                                                tluj1.nyomtatvanytipuskod = GetTanTipus(v);
                                                tluj1.db = vkInDb.UjVk;
                                                tluj1.Sor1 = t1.Sor1;
                                                tluj1.Sor2 = t1.Sor2;
                                                tluj1.oid = tluj.oid;

                                                VkTan.Add(tluj1.Vonalkod, tluj1);
                                                t3.Add(tluj1);

                                                TreeNode tln1 = VkTree[tluj.Vonalkod].Nodes.Add(tluj1.Vonalkod + " (Új " + NyTip[tluj1.nyomtatvanytipuskod] + ")");
                                                VkTree.Add(tluj1.Vonalkod, tln1);
                                            }

                                            PirosSzurke.Clear();
                                            tbFigy.Text = string.Empty;


                                            tvVonalkodok.SelectedNode = tln;
                                            tvVonalkodok.SelectedNode.ExpandAll();
                                            tvVonalkodok.SelectedNode.EnsureVisible();

                                            tvVonalkodok.EndUpdate();

                                            break;

                                        default:
                                            if(chkKemenysepro.Checked && (GetTanTipus(tbVk.Text) == "PIROSFIGYELMEZTETO" || GetTanTipus(tbVk.Text) == "SZURKEERTESITO"))
                                            {
                                                if(string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-") // nincs lakástanúsítvány vagy a főtanúsítvány van kiválasztva
                                                {
                                                    if(MessageBox.Show("A már beolvasott főtanúsítványhoz tartozik a figyelmeztető?","Tanúsítványválasztás", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                                    {
                                                        // főtanúsítványhoz kell hozzáadni
                                                        Tan tlujf = new Tan();
                                                        tlujf.Vonalkod = tbVk.Text;
                                                        tlujf.SzuloId = t1.TanId;
                                                        tlujf.SzuloVk = t1.Vonalkod;
                                                        tlujf.SzuloNyomtTipKod = t1.nyomtatvanytipuskod;
                                                        tlujf.MunkaId = t1.MunkaId;
                                                        tlujf.FotanId = t1.TanId;
                                                        tlujf.FotanVk = t1.Vonalkod;
                                                        tlujf.nyomtatvanytipuskod = GetTanTipus(tbVk.Text);
                                                        tlujf.db = vkInDb.UjVk;
                                                        tlujf.Sor1 = t1.Sor1;
                                                        tlujf.Sor2 = t1.Sor2;
                                                        tlujf.BeolvasasIdopontja = DateTime.Now;
                                                        tlujf.oid = t1.oid;

                                                        //tbLak.Text = tluj.Vonalkod;

                                                        VkTan.Add(tlujf.Vonalkod, tlujf);
                                                        t2.Add(tlujf);

                                                        tvVonalkodok.BeginUpdate();

                                                        TreeNode tln4 = VkTree[tbFo.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (Új " + NyTip[tlujf.nyomtatvanytipuskod] + ")");
                                                        VkTree.Add(tbVk.Text, tln4);

                                                        tvVonalkodok.SelectedNode = tln4;
                                                        tvVonalkodok.SelectedNode.ExpandAll();
                                                        tvVonalkodok.SelectedNode.EnsureVisible();

                                                        tvVonalkodok.EndUpdate();
                                                    }
                                                    else
                                                    {
                                                        PirosSzurke.Add(tbVk.Text);
                                                        tbFigy.Visible = true;
                                                        tbFigy.Text = string.Empty;
                                                        foreach (var a in PirosSzurke)
                                                        {
                                                            tbFigy.Text += a + "\r\n";
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    PirosSzurke.Add(tbVk.Text);
                                                    tbFigy.Visible = true;
                                                    tbFigy.Text = string.Empty;
                                                    foreach (var a in PirosSzurke)
                                                    {
                                                        tbFigy.Text += a + "\r\n";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Tan tluj2 = new Tan();
                                                tluj2.Vonalkod = tbVk.Text;

                                                if (string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-")
                                                {
                                                    // főtanúsítványhoz kell hozzáadni

                                                    tluj2.SzuloId = t1.TanId;
                                                    tluj2.SzuloVk = t1.Vonalkod;
                                                    tluj2.SzuloNyomtTipKod = t1.nyomtatvanytipuskod;
                                                    tluj2.MunkaId = t1.MunkaId;
                                                    tluj2.FotanId = t1.TanId;
                                                    tluj2.FotanVk = t1.Vonalkod;
                                                    tluj2.nyomtatvanytipuskod = GetTanTipus(tbVk.Text);
                                                    tluj2.db = vkInDb.UjVk;
                                                    tluj2.Sor1 = t1.Sor1;
                                                    tluj2.Sor2 = t1.Sor2;
                                                    tluj2.BeolvasasIdopontja = DateTime.Now;
                                                    tluj2.oid = t1.oid;

                                                    //tbLak.Text = tluj.Vonalkod;

                                                    VkTan.Add(tluj2.Vonalkod, tluj2);
                                                    t2.Add(tluj2);

                                                    tvVonalkodok.BeginUpdate();

                                                    TreeNode tln2 = VkTree[tbFo.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (Új " + NyTip[tluj2.nyomtatvanytipuskod] + ")");
                                                    VkTree.Add(tbVk.Text, tln2);

                                                    tvVonalkodok.SelectedNode = tln2;
                                                    tvVonalkodok.SelectedNode.EnsureVisible();

                                                    tvVonalkodok.EndUpdate();
                                                }
                                                else
                                                {
                                                // lakástanúsítványhoz kell hozzáadni
                                                    Tan tlujn = new Tan();
                                                    tlujn.Vonalkod = tbVk.Text;
                                                    tlujn.SzuloId = VkTan[tbLak.Text].TanId;
                                                    tlujn.SzuloVk = VkTan[tbLak.Text].Vonalkod;
                                                    tlujn.SzuloNyomtTipKod = VkTan[tbLak.Text].nyomtatvanytipuskod;
                                                    tlujn.MunkaId = VkTan[tbLak.Text].MunkaId;
                                                    tlujn.FotanId = VkTan[tbLak.Text].FotanId;
                                                    tlujn.FotanVk = VkTan[tbLak.Text].FotanVk;
                                                    tlujn.nyomtatvanytipuskod = GetTanTipus(tbVk.Text);
                                                    tlujn.BeolvasasIdopontja = DateTime.Now;
                                                    tlujn.db = vkInDb.UjVk;
                                                    tlujn.Sor1 = t1.Sor1;
                                                    tlujn.Sor2 = t1.Sor2;
                                                    tlujn.oid = VkTan[tbLak.Text].oid;

                                                    VkTan.Add(tlujn.Vonalkod, tlujn);
                                                    t3.Add(tlujn);

                                                    tvVonalkodok.BeginUpdate();

                                                    TreeNode tln3 = VkTree[tbLak.Text].Nodes.Add(VkTan[tbVk.Text].Vonalkod + " (Új " + NyTip[tlujn.nyomtatvanytipuskod] + ")");
                                                    VkTree.Add(tbVk.Text, tln3);

                                                    tvVonalkodok.SelectedNode = tln3;
                                                    tvVonalkodok.SelectedNode.ExpandAll();
                                                    tvVonalkodok.SelectedNode.EnsureVisible();

                                                    tvVonalkodok.EndUpdate();
                                                }
                                            }
                                            break;
                                    }
                                }
                                #endregion
                                else
                                #region Adatbázisban szereplő, memóriába nem beolvasott vonalkód (másik munkához tartozik)
                                {
                                    MessageBox.Show("Más munkához tartozó vonalkód, itt nem rögzíthető");
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
                    tbLepcsohaz.Enabled = false;
                    cbEmeletjel.Enabled = false;
                    tbAjto.Enabled = false;
                    tbAjtotores.Enabled = false;
                    tbMegj.Enabled = false;
                    chkEvesEllenorzes.Enabled = false;
                    tbTulaj.Enabled = false;
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
                tvVonalkodok.BeginUpdate();
                tvVonalkodok.Nodes[tvVonalkodok.Nodes.Count - 1].EnsureVisible();
                tvVonalkodok.SelectedNode.EnsureVisible();
                tvVonalkodok.EndUpdate();
            }

            e.Node.BackColor = System.Drawing.SystemColors.ButtonFace;
            string Vonalkod = VkTree.FirstOrDefault(x => x.Value == e.Node).Key;

            switch (GetTanTipus(Vonalkod))
            {
                case "FOTANUSITVANY":
                    {
                        tbLak.Text = "";

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
                        tbTulaj.Text = VkTan[Vonalkod].OreTulaj;
                    break;
                    }
                case "LAKASTANUSITVANY":
                    {
                        tbLak.Text = Vonalkod;

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
                        tbTulaj.Text = VkTan[Vonalkod].OreTulaj;
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

                        tbLepcsohaz.Text = VkTan[VkTan[Vonalkod].SzuloVk].lepcsohaz;
                        cbEmeletjel.SelectedIndex = cbEmeletjel.FindStringExact(VkTan[VkTan[Vonalkod].SzuloVk].emelet);
                        tbAjto.Text = VkTan[VkTan[Vonalkod].SzuloVk].ajto;
                        tbAjtotores.Text = VkTan[VkTan[Vonalkod].SzuloVk].ajtotores;
                        tbOid.Text = VkTan[VkTan[Vonalkod].SzuloVk].oid.ToString();
                        tbMegj.Text = VkTan[VkTan[Vonalkod].SzuloVk].OreMegjegyzes;
                        tbTulaj.Text = VkTan[VkTan[Vonalkod].SzuloVk].OreTulaj;

                    break;
                    }
            }

            if (VkTan[Vonalkod].NemMozgathato == false && VkTan[Vonalkod].NemTorolheto == false && VkTan[Vonalkod].db != vkInDb.IsmertVk && !(from Tan v in VkTan.Values where v.SzuloVk == VkTan[Vonalkod].Vonalkod && (v.NemTorolheto == true || v.NemMozgathato == true) select v).Any() && (GetTanTipus(Vonalkod) != "FOTANUSITVANY")) // adatbázisban szereplő tanúsítványt sem törlünk (vagy implementálni kell a törlést, de a bonyolult lekérdezés sebessége miatt nincs minden információ a memóriában)
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

            /* TODO  (ezt az adatot nem kértük le az adatbázisból, mert túl lassú)

                        int? so = VkTan[Vonalkod].oid != null ? VkTan[Vonalkod].oid : VkTan[Vonalkod].SzuloVk == string.Empty ? (int?)null : VkTan[VkTan[Vonalkod].SzuloVk].oid;
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
            if ((Lakasok != null && !(Lakasok.Count() == 1 && (t2 == null || t2.Count == 0))) && !chkKemenysepro.Checked)
            {
                grLakasLista.Visible = true;
                if (string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-")
                {
                    // nincs érvényes lakástanúsítvány -> az összes adatot megjeleníti+nincs kiválasztott sor
                    UpdateGridDataSource(Lakasok);
                    grLakasLista.ClearSelection();
                    grLakasLista.AutoResizeColumns();
                    grLakasLista.Visible = !chkKemenysepro.Checked;

                    AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas;
                    tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
                    tbLepcsohaz.Enabled = false;
                    cbEmeletjel.Enabled = false;
                    tbAjto.Enabled = false;
                    tbAjtotores.Enabled = false;
                    tbMegj.Enabled = false;
                    chkEvesEllenorzes.Enabled = false;
                    tbTulaj.Enabled = false;

                    lblStatus.Text = "Vonalkódolvasásra vár"; // string.Empty;
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
                        tbLepcsohaz.Enabled = false;
                        cbEmeletjel.Enabled = false;
                        tbAjto.Enabled = false;
                        tbAjtotores.Enabled = false;
                        tbMegj.Enabled = false;
                        chkEvesEllenorzes.Enabled = false;
                        tbTulaj.Enabled = false;

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

                        AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas;
                        tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
                        tbLepcsohaz.Enabled = false;
                        cbEmeletjel.Enabled = false;
                        tbAjto.Enabled = false;
                        tbAjtotores.Enabled = false;
                        tbMegj.Enabled = false;
                        chkEvesEllenorzes.Enabled = false;
                        tbTulaj.Enabled = false;

                        // TODO: grid selectionchange-be mozgatandó
                        btnLakasadatModositas.Visible = false;
                        btnUjLakas.Visible = false;

                    }
                    else
                    {
                        // szerkeszthető lakásadat, de nem választható; új nem hozható létre
                        UpdateGridDataSource(Lakasok, x => x.vonalkod == tbLak.Text);
                        grLakasLista.Enabled = false;

                        AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas;
                        tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
                        tbLepcsohaz.Enabled = false;
                        cbEmeletjel.Enabled = false;
                        tbAjto.Enabled = false;
                        tbAjtotores.Enabled = false;
                        tbMegj.Enabled = false;
                        chkEvesEllenorzes.Enabled = false;
                        tbTulaj.Enabled = false;

                        lblStatus.Text = "A lakás adatai szerkeszthetőek";

                        // TODO ???: grid selectionchange-be mozgatandó
                        btnLakasadatModositas.Visible = true;
                        btnUjLakas.Visible = false;
                    }
                }
                else if (!chkKemenysepro.Checked)
                {
                    // itt engedni kell új lakást hozzáadni, ha van lakástanúsítvány. Csak admin módban, csak ha van munkaid
                    UpdateGridDataSource(Lakasok, x => !String.IsNullOrEmpty(x.vonalkod));
                    grLakasLista.Refresh();
                    PrevGridEnabledStatus = grLakasLista.Enabled;
                    grLakasLista.Enabled = true;
                    grLakasLista.ClearSelection();

                    // TODO ???: grid selectionchange-be mozgatandó
                    if (!string.IsNullOrEmpty(tbFo.Text) && VkTan[tbFo.Text].MunkaId != null)
                    {
                        btnUjLakas.Visible = true;
                        btnLakasadatModositas.Visible = false;
                    }

                    AdatbevitelStatusz = AdatBevitel.LakasadatKereses;
                    tbLepcsohaz.Enabled = false;
                    cbEmeletjel.Enabled = false;
                    tbAjto.Enabled = false;
                    tbAjtotores.Enabled = false;
                    tbMegj.Enabled = false;
                    chkEvesEllenorzes.Enabled = false;
                    tbTulaj.Enabled = false;

                    tbVk.Select();
                    lblStatus.Text = "Vonalkódbeolvasásra vár";
                }
            }
            else if (!chkKemenysepro.Checked && t2.Count() != 0)
            {
                // TODO ???: grid selectionchange-be mozgatandó
                // üres a lakáslista->új lakást lehet hozzáadni
                btnUjLakas.Visible = true;
                AdatbevitelStatusz = AdatBevitel.LakasadatKereses;
                tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
                tbLepcsohaz.Enabled = false;
                cbEmeletjel.Enabled = false;
                tbAjto.Enabled = false;
                tbAjtotores.Enabled = false;
                tbMegj.Enabled = false;
                chkEvesEllenorzes.Enabled = false;
                tbTulaj.Enabled = false;

                btnUjLakas.Visible = true;
                btnLakasadatModositas.Visible = false;
                tbVk.Select();
                lblStatus.Text = "Vonalkódbeolvasásra vár";
            }
        }

        private void grLakasLista_SelectionChanged(object sender, EventArgs e)
        {

            Console.WriteLine("GridSelectionChanged (adatbevitelstátusz: {0}; selected row-oid: {1}, number of selected rows: {2}, tblak: {3})", AdatbevitelStatusz, grLakasLista.SelectedRows.Count == 0 ? "Nincs kiválasztott sor" : grLakasLista.SelectedRows[0].Cells["oid"].Value.ToString(), grLakasLista.SelectedRows.Count.ToString(), tbLak.Text);

            if((AdatbevitelStatusz != AdatBevitel.LakasadatKereses) || (AdatbevitelStatusz == AdatBevitel.LakasadatKereses && grLakasLista.Focused))
            {
                if (grLakasLista.SelectedRows.Count == 0)
                {
                    tbLepcsohaz.Text = string.Empty;
                    cbEmeletjel.SelectedIndex = cbEmeletjel.FindStringExact("<Nincs emelet>");
                    tbAjto.Text = string.Empty;
                    tbAjtotores.Text = string.Empty;
                    chkEvesEllenorzes.Checked = false;
                    tbMegj.Text = string.Empty;
                    tbTulaj.Text = string.Empty;
                    tbOid.Text = string.Empty;
                    btnLakasadatModositas.Visible = false;
                }
                else
                {
                    int loid;
                    if (!int.TryParse(grLakasLista.SelectedRows[0].Cells["oid"].Value.ToString(), out loid))
                    {
                        MessageBox.Show("Nincs lakásazonosító (hiba)");
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
                        tbTulaj.Text = ls.Tulaj;
                        tbOid.Text = loid.ToString();

                        if (!ls.NemTorolhetoEsetiRendelesMiatt) // módosítható-e a lakásadat
                        {
                            btnLakasadatModositas.Visible = true;
                        }

                        switch (AdatbevitelStatusz) // ez nem biztos, hogy kell...
                        {
                            case AdatBevitel.LakasadatKereses:
                            btnLakasValasztas.Visible = true;
                            tbLepcsohaz.Enabled = false;
                            cbEmeletjel.Enabled = false;
                            tbAjto.Enabled = false;
                            tbAjtotores.Enabled = false;
                            tbMegj.Enabled = false;
                            chkEvesEllenorzes.Enabled = false;
                            tbTulaj.Enabled = false;

                            break;
                            default:
                            break;
                        }
                    }
                }

                tbVk.Select();
            }
            else
            {
                // lakáskeresés üzemmód
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

                    if(!chkKemenysepro.Checked)
                    {
                        #region Új lakások adatbázisba írása
                        ORE_t o = new ORE_t();
                        foreach (Lakas luj in (from l in Lakasok where l.uj == true select l))
                        {
                            o.Eid = (int)eid;
                            o.Cid = (int)cid;
                            o.Lepcsohaz = luj.lepcsohaz;
                            o.Emelet = luj.emelet;
                            o.EmeletJelKod = luj.emeletjelkod;
                            o.Ajto = luj.ajto;
                            o.Ajtotores = luj.ajtotores;
                            o.LegterOsszekottetesekSzama = 1;
                            o.COMeroKoteles = false;
                            o.Megjegyzes = "Vonalkódolvasó alkalmazásban rögzítve";
                            o.Ervenyes = true;
                            o.UtolsoModositasDatuma = DateTime.Now;
                            o.UtolsoModositoId = (int)LoginHelper.LoggedOnUser.userid;
                            o.IdoszakosanHasznalt = false;
                            o.SormunkaPeriodus = 1;
                            o.Torolve = false;

                            ke.ORE_t.Add(o);
                            ke.SaveChanges(); // ekkor már kitölti az oid-t

                            if (!string.IsNullOrEmpty(luj.vonalkod))
                            {
                                VkTan[luj.vonalkod].oid = o.Oid;
                                var t = t2.Find(r => r.Vonalkod == tbVk.Text);
                                if(t != null)
                                {
                                    t.oid = o.Oid;
                                }
                            }

                            ke.AuditLog.Add(new AuditLog {
                                UserID = (int)LoginHelper.LoggedOnUser.userid,
                                EventDateUTC = DateTime.Now,
                                EventType = "A",
                                TableName = "Ore",
                                RecordID = o.Oid.ToString(),
                                ColumnName = "*Összes oszlop",
                                OriginalValue = null,
                                NewValue =  string.Format("Eid: {0}; Lepcsohaz: {1}; Emelet: {2}; EmeletJelKod: {3}; Ajto: {4}; Ajtotores: {5}; Megjegyzes: {6}",o.Eid.ToString(),o.Lepcsohaz,o.Emelet,o.EmeletJelKod,o.Ajto,o.Ajtotores,o.Megjegyzes)
                                });
                        }
                        #endregion
                        #region Módosított lakások adatbázisba írása
                        foreach(Lakas l in (from l in Lakasok select l))
                        {
                            l.lepcsohaz = string.IsNullOrEmpty(l.lepcsohaz) ? null : l.lepcsohaz;
                            l.emelet = string.IsNullOrEmpty(l.emelet) ? null : l.emelet;
                            l.emeletjelkod = string.IsNullOrEmpty(l.emeletjelkod) ? null : l.emeletjelkod;
                            l.ajto = string.IsNullOrEmpty(l.ajto) ? null : l.ajto;
                            l.ajtotores = string.IsNullOrEmpty(l.ajtotores) ? null : l.ajtotores;
                            l.megjegyzes = string.IsNullOrEmpty(l.megjegyzes) ? null : l.megjegyzes;
                            l.Tulaj = string.IsNullOrEmpty(l.Tulaj) ? null : l.Tulaj;
                            // l.orglepcsohaz = string.IsNullOrEmpty(l.orglepcsohaz) ? null : l.orglepcsohaz;

                        }

                        /* foreach (Lakas l in (from l in Lakasok where l.uj == false && (
                                             (!(string.IsNullOrEmpty(l.lepcsohaz) && string.IsNullOrEmpty(l.orglepcsohaz)) && (l.lepcsohaz != l.orglepcsohaz)) ||
                                             (!(string.IsNullOrEmpty(l.emeletjelkod) && string.IsNullOrEmpty(l.orgemeletjelkod)) && (l.emeletjelkod != l.orgemeletjelkod)) ||
                                             (!(string.IsNullOrEmpty(l.ajto) && string.IsNullOrEmpty(l.orgajto)) && (l.ajto != l.orgajto)) ||
                                             (!(string.IsNullOrEmpty(l.ajtotores) && string.IsNullOrEmpty(l.orgajtotores)) && (l.ajtotores != l.orgajtotores)) ||
                                             (!(string.IsNullOrEmpty(l.megjegyzes) && string.IsNullOrEmpty(l.orgmegjegyzes)) && (l.megjegyzes != l.orgmegjegyzes)))
                                             select l))
                                             */
                        foreach (Lakas l in (from l in Lakasok where l.uj == false && ((l.lepcsohaz != l.orglepcsohaz) || (l.emeletjelkod != l.orgemeletjelkod) || (l.ajto != l.orgajto) || (l.ajtotores != l.orgajtotores) || (l.megjegyzes != l.orgmegjegyzes)) select l))
                        {
                            o = (from dl in ke.ORE_t where dl.Oid == l.oid select dl).FirstOrDefault();
                            if(o != null) // elvileg nem lehet null
                            {
                                if (l.lepcsohaz != l.orglepcsohaz)
                                {
                                    o.Lepcsohaz = l.lepcsohaz;
                                    ke.AuditLog.Add(new AuditLog
                                    {
                                        UserID = (int)LoginHelper.LoggedOnUser.userid,
                                        EventDateUTC = DateTime.Now,
                                        EventType = "M",
                                        TableName = "Ore",
                                        RecordID = o.Oid.ToString(),
                                        ColumnName = "LepcsoHaz",
                                        OriginalValue = l.orglepcsohaz,
                                        NewValue = l.lepcsohaz
                                    });
                                }

                                if(l.emeletjelkod != l.orgemeletjelkod)
                                {
                                    o.EmeletJelKod = l.emeletjelkod;

                                    ke.AuditLog.Add(new AuditLog
                                    {
                                        UserID = (int)LoginHelper.LoggedOnUser.userid,
                                        EventDateUTC = DateTime.Now,
                                        EventType = "M",
                                        TableName = "Ore",
                                        RecordID = o.Oid.ToString(),
                                        ColumnName = "EmeletJelKod",
                                        OriginalValue = l.orgemeletjelkod,
                                        NewValue = l.emeletjelkod
                                    });
                                    o.Emelet = l.emelet;
                                }

                                if (l.ajto != l.orgajto)
                                {
                                    o.Ajto = l.ajto;

                                    ke.AuditLog.Add(new AuditLog
                                    {
                                        UserID = (int)LoginHelper.LoggedOnUser.userid,
                                        EventDateUTC = DateTime.Now,
                                        EventType = "M",
                                        TableName = "Ore",
                                        RecordID = o.Oid.ToString(),
                                        ColumnName = "Ajto",
                                        OriginalValue = l.orgajto,
                                        NewValue = l.ajto
                                    });
                                }

                                if (l.ajtotores != l.orgajtotores)
                                {
                                    o.Ajtotores = l.ajtotores;

                                    ke.AuditLog.Add(new AuditLog
                                    {
                                        UserID = (int)LoginHelper.LoggedOnUser.userid,
                                        EventDateUTC = DateTime.Now,
                                        EventType = "M",
                                        TableName = "Ore",
                                        RecordID = o.Oid.ToString(),
                                        ColumnName = "Ajtotores",
                                        OriginalValue = l.orgajtotores,
                                        NewValue = l.ajtotores
                                    });
                                }

                                if (l.megjegyzes != l.orgmegjegyzes)
                                {
                                    o.Megjegyzes = l.megjegyzes;

                                    ke.AuditLog.Add(new AuditLog
                                    {
                                        UserID = (int)LoginHelper.LoggedOnUser.userid,
                                        EventDateUTC = DateTime.Now,
                                        EventType = "M",
                                        TableName = "Ore",
                                        RecordID = o.Oid.ToString(),
                                        ColumnName = "Megjegyzes",
                                        OriginalValue = l.orgmegjegyzes,
                                        NewValue = l.megjegyzes
                                    });
                                }
                            }
                            else
                            {
                                Console.WriteLine("Nem azonosított lakás, ez hiba (lakásadatmódosítások adatbázisba írása");
                            }
                        }
                        #endregion
                    }


                    #region Tanúsítványok adatbázisba írása
                    if (t2 != null)
                    {
                        foreach (Tan v in (from w in t2 where w.db == vkInDb.UjVk select w))
                        {
                            Tanusitvany_t t = new Tanusitvany_t();
                            t.MunkaId = v.MunkaId;
                            t.Technikai = v.MunkaId == null ? true : false;
                            t.Vonalkod = v.Vonalkod;
                            t.SzulotanusitvanyId = v.SzuloId;
                            t.NyomtatvanyTipusKod = v.nyomtatvanytipuskod;
                            t.Oid = v.oid;
                            t.BeolvasasIdopontja = v.BeolvasasIdopontja;
                            t.Ketszerzart = v.Ketszerzart;
                            t.EvesEllenorzesIndokolt = v.EvesEllenorzes;
                            t.Tulajdonos = v.OreTulaj;

                            ke.Tanusitvany_t.Add(t);
                        }
                        ke.SaveChanges();

                        foreach (Tan v in (from w in t2 where w.TanId == null select w)) // új lakástanúsítványok-TanId visszakeresése
                        {
                            Tan y = (from w in ke.Tanusitvany_t where w.Vonalkod == v.Vonalkod select new Tan() { TanId = w.TanusitvanyId }).FirstOrDefault();
                            v.TanId = y.TanId;

                            ke.AuditLog.Add(new AuditLog
                            {
                                UserID = (int)LoginHelper.LoggedOnUser.userid,
                                EventDateUTC = DateTime.Now,
                                EventType = "A",
                                TableName = "Tanusitvany",
                                RecordID = v.TanId.ToString(),
                                ColumnName = "*Összes oszlop",
                                OriginalValue = null,
                                NewValue = string.Format("Vonalkod: {0}; NyomtatvanyTipusKod: {1}; SzulotanusitvanyId: {2}; MunkaId: {3}; Ketszerzart: {4}; EvesEllenorzesIndokolt: {5}", v.Vonalkod, v.nyomtatvanytipuskod, v.SzuloId.ToString(), v.MunkaId.ToString(), v.Ketszerzart, v.EvesEllenorzes)
                            });
                        }

                        foreach (Tan v in (from w in t2 where w.SzuloVk != "" && w.SzuloId == null select w)) // hiányzó szuloid-k beírása
                        {
                            Tan y = (from w in ke.Tanusitvany_t where w.Vonalkod == v.SzuloVk select new Tan() { TanId = w.TanusitvanyId }).FirstOrDefault();
                            v.SzuloId = y.TanId;
                            if(v.db == vkInDb.IsmertVk)
                            {
                                Console.WriteLine("Ismert tanúsítvány hiányzó szuloid pótolva-audit hiányzik");

                                ke.AuditLog.Add(new AuditLog
                                {
                                    UserID = (int)LoginHelper.LoggedOnUser.userid,
                                    EventDateUTC = DateTime.Now,
                                    EventType = "M",
                                    TableName = "Tanusitvany",
                                    RecordID = v.TanId.ToString(),
                                    ColumnName = "SzuloId",
                                    OriginalValue = null,
                                    NewValue = y.TanId.ToString()
                                });
                            }
                        }

                        foreach (Tan v in (from w in t2 where w.SzuloIdModositott == true select w))
                        {
                            var x = (from w in ke.Tanusitvany_t where w.TanusitvanyId == v.TanId select w).First();

                            if(v.db == vkInDb.IsmertVk)
                            {
                                ke.AuditLog.Add(new AuditLog
                                {
                                    UserID = (int)LoginHelper.LoggedOnUser.userid,
                                    EventDateUTC = DateTime.Now,
                                    EventType = "M",
                                    TableName = "Tanusitvany",
                                    RecordID = v.TanId.ToString(),
                                    ColumnName = "SzuloId",
                                    OriginalValue = x.SzulotanusitvanyId.ToString(),
                                    NewValue = v.SzuloId.ToString()
                                });
                            }

                            x.SzulotanusitvanyId = v.SzuloId;
                        }

                        foreach (Tan v in (from w in t2 where w.nyomtatvanytipuskod == "LAKASTANUSITVANY" select w))
                        {
                            var x = (from w in ke.Tanusitvany_t where w.TanusitvanyId == v.TanId select w).First();

                            if (v.db == vkInDb.IsmertVk && x.Oid != v.oid)
                            {
                                ke.AuditLog.Add(new AuditLog
                                {
                                    UserID = (int)LoginHelper.LoggedOnUser.userid,
                                    EventDateUTC = DateTime.Now,
                                    EventType = "M",
                                    TableName = "Tanusitvany",
                                    RecordID = v.TanId.ToString(),
                                    ColumnName = "Oid",
                                    OriginalValue = x.Oid.ToString(),
                                    NewValue = v.oid.ToString()
                                });
                            }

                            x.Oid = v.oid;
                        }

                        // meglevő tanúsítványok-tulajdonosadat kitöltése
                        if (Lakasok != null) // TODO: még ellenőrizendő, hogy elég-e ez a szűrés
                        { 
                            foreach (Lakas l in (from lm in Lakasok where !string.IsNullOrEmpty(lm.vonalkod) && !(string.IsNullOrEmpty(lm.OrgTulaj) && string.IsNullOrEmpty(lm.Tulaj)) && (lm.OrgTulaj != lm.Tulaj) select lm))
                            {
                                var t = (from tx in ke.Tanusitvany_t where tx.Vonalkod == l.vonalkod select tx).FirstOrDefault();
                                if(t != null)
                                {
                                    ke.AuditLog.Add(new AuditLog
                                    {
                                        UserID = (int)LoginHelper.LoggedOnUser.userid,
                                        EventDateUTC = DateTime.Now,
                                        EventType = "M",
                                        TableName = "Tanusitvany",
                                        RecordID = t.TanusitvanyId.ToString(),
                                        ColumnName = "Tulajdonos",
                                        OriginalValue = t.Tulajdonos,
                                        NewValue = l.Tulaj
                                    });

                                    t.Tulajdonos = l.Tulaj;
                                }
                            }
                        }
                    }

                    if (t3 != null)
                    {
                        foreach (Tan v in (from w in t3 select w)) // oid hozzárendelések rendberakása
                        {
                            Tan y = (from w in t2 where w.Vonalkod == v.SzuloVk select w).FirstOrDefault();
                            if(v.oid != y.oid)
                            {
                                v.oid = y.oid;
                                if(v.db == vkInDb.IsmertVk)
                                {
                                    var z = (from w in ke.Tanusitvany_t where w.Vonalkod == v.Vonalkod select w).FirstOrDefault();
                                    if(z != null) // elvileg nem lehet null
                                    {
                                        ke.AuditLog.Add(new AuditLog
                                        {
                                            UserID = (int)LoginHelper.LoggedOnUser.userid,
                                            EventDateUTC = DateTime.Now,
                                            EventType = "M",
                                            TableName = "Tanusitvany",
                                            RecordID = z.TanusitvanyId.ToString(),
                                            ColumnName = "Oid",
                                            OriginalValue = z.Oid.ToString(),
                                            NewValue = y.oid.ToString()
                                        });
                                        z.Oid = y.oid;
                                    }
                                }
                            }
                        }

                        foreach (Tan v in (from w in t3 where !string.IsNullOrEmpty(w.SzuloVk) && w.SzuloId == null select w)) // hiányzó szuloid-k beírása
                        {
                            Tan y = (from w in ke.Tanusitvany_t where w.Vonalkod == v.SzuloVk select new Tan() { TanId = w.TanusitvanyId }).FirstOrDefault();
                            if (v.db == vkInDb.IsmertVk)
                            {
                                ke.AuditLog.Add(new AuditLog
                                {
                                    UserID = (int)LoginHelper.LoggedOnUser.userid,
                                    EventDateUTC = DateTime.Now,
                                    EventType = "M",
                                    TableName = "Tanusitvany",
                                    RecordID = v.TanId.ToString(),
                                    ColumnName = "SzuloId",
                                    OriginalValue = v.SzuloId.ToString(),
                                    NewValue = y.TanId.ToString()
                                });
                            }
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

                        foreach (Tan v in (from w in t3 where w.SzuloIdModositott == true && w.db == vkInDb.IsmertVk select w))
                        {
                            var x = (from w in ke.Tanusitvany_t where w.TanusitvanyId == v.TanId select w).First();

                            if (v.db == vkInDb.IsmertVk)
                            {
                                ke.AuditLog.Add(new AuditLog
                                {
                                    UserID = (int)LoginHelper.LoggedOnUser.userid,
                                    EventDateUTC = DateTime.Now,
                                    EventType = "M",
                                    TableName = "Tanusitvany",
                                    RecordID = v.TanId.ToString(),
                                    ColumnName = "SzuloId",
                                    OriginalValue = x.SzulotanusitvanyId.ToString(),
                                    NewValue = v.SzuloId.ToString()
                                });
                            }
                            x.SzulotanusitvanyId = v.SzuloId;
                        }

                        ke.SaveChanges();

                        foreach (Tan v in (from w in t3 where w.TanId == null select w)) // új lakástanúsítványok-TanId visszakeresése
                        {
                            Tan y = (from w in ke.Tanusitvany_t where w.Vonalkod == v.Vonalkod select new Tan() { TanId = w.TanusitvanyId }).FirstOrDefault();
                            v.TanId = y.TanId;

                            ke.AuditLog.Add(new AuditLog()
                            {
                                UserID = (int)LoginHelper.LoggedOnUser.userid,
                                EventDateUTC = DateTime.Now,
                                EventType = "A",
                                TableName = "Tanusitvany",
                                RecordID = v.TanId.ToString(),
                                ColumnName = "*Összes oszlop",
                                OriginalValue = null,
                                NewValue = string.Format("Vonalkod: {0}; NyomtatvanyTipusKod: {1}; SzulotanusitvanyId: {2}; MunkaId: {3}; Ketszerzart: {4}; EvesEllenorzesIndokolt: {5}", v.Vonalkod, v.nyomtatvanytipuskod, v.SzuloId.ToString(), v.MunkaId.ToString(), v.Ketszerzart, v.EvesEllenorzes)
                            });
                        }
                        #endregion
                        // TODO: munkatárgya kezelése
                        // TODO: első zártság, második zártság, munkatargyahiba kezelése
                    }

                    {
                        bool tmp = chkKemenysepro.Checked;
                        Utilities.ResetAllControls(this);
                        chkKemenysepro.Checked = tmp; // ezt nem kell alapállapotba állítani...
                    }

                    grLakasLista.Visible = false;
                    btnUjLakas.Visible = false;
                    tbLak.Text = "-";
                    lbl99.Visible = false;
                    pb99.Visible = false;
                    lblTorles.Visible = false;
                    pbTorles.Visible = false;
                    VkTan.Clear();
                    VkTree.Clear();
                    VkTan.Clear();
                    VkTree.Clear();
                    ta = null;
                    t1 = null;
                    if (t2 != null)
                    {
                        t2.Clear();
                    }
                    if (t3 != null)
                    {
                        t3.Clear();
                    }
                    if (Lakasok != null)
                    {
                        Lakasok.Clear();
                    }
                    grLakasLista.DataSource = null;
                    grLakasLista.Visible = false;
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
            if (string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-")
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
                {
                    bool tmp = chkKemenysepro.Checked;
                    Utilities.ResetAllControls(this);
                    chkKemenysepro.Checked = tmp; // ezt nem kell alapállapotba állítani...
                }
                
                if(ujfo)
                {
                    using (KotorEntities ke = new KotorEntities())
                    {
                        var ft = (from v in ke.Tanusitvany_t where v.TanusitvanyId == ta.TanId select v).FirstOrDefault();
                        if(ft != null)
                        {
                            ke.Tanusitvany_t.Remove(ft);
                            ke.SaveChanges();
                        }
                    }
                }

                grLakasLista.Enabled = false;
                grLakasLista.DataSource = null;
                grLakasLista.Visible = false;

                btnUjLakas.Visible = false;
                tbLak.Text = "-";
                lbl99.Visible = false;
                pb99.Visible = false;
                lblTorles.Visible = false;
                pbTorles.Visible = false;
                VkTan.Clear();
                VkTree.Clear();
                ta = null;
                t1 = null;
                if (t2 != null)
                {
                    t2.Clear();
                }
                if(t3!=null)
                {
                    t3.Clear();
                }
                if (Lakasok != null)
                {
                    Lakasok.Clear();
                }
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
            tbTulaj.Enabled = !chkKemenysepro.Checked;
            chkEvesEllenorzes.Enabled = !chkKemenysepro.Checked;
            if (grLakasLista.DataSource != null)
            {
                grLakasLista.Visible = !chkKemenysepro.Checked;
            }

        }

        private void UpdateGridDataSource(IEnumerable<Lakas> originalData, params Func<Lakas, bool>[] filters)
        {
            var result = originalData;
            if(result != null)
            {
                foreach (var filter in filters)
                {
                    result = result.Where(filter);
                }

                result.OrderBy(x => x, lakasComparer);

                grLakasLista.DataSource = result.ToList();
                grLakasLista.Refresh();
            }
        }

        private void btnLakasadatSave_Click(object sender, EventArgs e)
        {
            Console.WriteLine("LakasadatSaveClick ({0})", AdatbevitelStatusz.ToString());
            switch (AdatbevitelStatusz)
            {
            case AdatBevitel.Lakasadatmodositas:
                // módosított lakásadatok mentése
                // nem lehet 99. emelet; ajtó, megjegyzés adatok ellenőrzése (ne legyen automatikusan generált adat benne)

                string hibak = string.Empty;
                if(((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Key == "99")
                {
                    hibak = "Az emelet értéke nem lehet 99, kérem a helyes értéket beírni";
                    cbEmeletjel.Select();
                }
                if(tbMegj.Text.Contains("adat nélküli lakás") || tbMegj.Text.Contains(". lakás") || tbMegj.Text.Contains("családi"))
                {
                    if(!string.IsNullOrEmpty(hibak))
                    {
                        hibak += Environment.NewLine;
                    }
                    hibak += "A megjegyzést javítani vagy törölni kell, ha az automatikusan létrehozott adatok változnak (az ajtó adatokat is ellenőrizni kell!)";
                    tbAjto.Select();
                }

                if(string.IsNullOrEmpty(hibak)) // csak akkor van mentés, ha nincs fölismert hibás adat
                {
                    int loid;
                    if (int.TryParse(tbOid.Text, out loid))
                    {
                        var lm = (from l in Lakasok where l.oid == loid select l).FirstOrDefault();
                        lm.lepcsohaz = tbLepcsohaz.Text;
                        if (((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Key == "<NA>")
                        {
                            lm.emeletjelkod = null;
                            lm.emelet = string.Empty;
                        }
                        else
                        {
                            lm.emelet = ((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Value;
                            lm.emeletjelkod = ((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Key;
                        }
                        lm.ajto = tbAjto.Text;
                        lm.ajtotores = tbAjtotores.Text;
                        lm.megjegyzes = tbMegj.Text;
                        lm.Tulaj = tbTulaj.Text;
                        lm.EvesEllenorzesIndokolt = chkEvesEllenorzes.Checked;

                        if (!string.IsNullOrEmpty(lm.vonalkod))
                        {
                            VkTan[lm.vonalkod].lepcsohaz = lm.lepcsohaz;
                            VkTan[lm.vonalkod].emelet = lm.emelet;
                            VkTan[lm.vonalkod].emeletjel = lm.emeletjelkod;
                            VkTan[lm.vonalkod].ajto = lm.ajto;
                            VkTan[lm.vonalkod].ajtotores = lm.ajtotores;
                            VkTan[lm.vonalkod].OreMegjegyzes = lm.megjegyzes;
                            VkTan[lm.vonalkod].EvesEllenorzes = lm.EvesEllenorzesIndokolt;
                            VkTan[lm.vonalkod].OreTulaj = lm.Tulaj;

                            tvVonalkodok.BeginUpdate();
                            VkTree[lm.vonalkod].Text = lm.vonalkod + " (" + (VkTan[lm.vonalkod].db == vkInDb.UjVk ? "Új " : string.Empty) + NyTip[VkTan[lm.vonalkod].nyomtatvanytipuskod] + ")" + (lm.oid == null ? "" : " " + getLakCim(lm.lepcsohaz, lm.emelet, lm.ajto, lm.ajtotores)) + (VkTan[lm.vonalkod].Ketszerzart == true ? KetszerZartSzoveg : "");
                            tvVonalkodok.EndUpdate();
                        }
                    }

                    grLakasLista.Refresh();

                    // ha a vonalkód üres és van kiválasztott lakás, akkor a hozzárendelés is megjelenik
                    if (grLakasLista.SelectedRows.Count != 0 && string.IsNullOrEmpty((String)grLakasLista.SelectedRows[0].Cells["vk"].Value))
                    {
                        btnLakasValasztas.Visible = true;
                    }

                    // lblStatus.Text = "Vonalkód beolvasásra vár";
                    lblStatus.Text = PrevLblStatus;
                    AdatbevitelStatusz = PrevAdatbevitelStatusz;
                    tbBevitelStatusz.Text = AdatbevitelStatusz.ToString() + " (előző érték: " + PrevAdatbevitelStatusz.ToString() + ")"; // debug
                    btnUjLakas.Visible = PrevbtnUjlakasStatus;
                    tbLepcsohaz.Enabled = false; // mentés után mindig nem szerkeszthető állapot következik
                    cbEmeletjel.Enabled = false;
                    tbAjto.Enabled = false;
                    tbAjtotores.Enabled = false;
                    tbMegj.Enabled = false;
                    chkEvesEllenorzes.Enabled = false;
                    tbTulaj.Enabled = false;

                    btnLakasadatSave.Visible = false;
                    btnMegsem.Visible = false;
                    btnLakasadatModositas.Visible = true;
                    tvVonalkodok.Enabled = true;
                    if (AdatbevitelStatusz == AdatBevitel.VonalkodOlvasas)
                    {
                        tbVk.Select();
                        tbLepcsohaz.Enabled = false;
                        cbEmeletjel.Enabled = false;
                        tbAjto.Enabled = false;
                        tbAjtotores.Enabled = false;
                        tbMegj.Enabled = false;
                        chkEvesEllenorzes.Enabled = false;
                        tbTulaj.Enabled = false;
                    }
                }
                break;

            case AdatBevitel.Ujlakas:
                // új lakás adatainak mentése (adatbázisba is az oid miatt)
                // nem lehet 99. emelet

                lblStatus.Text = PrevLblStatus;
                // AdatbevitelStatusz = PrevAdatbevitelStatusz;
                AdatbevitelStatusz = VkTan[tbLak.Text].oid == null ? AdatBevitel.LakasadatKereses : AdatBevitel.VonalkodOlvasas;
                grLakasLista.Enabled = AdatbevitelStatusz == AdatBevitel.LakasadatKereses ? true : false;

                tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
                tbLepcsohaz.Enabled = false; // mentés után mindig nem szerkeszthetőállapot következik
                cbEmeletjel.Enabled = false;
                tbAjto.Enabled = false;
                tbAjtotores.Enabled = false;
                tbMegj.Enabled = false;
                chkEvesEllenorzes.Enabled = false;
                tbTulaj.Enabled = false;

                btnLakasadatSave.Visible = false;
                btnMegsem.Visible = false;
                btnLakasadatModositas.Visible = true;
                btnUjLakas.Visible = true;
                tvVonalkodok.Enabled = true;

                Lakas luj = new Lakas();
                luj.lepcsohaz = tbLepcsohaz.Text;
                if (((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Key == "<NA>")
                {
                    luj.emeletjelkod = null;
                    luj.emelet = string.Empty;
                }
                else
                {
                    luj.emelet = ((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Value;
                    luj.emeletjelkod = ((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Key;
                }
                luj.ajto = tbAjto.Text;
                luj.ajtotores = tbAjtotores.Text;
                luj.megjegyzes = tbMegj.Text;
                luj.Tulaj = tbTulaj.Text;
                luj.EvesEllenorzesIndokolt = chkEvesEllenorzes.Checked;
                luj.uj = true;
                luj.oid = ujlakasid--;
                tbOid.Text = luj.oid.ToString();
                Lakasok.Add(luj);

                // ez így nem működik (datasource sorok száma megnő, de nem írja ki) TODO: miért???
                // var orgLst = grLakasLista.DataSource;
                // ((List<Lakas>)orgLst).Add(luj);
                // ((List<Lakas>)orgLst).OrderBy(x => x, lakasComparer);
                // grLakasLista.DataSource = ((List<Lakas>)orgLst);

                // ((List<Lakas>)grLakasLista.DataSource).Add(luj); // ez így nem működik (datasource sorok száma megnő, de nem írja ki)



                // grLakasLista.Rows.OfType<DataGridViewRow>().Where(x => (int)x.Cells["oid"].Value == luj.oid).ToArray<DataGridViewRow>()[0].Selected = true;

                UpdateGridDataSource(Lakasok, x => string.IsNullOrEmpty(x.vonalkod));
                if(grLakasLista.Rows.OfType<DataGridViewRow>().Where(x => (int)x.Cells["oid"].Value == luj.oid) != null)
                {
                    grLakasLista.Rows.OfType<DataGridViewRow>().Where(x => (int)x.Cells["oid"].Value == luj.oid).ToArray<DataGridViewRow>()[0].Selected = true;
                    grLakasLista.FirstDisplayedScrollingRowIndex = grLakasLista.SelectedRows[0].Index;
                    if(!string.IsNullOrEmpty(tbLak.Text))
                    {
                        btnLakasValasztas.Visible = true;
                    }
                }
                grLakasLista.Refresh();

                break;
            default:
                break;
            }

            if (AdatbevitelStatusz == AdatBevitel.VonalkodOlvasas)
            {
                tbVk.Select();
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

                PrevAdatbevitelStatusz = AdatbevitelStatusz;
                PrevbtnUjlakasStatus = btnUjLakas.Visible;
                PrevLblStatus = lblStatus.Text;

                AdatbevitelStatusz = AdatBevitel.Lakasadatmodositas;
                tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
                tbLepcsohaz.Enabled = true;
                cbEmeletjel.Enabled = true;
                tbAjto.Enabled = true;
                tbAjtotores.Enabled = true;
                tbMegj.Enabled = true;
                chkEvesEllenorzes.Enabled = true;
                tbTulaj.Enabled = true;

                btnLakasadatSave.Visible = true;
                btnLakasadatModositas.Visible = false;
                btnMegsem.Visible = true;
                tvVonalkodok.Enabled = false;
                PrevGridEnabledStatus = grLakasLista.Enabled;
                grLakasLista.Enabled = false;
                lblStatus.Text = "Gridben kijelölt lakás adatainak módosítása";
                tbLepcsohaz.Select();
                PrevbtnUjlakasStatus = btnUjLakas.Visible;
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
            tbLepcsohaz.Enabled = true;
            cbEmeletjel.Enabled = true;
            tbAjto.Enabled = true;
            tbAjtotores.Enabled = true;
            tbMegj.Enabled = true;
            chkEvesEllenorzes.Enabled = true;
            tbTulaj.Enabled = true;

            tbLepcsohaz.Text = string.Empty;
            cbEmeletjel.SelectedIndex = 0;
            tbAjto.Text = string.Empty;
            tbAjtotores.Text = string.Empty;
            tbMegj.Text = string.Empty;
            tbTulaj.Text = string.Empty;
            chkEvesEllenorzes.Checked = false;
            tbOid.Text = string.Empty;

            tbLepcsohaz.Select();
            tvVonalkodok.Enabled = false;
            PrevGridEnabledStatus = grLakasLista.Enabled;
            grLakasLista.Enabled = false;
            grLakasLista.ClearSelection();

            btnLakasadatSave.Visible = true;
            btnMegsem.Visible = true;
            btnLakasadatModositas.Visible = false;
            btnUjLakas.Visible = false;

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
                case AdatBevitel.Ujlakas:
                    tvVonalkodok.Enabled = true;
                    grLakasLista.Enabled = PrevGridEnabledStatus;
                    btnMegsem.Visible = false;
                    btnLakasadatSave.Visible = false;
                    btnLakasadatModositas.Visible = true;
                    btnUjLakas.Visible = PrevbtnUjlakasStatus;

                    AdatbevitelStatusz = PrevAdatbevitelStatusz;
                    tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
                    tbLepcsohaz.Enabled = false; // Mégsem után mindig nem szerkeszthető státusz következik
                    cbEmeletjel.Enabled = false;
                    tbAjto.Enabled = false;
                    tbAjtotores.Enabled = false;
                    tbMegj.Enabled = false;
                    chkEvesEllenorzes.Enabled = false;
                    tbTulaj.Enabled = false;

                // ha a vonalkód üres, akkor a hozzárendelés is megjelenik
                if (grLakasLista.SelectedRows.Count != 0 && string.IsNullOrEmpty((String)grLakasLista.SelectedRows[0].Cells["vk"].Value))
                    {
                        btnLakasValasztas.Visible = true;
                    }

                    // lblStatus.Text = "Vonalkód beolvasásra vár";
                    lblStatus.Text = PrevLblStatus;
                    break;
/*
                case AdatBevitel.Ujlakas:
                    tvVonalkodok.Enabled = true;
                    grLakasLista.Enabled = true;
                    AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas; // itt lehet, hogy keresés kellene??? Vonalkódhozzárendelés???
                    tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug

                    btnMegsem.Visible = false;
                    btnLakasadatSave.Visible = false;
                    btnUjLakas.Visible = true;
                    break;*/

                default:
                    grLakasLista.Enabled = PrevGridEnabledStatus;
                    AdatbevitelStatusz = AdatBevitel.VonalkodOlvasas;
                    tbBevitelStatusz.Text = AdatbevitelStatusz.ToString(); // debug
                    tbLepcsohaz.Enabled = false;
                    cbEmeletjel.Enabled = false;
                    tbAjto.Enabled = false;
                    tbAjtotores.Enabled = false;
                    tbMegj.Enabled = false;
                    chkEvesEllenorzes.Enabled = false;
                    tbTulaj.Enabled = false;

                break;
            }
        }

        private void btnLakasValasztas_Click(object sender, EventArgs e)
        {
            int loid;
            if (int.TryParse(tbOid.Text, out loid) && !(string.IsNullOrEmpty(tbLak.Text) || tbLak.Text == "-"))
            {
                var lm = (from l in Lakasok where l.oid == loid select l).FirstOrDefault();
                lm.vonalkod = tbLak.Text;

                VkTan[tbLak.Text].lepcsohaz = tbLepcsohaz.Text;
                VkTan[tbLak.Text].emelet = ((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Value;
                VkTan[tbLak.Text].emeletjel = ((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Key;
                VkTan[tbLak.Text].ajto = tbAjto.Text;
                VkTan[tbLak.Text].ajtotores = tbAjto.Text;
                VkTan[tbLak.Text].OreMegjegyzes = tbMegj.Text;
                VkTan[tbLak.Text].OreTulaj = tbTulaj.Text;
                VkTan[tbLak.Text].EvesEllenorzes = chkEvesEllenorzes.Checked;
                VkTan[tbLak.Text].oid = loid;

                tvVonalkodok.BeginUpdate();
                VkTree[tbLak.Text].Text = lm.vonalkod + " (" + (VkTan[lm.vonalkod].db == vkInDb.UjVk ? "Új " : string.Empty) + NyTip[VkTan[lm.vonalkod].nyomtatvanytipuskod] + ")" + (lm.oid == null ? "" : " " + getLakCim(lm.lepcsohaz, lm.emelet, lm.ajto, lm.ajtotores)) + (VkTan[lm.vonalkod].Ketszerzart == true ? KetszerZartSzoveg : "");
                tvVonalkodok.EndUpdate();

                UpdateGridDataSource(Lakasok, x => x.vonalkod == lm.vonalkod);

                btnLakasValasztas.Visible = false;
            }
        }

        private void tbLepcsohaz_TextChanged(object sender, EventArgs e)
        {
            if (AdatbevitelStatusz == AdatBevitel.LakasadatKereses && !grLakasLista.Focused)
            {
                LakasKereses();
            }
        }

        private void cbEmeletjel_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (AdatbevitelStatusz == AdatBevitel.LakasadatKereses && !grLakasLista.Focused)
            {
                LakasKereses();
            }
        }

        private void tbAjto_TextChanged(object sender, EventArgs e)
        {
            if (AdatbevitelStatusz == AdatBevitel.LakasadatKereses && !grLakasLista.Focused)
            {
                LakasKereses();
            }
        }

        private void tbAjtotores_TextChanged(object sender, EventArgs e)
        {
            if (AdatbevitelStatusz == AdatBevitel.LakasadatKereses && !grLakasLista.Focused)
            {
                LakasKereses();
            }
        }

        private void LakasKereses()
        {
            UpdateGridDataSource(Lakasok,
                x => String.IsNullOrEmpty(x.vonalkod)
                && (String.IsNullOrEmpty(tbLepcsohaz.Text) || x.lepcsohaz == tbLepcsohaz.Text)
                && (cbEmeletjel.SelectedItem == null || String.IsNullOrEmpty(((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Key) || ((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Key == "<NA>" || x.emeletjelkod == ((KeyValuePair<String, String>)cbEmeletjel.SelectedItem).Key)
                && (String.IsNullOrEmpty(tbAjto.Text) || x.ajto == tbAjto.Text)
                && (String.IsNullOrEmpty(tbAjtotores.Text) || x.ajtotores == tbAjtotores.Text));
            grLakasLista.ClearSelection();
        }

        private void tbVk_Enter(object sender, EventArgs e)
        {
            Console.WriteLine("tbVk_Enter");
            if(tvVonalkodok.SelectedNode != null)
            {
                tvVonalkodok.SelectedNode.EnsureVisible();
            }
            // TODO: státuszmentés; a tbVk_Leave-ben visszaállítás, ha nem volt vonalkódolvasás
        }

        private void tbFo_TextChanged(object sender, EventArgs e)
        {
                chkKemenysepro.Enabled = string.IsNullOrEmpty(tbFo.Text);
        }
    }
}
