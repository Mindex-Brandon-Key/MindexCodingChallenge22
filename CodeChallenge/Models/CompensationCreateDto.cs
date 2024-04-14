using System;

namespace CodeChallenge.Models;

/// <summary>
/// Data Model specific to creating a <see cref="Compensation"/>
/// </summary>
public class CompensationCreateDto
{
    public decimal Salary { get; set; }
    public DateTime EffectiveDate { get; set; }
}
