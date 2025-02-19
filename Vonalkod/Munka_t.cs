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
    
    public partial class Munka_t
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Munka_t()
        {
            this.Tanusitvany_t = new HashSet<Tanusitvany_t>();
            this.MunkaTargya_cs = new HashSet<MunkaTargya_cs>();
        }
    
        public int MunkaId { get; set; }
        public string Munkaszam { get; set; }
        public string MunkaStatuszKod { get; set; }
        public int RendelesId { get; set; }
        public Nullable<int> TervezesiEgysegId { get; set; }
        public Nullable<System.DateTime> TervezettMunkaKezdes { get; set; }
        public Nullable<System.DateTime> TervezettMunkaBefejezes { get; set; }
        public Nullable<System.DateTime> MunkaVegezesKezdes { get; set; }
        public Nullable<System.DateTime> MunkaVegezesBefejezes { get; set; }
        public Nullable<System.DateTime> TervezettPlakatolasKezdes { get; set; }
        public Nullable<System.DateTime> TervezettPlakatolasBefejezes { get; set; }
        public Nullable<System.DateTime> PlakatolasKezdes { get; set; }
        public Nullable<System.DateTime> PlakatolasBefejezes { get; set; }
        public byte[] Version { get; set; }
        public Nullable<System.DateTime> TervezettMunkaKezdesSor2 { get; set; }
        public Nullable<System.DateTime> TervezettMunkaBefejezesSor2 { get; set; }
        public Nullable<System.DateTime> MunkaVegezesKezdesSor2 { get; set; }
        public Nullable<System.DateTime> MunkaVegezesBefejezesSor2 { get; set; }
        public Nullable<System.DateTime> MegrendelovelEgyeztetettKezdes { get; set; }
        public Nullable<System.DateTime> MegrendelovelEgyeztetettBefejezes { get; set; }
        public bool Plakatolando { get; set; }
        public Nullable<double> KalkulaltMunkaegyenertek { get; set; }
        public Nullable<double> KalkulaltMunkaegyenertek2 { get; set; }
        public string TervezettMunkaNapszakSor2Kod { get; set; }
    
        public virtual Rendeles_t Rendeles_t { get; set; }
        public virtual TervezesiEgyseg_t TervezesiEgyseg_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tanusitvany_t> Tanusitvany_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MunkaTargya_cs> MunkaTargya_cs { get; set; }
    }
}
