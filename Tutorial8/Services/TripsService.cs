using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=s30519;Integrated Security=True;Trust Server Certificate=True;MultipleActiveResultSets=True";
    
    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        // Selects all trips and their information
        string command = "SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM Trip";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetOrdinal("IdTrip");

                    var trip = new TripDTO()
                    {
                        Id = reader.GetInt32(idOrdinal),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                        DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                        MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                        Countries = await GetCountriesByTripId(reader.GetInt32(idOrdinal))
                    };

                    trips.Add(trip);
                }
            }
        }

        return trips;
    }

    public async Task<bool> DoesTripExist(int id)
    {
        // Selects trip by id
        string command = "SELECT COUNT(1) FROM Trip WHERE IdTrip = @Id";
            
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }
    }
    
    public async Task<TripDTO> GetTrip(int id)
    {
        // Selects trip's information (by id)
        string command = @"
        SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople
        FROM Trip
        WHERE IdTrip = @Id";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var trip = new TripDTO()
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                        DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                        MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                        Countries = await GetCountriesByTripId(id)
                    };

                    return trip;
                }
            }
        }

        return null;
    }
    
    public async Task<List<CountryDTO>> GetCountriesByTripId(int tripId)
    {
        var countries = new List<CountryDTO>();

        // Selects countries the trip is associated with
        string command = @"
        SELECT c.IdCountry, c.Name
        FROM Country_Trip ct
        JOIN Country c ON ct.IdCountry = c.IdCountry
        WHERE ct.IdTrip = @TripId";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@TripId", tripId);

            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    countries.Add(new CountryDTO()
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("IdCountry")),
                        Name = reader.GetString(reader.GetOrdinal("Name"))
                    });
                }
            }
        }

        return countries;
    }
    
    public async Task<bool> IsTripFull(int tripId)
    {
        // Counts People registered and also takes param MaxPeople, to later compare them
        string command = @"
        SELECT 
            (SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @TripId) AS RegisteredCount,
            (SELECT MaxPeople FROM Trip WHERE IdTrip = @TripId) AS MaxPeople";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@TripId", tripId);
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    int registeredCount = reader.GetInt32(reader.GetOrdinal("RegisteredCount"));
                    int maxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople"));

                    return registeredCount >= maxPeople;
                }
            }
        }

        return true;
    }
    
    // Checks if client is already registered for a trip
    public async Task<bool> IsClientAlreadyRegistered(int clientId, int tripId)
    {
        // Checks if there is already client in a trip, in the database
        const string command = @"
        SELECT COUNT(1)
        FROM Client_Trip
        WHERE IdClient = @ClientId AND IdTrip = @TripId";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@ClientId", clientId);
            cmd.Parameters.AddWithValue("@TripId", tripId);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
    }

    // Returns true if client can be registered to the trip
    public async Task<Boolean> canRegisterClient(int tripId, int clientId)
    {
        bool tripExists = await DoesTripExist(tripId);
        if (!tripExists)
            return false;
        
        bool tripIsFull = await IsTripFull(tripId);
        if (tripIsFull)
            return false;

        bool clientAlreadyRegistered = await IsClientAlreadyRegistered(clientId, tripId);
        if (clientAlreadyRegistered)
            return false;

        return true;
    }
}