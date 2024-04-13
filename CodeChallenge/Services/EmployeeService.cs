using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Models;
using Microsoft.Extensions.Logging;
using CodeChallenge.Repositories;

namespace CodeChallenge.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public Employee Create(Employee employee)
        {
            if(employee != null)
            {
                _employeeRepository.Add(employee);
                _employeeRepository.SaveAsync().Wait();
            }

            return employee;
        }

        public Employee GetById(string id)
        {
            if(!String.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetById(id);
            }

            return null;
        }

        public Employee Replace(Employee originalEmployee, Employee newEmployee)
        {
            if(originalEmployee != null)
            {
                _employeeRepository.Remove(originalEmployee);
                if (newEmployee != null)
                {
                    // ensure the original has been removed, otherwise EF will complain another entity w/ same id already exists
                    _employeeRepository.SaveAsync().Wait();

                    _employeeRepository.Add(newEmployee);
                    // overwrite the new id with previous employee id
                    newEmployee.EmployeeId = originalEmployee.EmployeeId;
                }
                _employeeRepository.SaveAsync().Wait();
            }

            return newEmployee;
        }

        public ReportingStructure CalculateReportingStructure(String employeeId, int maxDepth)
        {
            // Retrieve the employee
            var employee = _employeeRepository.GetById(employeeId);
            if (employee == null)
            {
                return null;
            }

            // Calculate the direct reports
            var reportIds = new HashSet<string>();
            CalculateDirectReports(employee, 0, maxDepth, reportIds);

            // Return the reporting structure
            return new ReportingStructure
            {
                Employee = employee,
                NumberOfReports = reportIds.Count,
            };
        }

        private void CalculateDirectReports(Employee employee, int currentDepth, int maxDepth, HashSet<string> visitedIds)
        {
            // Check if maximum recursion depth is reached
            if (currentDepth >= maxDepth)
            {
                return;
            }

            // Load direct reports for the current employee
            // Note: this goes away if Lazy loading is enabled in the future.
            _employeeRepository.LoadDirectReports(employee);

            if (employee.DirectReports == null)
            {
                return;
            }

            foreach (var directReport in employee.DirectReports)
            {
                // Check for unique employee ID using HashSet
                if (visitedIds.Add(directReport.EmployeeId))
                {
                    // If ID is added successfully, it's unique; continue recursion
                    CalculateDirectReports(directReport, currentDepth + 1, maxDepth, visitedIds);
                }
            }
        }
    }
}
