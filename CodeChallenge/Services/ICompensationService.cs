using CodeChallenge.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeChallenge.Services;
public interface ICompensationService
{
    Task<Compensation> AddCompensationAsync(CompensationCreateDto createRequest, string employeeId);
    Task<List<Compensation>> GetCompensationsByEmployeeIdAsync(string employeeId);
}