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
    
    public partial class EmeletJel_m
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EmeletJel_m()
        {
            this.Cim_t = new HashSet<Cim_t>();
            this.ORE_t = new HashSet<ORE_t>();
        }
    
        public string Kod { get; set; }
        public string RovidNev { get; set; }
        public string Nev { get; set; }
        public byte[] Version { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cim_t> Cim_t { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORE_t> ORE_t { get; set; }
    }
}
