using System.ComponentModel.DataAnnotations;

namespace StoreHub.API.DTOs;

public record CustomerUpdateDto(
    int Id,
    [Required] string FirstName,
    [Required] string LastName,
    [Required][EmailAddress] string Email
);
