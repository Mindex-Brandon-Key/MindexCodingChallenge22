using CodeChallenge.Models;
using CodeChallenge.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AutoMapper;

namespace CodeChallenge.Services;

public class CompensationService : ICompensationService
{
    private readonly ICompensationRepository _compensationRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public CompensationService(
        ICompensationRepository compensationRepository,
        IEmployeeRepository employeeRepository,
        IMapper mapper)
    {
        _compensationRepository = compensationRepository;
        _employeeRepository = employeeRepository;
        _mapper = mapper;
    }

    public async Task<Compensation> AddCompensationAsync(CompensationCreateDto createRequest, string employeeId)
    {
        if (createRequest == null)
        {
            throw new ArgumentNullException(nameof(createRequest));
        }

        // Transform the request into a compensation object.
        // In order to prevent the user from trying any funny business by specifying
        // values in the Employee field, the request just has the compensation fields.
        var compensation = _mapper.Map<Compensation>(createRequest);
        compensation.EmployeeId = employeeId;

        _compensationRepository.Add(compensation);
        await _compensationRepository.SaveAsync();

        return compensation;
    }

    public async Task<List<Compensation>> GetCompensationsByEmployeeIdAsync(string employeeId)
    {
        if (string.IsNullOrEmpty(employeeId))
        {
            throw new ArgumentNullException(nameof(employeeId), "Missing employee ID.");
        }

        if (_employeeRepository.GetById(employeeId) == null)
        {
            return null;
        }

        return await _compensationRepository.GetByEmployeeId(employeeId);
    }
}
