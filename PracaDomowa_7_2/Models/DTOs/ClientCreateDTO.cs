using System.ComponentModel.DataAnnotations;

namespace PracaDomowa_7_2.Models.DTOs;

public class ClientCreateDTO
{
    [Required]
    [StringLength(120)]
    public string FirstName { get; set; }
    
    [Required]
    [StringLength(120)]
    public string LastName { get; set; }
    
    [Required]
    [StringLength(120)]
    public string Email { get; set; }
    
    [Required]
    [StringLength(120)]
    public string Telephone { get; set; }
    
    [Required]
    [StringLength(120)]
    public string Pesel { get; set; }
}