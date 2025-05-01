$(document).ready(() => {


    //-------Fetch Attendance-------------------------------------
    const today = new Date();
    $('select[name="month"]').val(today.getMonth() + 1);
    $('select[name="year"]').val(today.getFullYear());

    // Fetch attendance with a slight delay
    setTimeout(function () {
        fetchAttendance();
    }, 500);

    // Fetch attendance on form submit (with year/month change)
    $('#attendanceForm').on('submit', function (e) {
    

        e.preventDefault();
        fetchAttendance();
    });


    //----------------------------------------------------

    LoadAttendanceDetails(); // Load attendance on page load


    //updateButtonLabels();
    setTimeout(function () {
        updateButtonLabels();
    }, 200);


    $('#checkInBtn').off('click').on('click', () => handleAttendance('CheckIn'));
    $('#checkOutBtn').off('click').on('click', () => handleAttendance('CheckOut'));
    $('#logoutBtn').off('click').on('click', handleLogout);

    // Handle "View Location" button clicks
    $(document).on('click', '.view-location-btn', function () {
        let lat = $(this).data('lat');
        let lng = $(this).data('lng');
        let mapUrl = `https://www.google.com/maps?q=${lat},${lng}&output=embed`;

        $("#mapIframe").attr("src", mapUrl);
        $("#mapModal").removeAttr('aria-hidden'); // Ensure accessibility
        $("#mapModal").modal('show');
    });


    // Ensure iframe is cleared when modal closes
    $("#mapModal").on("hidden.bs.modal", function () {
        $(this).attr('aria-hidden', 'true'); // Restore when closed
        $("#mapIframe").attr("src", "");
    });





   





























});





// ✅ Unified Function for Handling Check-In & Check-Out
function handleAttendance(action) {
   
    const employeeId = $('#employee').val();
    if (!employeeId) {
        alert("Please select an employee before proceeding.");
        return;
    }


    let requestData = { employeeId, action };
    let url = `/Employee/Employee/${action}`;

    //if (['CheckIn', 'CheckOut'].includes(action) && navigator.geolocation)


    if (action === 'CheckIn' && $('#checkInBtn').text().includes('Re-CheckIn')) {
        requestData.followUpShift = 'Yes';
    }

    if (action === 'CheckOut' && $('#checkOutBtn').text().includes('Re-Check-Out')) {
        requestData.followUpShift = 'No';
    }


    if ((action === 'CheckIn' || action === 'CheckOut') && navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            (position) => {
                //requestData.location = `Lat: ${position.coords.latitude}, Long: ${position.coords.longitude}`;
                requestData.location = `${position.coords.latitude},${position.coords.longitude}`;
                sendAttendanceRequest(url, requestData);
            },
            (error) => {
                console.error("❌ Location error:", error);
                alert("Enable GPS and allow location access for accurate check-in.");
            },
            { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
        );
    } else {
        sendAttendanceRequest(url, requestData);
    }
}

// ✅ Send Attendance AJAX Request
function sendAttendanceRequest(url, requestData) {
    $.ajax({
        url,
        method: 'POST',
        data: requestData,
        success: function () {

            console.log(`✅ ${requestData.action} successful.`);
            LoadAttendanceDetails(); // Refresh UI
            setTimeout(() => {
                updateButtonLabels();
            }, 500);
            fetchAttendance()
        },
        error: function (xhr) {
            console.error(`❌ ${requestData.action} failed:`, xhr.responseText);
        }
    });
}

// ✅ Logout Function
function handleLogout() {
    $.ajax({
        url: '/account/Logout',
        method: 'POST',
        headers: {
            "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
        },
        success: function () {
            window.location.href = '/Home/Index';
        },
        error: function (xhr) {
            console.error("Logout failed:", xhr.responseText);
            alert("Logout failed. Please try again.");
        }
    });
}

// ✅ Load Attendance Details
function LoadAttendanceDetails() {
    $.ajax({
        url: '/Employee/Employee/LoadAttendanceDetails',
        method: 'GET',
        success: function (data) {
            $('#attendanceDetailsContainer').html(data);
            updateButtonVisibility($('#attendanceStatus').val());
        },
        error: function (xhr) {
            console.error("❌ Failed to refresh attendance details:", xhr.responseText);
        }
    });
}



// ✅ Update Check-In & Check-Out Button Visibility
function updateButtonVisibility(status) {
    $('#checkInBtn').toggle(status !== "Present");
    $('#checkOutBtn').toggle(status === "Present");
    

    //Not Available

}







