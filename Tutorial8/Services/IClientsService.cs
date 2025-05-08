using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientTripDTO>> GetTripsByClientId(int clientId);
    Task<bool> DoesClientExist(int clientId);

    Task<int> CreateClient(ClientDTO clientDTO);
    Task<bool> IsValidClientData(ClientDTO clientDto);

    Task<bool> RegisterClientToTrip(int clientId, int tripId);
}