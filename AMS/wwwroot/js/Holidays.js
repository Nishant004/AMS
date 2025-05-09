// Show success/error message
function showResult(message, success = true) {
    const result = document.getElementById("result");
    result.className = "alert " + (success ? "alert-success" : "alert-danger");
    result.innerText = message;
    result.style.display = "block";
    result.scrollIntoView({ behavior: "smooth" });

    setTimeout(() => {
        result.style.display = "none";
    }, 4000);
}

// Quota form submission
document.getElementById("bulkQuotaForm")?.addEventListener("submit", async function (e) {
    e.preventDefault();
    const data = Object.fromEntries(new FormData(this));
    const res = await fetch("/Admin/Holiday/SetQuotaForAll", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    });
    const msg = await res.text();
    showResult(msg, res.ok);
    if (res.ok) loadQuotaTable(data.year);
});

// Load quota table
async function loadQuotaTable() {
    const year = document.getElementById("quotaViewYear").value;
    try {
        const res = await fetch(`/Admin/Holiday/GetAllQuotas?year=${year}`);
        const html = await res.text();
        document.getElementById("quotaTableContainer").innerHTML = html;
    } catch {
        document.getElementById("quotaTableContainer").innerHTML = "<div class='alert alert-danger'>Failed to load data.</div>";
    }
}

// Update leave status
function updateLeaveStatus(leaveId) {
    const dropdown = document.querySelector(`.status-dropdown[data-leave-id='${leaveId}']`);
    const newStatus = dropdown.value;

    fetch('/Admin/Holiday/UpdateLeaveStatus', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ leaveId, status: newStatus })
    }).then(res => {
        if (res.ok) {
            showResult("Status updated.");
            loadPendingLeaves();
        } else {
            showResult("Failed to update status.", false);
        }
    });
}

// Load Add Holiday tab
async function AddHolidayLeaves() {
    const res = await fetch("/Admin/Holiday/AddHolidayForm");
    const html = await res.text();
    document.getElementById("AddHolidayContainer").innerHTML = html;
    attachAddHolidayFormHandler();
    loadHolidays(); // refresh list
}

// Handle Add Holiday Form
function attachAddHolidayFormHandler() {
    const form = document.getElementById("addHolidayForm");
    if (!form) return;

    form.addEventListener("submit", async function (e) {
        e.preventDefault();
        const formData = Object.fromEntries(new FormData(form));
        const res = await fetch('/Admin/Holiday/AddHoliday', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(formData)
        });

        const msg = await res.text();
        document.getElementById("addHolidayResult").innerHTML =
            `<div class="alert ${res.ok ? 'alert-success' : 'alert-danger'}">${msg}</div>`;

        if (res.ok) loadHolidays();
    });
}

// Add all Sundays
async function addSundays() {
    const year = parseInt(document.getElementById("sundayYear").value);
    console.log("Year entered:", year); // Debug log
    if (!year || year < 2000 || year > 2100) {
        document.getElementById("addSundaysResult").innerHTML =
            `<div class="alert alert-danger">Please enter a valid year (2000-2100).</div>`;
        return;
    }

    const res = await fetch(`/Admin/Holiday/AddSundays/${year}`, { method: "POST" });
    const msg = await res.text();
    document.getElementById("addSundaysResult").innerHTML =
        `<div class="alert ${res.ok ? 'alert-success' : 'alert-danger'}">${msg}</div>`;

    if (res.ok) loadHolidays();
}

// Load holiday list
async function loadHolidays() {
    try {
        const res = await fetch("/Admin/Holiday/GetAllHolidays");
        const html = await res.text();
        const container = document.getElementById("holidayListContainer");
        if (container) container.innerHTML = html;
    } catch {
        document.getElementById("holidayListContainer").innerHTML =
            "<div class='alert alert-danger'>Failed to load holidays.</div>";
    }
}

// Leave status tab loading
async function loadPendingLeaves() {
    const res = await fetch("/Admin/Holiday/GetPendingLeaves");
    document.getElementById("pendingLeavesContainer").innerHTML = await res.text();
}

async function loadApprovedLeaves() {
    const res = await fetch("/Admin/Holiday/GetApprovedLeaves");
    document.getElementById("approvedLeavesContainer").innerHTML = await res.text();
}

async function loadRejectedLeaves() {
    const res = await fetch("/Admin/Holiday/GetRejectedLeaves");
    document.getElementById("rejectedLeavesContainer").innerHTML = await res.text();
}

// Initialize
document.addEventListener("DOMContentLoaded", function () {
    loadQuotaTable();

    document.getElementById("pending-tab")?.addEventListener("click", loadPendingLeaves);
    document.getElementById("approved-tab")?.addEventListener("click", loadApprovedLeaves);
    document.getElementById("rejected-tab")?.addEventListener("click", loadRejectedLeaves);
    document.getElementById("AddHoliday-tab")?.addEventListener("click", AddHolidayLeaves);
});
