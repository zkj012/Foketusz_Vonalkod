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
    
    public partial class RendelesStatusz_m
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RendelesStatusz_m()
        {
            this.Rendeles_t = new HashSet<Rendeles_t>();
        }
    
        public string Kod { get; set; }
        public string Nev { get; set; }
        public byte[] Version { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Rendeles_t> Rendeles_t { get; set; }
    }
}
