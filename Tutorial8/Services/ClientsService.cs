using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=s30519;Integrated Security=True;Trust Server Certificate=True;MultipleActiveResultSets=True";

    public async Task<bool> DoesClientExist(int clientId)
    {
        // Selects the client by id
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

    public async Task<List<ClientTripDTO>> GetTripsByClientId(int clientId)
    {
        var trips = new List<ClientTripDTO>();

        // Returns trip information and registered/payment from joined table
        string command = @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, 
                   ct.RegisteredAt, ct.PaymentDate
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
                    var clientTrip = new ClientTripDTO()
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                        DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                        MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                        RegisteredAt = reader.GetInt32(reader.GetOrdinal("RegisteredAt")), // Assuming RegisteredAt is an integer
                        PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate")) 
                                      ? (int?)null 
                                      : reader.GetInt32(reader.GetOrdinal("PaymentDate")), // Handle nullability for PaymentDate
                        Countries = new List<CountryDTO>()
                    };

                    // Fetch countries associated with the trip id
                    var countryCommand = @"
                        SELECT c.IdCountry, c.Name
                        FROM Country c
                        INNER JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
                        WHERE ct.IdTrip = @TripId";

                    using (var countryCmd = new SqlCommand(countryCommand, conn))
                    {
                        countryCmd.Parameters.AddWithValue("@TripId", clientTrip.Id);
                        using (var countryReader = await countryCmd.ExecuteReaderAsync())
                        {
                            while (await countryReader.ReadAsync())
                            {
                                clientTrip.Countries.Add(new CountryDTO()
                                {
                                    Id = countryReader.GetInt32(countryReader.GetOrdinal("IdCountry")),
                                    Name = countryReader.GetString(countryReader.GetOrdinal("Name"))
                                });
                            }
                        }
                    }

                    trips.Add(clientTrip);
                }
            }
        }

        return trips;
    }
    
    public async Task<int> CreateClient(ClientDTO clientDTO)
    {
        // Inserts client to database. SCOPE_IDENTITY is for returning the last generated id.
        string command = @"
        INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
        VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
        SELECT SCOPE_IDENTITY();";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@FirstName", clientDTO.FirstName);
            cmd.Parameters.AddWithValue("@LastName", clientDTO.LastName);
            cmd.Parameters.AddWithValue("@Email", clientDTO.Email);
            cmd.Parameters.AddWithValue("@Telephone", clientDTO.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", clientDTO.Pesel);

            await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }

    public async Task<bool> IsValidClientData(ClientDTO clientDto)
    {
        // Checking class members validation
        return !(
            string.IsNullOrEmpty(clientDto.FirstName) 
            || string.IsNullOrEmpty(clientDto.LastName) 
            || string.IsNullOrEmpty(clientDto.Email)
            || string.IsNullOrEmpty(clientDto.Telephone)
            || string.IsNullOrEmpty(clientDto.Pesel)
        );
    }

    public async Task<bool> RegisterClientToTrip(int clientId, int tripId)
    {
        // Inserts to database the registration details
        const string command = @"
        INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
        VALUES (@ClientId, @TripId, @RegisteredAt)";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@ClientId", clientId);
            cmd.Parameters.AddWithValue("@TripId", tripId);
            cmd.Parameters.AddWithValue("@RegisteredAt", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            await conn.OpenAsync();

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}