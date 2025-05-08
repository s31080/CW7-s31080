using Microsoft.AspNetCore.Mvc;
using PracaDomowa_7_2.Models.DTOs;
using PracaDomowa_7_2.Services;

namespace PracaDomowa_7_2.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ClientsController : ControllerBase
{
    private readonly iClientService _clientService;
        private readonly iTripsService _tripsService;

        public ClientsController(iClientService clientService, iTripsService tripsService)
        {
            _clientService = clientService;
            _tripsService = tripsService;
        }

        
        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetTripsByClientId(int id)
        {
            var client = await _clientService.GetClientByIdAsync(id);

            if (client == null)
            {
                return NotFound("Client not found.");
            }
            
            client.Trips = await _tripsService.GetTripsByClientIdAsync(id);

            if (client.Trips == null || !client.Trips.Any())
            {
                return Ok("Client has no registered trips.");
            }

            return Ok(client.Trips);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClientById(int id)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
                return NotFound("Client not found.");
            return Ok(client);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddClient([FromBody] ClientCreateDTO body)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            var client = await _clientService.AddClientAsync(body);
            return Created($"/clients/{client.IdClient}", client);
        }

        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
        {
            var result = await _tripsService.AddClientToTripAsync(id, tripId);

            return result switch
            {
                "Client not found" => NotFound("Client not found"),
                "Trip not found" => NotFound("Trip not found"),
                "Trip is full" => BadRequest("Trip is full"),
                "Client already registered to this trip" => BadRequest("Client already registered"),
                "OK" => Ok("Client successfully registered to trip"),
                _ => StatusCode(500, "Unknown error")
            };
        }

        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> RemoveClientFromTrip(int id, int tripId)
        {
            var result = await _tripsService.DeleteClientFromTripAsync(id, tripId);
            return result switch
            {
                "Registration not found" => NotFound("Registration not found"),
                "Error" => BadRequest("Error"),
                "OK" => Ok("Client successfully removed from trip"),
                _ => StatusCode(500, "Unknown error")
            };
        }
    }
