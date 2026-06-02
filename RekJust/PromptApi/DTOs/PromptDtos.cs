using System.ComponentModel.DataAnnotations;

namespace PromptApi.DTOs;

/// <summary>
/// DTO zwracane klientowi. Nie eksponuje wewnętrznych pól encji EF Core.
/// </summary>
public record PromptResponseDto(
    Guid Id,
    string Text,
    string Status,
    string? Result,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

/// <summary>
/// DTO przyjmowane od klienta przy tworzeniu promptu.
/// </summary>
public class CreatePromptDto
{
    [Required(ErrorMessage = "Prompt nie może być pusty")]
    [MinLength(3, ErrorMessage = "Prompt musi mieć min. 3 znaki")]
    [MaxLength(4000, ErrorMessage = "Prompt może mieć max. 4000 znaków")]
    public string Text { get; set; } = string.Empty;
}
