using System;

namespace CodeChallenge.Models;

/// <summary>
/// Data Model specific to a list item for a <see cref="Compensation"/>
/// </summary>
public class CompensationListItemDto
{
    public String CompensationId { get; set; }
    public decimal Salary { get; set; }
    public DateTime EffectiveDate { get; set; }
}
