﻿using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<TripDTO> GetTrip(int id);
    Task<Boolean> DoesTripExist(int id);
    Task<List<CountryDTO>> GetCountriesByTripId(int id);
    
    Task<Boolean> IsTripFull(int tripId);
    Task<bool> IsClientAlreadyRegistered(int clientId, int tripId);
    Task<Boolean> canRegisterClient(int tripId, int clientId);
}