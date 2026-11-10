using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Entities;
using SchoolAdmission.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SchoolAdmission.Api.Services;

public interface IAdmissionService
{
    Task<ClassDto> CreateClassAsync(string name, int totalSeats, CancellationToken cancellationToken = default);
    Task<List<ClassDto>> GetClassesAsync(CancellationToken cancellationToken = default);
    Task<ClassDto?> GetClassAsync(string classId, CancellationToken cancellationToken = default);
    Task<CreateHoldResponse> ApplyAsync(string classId, string studentId, CancellationToken cancellationToken = default);
    Task<HoldDto> GetHoldAsync(string holdId, CancellationToken cancellationToken = default);
    Task<ConfirmResponse> ConfirmAsync(string holdId, string studentId, CancellationToken cancellationToken = default);
    Task CancelAsync(string holdId, string studentId, CancellationToken cancellationToken = default);
    Task<List<MyHoldDto>> GetMyHoldsAsync(string studentId, CancellationToken cancellationToken = default);
    Task<int> ReleaseExpiredHoldsAsync(CancellationToken cancellationToken = default);
}

public class AdmissionService : IAdmissionService
{
    private readonly AppDbContext _context;
    private readonly IClassRepository _classRepository;
    private readonly IHoldRepository _holdRepository;
    private readonly TimeProvider _timeProvider;
    private readonly int _holdDurationSeconds;

    public AdmissionService(
        AppDbContext context,
        IClassRepository classRepository,
        IHoldRepository holdRepository,
        TimeProvider timeProvider,
        int holdDurationSeconds)
    {
        _context = context;
        _classRepository = classRepository;
        _holdRepository = holdRepository;
        _timeProvider = timeProvider;
        _holdDurationSeconds = holdDurationSeconds;
    }

    public async Task<ClassDto> CreateClassAsync(string name, int totalSeats, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Class name is required.");
        if (totalSeats <= 0)
            throw new BadRequestException("Total seats must be a positive integer.");

        var admissionClass = new AdmissionClass
        {
            Id = GenerateClassId(),
            Name = name.Trim(),
            TotalSeats = totalSeats,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        await _classRepository.CreateAsync(admissionClass, cancellationToken);

        return new ClassDto(admissionClass.Id, admissionClass.Name, admissionClass.TotalSeats, 0, 0, admissionClass.TotalSeats);
    }

    public async Task<List<ClassDto>> GetClassesAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var classes = await _classRepository.GetAllAsync(cancellationToken);

        var heldCounts = await _context.AdmissionHolds
            .Where(h => h.Status == HoldStatus.HELD && h.ExpiresAt >= now)
            .GroupBy(h => h.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ClassId, g => g.Count, cancellationToken);

        var confirmedCounts = await _context.AdmissionHolds
            .Where(h => h.Status == HoldStatus.CONFIRMED)
            .GroupBy(h => h.ClassId)
            .Select(g => new { ClassId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ClassId, g => g.Count, cancellationToken);

        return classes.Select(c =>
        {
            var held = heldCounts.TryGetValue(c.Id, out var h) ? h : 0;
            var confirmed = confirmedCounts.TryGetValue(c.Id, out var k) ? k : 0;
            var available = Math.Max(0, c.TotalSeats - held - confirmed);
            return new ClassDto(c.Id, c.Name, c.TotalSeats, held, confirmed, available);
        }).ToList();
    }

    public async Task<ClassDto?> GetClassAsync(string classId, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var c = await _classRepository.GetByIdLockedAsync(classId, cancellationToken);
        if (c == null) return null;

        var held = await _classRepository.CountHoldsAsync(classId, HoldStatus.HELD, now, cancellationToken);
        var confirmed = await _classRepository.CountHoldsAsync(classId, HoldStatus.CONFIRMED, now, cancellationToken);
        var available = Math.Max(0, c.TotalSeats - held - confirmed);
        return new ClassDto(c.Id, c.Name, c.TotalSeats, held, confirmed, available);
    }

    public async Task<CreateHoldResponse> ApplyAsync(string classId, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new BadRequestException("Student ID is required.");
        if (!IsValidStudentId(studentId))
            throw new BadRequestException("Student ID must be a 17-digit birth registration number.");

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        var admissionClass = await _classRepository.GetByIdLockedAsync(classId, cancellationToken);
        if (admissionClass == null)
        {
            await tx.CommitAsync(cancellationToken);
            throw new NotFoundException($"Class with ID '{classId}' was not found.");
        }

        // Release any holds for this class that have already expired.
        await _holdRepository.ExpireHoldsAsync(classId, now, cancellationToken);

        var held = await _classRepository.CountHoldsAsync(classId, HoldStatus.HELD, now, cancellationToken);
        var confirmed = await _classRepository.CountHoldsAsync(classId, HoldStatus.CONFIRMED, now, cancellationToken);

        if (held + confirmed >= admissionClass.TotalSeats)
        {
            await tx.CommitAsync(cancellationToken);
            throw new ConflictException("No seats are available for this class.");
        }

        var alreadyHasActive = await _holdRepository.HasActiveOrConfirmedAsync(classId, studentId, cancellationToken);
        if (alreadyHasActive)
        {
            await tx.CommitAsync(cancellationToken);
            throw new ConflictException("You already have an active or confirmed application for this class.");
        }

        var expiresAt = now.AddSeconds(_holdDurationSeconds);
        var hold = new AdmissionHold
        {
            Id = GenerateHoldId(),
            ClassId = classId,
            StudentId = studentId,
            Status = HoldStatus.HELD,
            ExpiresAt = expiresAt,
            CreatedAt = now
        };

        await _holdRepository.AddAsync(hold, cancellationToken);
        await _holdRepository.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);

        return new CreateHoldResponse(hold.Id, hold.ClassId, hold.StudentId, "HELD", hold.ExpiresAt);
    }

