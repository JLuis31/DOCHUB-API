using DOCHUB.APP.Models;
using DOCHUB.APP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace DOCHUB.APP.Repositories
{
    public class InformacionPerfilRepository
    {

        private readonly AppDBContext _context;

        public InformacionPerfilRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<InformacionPerfil> GetInformacionPerfil(string idUsuario)
        {

            var informacionPerfil = await _context.usuarios.Where(u => u.id == int.Parse(idUsuario)).FirstOrDefaultAsync();

            return new InformacionPerfil
            {
                Nombre = informacionPerfil?.nombre,
                Email = informacionPerfil?.email,
                Password = informacionPerfil?.password
            };
        }
    }
}