using System;
using System.Collections.Generic;

namespace CodeChallenge.Models;

/// <summary>
/// Data Model specific to Listing a <see cref="Compensation"/>.
/// Provides an abstraction layer over the Compensation, and allows for future enhancements like sort/filter without changing the underlying compensation.
/// </summary>
public class CompensationListDto
{
    public String EmployeeId { get; set; }
    public int Count { get; set; }
    public List<CompensationListItemDto> Compensations { get; set; }
}
