using System.ComponentModel.DataAnnotations;

namespace Tutorial8.Models.DTOs;

public class CountryDTO
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}