using CodeChallenge.Data;
using CodeChallenge.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CodeChallenge.Repositories;

public class CompensationRepository : ICompensationRepository
{
    private readonly EmployeeContext _employeeContext;

    public CompensationRepository(EmployeeContext context)
    {
        _employeeContext = context;
    }

    public void Add(Compensation compensation)
    {
        if (compensation == null)
        {
            throw new ArgumentNullException(nameof(compensation));
        }

        compensation.CompensationId = Guid.NewGuid().ToString();

        // The in-memory database does not enforce foreign key constraints, so we will have to 
        // manually check the foreign key.
        if (!_employeeContext.Employees.Any(x => x.EmployeeId == compensation.EmployeeId))
        {
            throw new InvalidOperationException($"Employee with Id '{compensation.EmployeeId}' could not be found.");
        }

        _employeeContext.Compensations.Add(compensation);
    }

    public async Task SaveAsync()
    {
        await _employeeContext.SaveChangesAsync();
    }

    public async Task<List<Compensation>> GetByEmployeeId(string employeeId)
    {
        // Note: This is a list operation and it may require pagination if the data set gets too large.
        //       There shouldn't be too many compensations for a single employee,
        //       but bad/unexpected data could cause an issue.
        return await _employeeContext.Compensations
            .Include(c => c.Employee)
            .Where(c => c.EmployeeId == employeeId)
            .OrderBy(c => c.EffectiveDate)
            .ToListAsync();
    }
}
