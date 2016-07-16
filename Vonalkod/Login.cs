using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data.Entity.Core;

namespace Vonalkod
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        const int retrycountdefault = 3;
        private int retrycount = 3;

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            Sha1PasswordHasher pwd = new Sha1PasswordHasher();

            using (KotorEntities ke = new KotorEntities())
            {
                try
                {
                tryagain:
                    this.Enabled = false;
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        ke.Database.CommandTimeout = 60;

                        LoginHelper.LoggedOnUser = (from d in ke.Munkatars_t
                                                    where d.LoginNev == textBoxUsername.Text
                                                    select new UserData { userid = d.MunkatarsId, username = d.Nev, loginname = d.LoginNev, regionev = d.Regio_t.Nev, regioid=d.RegioId, munkakorkod = d.MunkakorKod ,jelszohash = d.Jelszo }).FirstOrDefault();
                        if (retrycount <= 0)
                        {
                            this.Controls.Clear();
                            this.InitializeComponent();
                        }
                        else
                        {
                            if (LoginHelper.LoggedOnUser == null || !pwd.VerifyHashedPassword(LoginHelper.LoggedOnUser.jelszohash, textBoxJelszo.Text))
                            {
                                MessageBox.Show("Hibás jelszó vagy felhasználónév");
                            }
                            else
                            {
                                // sikeres bejelentkezés
                                LoginHelper.LoggedOnUser.jelszohash = "";
                                if (LoginHelper.LoggedOnUser.munkakorkod == "KEMENYSEPRO")
                                {
                                    LoginHelper.LoggedOnUser.kemenysepro = true;
                                }
                                else
                                {
                                    LoginHelper.LoggedOnUser.kemenysepro = false;
                                }
                                this.Hide();
                                VonalkodBeolvasas vk = new VonalkodBeolvasas();
                                vk.ShowDialog();
                                this.Close();
                            }
                        }
                    }
                    catch (Exception ex) // EntityCommandExecutionException ex)
                    {
                        if(string.Compare(ex.Message, "The underlying provider failed on Open.") == 0 && (retrycount-- > 0))
                        {
                            lblLoginStatus.Text = string.Format("Bejelentkezés újrapróbálkozás ({0})", retrycountdefault - retrycount);
                            lblLoginStatus.Refresh();
                            goto tryagain;
                        }

                        MessageBox.Show(string.Format("Hiba történt! Kérem próbálja újra! ({0})",ex.Message));
                        retrycount = retrycountdefault;
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }

                }
                finally
                {
                    this.Enabled = true;
                }
            }
        }
    }
}