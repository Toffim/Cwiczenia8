using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;

        public ClientsController(IClientsService clientsService)
        {
            _clientsService = clientsService;
        }

        // Returns all trips of a client specified by id
        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetTripsByClientId(int id)
        {
            if (!await _clientsService.DoesClientExist(id))
            {
                return NotFound($"Client with ID {id} not found.");
            }

            var trips = await _clientsService.GetTripsByClientId(id);
            if (trips == null || !trips.Any())
            {
                return NotFound($"Client with ID {id} does not have any trips.");
            }

            return Ok(trips);
        }
        
        // POST /api/clients
        // Creates new client in database
        [HttpPost]
        public async Task<IActionResult> CreateClient(ClientDTO clientDTO)
        {
            if (clientDTO == null || string.IsNullOrEmpty(clientDTO.FirstName) || string.IsNullOrEmpty(clientDTO.LastName) || string.IsNullOrEmpty(clientDTO.Email))
            {
                return BadRequest("Invalid client data.");
            }

            int clientId = await _clientsService.CreateClient(clientDTO);
            return Ok(clientId);
        }
    }
}
