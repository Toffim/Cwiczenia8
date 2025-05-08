using System.ComponentModel.DataAnnotations;

namespace Tutorial8.Models.DTOs;

public class ClientTripDTO : TripDTO
{
    public int RegisteredAt { get; set; }
    public int? PaymentDate { get; set; }
}