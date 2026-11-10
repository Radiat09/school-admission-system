namespace SchoolAdmission.Api.DTOs;

public record CreateClassRequest(string Name, int TotalSeats);

public record ClassDto(string Id, string Name, int TotalSeats, int HeldSeats, int ConfirmedSeats, int AvailableSeats);

public record CreateHoldResponse(string HoldId, string ClassId, string StudentId, string Status, DateTime ExpiresAt);

public record HoldDto(string HoldId, string ClassId, string Status, DateTime ExpiresAt);

public record ConfirmResponse(string HoldId, string Status);

public record MyHoldDto(string HoldId, string ClassId, string ClassName, string Status, DateTime ExpiresAt);

public record ErrorResponse(string Error, string? Message = null);
