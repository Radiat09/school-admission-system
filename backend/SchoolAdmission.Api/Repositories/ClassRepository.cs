using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace SchoolAdmission.Api.Repositories;

public interface IClassRepository
{
    Task<AdmissionClass?> GetByIdLockedAsync(string id, CancellationToken cancellationToken = default);
    Task<List<AdmissionClass>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AdmissionClass> CreateAsync(AdmissionClass admissionClass, CancellationToken cancellationToken = default);
    Task<int> CountHoldsAsync(string classId, HoldStatus status, DateTime now, CancellationToken cancellationToken = default);
}

public class ClassRepository : IClassRepository
{
    private readonly AppDbContext _context;

    public ClassRepository(AppDbContext context) => _context = context;

    public async Task<AdmissionClass?> GetByIdLockedAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.AdmissionClasses
            .FromSqlInterpolated($"SELECT * FROM \"AdmissionClasses\" WHERE \"Id\" = {id} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<AdmissionClass>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _context.AdmissionClasses.OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<AdmissionClass> CreateAsync(AdmissionClass admissionClass, CancellationToken cancellationToken = default)
    {
        _context.AdmissionClasses.Add(admissionClass);
        await _context.SaveChangesAsync(cancellationToken);
        return admissionClass;
    }

    public Task<int> CountHoldsAsync(string classId, HoldStatus status, DateTime now, CancellationToken cancellationToken = default)
    {
        return _context.AdmissionHolds.CountAsync(h =>
            h.ClassId == classId && h.Status == status && h.ExpiresAt >= now, cancellationToken);
    }
}
