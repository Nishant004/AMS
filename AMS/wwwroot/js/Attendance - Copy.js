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
    console.log("GetEmployees is running");

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
    console.log("fetchAttendance is called");

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
            console.log("hi");
            console.log('Received data:', data); 

            const tableHead = $('#attendanceHead');
            const tableBody = $('#attendanceBody');
            const tableContainer = $('#attendanceTableContainer');
            const messageContainer = $('#noDataMessage');
            const instructions = $('#instructions');

            tableHead.empty();
            tableBody.empty();

            console.log("hi")

            //console.log(`data: ${data}`);
            //renderTable(data);


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
        error: function (err) {

            console.error("Error fetching attendance:", err.responseText || err.statusText || err);

            //console.error("Error fetching attendance:", err);
        }
    });
}






















//function fetchAttendance() {
//    console.log("fetchAttendance is called");

//    const today = new Date();
//    const month = $('select[name="month"]').val() || today.getMonth() + 1;
//    const year = $('select[name="year"]').val() || today.getFullYear();
//    const employeeId = $("#employee").val() || 0;

//    $.ajax({
//        url: '/Attendance/GetEmployeeAttendance',
//        method: 'GET',
//        data: {
//            employee: employeeId,
//            month: month,
//            year: year
//        },
//        success: function (data) {
//            const tableHead = $('#attendanceHead');
//            const tableBody = $('#attendanceBody');
//            const tableContainer = $('#attendanceTableContainer');
//            const messageContainer = $('#noDataMessage');
//            const instructions = $('#instructions');

//            tableHead.empty();
//            tableBody.empty();

//            if (!data || data.length === 0) {
//                tableContainer.hide();
//                instructions.addClass("d-none");
//                messageContainer.text("No attendance data found. Please check the employee name, month, or year.").show();
//                return;
//            }

//            messageContainer.hide();
//            tableContainer.show();
//            instructions.removeClass("d-none").addClass("d-flex");

//            const daysInMonth = new Date(year, month, 0).getDate();

//            // Build table headers
//            let headerRow1 = `<tr><th rowspan="2">Name</th>`;
//            let headerRow2 = `<tr>`;
//            for (let d = 1; d <= daysInMonth; d++) {
//                const dateObj = new Date(year, month - 1, d);
//                headerRow1 += `<th>${d}</th>`;
//                headerRow2 += `<th>${dateObj.toLocaleDateString('en-us', { weekday: 'short' })}</th>`;
//            }
//            headerRow1 += `</tr>`;
//            headerRow2 += `</tr>`;
//            tableHead.append(headerRow1);
//            tableHead.append(headerRow2);

//            // Group attendance by employee
//            const grouped = {};
//            data.forEach(item => {
//                const date = new Date(item.attendanceDate).getDate();
//                const empId = item.employeeID || 'holiday-only'; // Handle holidays
//                if (!grouped[empId]) {
//                    grouped[empId] = {
//                        name: item.employeeName || (empId === 'holiday-only' ? 'Holiday' : `Employee ${empId}`),
//                        attendance: {}
//                    };
//                }
//                grouped[empId].attendance[date] = item.status || item.description; // Use description for holidays
//            });

//            // Build rows
//            for (const empId in grouped) {
//                const employee = grouped[empId];
//                let row = `<tr><td>${empId === 'holiday-only' ? employee.name : `<a href="/Admin/Dashboard/EmployeeDetails/${empId}?month=${month}&year=${year}">${employee.name}</a>`}</td>`;

//                for (let d = 1; d <= daysInMonth; d++) {
//                    const dateObj = new Date(year, month - 1, d);
//                    const status = employee.attendance[d];

//                    if (status === "Present") {
//                        row += "<td>✔️</td>";
//                    } else if (status === "Absent") {
//                        row += "<td>❌</td>";
//                    } else if (status === "Leave") {
//                        row += "<td>⏸️</td>";
//                    } else if (status === "Half Day") {
//                        row += "<td>🌓</td>";
//                    } else if (status === "Holiday" || status === "Public" || status === "Weekend") {
//                        row += "<td>🎉</td>";
//                    } else {
//                        // Optionally auto-detect Sunday
//                        if (dateObj.getDay() === 0) {
//                            row += "<td>🎉</td>";
//                        } else {
//                            row += "<td></td>";
//                        }
//                    }
//                }

