using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AMS.Models;

public class Employees
{
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = null!;

    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
    public string? Department { get; set; }

    [StringLength(100, ErrorMessage = "Designation cannot exceed 100 characters.")]
    public string? Designation { get; set; }

    [Required(ErrorMessage = "Joining date is required.")]
    [DataType(DataType.Date)]
    public DateTime JoiningDate { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
    public string Status { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Project name cannot exceed 100 characters.")]
    public string Project { get; set; } = "Not Assigned";

    public bool IsDelete { get; set; }
}


