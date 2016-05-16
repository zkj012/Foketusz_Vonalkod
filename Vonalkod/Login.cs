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


        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Sha1PasswordHasher pwd = new Sha1PasswordHasher();

            using (KotorEntities ke = new KotorEntities())
            {
                try
                {

                    this.Enabled = false;
                    try
                    {
                        ke.Database.CommandTimeout = 180;

                        LoginHelper.LoggedOnUser = (from d in ke.Munkatars_t
                                                    where d.LoginNev == textBoxUsername.Text
                                                    select new UserData { userid = d.MunkatarsId, username = d.Nev, loginname = d.LoginNev, regionev = d.Regio_t.Nev, regioid=d.RegioId, munkakorkod = d.MunkakorKod ,jelszohash = d.Jelszo }).FirstOrDefault();

                    }
                    catch (Exception ex) // EntityCommandExecutionException ex)
                    {
                        MessageBox.Show("Hiba történt! Kérem próbálja újra!");
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                    if (LoginHelper.LoggedOnUser == null || !pwd.VerifyHashedPassword(LoginHelper.LoggedOnUser.jelszohash, textBoxJelszo.Text)) 
                    {
                        MessageBox.Show("Hibás jelszó vagy felhasználónév");
                    }
                    else
                    {
                        // sikeres bejelentkezés
                        LoginHelper.LoggedOnUser.jelszohash = "";
                        if(LoginHelper.LoggedOnUser.munkakorkod == "KEMENYSEPRO")
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
                finally
                {
                    this.Enabled = true;
                }
            }
        }
    }
}