﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class KotorEntities : DbContext
    {
        public KotorEntities()
            : base("name=KotorEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Cim_t> Cim_t { get; set; }
        public virtual DbSet<Tanusitvany_t> Tanusitvany_t { get; set; }
        public virtual DbSet<TervezesiEgyseg_t> TervezesiEgyseg_t { get; set; }
        public virtual DbSet<VonalkodTartomany_t> VonalkodTartomany_t { get; set; }
        public virtual DbSet<Tanusitvany2015FO> Tanusitvany2015FO { get; set; }
        public virtual DbSet<Tanusitvany2015ORE> Tanusitvany2015ORE { get; set; }
        public virtual DbSet<vCim> vCim { get; set; }
        public virtual DbSet<vEpuletElsCim> vEpuletElsCim { get; set; }
        public virtual DbSet<vOreCim> vOreCim { get; set; }
        public virtual DbSet<Munkatars_t> Munkatars_t { get; set; }
        public virtual DbSet<NyomtatvanyTipus_m> NyomtatvanyTipus_m { get; set; }
        public virtual DbSet<MunkaTargyaHiba_t> MunkaTargyaHiba_t { get; set; }
        public virtual DbSet<ORE_t> ORE_t { get; set; }
        public virtual DbSet<Rendeles_t> Rendeles_t { get; set; }
        public virtual DbSet<Munka_t> Munka_t { get; set; }
        public virtual DbSet<RendelesStatusz_m> RendelesStatusz_m { get; set; }
        public virtual DbSet<Mesterkorzet_t> Mesterkorzet_t { get; set; }
        public virtual DbSet<Regio_t> Regio_t { get; set; }
        public virtual DbSet<MunkaTargya_cs> MunkaTargya_cs { get; set; }
        public virtual DbSet<EmeletJel_m> EmeletJel_m { get; set; }
    }
}
