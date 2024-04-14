
using System.Collections.Generic;
using System.Text;
using Bogus;
using CodeChallenge.Data;
using CodeChallenge.Models;
using CodeChallenge.Repositories;
using CodeChallenge.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CodeCodeChallenge.Tests.Integration
{
    [TestClass]
    public class EmployeeServiceTests
    {
        private EmployeeContext _employeeContext;
        private IEmployeeRepository _employeeRepository;
        private Mock<ILogger<EmployeeService>> _mockLogger;
        private Faker<Employee> _employeeFaker;

        private IEmployeeService _employeeService;

        [TestInitialize]
        public void SetupEmployeeServiceTests()
        {
            _mockLogger = new Mock<ILogger<EmployeeService>>();
            _employeeContext = new EmployeeContext(
                    new DbContextOptionsBuilder<EmployeeContext>().UseInMemoryDatabase("EmployeeDB").Options);

            var repositoryLoggerMock = new Mock<ILogger<IEmployeeRepository>>();
            _employeeRepository = new EmployeeRepository(repositoryLoggerMock.Object, _employeeContext);

            _employeeFaker = new Faker<Employee>()
                .RuleFor(x => x.EmployeeId, f => f.Random.Guid().ToString())
                .RuleFor(x => x.Department, f => f.Name.JobArea())
                .RuleFor(x => x.FirstName, f => f.Name.FirstName())
                .RuleFor(x => x.LastName, f => f.Name.LastName())
                .RuleFor(x => x.Position, f => f.Name.JobTitle())
                .RuleFor(x => x.DirectReports, new List<Employee>())
                ;

            _employeeService = new EmployeeService(_mockLogger.Object, _employeeRepository);
        }

        [TestCleanup]
        public void CleanupEmployeeServiceTests()
        {
            _employeeContext?.Database?.EnsureDeleted();
            _employeeContext?.Dispose();
        }

        [TestMethod]
        public void CalculateReportingStructure_RecursiveStructure_ShouldCountUnique()
        {
            // Arrange
            var firstLevelEmployees = 3;
            var secondLevelEmployees = 2;
            var expectedDirectReports = firstLevelEmployees + (firstLevelEmployees * secondLevelEmployees);

            var rootEmployee = _employeeFaker.Generate();

            rootEmployee.DirectReports = _employeeFaker.Generate(firstLevelEmployees);
            foreach(var firstLevelEmployee in rootEmployee.DirectReports)
            {
                firstLevelEmployee.DirectReports = _employeeFaker.Generate(secondLevelEmployees);
            }

            rootEmployee = _employeeContext.Employees.Add(rootEmployee).Entity;

            // Add the circular reference
            rootEmployee.DirectReports[0].DirectReports[0].DirectReports.Add(rootEmployee);

            _employeeContext.SaveChanges();

            // Act
            var result = _employeeService.CalculateReportingStructure(rootEmployee.EmployeeId, 100);

            // Assert
            Assert.AreEqual(expectedDirectReports, result.NumberOfReports);
        }

        [TestMethod]
        public void CalculateReportingStructure_MaxDepth_ShouldLimitDepth()
        {
            // Arrange
            var firstLevelEmployees = 3;
            var secondLevelEmployees = 2;
            var maxDepth = 1;
            var expectedDirectReports = firstLevelEmployees;

            var rootEmployee = _employeeFaker.Generate();

            rootEmployee.DirectReports = _employeeFaker.Generate(firstLevelEmployees);
            foreach (var firstLevelEmployee in rootEmployee.DirectReports)
            {
                firstLevelEmployee.DirectReports = _employeeFaker.Generate(secondLevelEmployees);
            }

            rootEmployee = _employeeContext.Employees.Add(rootEmployee).Entity;

            _employeeContext.SaveChanges();

            // Act
            var result = _employeeService.CalculateReportingStructure(rootEmployee.EmployeeId, maxDepth);

            // Assert
            Assert.AreEqual(expectedDirectReports, result.NumberOfReports);
        }

        [TestMethod]
        public void CalculateReportingStructure_MissingEmployee_ShouldReturnNull()
        {
            // Arrange

            // Act
            var result = _employeeService.CalculateReportingStructure("asdf", 100);

            // Assert
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void CalculateReportingStructure_NoReports_ShouldReturn0()
        {
            // Arrange
            var expectedDirectReports = 0;
            var rootEmployee = _employeeFaker.Generate();

            rootEmployee = _employeeContext.Employees.Add(rootEmployee).Entity;

            _employeeContext.SaveChanges();

            // Act
            var result = _employeeService.CalculateReportingStructure(rootEmployee.EmployeeId, 100);

            // Assert
            Assert.AreEqual(expectedDirectReports, result.NumberOfReports);
        }
    }
}
