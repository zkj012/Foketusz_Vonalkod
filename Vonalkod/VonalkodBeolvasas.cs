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
Robi:
    Login: timeout kezelés, 4-5 retry kellhet néha; ez nem lenne rossz, ha meglenne - kapott egy try-catchet, ott kell kezelni.
    loggedonuser: elérhető később is - LoginHelper.LoggedOnUser
    lekérdezések, írások hibakezelése
    savechanges: tudtam úgy duplán kattintani, hogy  még egyszer megpróbálta elmenteni (Kotorra is jellemző, hogy –elsősorban beolvasáskor –pl. bejelentkezés- hajlandó duplán reagálni) - try elején this.enabled=false, finallyban this.enabled=true
    logikus struktúra kialakítása (opcionális)

Én:    
    form újrainicializálás mentés után
    törlés (ha törölhető és nincs alatta más (vagy más nem törölhető? Ez utóbbi esetben az alatta levőek törlése is)
    törlés vonalkód megjelenítése: főtanúsítványnál nincs; alatta levőek kezelése
    Új főtanúsítvány:munkához rendelés
    Kétszer zárt lakások kezelése
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

                tvVonalkodok.HideSelection = false;
                tvVonalkodok.DrawMode = TreeViewDrawMode.OwnerDrawText;
                tvVonalkodok.DrawNode += (o, ev) =>
                {
                    if (!ev.Node.TreeView.Focused && ev.Node == ev.Node.TreeView.SelectedNode)
                    {
                        Font treeFont = ev.Node.NodeFont ?? ev.Node.TreeView.Font;
                        ev.Graphics.FillRectangle(Brushes.DodgerBlue, ev.Bounds);
                        // Nem működik ev.Graphics.FillRectangle((Brush) (new SolidBrush(System.Drawing.SystemColors.ButtonFace)), ev.Bounds);
                        ControlPaint.DrawFocusRectangle(ev.Graphics, ev.Bounds, SystemColors.HighlightText, SystemColors.Highlight);
                        TextRenderer.DrawText(ev.Graphics, ev.Node.Text, treeFont, ev.Bounds, SystemColors.HighlightText, TextFormatFlags.GlyphOverhangPadding);
                    }
                    else
                        ev.DrawDefault = true;
                };
            }
            finally
            {
                this.Enabled = true;
            }
            /*            tvVonalkodok.MouseDown += (o, e) =>
                        {
                            TreeNode node = tvVonalkodok.GetNodeAt(e.X, e.Y);
                            if (node != null && node.Bounds.Contains(e.X, e.Y))
                            {
                                tvVonalkodok.SelectedNode = node;
                            }
                        };
            */
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

                    using (KotorEntities ke = new KotorEntities())
                    {
                        tbLastVk.Text = tbVk.Text;

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
                                          nyomtatvanytipuskod = v.NyomtatvanyTipusKod,
                                          nyomtatvanytipus = (from x in ke.NyomtatvanyTipus_m where x.Kod == v.NyomtatvanyTipusKod select new { x.Nev }).FirstOrDefault().Nev,
                                          NemMozgathato = (from w in ke.MunkaTargyaHiba_t where w.FigyelmeztetoVonalkod == v.Vonalkod select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0
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
                                              Sor1 = v.Munka_t.MunkaVegezesKezdes,
                                              Sor2 = v.Munka_t.MunkaVegezesKezdesSor2,
                                              oid = v.Oid,
                                              FotanId = v.SzulotanusitvanyId == null ? v.TanusitvanyId : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.TanusitvanyId : v.Tanusitvany_t2.SzulotanusitvanyId),
                                              FotanVk = v.SzulotanusitvanyId == null ? v.Vonalkod : (v.Tanusitvany_t2.SzulotanusitvanyId == null ? v.Tanusitvany_t2.Vonalkod : v.Tanusitvany_t2.Tanusitvany_t2.Vonalkod),
                                              db = vkInDb.IsmertVk,
                                              nyomtatvanytipuskod = v.NyomtatvanyTipusKod,
                                              nyomtatvanytipus = (from x in ke.NyomtatvanyTipus_m where x.Kod == v.NyomtatvanyTipusKod select new { x.Nev }).FirstOrDefault().Nev
                                              /* A Ta-nál erre nincs szükség, a t1,t2,t3-nál igen
                                              ,
                                              NemMozgathato = (from w in ke.MunkaTargyaHiba_t where w.FigyelmeztetoVonalkod == v.Vonalkod select new { vk = w.FigyelmeztetoVonalkod }).Count() != 0,
                                              NemTorolheto = (from m in ke.MunkaTargya_cs where m.TanusitvanyId == ta.FotanId select new {vx = m.TanusitvanyId }).Count() != 0,
                                              lepcsohaz = v.Oid == null ? null : (from w in ke.ORE_t where w.Oid == v.Oid select new { lh = w.Lepcsohaz }).FirstOrDefault().lh,
                                              emelet = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select new { em = w.Emelet }).FirstOrDefault().em,
                                              emeletjel = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select new { emj = w.EmeletJelKod }).FirstOrDefault().emj,
                                              ajto = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select new { aj = w.Ajto }).FirstOrDefault().aj,
                                              ajtotores = v.Oid == null ? "" : (from w in ke.ORE_t where w.Oid == v.Oid select new { ajt = w.Ajtotores }).FirstOrDefault().ajt */
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
                                              NemTorolheto = (from m in ke.MunkaTargya_cs where m.TanusitvanyId == v.TanusitvanyId select new { vx = m.TanusitvanyId }).Count() != 0
                                              /*,
                                              lepcsohaz = v.Oid == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { lh = w.lepcsohaz }) == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { lh = w.lepcsohaz }).FirstOrDefault().lh,
                                              emelet = v.Oid == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { em = w.emelet }) == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { em = w.emelet }).FirstOrDefault().em,
                                              emeletjel = v.Oid == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { emj = w.emeletjelkod }) == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { emj = w.emeletjelkod }).FirstOrDefault().emj,
                                              ajto = v.Oid == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { aj = w.ajto }) == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { aj = w.ajto }).FirstOrDefault().aj,
                                              ajtotores = v.Oid == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { ajt = w.ajtotores }) == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { ajt = w.ajtotores }).FirstOrDefault().ajt*/
                                          }).ToList();
                                    // ORE adatok kitöltése a t2-ben
                                    foreach (Tan t in t2)
                                    {
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
                                    foreach (Lakas l in Lakasok)
                                    {
                                        l.vonalkod = (from t in t2 where t.oid == l.oid && t.nyomtatvanytipuskod == "LAKASTANUSITVANY" select new { t.Vonalkod }).FirstOrDefault() == null ? "" : (from t in t2 where t.oid == l.oid select new { t.Vonalkod }).FirstOrDefault().ToString();
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
                                              /*,
                                              lepcsohaz = v.Oid == null ? null : (from w in Lakasok where w.oid == v.Oid select new { lh = w.lepcsohaz }).FirstOrDefault().lh,
                                              emelet = v.Oid == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { em = w.emelet }).FirstOrDefault().em,
                                              emeletjel = v.Oid == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { emj = w.emeletjelkod }).FirstOrDefault().emj,
                                              ajto = v.Oid == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { aj = w.ajto }).FirstOrDefault().aj,
                                              ajtotores = v.Oid == null ? "" : (from w in Lakasok where w.oid == v.Oid select new { ajt = w.ajtotores }).FirstOrDefault().ajt */
                                          }).ToList();

                                    // teljes fa újraépítése
                                    tvVonalkodok.Nodes.Clear();
                                    tvVonalkodok.BeginUpdate();
                                    // Dictionary<Tan,TreeNode> tn = new Dictionary<Tan,TreeNode>();

                                    TreeNode tf = tvVonalkodok.Nodes.Add(t1.Vonalkod + " (" + NyTip[t1.nyomtatvanytipuskod] + ")" + (t1.oid == null ? "" : " " + getLakCim(t1.lepcsohaz, t1.emelet, t1.ajto, t1.ajtotores)));
                                    VkTan.Add(t1.Vonalkod, t1);
                                    VkTree.Add(t1.Vonalkod, tf);

                                    foreach (Tan tl in t2)
                                    {
                                        TreeNode tln = tf.Nodes.Add(tl.Vonalkod + " (" + NyTip[tl.nyomtatvanytipuskod] + ")" + (tl.oid == null ? "" : " " + getLakCim(tl.lepcsohaz, tl.emelet, tl.ajto, tl.ajtotores)));
                                        VkTan.Add(tl.Vonalkod, tl);
                                        VkTree.Add(tl.Vonalkod, tln);

                                        foreach (Tan te in (from t in t3
                                                            where t.SzuloId == tl.TanId
                                                            select new Tan()
                                                            {
                                                                Vonalkod = t.Vonalkod,
                                                                TanId = t.TanId,
                                                                SzuloId = t.SzuloId,
                                                                SzuloVk = t.SzuloVk,
                                                                SzuloNyomtTipKod = t.SzuloNyomtTipKod,
                                                                MunkaId = t.MunkaId,
                                                                oid = t.oid,
                                                                FotanId = t.FotanId,
                                                                FotanVk = t.FotanVk,
                                                                nyomtatvanytipuskod = t.nyomtatvanytipuskod
                                                            }))
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
                                    VkTree[ta.Vonalkod].EnsureVisible();


                                    tvVonalkodok.EndUpdate();
                                }
                            }
                            #endregion
                            else
                            #region Van alapértelmezett főtanúsítvány
                            {
                                if (t1.Vonalkod == tbVk.Text || t2.Find(x => x.Vonalkod == tbVk.Text) != null || t3.Find(y => y.Vonalkod == tbVk.Text) != null || tbVk.Text == "99" || tbVk.Text == "9999" || tbVk.Text == "0000")
                                #region Memóriában szerepel a beolvasott vonalkód vagy kétszer zárt (99, 9999) vagy törlés (0000)
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
                                                    tvVonalkodok.BeginUpdate();
                                                    tbLak.Text = tbVk.Text;
                                                    tvVonalkodok.SelectedNode = VkTree[tbVk.Text];
                                                    VkTree[tbVk.Text].EnsureVisible();
                                                    tvVonalkodok.EndUpdate();
                                                    break;
                                                }
                                            default:
                                                {
                                                    var tp = t2.Find(x => x.TanId == VkTan[tbVk.Text].SzuloId);
                                                    if (tp != null) // lakástanúsítvány a szülő
                                                    {
                                                        if (tp.Vonalkod == tbLak.Text)
                                                        {
                                                            tvVonalkodok.BeginUpdate();
                                                            tvVonalkodok.SelectedNode = VkTree[tbVk.Text];
                                                            VkTree[tbVk.Text].EnsureVisible();
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

                                                                    if (tbLak.Text == "")
                                                                    {
                                                                        t2.Add(VkTan[tbVk.Text]);
                                                                        t3.Remove(t3.Find(r => r.Vonalkod == tbVk.Text));
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
                                                    else if (tp == null && t1.TanId == VkTan[tbVk.Text].SzuloId && tbLak.Text != "")
                                                    {
                                                        // főtan->laktan áthelyezés
                                                        // nem helyezhető át, ha van rá hiba rögzítve
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

                                                        tbLak.Text = tluj.Vonalkod;

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

                    /*                tvVonalkodok.BeginUpdate();
                                    Console.WriteLine("Eseményen kívül background csere");
                                    tvVonalkodok.SelectedNode.BackColor = System.Drawing.SystemColors.ButtonFace;
                                    tvVonalkodok.EndUpdate();
                    */
                }
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
         * */

        private void btnBeolvas_Click(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;
                Console.WriteLine("Vonalkódolvasás: {0} (időpont: {1})", tbVk.Text, DateTime.Now.ToString("HH:mm:ss tt"));
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private void tvVonalkodok_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Console.Write("Treeview-Afterselect (selectednode: {0}; argumentnode: {1}) \n\r", tvVonalkodok.SelectedNode == null ? "" : tvVonalkodok.SelectedNode.Text, e.Node == null ? "" : e.Node.Text);
            e.Node.BackColor = System.Drawing.SystemColors.ButtonFace;
            string Vonalkod = VkTree.FirstOrDefault(x => x.Value == e.Node).Key;
            tbLepcsohaz.Text = VkTan[Vonalkod].lepcsohaz;
            tbEmelet.Text = VkTan[Vonalkod].emelet;
            tbEmeletjel.Text = VkTan[Vonalkod].emeletjel;
            tbAjto.Text = VkTan[Vonalkod].ajto;
            tbAjtotores.Text = VkTan[Vonalkod].ajtotores;
            tbOid.Text = VkTan[Vonalkod].oid.ToString();

            tbLak.Text = Vonalkod;

            if (VkTan[Vonalkod].NemMozgathato == false && VkTan[Vonalkod].NemTorolheto == false)
            {
                lblTorles.Visible = true;
                pbTorles.Visible = true;
            }
            else
            {
                lblTorles.Visible = false;
                pbTorles.Visible = false;
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
                        foreach (Tan v in (from w in t2 where w.db == vkInDb.UjVk select new Tan() { MunkaId = w.MunkaId, Vonalkod = w.Vonalkod, SzuloId = w.SzuloId, oid = w.oid, BeolvasasIdopontja = w.BeolvasasIdopontja }))
                        {
                            ke.Tanusitvany_t.Add(new Tanusitvany_t()
                            {
                                MunkaId = v.MunkaId,
                                Vonalkod = v.Vonalkod,
                                SzulotanusitvanyId = v.SzuloId,
                                NyomtatvanyTipusKod = v.nyomtatvanytipuskod,
                                Oid = v.oid,
                                BeolvasasIdopontja = v.BeolvasasIdopontja
                            });
                        }
                        ke.SaveChanges(); // itt tudtam úgy duplán kattintani, hogy még egyszer megpróbálta elmenteni


                        foreach (Tan v in (from w in t2 where w.SzuloVk != "" && w.SzuloId == null select new Tan() { TanId = w.TanId, SzuloVk = w.SzuloVk }))
                        {
                            Tan y = (from w in ke.Tanusitvany_t where w.Vonalkod == v.SzuloVk select new Tan() { TanId = w.TanusitvanyId }).FirstOrDefault();
                            v.SzuloId = y.TanId;
                        }

                        foreach (Tan v in (from w in t2 where w.SzuloIdModositott == true select new Tan() { TanId = w.TanId, MunkaId = w.MunkaId, Vonalkod = w.Vonalkod, SzuloId = w.SzuloId, oid = w.oid, BeolvasasIdopontja = w.BeolvasasIdopontja }))
                        {
                            var x = (from w in ke.Tanusitvany_t where w.TanusitvanyId == v.TanId select w).First();
                            x.SzulotanusitvanyId = v.SzuloId;
                        }
                    }

                    if (t3 != null)
                    {
                        foreach (Tan v in (from w in t3 where w.db == vkInDb.UjVk select new Tan() { MunkaId = w.MunkaId, Vonalkod = w.Vonalkod, SzuloId = w.SzuloId, oid = w.oid, BeolvasasIdopontja = w.BeolvasasIdopontja }))
                        {
                            ke.Tanusitvany_t.Add(new Tanusitvany_t()
                            {
                                MunkaId = v.MunkaId,
                                Vonalkod = v.Vonalkod,
                                SzulotanusitvanyId = v.SzuloId,
                                NyomtatvanyTipusKod = v.nyomtatvanytipuskod,
                                Oid = v.oid,
                                BeolvasasIdopontja = v.BeolvasasIdopontja
                            });
                        }

                        foreach (Tan v in (from w in t3 where w.SzuloIdModositott == true select new Tan() { TanId = w.TanId, MunkaId = w.MunkaId, Vonalkod = w.Vonalkod, SzuloId = w.SzuloId, oid = w.oid, BeolvasasIdopontja = w.BeolvasasIdopontja }))
                        {
                            var x = (from w in ke.Tanusitvany_t where w.TanusitvanyId == v.TanId select w).First();
                            x.SzulotanusitvanyId = v.SzuloId;
                        }
                        ke.SaveChanges();
                    }

                    // ez így nem jó
                    // valami ilyesmi kell: http://stackoverflow.com/questions/15569641/reset-all-the-items-in-a-form
                    /*                VonalkodBeolvasas ujvkfrm = new VonalkodBeolvasas();
                                    this.Hide();
                                    ujvkfrm.Show();
                                    this.Dispose();*/
                }
            }
            finally
            {
                this.Enabled = true;
            }
        }
    }
}
