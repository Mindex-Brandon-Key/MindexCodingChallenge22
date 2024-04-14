using System;

namespace CodeChallenge.Models;

/// <summary>
/// Data Model specific to creating a <see cref="Compensation"/>.
/// Id, EmployeeId, and Employee are not present.
/// Id is generated upon creation.
/// EmployeeId is part of the request.
/// Employee data cannot be specified in the create of a compensation.
/// </summary>
public class CompensationCreateDto
{
    public decimal Salary { get; set; }
    public DateTime EffectiveDate { get; set; }
}
