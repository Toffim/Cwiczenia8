using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripsService;

        public TripsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
        }

        // Returns all trips and their information
        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _tripsService.GetTrips();
            return Ok(trips);
        }

        // This is the endpoint I did for myself, despite it not being in the assignment.
        // Returns trip information by trip id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrip(int id)
        {
            if(!await _tripsService.DoesTripExist(id)){
                return NotFound();
            }

            var trip = await _tripsService.GetTrip(id);
            return Ok(trip);
        }
    }
}
