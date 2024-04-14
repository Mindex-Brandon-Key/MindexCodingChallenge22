using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CodeChallenge.Services;
using CodeChallenge.Models;
using AutoMapper;

namespace CodeChallenge.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = "getEmployeeById")]
        public IActionResult GetEmployeeById(String id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(String id, [FromBody]Employee newEmployee)
        {
            _logger.LogDebug($"Received employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        [HttpGet("{id}/reporting-structure")]
        public IActionResult GetReportingStructure(String id)
        {
            _logger.LogDebug($"Received employee reporting structure get request for '{id}'");

            if (!HttpContext.Request.Headers.TryGetValue("Max-Depth", out var maxDepthValue))
            {
                // Default depth limit. A typical company shouldn't have more than 100 layers of employees, but a large company could overwrite, if needed.
                maxDepthValue = "100";
            }

            if (!int.TryParse(maxDepthValue, out var maxDepth))
            {
                return BadRequest("Invalid Max-Depth header value.");
            }

            var reportingStructure = _employeeService.CalculateReportingStructure(id, maxDepth);

            if (reportingStructure == null)
            {
                return NotFound();
            }

            return Ok(reportingStructure);
        }

        [HttpPost("{id}/compensations")]
        public async Task<IActionResult> CreateCompensation(
            string id,
            [FromBody] CompensationCreateDto compensationRequest,
            [FromServices] ICompensationService compensationService)
        {
            if (compensationRequest == null)
            {
                return BadRequest("Compensation data is required.");
            }

            try
            {
                var compensation = await compensationService.AddCompensationAsync(compensationRequest, id);
                return CreatedAtAction(nameof(GetCompensationByEmployeeId), new { id = compensation.CompensationId }, compensation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}/compensations")]
        public async Task<IActionResult> GetCompensationByEmployeeId(
            string id,
            [FromServices] ICompensationService compensationService,
            [FromServices] IMapper mapper)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid employee ID.");
            }

            var compensations = await compensationService.GetCompensationsByEmployeeIdAsync(id);
            if (compensations == null)
            {
                return NotFound($"No compensations found for employee ID {id}.");
            }

            var result = new CompensationListDto()
            {
                EmployeeId = id,
                Count = compensations.Count,
                Compensations = compensations
                    .Select(x => mapper.Map<CompensationListItemDto>(x))
                    .ToList(),
            };

            return Ok(result);
        }
    }
}
