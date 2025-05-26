using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AMS.Models;
using AMS.Models.ViewModel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AMS.Helpers
{
    public class EmployeeDetailsDocument : IDocument
    {
        private readonly EmployeeDetailsViewModel _model;

        public EmployeeDetailsDocument(EmployeeDetailsViewModel model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                // Header
                page.Header().ShowOnce().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(60).Image("wwwroot/images/company-logo.png", ImageScaling.FitArea);
                        row.RelativeItem();
                    });

                    col.Item().AlignCenter().Text("Employee Attendance Details")
                        .SemiBold().FontSize(20).FontColor(Colors.Black);
                });

                // Content
                page.Content().Column(col =>
                {
                    col.Spacing(5);

                    // Employee Info Box
                    col.Item().PaddingTop(15).Border(1).Padding(10).Column(empBox =>
                    {
                        empBox.Spacing(5);

                        empBox.Item().Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("👤 Name: ").SemiBold();
                                txt.Span($"{_model.Employee?.FirstName} {_model.Employee?.LastName}");
                            });

                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("📧 Email: ").SemiBold();
                                txt.Span($"{_model.Employee?.Email ?? "N/A"}");
                            });
                        });

                        empBox.Item().Row(row =>
                        {
                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("🏢 Department: ").SemiBold();
                                txt.Span($"{_model.Employee?.Department ?? "N/A"}");
                            });

                            row.RelativeItem().Text(txt =>
                            {
                                txt.Span("🧑‍💼 Designation: ").SemiBold();
                                txt.Span($"{_model.Employee?.Designation ?? "N/A"}");
                            });
                        });
                    });

                    // Attendance Status Summary
                    int presentCount = _model.AttendanceRecord?.Count(a => a.Status?.ToLower() == "present") ?? 0;
                    int absentCount = _model.AttendanceRecord?.Count(a => a.Status?.ToLower() == "absent") ?? 0;
                    int leaveCount = _model.AttendanceRecord?.Count(a => a.Status?.ToLower() == "leave") ?? 0;
                    int halfDayCount = _model.AttendanceRecord?.Count(a => a.Status?.ToLower() == "half day") ?? 0;
                    int holidayCount = _model.HolidayList?.Count ?? 0;

                    col.Item().PaddingTop(10).Text(text =>
                    {
                        text.Span("✔️ Present: ").SemiBold().FontColor(Colors.Green.Darken2);
                        text.Span($"{presentCount}    ");

                        text.Span("❌ Absent: ").SemiBold().FontColor(Colors.Red.Darken2);
                        text.Span($"{absentCount}    ");

                        text.Span("⏸️ Leave: ").SemiBold().FontColor(Colors.Orange.Darken2);
                        text.Span($"{leaveCount}    ");

                        text.Span("🌓 Half Day: ").SemiBold().FontColor(Colors.Blue.Darken2);
                        text.Span($"{halfDayCount}    ");

                        text.Span("🎉 Holiday: ").SemiBold().FontColor(Colors.Purple.Darken2);
                        text.Span($"{holidayCount}");
                    });

                    // Month-Year Title
                    var monthYear = _model.AttendanceRecord?.FirstOrDefault()?.AttendanceDate.ToString("MMMM yyyy") ?? "";
                    col.Item().PaddingTop(10).Text($"📅 Attendance Record: {monthYear}")
                        .Bold().FontSize(12).FontColor(Colors.Black);

                    // Attendance Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Date
                            columns.RelativeColumn(2); // Check-in
                            columns.RelativeColumn(2); // Check-out
                            columns.RelativeColumn(2); // Status
                            columns.RelativeColumn(2); // Hours
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Date").Bold().FontSize(10);
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Check-in").Bold().FontSize(10);
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Check-out").Bold().FontSize(10);
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Status").Bold().FontSize(10);
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Hours").Bold().FontSize(10);
                        });

                        if (_model.AttendanceRecord != null)
                        {
                            foreach (var attendance in _model.AttendanceRecord)
                            {
                                TimeSpan? totalHours = attendance.CheckOutTime.HasValue
                                    ? attendance.CheckOutTime.Value - attendance.CheckInTime
                                    : null;

                                string status = attendance.Status?.ToLower() ?? "unknown";
                                string bgColor = status == "absent" ? Colors.Red.Lighten3 :
                                                 status == "present" ? Colors.Green.Lighten4 :
                                                 status == "leave" ? Colors.Orange.Lighten3 :
                                                 status == "half day" ? Colors.Blue.Lighten4 :
                                                 Colors.Grey.Lighten3;

                                table.Cell().Background(bgColor).Padding(2).Text(attendance.AttendanceDate.ToString("dd MMM yyyy")).FontSize(9);
                                table.Cell().Background(bgColor).Padding(2).Text(
                                    attendance.CheckInTime.HasValue
                                        ? DateTime.Today.Add(attendance.CheckInTime.Value).ToString("hh:mm tt")
                                        : "--"
                                ).FontSize(9);
                                table.Cell().Background(bgColor).Padding(2).Text(
                                    attendance.CheckOutTime.HasValue
                                        ? DateTime.Today.Add(attendance.CheckOutTime.Value).ToString("hh:mm tt")
                                        : "--").FontSize(9);
                                table.Cell().Background(bgColor).Padding(2).Text(attendance.Status ?? "--").FontSize(9);
                                table.Cell().Background(bgColor).Padding(2).Text(
                                    totalHours.HasValue
                                        ? $"{totalHours.Value.Hours}h {totalHours.Value.Minutes}m"
                                        : "--").FontSize(9);
                            }
                        }
                    });

                    // 🎉 Holidays Table
                    if (_model.HolidayList != null && _model.HolidayList.Any())
                    {
                        col.Item().PaddingTop(15).Text("🎉 Holidays This Month")
                            .Bold().FontSize(12).FontColor(Colors.Black);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Date
                                columns.RelativeColumn(4); // Name
                                columns.RelativeColumn(4); // Description
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Date").Bold().FontSize(10);
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Holiday Name").Bold().FontSize(10);
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Description").Bold().FontSize(10);
                            });

                            foreach (var holiday in _model.HolidayList.OrderBy(h => h.HolidayDate))
                            {
                                table.Cell().Background(Colors.Yellow.Lighten4).Padding(2).Text(holiday.HolidayDate.ToString("dd MMM yyyy")).FontSize(9);
                                table.Cell().Background(Colors.Yellow.Lighten4).Padding(2).Text(holiday.HolidayName).FontSize(9);
                                table.Cell().Background(Colors.Yellow.Lighten4).Padding(2).Text(holiday.Description ?? "--").FontSize(9);
                            }
                        });
                    }
                    else
                    {
                        col.Item().PaddingTop(15).Text("🎉 No holidays recorded for this month.")
                            .Italic().FontSize(10).FontColor(Colors.Grey.Darken1);
                    }
                });

                // Footer
                page.Footer().Column(footer =>
                {
                    footer.Spacing(5);
                    footer.Item().Row(row =>
                    {
                        row.RelativeItem().AlignRight().Text("🖊️ HR Signature: ____________________________");
                    });

                    footer.Item().AlignCenter().Text(text =>
                    {
                        text.Span("Generated on: ");
                        text.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }
    }
}




























//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;
//using QuestPDF.Drawing;
//using AMS.Models;
//using AMS.Models.ViewModel;
//using System;
//using System.Linq;

//namespace AMS.Helpers
//{
//    public class EmployeeDetailsDocument : IDocument
//    {
//        private readonly EmployeeDetailsViewModel _model;

//        public EmployeeDetailsDocument(EmployeeDetailsViewModel model)
//        {
//            _model = model ?? throw new ArgumentNullException(nameof(model));
//        }

//        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

//        public void Compose(IDocumentContainer container)
//        {
//            container.Page(page =>
//            {
//                page.Margin(20);
//                page.Size(PageSizes.A4);
//                page.PageColor(Colors.White);
//                page.DefaultTextStyle(x => x.FontSize(12));

//                // Header
//                page.Header().ShowOnce().Column(col =>
//                {
//                    col.Item().Row(row =>
//                    {
//                        row.ConstantItem(60).Image("wwwroot/images/company-logo.png", ImageScaling.FitArea);
//                        row.RelativeItem();
//                    });

//                    col.Item().AlignCenter().Text("Employee Attendance Details")
//                        .SemiBold().FontSize(20).FontColor(Colors.Black);
//                });

//                // Content
//                page.Content().Column(col =>
//                {
//                    col.Spacing(5);

//                    // Employee Info Box
//                    col.Item().PaddingTop(15).Border(1).Padding(10).Column(empBox =>
//                    {
//                        empBox.Spacing(5);

//                        empBox.Item().Row(row =>
//                        {
//                            row.RelativeItem().Text(txt =>
//                            {
//                                txt.Span("👤 Name: ").SemiBold();
//                                txt.Span($"{_model.Employee?.FirstName} {_model.Employee?.LastName}");
//                            });

//                            row.RelativeItem().Text(txt =>
//                            {
//                                txt.Span("📧 Email: ").SemiBold();
//                                txt.Span($"{_model.Employee?.Email ?? "N/A"}");
//                            });
//                        });

//                        empBox.Item().Row(row =>
//                        {
//                            row.RelativeItem().Text(txt =>
//                            {
//                                txt.Span("🏢 Department: ").SemiBold();
//                                txt.Span($"{_model.Employee?.Department ?? "N/A"}");
//                            });

//                            row.RelativeItem().Text(txt =>
//                            {
//                                txt.Span("🧑‍💼 Designation: ").SemiBold();
//                                txt.Span($"{_model.Employee?.Designation ?? "N/A"}");
//                            });
//                        });
//                    });

//                    // ✅ Attendance Status Summary
//                    int presentCount = _model.AttendanceRecord?.Count(a => a.Status?.ToLower() == "present") ?? 0;
//                    int absentCount = _model.AttendanceRecord?.Count(a => a.Status?.ToLower() == "absent") ?? 0;
//                    int leaveCount = _model.AttendanceRecord?.Count(a => a.Status?.ToLower() == "leave") ?? 0;
//                    int halfDayCount = _model.AttendanceRecord?.Count(a => a.Status?.ToLower() == "half day") ?? 0;

//                    col.Item().PaddingTop(10).Text(text =>
//                    {
//                        text.Span("✔️ Present: ").SemiBold().FontColor(Colors.Green.Darken2);
//                        text.Span($"{presentCount}    ");

//                        text.Span("❌ Absent: ").SemiBold().FontColor(Colors.Red.Darken2);
//                        text.Span($"{absentCount}    ");

//                        text.Span("⏸️ Leave: ").SemiBold().FontColor(Colors.Orange.Darken2);
//                        text.Span($"{leaveCount}    ");

//                        text.Span("🌓 Half Day: ").SemiBold().FontColor(Colors.Blue.Darken2);
//                        text.Span($"{halfDayCount}");


//                        //text.Span("🎉 Holiday: ").SemiBold().FontColor(Colors.Blue.Darken2);
//                        //text.Span($"{holiday}");

//                    });

//                    // Month-Year Title
//                    var monthYear = _model.AttendanceRecord?.FirstOrDefault()?.AttendanceDate.ToString("MMMM yyyy") ?? "";
//                    col.Item().PaddingTop(10).Text($"📅 Attendance Record: {monthYear}")
//                        .Bold().FontSize(12).FontColor(Colors.Black);

//                    // Attendance Table
//                    col.Item().Table(table =>
//                    {
//                        table.ColumnsDefinition(columns =>
//                        {
//                            columns.RelativeColumn(2); // Date
//                            columns.RelativeColumn(2); // Check-in
//                            columns.RelativeColumn(2); // Check-out
//                            columns.RelativeColumn(2); // Status
//                            columns.RelativeColumn(2); // Hours
//                        });

//                        table.Header(header =>
//                        {
//                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Date").Bold().FontSize(10);
//                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Check-in").Bold().FontSize(10);
//                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Check-out").Bold().FontSize(10);
//                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Status").Bold().FontSize(10);
//                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Hours").Bold().FontSize(10);
//                        });

//                        if (_model.AttendanceRecord != null)
//                        {
//                            foreach (var attendance in _model.AttendanceRecord)
//                            {
//                                TimeSpan? totalHours = attendance.CheckOutTime.HasValue
//                                    ? attendance.CheckOutTime.Value - attendance.CheckInTime
//                                    : null;

//                                string status = attendance.Status?.ToLower() ?? "unknown";
//                                string bgColor = status == "absent" ? Colors.Red.Lighten3 :
//                                                 status == "present" ? Colors.Green.Lighten4 :
//                                                 status == "leave" ? Colors.Orange.Lighten3 :
//                                                 status == "half day" ? Colors.Blue.Lighten4 :
//                                                 Colors.Grey.Lighten3;

//                                table.Cell().Background(bgColor).Padding(2).Text(attendance.AttendanceDate.ToString("dd MMM yyyy")).FontSize(9);
//                                //table.Cell().Background(bgColor).Padding(2).Text(DateTime.Today.Add(attendance.CheckInTime).ToString("hh:mm tt")).FontSize(9);
//                                table.Cell().Background(bgColor).Padding(2).Text(
//                                    attendance.CheckInTime.HasValue
//                                        ? DateTime.Today.Add(attendance.CheckInTime.Value).ToString("hh:mm tt")
//                                        : "--"
//                                ).FontSize(9);
//                                table.Cell().Background(bgColor).Padding(2).Text(
//                                    attendance.CheckOutTime.HasValue
//                                        ? DateTime.Today.Add(attendance.CheckOutTime.Value).ToString("hh:mm tt")
//                                        : "--").FontSize(9);
//                                table.Cell().Background(bgColor).Padding(2).Text(attendance.Status ?? "--").FontSize(9);
//                                table.Cell().Background(bgColor).Padding(2).Text(
//                                    totalHours.HasValue
//                                        ? $"{totalHours.Value.Hours}h {totalHours.Value.Minutes}m"
//                                        : "--").FontSize(9);
//                            }
//                        }
//                    });
//                });

//                // Footer
//                page.Footer().Column(footer =>
//                {
//                    footer.Spacing(5);
//                    footer.Item().Row(row =>
//                    {
//                        row.RelativeItem().AlignRight().Text("🖊️ HR Signature: ____________________________");
//                    });

//                    footer.Item().AlignCenter().Text(text =>
//                    {
//                        text.Span("Generated on: ");
//                        text.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontColor(Colors.Grey.Darken1);
//                    });
//                });
//            });
//        }
//    }
//}


