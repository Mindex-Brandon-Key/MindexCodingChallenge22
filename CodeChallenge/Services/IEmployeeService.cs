using CodeChallenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeChallenge.Services
{
    public interface IEmployeeService
    {
        Employee GetById(String id);
        Employee Create(Employee employee);
        Employee Replace(Employee originalEmployee, Employee newEmployee);
        /// <summary>
        /// Calculates the reporting structure for an employee with Id = <paramref name="employeeId"/>.
        /// </summary>
        /// <returns>
        ///     The filled out <see cref="ReportingStructure"/>, or null if the employee could not be found.
        /// </returns>
        ReportingStructure CalculateReportingStructure(String employeeId, int maxDepth);
    }
}
