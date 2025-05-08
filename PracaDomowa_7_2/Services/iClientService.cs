using PracaDomowa_7_2.Models.DTOs;

namespace PracaDomowa_7_2.Services;

public interface iClientService
{
    Task<List<ClientGetDTO>> GetClientsAsync();
    
    Task<ClientGetDTO> GetClientByIdAsync(int clientId);
    Task<ClientGetDTO> AddClientAsync(ClientCreateDTO clientCreateDto);
    
    
}
