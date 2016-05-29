using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vonalkod
{
    class Tan
    {
        public string Vonalkod;
        public int? TanId;
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
        public string OreMegjegyzes;
        public string nyomtatvanytipuskod;
        public string nyomtatvanytipus;
        public vkInDb db;
        public string epuletcim;
        public bool Ketszerzart;
        public bool KetszerzartRogzitve;
        public bool SzuloIdModositott;
        public bool NemMozgathato; // hiba van hozzárögzítve
        public bool NemTorolheto; // munkatárgya van hozzárögzítve
        public bool Torlendo; // törlésre megjelölt
        public DateTime? Sor1;
        public DateTime? Sor2;
        public DateTime? BeolvasasIdopontja;
        public bool EvesEllenorzes;
    }
}