//log attendance
document.addEventListener("DOMContentLoaded", () => {
    const now = new Date();
    const logYear = document.getElementById('logYear');
    const logMonth = document.getElementById('logMonth');
    const logDay = document.getElementById('logDay');

    // Populate year dropdown
    for (let y = 2022; y <= now.getFullYear(); y++) {
        const opt = new Option(y, y, y === now.getFullYear(), y === now.getFullYear());
        logYear.add(opt);
    }

    // Populate month dropdown
    for (let m = 1; m <= 12; m++) {
        const opt = new Option(m, m, m === (now.getMonth() + 1), m === (now.getMonth() + 1));
        logMonth.add(opt);
    }

    // Populate day dropdown
    for (let d = 1; d <= 31; d++) {
        const opt = new Option(d, d, d === now.getDate(), d === now.getDate());
        logDay.add(opt);
    }

    async function loadAttendanceLogs() {
        const year = logYear.value;
        const month = logMonth.value;
        const day = logDay.value;

        try {
            const response = await fetch(`/Employee/Employee/AttendanceLog?year=${year}&month=${month}&day=${day}`);
            if (!response.ok) throw new Error("Failed to load logs");

            const html = await response.text();
            document.getElementById('attendanceLogTableBody').innerHTML = html;
        } catch (error) {
            document.getElementById('attendanceLogTableBody').innerHTML = `
                        <tr><td colspan="3" class="text-danger text-center">Error loading logs</td></tr>`;
        }
    }

    // Event listeners
    document.getElementById('attendanceLogModal').addEventListener('show.bs.modal', loadAttendanceLogs);
    logYear.addEventListener('change', loadAttendanceLogs);
    logMonth.addEventListener('change', loadAttendanceLogs);
    logDay.addEventListener('change', loadAttendanceLogs);
});





// Fetch Attendance Data
function fetchAttendance() {
    console.log("fetchAttendance called");

    const today = new Date();
    const month = $('select[name="month"]').val() || today.getMonth() + 1;
    const year = $('select[name="year"]').val() || today.getFullYear();
    const employeeId = $("#employee").val(); // Using the employeeId from Razor


    //console.log(month);
    //console.log(year);
    //console.log(employeeId);

    $.ajax({
        url: '/Employee/Employee/GetEmployeeAttendanceById',
        method: 'GET',
        data: {
            employeeId: employeeId,
            month: month,
            year: year
        },
       
     



        success: function (data) {
            
            //console.log(`${data}`);

            //console.logFetched Data: " + data);

            //console.log("Fetched Data: " + JSON.stringify(data, null, 2));

            const tableHead = $('#attendanceHead');
            const tableBody = $('#attendanceBody');
            const tableContainer = $('#attendanceTableContainer');
            const messageContainer = $('#noDataMessage');
            const instructions = $('#instructions');

            tableHead.empty();
            tableBody.empty();

            if (!data || data.length === 0 || data.message) {
                tableContainer.hide();
                messageContainer.text(data.message || "No attendance data found for selected month and year.").show();
                instructions.removeClass("d-flex").addClass("d-none");
                return;
            }

            messageContainer.hide();
            tableContainer.show();
            instructions.removeClass("d-none").addClass("d-flex");

            const daysInMonth = new Date(year, month, 0).getDate();

            // Header rows
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

            // Group data by employee
            const employeeMap = {};
            data.forEach(item => {
                if (!employeeMap[item.employeeID]) {
                    employeeMap[item.employeeID] = {
                        name: item.employeeName,
                        attendance: {}
                    };
                }
                const day = new Date(item.attendanceDate).getDate();
                employeeMap[item.employeeID].attendance[day] = item.status;
            });

            // Render each employee's attendance
            for (const empId in employeeMap) {
                const emp = employeeMap[empId];
                let row = `<tr><td>${emp.name}</td>`;

                for (let d = 1; d <= daysInMonth; d++) {
                    const status = emp.attendance[d];
                    if (status === "Present") {
                        row += "<td>✔️</td>";
                    } else if (status === "Absent") {
                        row += "<td>❌</td>";
                    } else if (status === "Leave") {
                        row += "<td>⏸️</td>";
                    } else if (status === "Half Day") {
                        row += "<td>🌓</td>";
                    } else {
                        row += "<td></td>";
                    }
                }
                row += "</tr>";
                tableBody.append(row);
            }
        },
        error: function (err) {
            console.error("Error fetching attendance:", err);
            $('#noDataMessage').text("Error fetching attendance data. Please try again.").show();
            $('#attendanceTableContainer').hide();
            $('#instructions').removeClass("d-flex").addClass("d-none");
        }
    });














}



function updateButtonLabels() {
    const checkInTime = $('#checkInTime').val();
    const checkOutTime = $('#checkOutTime').val();
    const followUpShift = $('#followUpShift').val()?.trim();


    // Hide both buttons initially
    $('#checkInBtn').hide();
    $('#checkOutBtn').hide();

    // 1. No Check-In or Check-Out yet
    if (checkOutTime === "Not Available" && checkInTime === "Not Available" && followUpShift === "No" ) {
        $('#checkInBtn').html('✅ Check-In').show();
    }

    // 2. Checked In, but not yet Checked Out
    else if (checkOutTime === "Not Available" && checkInTime !== "Not Available" && followUpShift === "No") {
        $('#checkOutBtn').html('🚪 Check-Out').show();
    }

    // Case 3: Checked In and Checked Out already (Re-CheckIn & Re-CheckOut)
    else if (checkInTime !== "Not Available" && checkOutTime !== "Not Available" && followUpShift === "No") {
        $('#checkInBtn').html('✅ Re-CheckIn').show();
    }

    else if (checkInTime !== "Not Available" && checkOutTime !== "Not Available" && followUpShift === "Yes") {
              $('#checkOutBtn').html('🚪 Re-Check-Out').show();
        

    }
}
