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
        private readonly ITripsService _tripsService;

        public ClientsController(IClientsService clientsService, ITripsService tripsService)
        {
            _clientsService = clientsService;
            _tripsService = tripsService;
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
            if (!await _clientsService.IsValidClientData(clientDTO))
            {
                return BadRequest("Invalid client data.");
            }

            int clientId = await _clientsService.CreateClient(clientDTO);
            return Ok(clientId);
        }
        
        [HttpPut("{clientId}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientToTrip(int clientId, int tripId)
        {
            if (!await _clientsService.DoesClientExist(clientId))
            {
                return NotFound($"Client with ID {clientId} not found.");
            }
            
            if (!await _tripsService.canRegisterClient(tripId, clientId))
            {
                return Problem($"Trip with ID {tripId} cannot register the client.");
            }

            var result = await _clientsService.RegisterClientToTrip(clientId, tripId);
            if (!result)
            {
                return Problem($"Couldn't register client to a trip. ClientId {clientId}, TripId {tripId}");
            }

            return Ok("Client successfully registered.");
        }
    }
}
