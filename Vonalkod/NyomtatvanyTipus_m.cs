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
    
    public partial class NyomtatvanyTipus_m
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NyomtatvanyTipus_m()
        {
            this.Tanusitvany_t = new HashSet<Tanusitvany_t>();
            this.VonalkodTartomany_t = new HashSet<VonalkodTartomany_t>();
        }
    
        public string Kod { get; set; }
        public string Nev { get; set; }
        public byte[] Version { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tanusitvany_t> Tanusitvany_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VonalkodTartomany_t> VonalkodTartomany_t { get; set; }
    }
}