//                row += "</tr>";
//                tableBody.append(row);
//            }
//        },
//        error: function (err) {
//            console.error("Error fetching attendance:", err);
//        }
//    });
//}





//function fetchAttendance() {
//    console.log("fetchAttendance is called");


//    const today = new Date();



//    const month = $('select[name="month"]').val() || today.getMonth() + 1;
//    const year = $('select[name="year"]').val() || today.getFullYear();
//    const employeeId = $("#employee").val() || 0; // Using the employeeId from Razor



//    $.ajax({
//        url: '/Attendance/GetEmployeeAttendance',
//        method: 'GET',
//        data: {
//            employee: employeeId,
//            month: month,
//            year: year
//        },
//        success: function (data) {

//            //console.log("Attendance Data:", data);

//            const tableHead = $('#attendanceHead');
//            const tableBody = $('#attendanceBody');
//            const tableContainer = $('#attendanceTableContainer'); // wrap your table in a div with this ID
//            const messageContainer = $('#noDataMessage'); // a div for showing the message
//            const instructions = $('#instructions'); // holds the instructions

//            tableHead.empty();
//            tableBody.empty();

//            //tableContainer.hide();

//            if (!data || data.length === 0) {
//                tableContainer.hide();               // hide table
//                instructions.addClass("d-none");
//                messageContainer.text("No attendance data found. Please check the employee name, month, or year.").show();
//                return;
//            }

//            // Hide message and show table
//            messageContainer.hide();
//            tableContainer.show();
//            instructions.removeClass("d-none");
//            instructions.addClass("d-flex");

//            const daysInMonth = new Date(year, month, 0).getDate();

//            // Build the header row
//            let headerRow1 = `<tr><th rowspan="2">Name</th>`;
//            let headerRow2 = `<tr>`;
//            for (let d = 1; d <= daysInMonth; d++) {
//                const dateObj = new Date(year, month - 1, d);
//                headerRow1 += `<th>${d}</th>`;
//                headerRow2 += `<th>${dateObj.toLocaleDateString('en-us', { weekday: 'short' })}</th>`;
//            }
//            headerRow1 += `</tr>`;
//            headerRow2 += `</tr>`;
//            tableHead.append(headerRow1);
//            tableHead.append(headerRow2);

//            // Group attendance data by employee
//            const grouped = {};
//            data.forEach(item => {
//                const date = new Date(item.attendanceDate).getDate();
//                const empId = item.employeeID;
//                if (!grouped[empId]) {
//                    grouped[empId] = {
//                        name: item.employeeName || `Employee ${empId}`,
//                        attendance: {}
//                    }
//                }
//                grouped[empId].attendance[date] = item.status;
//            });



//            //console.log("Grouped Attendance Data:", grouped);
         
//            // Build rows
//            for (const empId in grouped) {
//                const employee = grouped[empId];
//                //let row = `<tr><td>${employee.name}</td>`;
//                //console.log("month", month)
//                //console.log("year", year)

//                let row = `<tr><td><a href="/Admin/Dashboard/EmployeeDetails/${empId}?month=${month}&year=${year}">${employee.name}</a></td>`;

                
//                //let employeeUrl = employeeDetailsBaseUrl.replace('__ID__', empId);
//                //let row = `<tr><td><a href="${employeeUrl}">${employee.name}</a></td>`;
//                for (let d = 1; d <= daysInMonth; d++) {
//                    const status = employee.attendance[d];
//                    if (status === "Present") {
//                        row += "<td>✔️</td>";
//                    } else if (status === "Absent") {
//                        row += "<td>❌</td>";
//                    } else if (status === "Leave") {
//                        row += "<td>⏸️</td>"; // Yellow dot for Leave
//                    } else if (status === "Half Day") {
//                        row += "<td>🌓</td>"; // Pause icon for Half Day
//                    } else {
//                        row += "<td></td>"; // No data
//                    }
//                }

//                row += "</tr>";
//                tableBody.append(row);
//            }
//        },
//        error: function (err) {
//            console.error("Error fetching attendance:", err);
//        }
//    });
//}

