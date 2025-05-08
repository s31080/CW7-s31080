using Microsoft.AspNetCore.Mvc;
using PracaDomowa_7_2.Services;

namespace PracaDomowa_7_2.Controllers;

[ApiController]
[Route("api/trips")]
public class TripsController : ControllerBase
{
    private readonly iTripsService _tripsService;

    public TripsController(iTripsService tripsService)
    {
        _tripsService = tripsService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllTripsWithCountries()
    {
        var trips = await _tripsService.GetAllTripsAsync();
        foreach (var trip in trips)
        {
            var tripCountries = await _tripsService.GetTripCountriesAsync(trip.IdTrip);
            trip.Countries = tripCountries;
        }
        return Ok(trips);
    }
}