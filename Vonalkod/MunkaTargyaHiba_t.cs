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
    
    public partial class MunkaTargyaHiba_t
    {
        public int MunkaTargyaHibaId { get; set; }
        public string HibaTipusKod { get; set; }
        public int MunkaTargyaId { get; set; }
        public string FigyelmeztetoVonalkod { get; set; }
        public System.DateTime EszlelesDatuma { get; set; }
        public bool ElozolegIsFennallt { get; set; }
        public bool Elharult { get; set; }
        public string Leiras { get; set; }
        public byte[] Version { get; set; }
        public string KemenyCsoport { get; set; }
        public Nullable<int> Oid { get; set; }
    
        public virtual ORE_t ORE_t { get; set; }
        public virtual MunkaTargya_cs MunkaTargya_cs { get; set; }
    }
}
