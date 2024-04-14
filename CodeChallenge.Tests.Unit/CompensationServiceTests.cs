using AutoMapper;
using Bogus;
using CodeChallenge.Config;
using CodeChallenge.Data;
using CodeChallenge.Models;
using CodeChallenge.Repositories;
using CodeChallenge.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeChallenge.Tests.Unit;

[TestClass]
public class CompensationServiceTests
{
    private EmployeeContext _employeeContext;
    private ICompensationRepository _compensationRepository;
    private IEmployeeRepository _employeeRepository;
    private IMapper _mapper;
    private Faker<Employee> _employeeFaker;
    private Faker<Compensation> _compensationFaker;

    private ICompensationService _compensationService;

    [TestInitialize]
    public void SetupEmployeeServiceTests()
    {
        _employeeContext = new EmployeeContext(
                new DbContextOptionsBuilder<EmployeeContext>().UseInMemoryDatabase("EmployeeDB").Options);

        var repositoryLoggerMock = new Mock<ILogger<IEmployeeRepository>>();
        _compensationRepository = new CompensationRepository(_employeeContext);
        _employeeRepository = new EmployeeRepository(repositoryLoggerMock.Object, _employeeContext);

        _employeeFaker = new Faker<Employee>()
            .RuleFor(x => x.EmployeeId, f => f.Random.Guid().ToString())
            .RuleFor(x => x.Department, f => f.Name.JobArea())
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.Position, f => f.Name.JobTitle())
            .RuleFor(x => x.DirectReports, new List<Employee>())
            ;

        _compensationFaker = new Faker<Compensation>()
            .RuleFor(x => x.EmployeeId, f => f.Random.Guid().ToString())
            .RuleFor(x => x.CompensationId, f => f.Random.Guid().ToString())
            .RuleFor(x => x.EffectiveDate, f => f.Date.Past())
            .RuleFor(x => x.Salary, f => f.Finance.Amount(10000, 500000))
            ;

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();

        _compensationService = new CompensationService(_compensationRepository, _employeeRepository, _mapper);
    }

    [TestCleanup]
    public void CleanupEmployeeServiceTests()
    {
        _employeeContext?.Database?.EnsureDeleted();
        _employeeContext?.Dispose();
    }

    [TestMethod]
    public async Task AddCompensationAsync_MissingEmployee_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var compensation = _compensationFaker.Generate();
        var compensationRequest = _mapper.Map<CompensationCreateDto>(compensation);

        // Act
        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await _compensationService.AddCompensationAsync(compensationRequest, "Bogus Id");
        });

        // Assert
        Assert.IsNotNull(exception);
    }

    [TestMethod]
    public async Task AddCompensationAsync_ValidRequest_ShouldReturnCompensation()
    {
        // Arrange
        var employee = _employeeFaker.Generate();
        var compensation = _compensationFaker.Generate();
        var compensationRequest = _mapper.Map<CompensationCreateDto>(compensation);

        _employeeContext.Employees.Add(employee);
        await _employeeContext.SaveChangesAsync();

        // Act
        var result = await _compensationService.AddCompensationAsync(compensationRequest, employee.EmployeeId);

        // Assert
        Assert.AreEqual(employee.EmployeeId, result.EmployeeId);
        Assert.AreEqual(compensation.Salary, result.Salary);
        Assert.AreEqual(compensation.EffectiveDate, result.EffectiveDate);
        Assert.IsNotNull(result.CompensationId);
    }
}
