﻿using System.ComponentModel.DataAnnotations;

namespace Tutorial8.Models.DTOs;

public class ClientDTO
{
    [Key]
    public int Id { get; set; }
    [MaxLength(120)]
    public string FirstName { get; set; }
    [MaxLength(120)]
    public string LastName { get; set; }
    [MaxLength(120)]
    public string Email { get; set; }
    [MaxLength(120)]
    public string Telephone { get; set; }
    [MaxLength(120)]
    public string Pesel { get; set; }
}