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
    
    public partial class Munkatars_t
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Munkatars_t()
        {
            this.Cim_t = new HashSet<Cim_t>();
            this.Cim_t1 = new HashSet<Cim_t>();
            this.Munkatars_t1 = new HashSet<Munkatars_t>();
            this.Munkatars_t11 = new HashSet<Munkatars_t>();
            this.Tanusitvany_t = new HashSet<Tanusitvany_t>();
            this.Tanusitvany_t1 = new HashSet<Tanusitvany_t>();
            this.ORE_t = new HashSet<ORE_t>();
            this.ORE_t1 = new HashSet<ORE_t>();
            this.Rendeles_t = new HashSet<Rendeles_t>();
            this.Mesterkorzet_t = new HashSet<Mesterkorzet_t>();
            this.Regio_t1 = new HashSet<Regio_t>();
            this.Kozterulet_t = new HashSet<Kozterulet_t>();
            this.Kozterulet_t1 = new HashSet<Kozterulet_t>();
        }
    
        public int MunkatarsId { get; set; }
        public Nullable<int> HRId { get; set; }
        public string Nev { get; set; }
        public string MunkakorKod { get; set; }
        public string KepzettsegKod { get; set; }
        public Nullable<int> MesterkorzetId { get; set; }
        public int RegioId { get; set; }
        public Nullable<int> FelettesId { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> MunkaviszonyKezdete { get; set; }
        public Nullable<System.DateTime> MunkaviszonyVege { get; set; }
        public Nullable<System.DateTime> SzuletesDatuma { get; set; }
        public bool Kulsos { get; set; }
        public bool Aktiv { get; set; }
        public string LoginNev { get; set; }
        public string DomainPrefix { get; set; }
        public string Jelszo { get; set; }
        public byte[] Version { get; set; }
        public string OklevelSzama { get; set; }
        public Nullable<int> Old_TavirDolgozoId { get; set; }
        public Nullable<int> Old_TavirMunkakorId { get; set; }
        public Nullable<int> Old_TavirOnelkerId { get; set; }
        public string Old_TavirMunkakor { get; set; }
        public string Old_TavirJelszo { get; set; }
        public string JelszoClearText { get; set; }
        public string ADUserID { get; set; }
        public bool ADLogin { get; set; }
        public Nullable<int> ElszamolasAlairoId { get; set; }
        public string Vonalkod { get; set; }
        public bool Tesztrendszer { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cim_t> Cim_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cim_t> Cim_t1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Munkatars_t> Munkatars_t1 { get; set; }
        public virtual Munkatars_t Munkatars_t2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Munkatars_t> Munkatars_t11 { get; set; }
        public virtual Munkatars_t Munkatars_t3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tanusitvany_t> Tanusitvany_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tanusitvany_t> Tanusitvany_t1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORE_t> ORE_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORE_t> ORE_t1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Rendeles_t> Rendeles_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Mesterkorzet_t> Mesterkorzet_t { get; set; }
        public virtual Mesterkorzet_t Mesterkorzet_t1 { get; set; }
        public virtual Regio_t Regio_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Regio_t> Regio_t1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Kozterulet_t> Kozterulet_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Kozterulet_t> Kozterulet_t1 { get; set; }
    }
}