    public async Task<HoldDto> GetHoldAsync(string holdId, CancellationToken cancellationToken = default)
    {
        var hold = await _context.AdmissionHolds.AsNoTracking()
            .Include(h => h.Class)
            .FirstOrDefaultAsync(h => h.Id == holdId, cancellationToken);
        if (hold == null)
            throw new NotFoundException($"Application with ID '{holdId}' was not found.");

        return new HoldDto(hold.Id, hold.ClassId, hold.Status.ToString(), hold.ExpiresAt);
    }

    public async Task<ConfirmResponse> ConfirmAsync(string holdId, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new BadRequestException("Student ID is required.");

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        var hold = await _holdRepository.GetByIdLockedAsync(holdId, cancellationToken);
        if (hold == null)
        {
            await tx.CommitAsync(cancellationToken);
            throw new NotFoundException($"Application with ID '{holdId}' was not found.");
        }

        if (hold.StudentId != studentId)
        {
            await tx.CommitAsync(cancellationToken);
            throw new ForbiddenException("You are not the owner of this application.");
        }

        if (hold.Status == HoldStatus.CONFIRMED)
        {
            // Idempotent: confirming an already confirmed hold is a no-op.
            await tx.CommitAsync(cancellationToken);
            return new ConfirmResponse(hold.Id, "CONFIRMED");
        }

        if (hold.Status != HoldStatus.HELD)
        {
            await tx.CommitAsync(cancellationToken);
            throw new ConflictException($"Cannot confirm an application with status '{hold.Status}'.");
        }

        if (hold.ExpiresAt < now)
        {
            await tx.CommitAsync(cancellationToken);
            throw new ConflictException("This reservation has expired and cannot be confirmed.");
        }

        hold.Status = HoldStatus.CONFIRMED;
        hold.ConfirmedAt = now;
        hold.UpdatedAt = now;

        await _holdRepository.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new ConfirmResponse(hold.Id, "CONFIRMED");
    }

    public async Task CancelAsync(string holdId, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new BadRequestException("Student ID is required.");

        using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        var hold = await _holdRepository.GetByIdLockedAsync(holdId, cancellationToken);
        if (hold == null)
        {
            await tx.CommitAsync(cancellationToken);
            throw new NotFoundException($"Application with ID '{holdId}' was not found.");
        }

        if (hold.StudentId != studentId)
        {
            await tx.CommitAsync(cancellationToken);
            throw new ForbiddenException("You are not the owner of this application.");
        }

        if (hold.Status != HoldStatus.HELD)
        {
            await tx.CommitAsync(cancellationToken);
            throw new ConflictException($"Only active reservations can be cancelled. Current status: {hold.Status}.");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        hold.Status = HoldStatus.CANCELLED;
        hold.UpdatedAt = now;

        await _holdRepository.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    public async Task<List<MyHoldDto>> GetMyHoldsAsync(string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new BadRequestException("Student ID is required.");

        var holds = await _holdRepository.GetByStudentAsync(studentId, cancellationToken);

        return holds.Select(h => new MyHoldDto(
            h.Id,
            h.ClassId,
            h.Class?.Name ?? string.Empty,
            h.Status.ToString(),
            h.ExpiresAt)).ToList();
    }

    public async Task<int> ReleaseExpiredHoldsAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var expiredCount = await _holdRepository.ExpireAllAsync(now, cancellationToken);
        await _holdRepository.SaveChangesAsync(cancellationToken);
        return expiredCount;
    }

    private static bool IsValidStudentId(string studentId)
    {
        return studentId.Length == 17 && studentId.All(char.IsDigit);
    }

    private static string GenerateClassId() => $"c_{Guid.NewGuid():N}";
    private static string GenerateHoldId() => $"h_{Guid.NewGuid():N}";
}
