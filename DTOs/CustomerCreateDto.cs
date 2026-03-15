using System.ComponentModel.DataAnnotations;

namespace StoreHub.API.DTOs;

public record CustomerCreateDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required][EmailAddress] string Email
);
