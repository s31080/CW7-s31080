using Microsoft.Data.SqlClient;
using PracaDomowa_7_2.Models.DTOs;

namespace PracaDomowa_7_2.Services;

public class ClientService : iClientService
{
    private readonly string conStr =
        "Data Source=localhost, 1433; User=SA; Password=TwojeHaslo123!; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";

    public async Task<List<ClientGetDTO>> GetClientsAsync()
    {
        var clients = new List<ClientGetDTO>();
        //dane klientów
        
        var cmdText = "select * from client";
        
        await using (SqlConnection conn = new SqlConnection(conStr))
        await using (SqlCommand cmd = new SqlCommand(cmdText, conn))
        {
            await conn.OpenAsync();
            var _reader = await cmd.ExecuteReaderAsync();
            
            int idClientOrdinal = _reader.GetOrdinal("Id");

            while (await _reader.ReadAsync())
            {
                clients.Add(new ClientGetDTO()
                {
                    IdClient = _reader.GetInt32(idClientOrdinal),
                    FirstName = _reader.GetString(1),
                    LastName = _reader.GetString(2),
                    Email = _reader.GetString(3),
                    Telephone = _reader.GetString(4),
                    Pesel = _reader.GetString(5),
                    Trips = new List<TripDTO>()
                });
            }
        }
        return clients;
        
    }

    public async Task<ClientGetDTO> GetClientByIdAsync(int clientId)
    {
        var client = new ClientGetDTO();
        //dane klienta po Id klienta
        
        var cmdText = @"select * from client where IdClient = @clientId";
        
        await using (SqlConnection conn = new SqlConnection(conStr))
        await using (SqlCommand cmd = new SqlCommand(cmdText, conn))
        {
            cmd.Parameters.AddWithValue("@clientId", clientId);

            await conn.OpenAsync();
            var _reader = await cmd.ExecuteReaderAsync();
            
            int idClientOrdinal = _reader.GetOrdinal("IdClient");

            while (await _reader.ReadAsync())
            {
                client = new ClientGetDTO
                {
                    IdClient = _reader.GetInt32(idClientOrdinal),
                    FirstName = _reader.GetString(1),
                    LastName = _reader.GetString(2),
                    Email = _reader.GetString(3),
                    Telephone = _reader.GetString(4),
                    Pesel = _reader.GetString(5),
                    Trips = new List<TripDTO>()
                };
            }
        }
        return client;
    }

    public async Task<ClientGetDTO> AddClientAsync(ClientCreateDTO client)
    {
        // Wstaw dane 
        var cmdText = @"INSERT INTO client (FirstName, LastName, Email, Telephone, Pesel)
                        VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
                        SELECT SCOPE_IDENTITY();";
        await using (SqlConnection conn = new SqlConnection(conStr))
        await using (SqlCommand cmd = new SqlCommand(cmdText, conn))
        {
            cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
            cmd.Parameters.AddWithValue("@LastName", client.LastName);
            cmd.Parameters.AddWithValue("@Email", client.Email);
            cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", client.Pesel);
            
            await conn.OpenAsync();
            
            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            
            return new ClientGetDTO()
            {
                IdClient = newId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Telephone = client.Telephone,
                Pesel = client.Pesel
            };
        }
    }
}