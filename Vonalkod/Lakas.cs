using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vonalkod
{
    class Lakas
    {

        public int? oid { get; set; }
        public string emelet { get; set; }
        public string emeletjelkod { get; set; }
        public string orgemeletjelkod { get; set; }
        public string lepcsohaz { get; set; }
        public string orglepcsohaz { get; set; }
        public string ajto { get; set; }
        public string orgajto { get; set; }
        public string ajtotores { get; set; }
        public string orgajtotores { get; set; }
        public string megjegyzes { get; set; }
        public string orgmegjegyzes { get; set; }
        public string Tulaj { get; set; }
        public string OrgTulaj { get; set; }
        public bool torolve { get; set; }
        public string vonalkod { get; set; }
        public bool torlendo { get; set; }
        public bool uj { get; set; }
        public bool NemTorolhetoEsetiRendelesMiatt { get; set; }
        public bool EvesEllenorzesIndokolt { get; set; }
        public bool Modositott { get; set; }

    }
}
