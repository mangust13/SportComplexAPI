// Controllers/ClientsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs;
using AutoMapper;

namespace SportComplexAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly SportComplexContext _context;
        private readonly IMapper _mapper;
        public ClientsController(SportComplexContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDTO>>> GetClients()
        {
            var clients = await _context.Clients
                .Include(c => c.Gender)
                .ToListAsync();
            var clientDtos = _mapper.Map<IEnumerable<ClientDTO>>(clients);
            return Ok(clientDtos);
        }
    }
}