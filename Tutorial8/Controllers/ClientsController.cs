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
        private readonly ITripsService _tripsService;

        public ClientsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
        }

        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetTripsByClientId(int id)
        {
            if (!await _tripsService.DoesClientExist(id))
            {
                return NotFound($"Client with ID {id} not found.");
            }

            var trips = await _tripsService.GetTripsByClientId(id);
            if (trips == null || !trips.Any())
            {
                return NotFound($"Client with ID {id} does not have any trips.");
            }

            return Ok(trips);
        }
    }
}
