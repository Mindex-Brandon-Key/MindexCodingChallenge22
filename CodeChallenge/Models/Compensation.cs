﻿using System;
using System.ComponentModel.DataAnnotations;

namespace CodeChallenge.Models;

/// <summary>
/// Represents a compensation for an <see cref="Employee"/>. An employee can have multiple
/// compensations.
/// </summary>
public class Compensation
{
    [Required]
    public String CompensationId { get; set; }

    [Required]
    public String EmployeeId { get; set; }

    public Employee Employee { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Salary { get; set; }

    public DateTime EffectiveDate { get; set; }

}
