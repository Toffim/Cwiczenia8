using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<TripDTO>> GetTripsByClientId(int clientId);
    Task<bool> DoesClientExist(int clientId);
}