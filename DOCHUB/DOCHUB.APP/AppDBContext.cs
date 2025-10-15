using DOCHUB.APP.Models;
using Microsoft.EntityFrameworkCore;

namespace DOCHUB.APP.Data
{

    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Usuario> usuarios { get; set; }
        public DbSet<Documento> documentos { get; set; }
        public DbSet<EstadoDocumento> estadodocumentos { get; set; }
        public DbSet<DocumentoHistorial> documentohistorial { get; set; }



    }
}