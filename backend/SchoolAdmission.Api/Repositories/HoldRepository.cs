using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace SchoolAdmission.Api.Repositories;

public interface IHoldRepository
{
    Task<AdmissionHold?> GetByIdLockedAsync(string holdId, CancellationToken cancellationToken = default);
    Task<List<AdmissionHold>> GetByStudentAsync(string studentId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveOrConfirmedAsync(string classId, string studentId, CancellationToken cancellationToken = default);
    Task AddAsync(AdmissionHold hold, CancellationToken cancellationToken = default);
    Task<int> ExpireHoldsAsync(string classId, DateTime now, CancellationToken cancellationToken = default);
    Task<int> ExpireAllAsync(DateTime now, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class HoldRepository : IHoldRepository
{
    private readonly AppDbContext _context;

    public HoldRepository(AppDbContext context) => _context = context;

    public async Task<AdmissionHold?> GetByIdLockedAsync(string holdId, CancellationToken cancellationToken = default)
    {
        return await _context.AdmissionHolds
            .FromSqlInterpolated($"SELECT * FROM \"AdmissionHolds\" WHERE \"Id\" = {holdId} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<AdmissionHold>> GetByStudentAsync(string studentId, CancellationToken cancellationToken = default)
    {
        return _context.AdmissionHolds
            .Include(h => h.Class)
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasActiveOrConfirmedAsync(string classId, string studentId, CancellationToken cancellationToken = default)
    {
        return _context.AdmissionHolds.AnyAsync(h =>
            h.ClassId == classId && h.StudentId == studentId &&
            (h.Status == HoldStatus.HELD || h.Status == HoldStatus.CONFIRMED), cancellationToken);
    }

    public Task AddAsync(AdmissionHold hold, CancellationToken cancellationToken = default)
    {
        _context.AdmissionHolds.Add(hold);
        return Task.CompletedTask;
    }

    public Task<int> ExpireHoldsAsync(string classId, DateTime now, CancellationToken cancellationToken = default)
    {
        return _context.AdmissionHolds
            .Where(h => h.ClassId == classId && h.Status == HoldStatus.HELD && h.ExpiresAt < now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(h => h.Status, HoldStatus.EXPIRED)
                .SetProperty(h => h.UpdatedAt, now), cancellationToken);
    }

    public Task<int> ExpireAllAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        return _context.AdmissionHolds
            .Where(h => h.Status == HoldStatus.HELD && h.ExpiresAt < now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(h => h.Status, HoldStatus.EXPIRED)
                .SetProperty(h => h.UpdatedAt, now), cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
