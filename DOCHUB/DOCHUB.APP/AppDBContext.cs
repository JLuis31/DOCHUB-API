using DOCHUB.APP.Models;
using Microsoft.EntityFrameworkCore;

namespace DOCHUB.APP.Data
{

    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<EstadoDocumento> EstadoDocumentos { get; set; }
        public DbSet<DocumentoHistorial> DocumentoHistorial { get; set; }



    }
}