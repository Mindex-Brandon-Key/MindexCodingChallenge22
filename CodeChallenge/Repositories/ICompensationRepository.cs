using CodeChallenge.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeChallenge.Repositories;
public interface ICompensationRepository
{
    void Add(Compensation compensation);
    Task<List<Compensation>> GetByEmployeeId(string employeeId);
    Task SaveAsync();
}