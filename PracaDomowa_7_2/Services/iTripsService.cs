using PracaDomowa_7_2.Models.DTOs;

namespace PracaDomowa_7_2.Services;

public interface iTripsService
{
    Task<List<TripDTO>> GetAllTripsAsync();
    Task<List<CountryDTO>> GetTripCountriesAsync(int idTrip);

    Task<List<TripDTO>> GetTripsByClientIdAsync(int clientId);
    Task<string>? AddClientToTripAsync(int clientId, int tripId);
    
    Task<string> DeleteClientFromTripAsync(int clientId, int tripId);

}