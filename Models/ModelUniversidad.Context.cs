﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SistemaUniversidadv1._0.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class UniversidadContext : DbContext
    {
        public UniversidadContext()
            : base("name=UniversidadContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CARRERA> CARRERA { get; set; }
        public virtual DbSet<CICLO> CICLO { get; set; }
        public virtual DbSet<CONDICIONESTUDIANTE> CONDICIONESTUDIANTE { get; set; }
        public virtual DbSet<CONDICIONESTUDIANTEMATERIA> CONDICIONESTUDIANTEMATERIA { get; set; }
        public virtual DbSet<CONDICIONUSUARIO> CONDICIONUSUARIO { get; set; }
        public virtual DbSet<ESTUDIANTE> ESTUDIANTE { get; set; }
        public virtual DbSet<ESTUDIANTEMATERIAEXAMEN> ESTUDIANTEMATERIAEXAMEN { get; set; }
        public virtual DbSet<INSCRIPCIONESTUDIANTEMATERIA> INSCRIPCIONESTUDIANTEMATERIA { get; set; }
        public virtual DbSet<LOCALIDAD> LOCALIDAD { get; set; }
        public virtual DbSet<MATERIA> MATERIA { get; set; }
        public virtual DbSet<PROFESORMATERIA> PROFESORMATERIA { get; set; }
        public virtual DbSet<ROL> ROL { get; set; }
        public virtual DbSet<SEXO> SEXO { get; set; }
        public virtual DbSet<USUARIO> USUARIO { get; set; }
    }
}