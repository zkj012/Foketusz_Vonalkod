//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Vonalkod
{
    using System;
    using System.Collections.Generic;
    
    public partial class TervezesiEgyseg_t
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TervezesiEgyseg_t()
        {
            this.Munka_t = new HashSet<Munka_t>();
        }
    
        public int TervezesiEgysegId { get; set; }
        public Nullable<int> TervezesiEgysegCsoportId { get; set; }
        public int Eid { get; set; }
        public Nullable<int> Sorszam { get; set; }
        public Nullable<double> KalkulaltMunkaegyenertek { get; set; }
        public Nullable<double> MegadottMunkaegyenertek { get; set; }
        public Nullable<double> MegadottRaforditasPercben { get; set; }
        public string Megjegyzes { get; set; }
        public byte[] Version { get; set; }
        public bool Lezart { get; set; }
        public Nullable<int> Periodus { get; set; }
        public string Vonalkod { get; set; }
        public string Nev { get; set; }
        public System.DateTime LetrehozasDatuma { get; set; }
        public bool MuszakiFelulvizsgalat { get; set; }
        public string kacs_Vonalkod2014 { get; set; }
        public Nullable<int> kacs_RegioId { get; set; }
        public Nullable<int> kacs_MesterkorzetId { get; set; }
        public Nullable<int> kacs_KemenyseproparId { get; set; }
        public Nullable<System.DateTime> Ev { get; set; }
        public Nullable<System.DateTime> kacs_Tanusitvany2015NyomdabaAdva { get; set; }
        public Nullable<bool> kacs_Tanusitvany2015NyomdabaAdando { get; set; }
        public bool Torolve { get; set; }
        public bool Ervenyes { get; set; }
        public Nullable<bool> kacs_TeNevBeallitva { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Munka_t> Munka_t { get; set; }
    }
}
