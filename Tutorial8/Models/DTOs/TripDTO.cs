using System.ComponentModel.DataAnnotations;

namespace Tutorial8.Models.DTOs;

public class TripDTO
{
    [Key]
    public int Id { get; set; }
    [MaxLength(120)]
    public string Name { get; set; }
    [MaxLength(220)]
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public List<CountryDTO> Countries { get; set; }
}