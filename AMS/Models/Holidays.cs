using System;
using System.Collections.Generic;

namespace AMS.Models;

    public partial class Holidays
    {
        public int HolidayId { get; set; }

        public string HolidayName { get; set; } = null!;

        public DateTime HolidayDate { get; set; }

        public string? Description { get; set; }
    }
