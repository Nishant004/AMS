$(document).ready(() => {
    //alert("Hello!");


    GetEmployees();

   
    const today = new Date();
    $('select[name="month"]').val(today.getMonth() + 1);
    $('select[name="year"]').val(today.getFullYear());
   

    $('#attendanceForm').on('submit', function (e) {

        e.preventDefault();
        fetchAttendance();
    });

})








function GetEmployees() {
   

    $.ajax({
        url: '/Attendance/GetEmployees',
        method: 'GET',
        success: function (result) {
            var $employeeSelect = $('#employee');

            $employeeSelect.empty(); // <--- FIRST: clear any old options

            // Add "All Employees" option first
            $employeeSelect.append('<option value="0">All Employees</option>');

            // Then add real employees
            $.each(result, function (i, data) {
                $employeeSelect.append('<option value="' + data.id + '">' + data.name + '</option>');
            });

            $employeeSelect.val(0).trigger('change'); // <--- Then select default value and trigger change

            fetchAttendance(); // <--- Then call fetchAttendance
        }
,
        error: function (xhr, status, error) {
            console.error("Error fetching employees:", error);
        }
    });

}









function fetchAttendance() {


    const today = new Date();
    const month = $('select[name="month"]').val() || today.getMonth() + 1;
    const year = $('select[name="year"]').val() || today.getFullYear();
    const employeeId = $("#employee").val() || 0;

    

    $.ajax({
        url: '/Attendance/GetEmployeeAttendance',
        method: 'GET',
        data: {
            employee: employeeId,
            month: month,
            year: year
        },


        success: function (data) {
       



            const tableHead = $('#attendanceHead');
            const tableBody = $('#attendanceBody');
            const tableContainer = $('#attendanceTableContainer');
            const messageContainer = $('#noDataMessage');
            const instructions = $('#instructions');

            tableHead.empty();
            tableBody.empty();

    

          


            if (!data || data.length === 0) {
                tableContainer.hide();
                instructions.addClass("d-none");
                messageContainer.text("No attendance data found. Please check the employee name, month, or year.").show();
                return;
            }

            messageContainer.hide();
            tableContainer.show();
            instructions.removeClass("d-none").addClass("d-flex");

            const daysInMonth = new Date(year, month, 0).getDate();

            // Build table headers
            let headerRow1 = `<tr><th rowspan="2">Name</th>`;
            let headerRow2 = `<tr>`;
            for (let d = 1; d <= daysInMonth; d++) {
                const dateObj = new Date(year, month - 1, d);
                headerRow1 += `<th>${d}</th>`;
                headerRow2 += `<th>${dateObj.toLocaleDateString('en-us', { weekday: 'short' })}</th>`;
            }
            headerRow1 += `</tr>`;
            headerRow2 += `</tr>`;
            tableHead.append(headerRow1);
            tableHead.append(headerRow2);

            // Group attendance by employee ID
            const grouped = {};
            data.forEach(item => {
                const date = new Date(item.attendanceDate).getDate();
                const empId = item.employeeID ?? `0`; // Use '0' for holidays-only row
                const empName = item.employeeName || "Unknown";

                if (!grouped[empId]) {
                    grouped[empId] = {
                        name: empName,
                        attendance: {}
                    };
                }

                grouped[empId].attendance[date] = item.status;
            });

            // Emoji map
            const statusMap = {
                "Present": "✔️",
                "Absent": "❌",
                "Leave": "⏸️",
                "Half Day": "🌓",
                "Holiday": "🎉",
                "Public": "🎉",
                "Weekend": "🎉"
            };

            // Build table rows
            for (const empId in grouped) {
                const employee = grouped[empId];
                let row = `<tr><td>${empId === '0'
                        ? employee.name
                        : `<a href="/Admin/Dashboard/EmployeeDetails/${empId}?month=${month}&year=${year}">${employee.name}</a>`
                    }</td>`;

                for (let d = 1; d <= daysInMonth; d++) {
                    const dateObj = new Date(year, month - 1, d);
                    const status = employee.attendance[d];
                    const emoji = statusMap[status] || (dateObj.getDay() === 0 ? "🎉" : "");
                    row += `<td title="${status || 'No data'}">${emoji}</td>`;
                    //const emoji = statusMap[status] || (dateObj.getDay() === 0 ? "🎉" : "");
                    //row += `<td>${emoji}</td>`;
                }

                row += "</tr>";
                tableBody.append(row);
            }
        },

        error: function (xhr, status, error) {
            console.error("AJAX Error:", status, error);
            console.error("Response:", xhr.responseText);
            console.error("Error fetching attendance:", err.responseText || err.statusText || err);
        },
        complete: function () {
            console.log("AJAX complete"); // Make sure at least this fires
        }
        
    });
}







