using Microsoft.Data.SqlClient;
using PracaDomowa_7_2.Models.DTOs;

namespace PracaDomowa_7_2.Services;

public class TripsService : iTripsService
{
    private string conStr = "Data Source=localhost, 1433; User=SA; Password=TwojeHaslo123!; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";
    public async Task<List<TripDTO>> GetAllTripsAsync()
    {
        var trips = new List<TripDTO>();
        //pobierz dane wycieczek
        
        var cmdText = "SELECT * FROM Trip";

        await using (SqlConnection conn = new SqlConnection(conStr))
        await using (SqlCommand cmd = new SqlCommand(cmdText, conn))
        {
            await conn.OpenAsync();
            var _reader = await cmd.ExecuteReaderAsync();

            int idTripOrdinal = _reader.GetOrdinal("IdTrip");

            while (await _reader.ReadAsync())
            {
                trips.Add(new TripDTO()
                {
                    IdTrip = _reader.GetInt32(idTripOrdinal),
                    Name = _reader.GetString(1),
                    Description = _reader.GetString(2),
                    DateFrom = _reader.GetDateTime(3),
                    DateTo = _reader.GetDateTime(4),
                    MaxPeople = _reader.GetInt32(5)
                });
            }
        }

        return trips;
    }
    
    public async Task<List<CountryDTO>> GetTripCountriesAsync(int idTrip)
    {
        var countries = new List<CountryDTO>();
        
        //pobierz dane krajów wycieczki po Id wycieczki
        
        var cmdText = @"SELECT c.IdCountry, c.Name
            FROM Country c
            JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
            WHERE ct.IdTrip = @idTrip";

        await using (SqlConnection conn = new SqlConnection(conStr))
        await using (SqlCommand cmd = new SqlCommand(cmdText, conn))
        {
            cmd.Parameters.AddWithValue("@idTrip", idTrip);
            await conn.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                countries.Add(new CountryDTO
                {
                    IdCountry = reader.GetInt32(reader.GetOrdinal("IdCountry")),
                    Name = reader.GetString(reader.GetOrdinal("Name"))
                });
            }
        }

        return countries;
    }
    
    public async Task<List<TripDTO>> GetTripsByClientIdAsync(int clientId)
    {
        var trips = new List<TripDTO>();

        var cmdText = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                   ct.RegisteredAt, ct.PaymentDate
            FROM Trip t
            INNER JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
            WHERE ct.IdClient = @clientId";

        await using (SqlConnection conn = new SqlConnection(conStr))
        await using (SqlCommand cmd = new SqlCommand(cmdText, conn))
        {
            cmd.Parameters.AddWithValue("@clientId", clientId);
            await conn.OpenAsync();
            var _reader = await cmd.ExecuteReaderAsync();

            int idTripOrdinal = _reader.GetOrdinal("IdTrip");

            while (await _reader.ReadAsync())
            {
                var trip = new TripDTO
                {
                    IdTrip = _reader.GetInt32(idTripOrdinal),
                    Name = _reader.GetString(1),
                    Description = _reader.GetString(2),
                    DateFrom = _reader.GetDateTime(3),
                    DateTo = _reader.GetDateTime(4),
                    MaxPeople = _reader.GetInt32(5),
                    RegisteredAt = _reader.GetInt32(_reader.GetOrdinal("RegisteredAt")),
                    PaymentDate = _reader.IsDBNull(_reader.GetOrdinal("PaymentDate"))
                        ? null
                        : _reader.GetInt32(_reader.GetOrdinal("PaymentDate")),
                    Countries = new List<CountryDTO>()
                };
                trip.Countries = await GetTripCountriesAsync(trip.IdTrip);
                trips.Add(trip);
            }
        }

        return trips;
    }
    
    public async Task<string> AddClientToTripAsync(int clientId, int tripId)
    {
        using (SqlConnection conn = new SqlConnection(conStr)){
            await conn.OpenAsync();
        
            SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                //czy klient istnieje
                
                SqlCommand checkClientCmd =
                    new SqlCommand("SELECT COUNT(1) FROM Client WHERE IdClient = @clientId", conn, transaction);
                    checkClientCmd.Parameters.AddWithValue("@clientId", clientId);
                var clientExists = (int)await checkClientCmd.ExecuteScalarAsync() > 0;
                if (!clientExists)
                {
                    transaction.Rollback();
                    return "Client not found";
                }


                //czy wycieczka istnieje
                
                SqlCommand checkTripCmd =
                new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @tripId", conn, transaction); 
                checkTripCmd.Parameters.AddWithValue("@tripId", tripId);
                var maxPeopleObj = await checkTripCmd.ExecuteScalarAsync();
                if (maxPeopleObj == null)
                {
                    transaction.Rollback();
                    return "Trip not found";
                }

                int maxPeople = (int)maxPeopleObj;

                //liczba zarejestrowanych
                
                SqlCommand countCmd = 
                    new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @tripId", conn, transaction);
                countCmd.Parameters.AddWithValue("@tripId", tripId);
                int registeredCount = (int)await countCmd.ExecuteScalarAsync();
                if (registeredCount >= maxPeople)
                {
                    transaction.Rollback();
                    return "Trip is full";
                }

                //czy klient już jest zapisany
                
                SqlCommand existsCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @tripId AND IdClient = @clientId", conn, transaction);
                existsCmd.Parameters.AddWithValue("@tripId", tripId);
                existsCmd.Parameters.AddWithValue("@clientId", clientId);
                int alreadyRegistered = (int)await existsCmd.ExecuteScalarAsync();
                if (alreadyRegistered > 0)
                {
                    transaction.Rollback();
                    return "Client already registered to this trip";
                }

                //wstaw dane
                
                var insertCmd = new SqlCommand(@"
                INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                VALUES (@clientId, @tripId, @registeredAt)", conn, transaction);
                insertCmd.Parameters.AddWithValue("@clientId", clientId);
                insertCmd.Parameters.AddWithValue("@tripId", tripId);
                insertCmd.Parameters.AddWithValue("@registeredAt", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds());

                await insertCmd.ExecuteNonQueryAsync();
                transaction.Commit();   
                return "OK";
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
    
    public async Task<string> DeleteClientFromTripAsync(int clientId, int tripId)
    {
        await using (SqlConnection conn = new SqlConnection(conStr))
        {
            await conn.OpenAsync();
            
            SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                //czy rejestracja klienta na wycieczkę istnieje
                
                SqlCommand existsCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @tripId AND IdClient = @clientId", conn, transaction);
                existsCmd.Parameters.AddWithValue("@tripId", tripId);
                existsCmd.Parameters.AddWithValue("@clientId", clientId);
                int registered = (int)await existsCmd.ExecuteScalarAsync();
                if (registered == 0)
                {
                    transaction.Rollback();
                    return "Registration not found";
                }
                //usuń rejestrację klienta z wycieczki
                
                var deleteCmd = @"DELETE FROM Client_Trip WHERE IdTrip = @tripId AND IdClient = @clientId";
                await using (SqlCommand cmd = new SqlCommand(deleteCmd, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@tripId", tripId);
                    cmd.Parameters.AddWithValue("@clientId", clientId);
                    
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                    {
                        transaction.Rollback();
                        return "Error";
                    }
                    transaction.Commit();
                    return "OK";
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }


}