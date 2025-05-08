using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=s30519;Integrated Security=True;Trust Server Certificate=True;MultipleActiveResultSets=True";
    
    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        string command = "SELECT IdTrip, Name FROM Trip";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetOrdinal("IdTrip");
                    trips.Add(new TripDTO()
                    {
                        Id = reader.GetInt32(idOrdinal),
                        Name = reader.GetString(1),
                    });
                }
            }
        }
        

        return trips;
    }
    
    public async Task<bool> DoesTripExist(int id)
    {
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
        // We use @" to allow for multilines, also below I use parameters to avoid sql injections, even though
        // Its probably hard to make it unsafe with just `int` stuff.
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
    
    public async Task<bool> DoesClientExist(int clientId)
    {
        string command = "SELECT COUNT(1) FROM Client WHERE IdClient = @ClientId";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@ClientId", clientId);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
    }

    public async Task<List<TripDTO>> GetTripsByClientId(int clientId)
    {
        var trips = new List<TripDTO>();

        string command = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople
            FROM Trip t
            JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
            WHERE ct.IdClient = @ClientId";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@ClientId", clientId);

            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var trip = new TripDTO()
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                        DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                        MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                        Countries = new List<CountryDTO>()
                    };

                    var countryCommand = @"
                    SELECT c.IdCountry, c.Name
                    FROM Country c
                    INNER JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
                    WHERE ct.IdTrip = @TripId";

                    using (var countryCmd = new SqlCommand(countryCommand, conn))
                    {
                        countryCmd.Parameters.AddWithValue("@TripId", trip.Id);
                        using (var countryReader = await countryCmd.ExecuteReaderAsync())
                        {
                            while (await countryReader.ReadAsync())
                            {
                                trip.Countries.Add(new CountryDTO()
                                {
                                    Id = countryReader.GetInt32(countryReader.GetOrdinal("IdCountry")),
                                    Name = countryReader.GetString(countryReader.GetOrdinal("Name"))
                                });
                            }
                        }
                    }

                    trips.Add(trip);
                }
            }
        }

        return trips;
    }
}