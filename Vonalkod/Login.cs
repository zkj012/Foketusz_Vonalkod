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

namespace Vonalkod
{
    public partial class Login : Form
    {
        public class UserData
        {
            public int? userid;
            public string username;
            public string loginname;
            public string regionev;
            public string jelszohash;
        }

        public Login()
        {
            InitializeComponent();
        }

        public UserData LoggedOnUser = new UserData();

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Sha1PasswordHasher pwd = new Sha1PasswordHasher();

            using (KotorEntities ke = new KotorEntities())
            {
                LoggedOnUser = (from d in ke.Munkatars_t
                              where d.LoginNev == textBoxUsername.Text
                              select new UserData { userid = d.MunkatarsId, username = d.Nev, loginname = d.LoginNev, regionev = d.Mesterkorzet_t1.Regio_t.Nev,jelszohash = d.Jelszo }).FirstOrDefault();

                Cursor.Current = Cursors.Default;

                if (LoggedOnUser == null)
                {
                    MessageBox.Show("Nincs ilyen felhasználó");
                }
                else if(pwd.VerifyHashedPassword(LoggedOnUser.jelszohash,textBoxJelszo.Text))
                {
                    // sikeres bejelentkezés
                    this.Hide();
                    VonalkodBeolvasas vk = new VonalkodBeolvasas();
                    vk.ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Hibás jelszó");
                }
            }
        }
    }

    class Sha1PasswordHasher
    {
        /// <summary>
        /// Visszaadja az adott szöveg SHA1 hash kódját.
        /// </summary>
        /// <param name="password">a kódolandó szöveg</param>
        /// <returns>az SHA1 hash kód string reprezentációban (40 karakter hosszú alfanumerikus sztring)</returns>
        /// <remarks>
        /// Ha ugyanezt a műveletet MS SQL adatbázisban akarjuk elkövetni, akkor:
        /// CONVERT(VARCHAR(40), HASHBYTES('SHA1', 'password'), 2)
        /// </remarks>
        public string HashPassword(string password)
        {
            if (password == null) return null;
            SHA1 sha1 = SHA1.Create();
            byte[] input = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha1.ComputeHash(input);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Jelszó egyezés vizsgálata.
        /// </summary>
        /// <param name="hashedPassword">az igazi jelszó SHA1 hash alakban</param>
        /// <param name="providedPassword">a vizsgálandó jelszó plain textként</param>
        public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
            {
                return false;
            }
            string hashedProvidedPassword = HashPassword(providedPassword);
            if (string.Equals(hashedPassword, hashedProvidedPassword, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}