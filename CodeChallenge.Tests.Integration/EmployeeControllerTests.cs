
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodeChallenge.Models;

using CodeCodeChallenge.Tests.Integration.Extensions;
using CodeCodeChallenge.Tests.Integration.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeCodeChallenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private HttpClient _httpClient;
        private TestServer _testServer;

        [TestInitialize]
        public void TestInitialize()
        {
            _testServer = new TestServer();
            _httpClient = _testServer.NewClient();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetReportingStructure_MissingEmployee_Returns_NotFound()
        {
            // Arrange
            var employeeId = "JustAFakeId";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reporting-structure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetReportingStructure_InvalidMaxDepth_Returns_BadRequest()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            _httpClient.DefaultRequestHeaders.Add("Max-Depth", "Pizza-Pie");

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reporting-structure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void GetReportingStructure_JohnLennon_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";
            var expectedNumberOfReports = 4;

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reporting-structure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(expectedFirstName, reportingStructure.Employee.FirstName);
            Assert.AreEqual(expectedLastName, reportingStructure.Employee.LastName);
            Assert.AreEqual(expectedNumberOfReports, reportingStructure.NumberOfReports);
        }

        [TestMethod]
        public void GetReportingStructure_JohnLennonAndMaxDepth1_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";
            var expectedNumberOfReports = 2;
            _httpClient.DefaultRequestHeaders.Add("Max-Depth", "1");

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reporting-structure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(expectedFirstName, reportingStructure.Employee.FirstName);
            Assert.AreEqual(expectedLastName, reportingStructure.Employee.LastName);
            Assert.AreEqual(expectedNumberOfReports, reportingStructure.NumberOfReports);
        }

        [TestMethod]
        public void GetReportingStructure_PeteBest_Returns_Ok()
        {
            // Arrange
            var employeeId = "62c1084e-6e34-4630-93fd-9153afb65309";
            var expectedFirstName = "Pete";
            var expectedLastName = "Best";
            var expectedNumberOfReports = 0;

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/reporting-structure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(expectedFirstName, reportingStructure.Employee.FirstName);
            Assert.AreEqual(expectedLastName, reportingStructure.Employee.LastName);
            Assert.AreEqual(expectedNumberOfReports, reportingStructure.NumberOfReports);
        }

        [TestMethod]
        public async Task CreateCompensation_MissingEmployee_Returns_BadRequest()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.BadRequest;

            var compensationRequest = new CompensationCreateDto()
            {
                Salary = 100000,
                EffectiveDate = DateTime.Today,
            };
            var requestContent = new JsonSerialization().ToJson(compensationRequest);

            var employeeId = "MissingEmployeeId";

            // Act
            var response =  await _httpClient.PostAsync($"api/employee/{employeeId}/compensations",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));

            // Assert
            Assert.AreEqual(expectedStatus, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateCompensation_EmptyBody_Returns_BadRequest()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.BadRequest;

            var employeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f";

            // Act
            var response = await _httpClient.PostAsync($"api/employee/{employeeId}/compensations",
               new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            // Assert
            Assert.AreEqual(expectedStatus, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateCompensation_ValidRequest_Returns_Created()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.Created;

            var compensationRequest = new CompensationCreateDto()
            {
                Salary = 100000,
                EffectiveDate = DateTime.Today,
            };
            var requestContent = new JsonSerialization().ToJson(compensationRequest);

            var employeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f";

            // Act
            var response = await _httpClient.PostAsync($"api/employee/{employeeId}/compensations",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));

            // Assert
            Assert.AreEqual(expectedStatus, response.StatusCode);
            var compensation = response.DeserializeContent<Compensation>();
            Assert.AreEqual(compensationRequest.Salary, compensation.Salary);
            Assert.AreEqual(compensationRequest.EffectiveDate, compensation.EffectiveDate);
            Assert.AreEqual(employeeId, compensation.EmployeeId);
            Assert.IsNotNull(compensation.CompensationId);
        }

        [TestMethod]
        public async Task GetCompensationByEmployeeId_MissingEmployee_Returns_NotFound()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.NotFound;

            var employeeId = "MissingEmployeeId";

            // Act
            var response = await _httpClient.GetAsync($"api/employee/{employeeId}/compensations");

            // Assert
            Assert.AreEqual(expectedStatus, response.StatusCode);
        }

        [TestMethod]
        public async Task GetCompensationByEmployeeId_NoCompensations_Returns_OkAndEmpty()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.OK;

            var employeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f";

            // Act
            var response = await _httpClient.GetAsync($"api/employee/{employeeId}/compensations");

            // Assert
            Assert.AreEqual(expectedStatus, response.StatusCode);
            var result = response.DeserializeContent<CompensationListDto>();
            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(employeeId, result.EmployeeId);
        }

        [TestMethod]
        public async Task GetCompensationByEmployeeId_Returns_Ok()
        {
            // Arrange
            var expectedStatus = HttpStatusCode.OK;

            var employeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f";

            var compensationRequestA = new CompensationCreateDto()
            {
                EffectiveDate = new DateTime(2023, 3, 13),
                Salary = 50000,
            };

            var compensationRequestB = new CompensationCreateDto()
            {
                EffectiveDate = new DateTime(2024, 4, 13),
                Salary = 60000,
            };

            var compensationA = await AddCompensation(compensationRequestA, employeeId);
            var compensationB = await AddCompensation(compensationRequestB, employeeId);

            // Act
            var response = await _httpClient.GetAsync($"api/employee/{employeeId}/compensations");

            // Assert
            Assert.AreEqual(expectedStatus, response.StatusCode);
            var result = response.DeserializeContent<CompensationListDto>();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(employeeId, result.EmployeeId);
            Assert.AreEqual(compensationA.CompensationId, result.Compensations[0].CompensationId);
            Assert.AreEqual(compensationRequestA.Salary, result.Compensations[0].Salary);
            Assert.AreEqual(compensationRequestA.EffectiveDate, result.Compensations[0].EffectiveDate);
            Assert.AreEqual(compensationB.CompensationId, result.Compensations[1].CompensationId);
            Assert.AreEqual(compensationRequestB.Salary, result.Compensations[1].Salary);
            Assert.AreEqual(compensationRequestB.EffectiveDate, result.Compensations[1].EffectiveDate);
        }

        private async Task<Compensation> AddCompensation(CompensationCreateDto compensationRequest, string employeeId)
        {
            var requestContent = new JsonSerialization().ToJson(compensationRequest);

            var response = await _httpClient.PostAsync($"api/employee/{employeeId}/compensations",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));

            var compensation = response.DeserializeContent<Compensation>();

            return compensation;
        }
    }
}
