using System.ComponentModel.DataAnnotations;

namespace KWinHub.Contracts.Protocols;

public record CreateProtocolRequest(
    [Required] string Name,
    [Required] string Code,
    string Version = "1.0",
    string? Category = null,
    string? Sport = null,
    string? Description = null);
