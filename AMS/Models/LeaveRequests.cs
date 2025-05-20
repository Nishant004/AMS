using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{

    public partial class LeaveRequests
    {
        public int LeaveID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Leave Type is required.")]
        [RegularExpression("^(Unpaid Leave|Paid Leave|Casual Leave|Sick Leave)$",
            ErrorMessage = "Invalid Leave Type.")]
        public string LeaveType { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        //public DateTime StartDate { get; set; }

        //public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Please provide a reason.")]
        [StringLength(500)]
        public string? Reason { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime RequestedAt { get; set; } = DateTime.Now;


        //public virtual Employees Employee { get; set; } = null!;
    }

}
