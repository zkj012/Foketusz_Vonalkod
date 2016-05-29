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
    
    public partial class ORE_t
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ORE_t()
        {
            this.MunkaTargyaHiba_t = new HashSet<MunkaTargyaHiba_t>();
            this.Tanusitvany_t = new HashSet<Tanusitvany_t>();
            this.Rendeles_t = new HashSet<Rendeles_t>();
            this.MunkaTargya_cs = new HashSet<MunkaTargya_cs>();
        }
    
        public int Oid { get; set; }
        public int Eid { get; set; }
        public int Cid { get; set; }
        public string Lepcsohaz { get; set; }
        public string Emelet { get; set; }
        public string Ajto { get; set; }
        public string Ajtotores { get; set; }
        public string HRSZ3 { get; set; }
        public string OREHasznalatModjaKod { get; set; }
        public int LegterOsszekottetesekSzama { get; set; }
        public bool COMeroKoteles { get; set; }
        public Nullable<System.DateTime> UtolsoCOMeresDatuma { get; set; }
        public Nullable<System.DateTime> UtolsoMuszakiFelulvizsgalatDatuma { get; set; }
        public Nullable<System.DateTime> UtolsoSormunkavegzesDatuma { get; set; }
        public string Megjegyzes { get; set; }
        public bool Ervenyes { get; set; }
        public System.DateTime UtolsoModositasDatuma { get; set; }
        public int UtolsoModositoId { get; set; }
        public Nullable<System.DateTime> EllenorzesDatuma { get; set; }
        public Nullable<int> EllenorzoId { get; set; }
        public byte[] Version { get; set; }
        public Nullable<System.DateTime> COMeroErvenyesseg { get; set; }
        public string Old_2014TavirVonalkod { get; set; }
        public string Old_2014SzigoruLakasvonalkod { get; set; }
        public bool IdoszakosanHasznalt { get; set; }
        public int SormunkaPeriodus { get; set; }
        public string CimFilterMask { get; set; }
        public string kacs_2014tanTulaj { get; set; }
        public string kacs_2014tanKezelo { get; set; }
        public string kacs_2014tanEpulet { get; set; }
        public string kacs_2014tanHasznalo { get; set; }
        public string kacs_2014tanEpulettores { get; set; }
        public string Old_2014TavirKS { get; set; }
        public Nullable<int> Old_2014TavirEpulID { get; set; }
        public Nullable<int> Old_2014SzigoruTuzeloBerendezesMenny { get; set; }
        public bool Torolve { get; set; }
        public string EmeletJelKod { get; set; }
        public Nullable<System.DateTime> IdoszakosHasznalatBejelentesDatuma { get; set; }
        public Nullable<int> kacs_AzonosOre { get; set; }
    
        public virtual Cim_t Cim_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MunkaTargyaHiba_t> MunkaTargyaHiba_t { get; set; }
        public virtual Munkatars_t Munkatars_t { get; set; }
        public virtual Munkatars_t Munkatars_t1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tanusitvany_t> Tanusitvany_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Rendeles_t> Rendeles_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MunkaTargya_cs> MunkaTargya_cs { get; set; }
        public virtual EmeletJel_m EmeletJel_m { get; set; }
    }
}
