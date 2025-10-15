using DOCHUB.APP.Models;
using DOCHUB.APP.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DOCHUB.APP.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InformacionPerfilController : ControllerBase
    {
        private readonly InformacionPerfilRepository _repository;

        public InformacionPerfilController(InformacionPerfilRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<InformacionPerfil>> GetInformacionPerfil()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var informacionPerfil = await _repository.GetInformacionPerfil(userId);
            if (informacionPerfil == null)
            {
                return NotFound();
            }
            return Ok(informacionPerfil);
        }
    }
}