function ticketPriBgColor() {
    let priority = document.getElementById("ticketPriorityText").innerText
    let ticketPri = "";
    if (priority == "Urgent") {
        ticketPri = "bg-danger";
    }
    else if (priority == "High") {
        ticketPri = "bg-warning";
    }
    else if (priority == "Medium") {
        ticketPri = "bg-success";
    }
    else if (priority == "Low") {
        ticketPri = "bg-primary";
    }
    document.getElementById("ticketPriorityBox").classList.toggle(ticketPri)
}
function ticketStatusBgColor() {
    let status = document.getElementById("ticketStatusText").innerText
    let ticketStatus = "";
    document.getElementById("ticketStatusBox").classList = ["small-box"]

    if (status == "Unassigned") {
        ticketStatus = "bg-danger";
    }
    else if (status == "Testing") {
        ticketStatus = "bg-warning";
    }
    else if (status == "Resolved") {
        ticketStatus = "bg-success";
    }
    else if (status == "Development") {
        ticketStatus = "bg-primary";
    }
    else if (status == "New") {
        ticketStatus = "bg-secondary"
    }
    document.getElementById("ticketStatusBox").classList.toggle(ticketStatus)
}
function ticketTypeBgColor() {
    let type = document.getElementById("ticketTypeText").innerText
    let ticketType = "";
    if (type == "Runtime") {
        ticketType = "bg-danger";
    }
    else if (type == "Maintenance") {
        ticketType = "bg-warning";
    }
    else if (type == "New Development") {
        ticketType = "bg-success";
    }
    else if (type == "UI") {
        ticketType = "bg-primary";
    }
    document.getElementById("ticketTypeBox").classList.toggle(ticketType)
}
//Ticket details Ajax, used to make ticket changes without resubmitting page.
var toTesting = $("#toTesting")
var returnTesting = $("#returnTesting")
var closeTicket = $("#closeTicket")
var reopenTicket = $("#reopenTicket")
var statusOverlay = $("#statusOverlay")
var devOverlay = $("#devOverlay")
statusOverlay.hide()
devOverlay.hide()
function statusButtons(status, num) {
    if (status == "Development") {
        toTesting.show(num)
        returnTesting.hide(num)
        reopenTicket.hide(num)
        closeTicket.hide(num)
    }
    else if (status == "Testing") {
        toTesting.hide(num)
        returnTesting.show(num)
        reopenTicket.hide(num)
        closeTicket.show(num)
    }
    else if (status == "Resolved" && canReopen) {
        toTesting.hide(num)
        returnTesting.hide(num)
        reopenTicket.show(num)
        closeTicket.hide(num)
    }
    else if (status == "unassigned" || status == "archived") {
        toTesting.hide(num)
        returnTesting.hide(num)
        reopenTicket.hide(num)
        closeTicket.hide(num)
    }
}
statusButtons(ticketStatus,0)
$(".assignForm").on("submit", function (e) {

    var dataString = $(this).serialize();
    var userFullName = $(this).children("div").children("select").children("option").filter(":selected").text()
    $.ajax({
        type: "POST",
        url: "/Tickets/AssignUser",
        data: dataString,
        success: function (result) {
            
            toastr.success(`${userFullName} assigned to ticket`)
            $("#developerName").text(userFullName)
            $("#ticketStatusText").text("Development")
            ticketStatus = $("#ticketStatusText").text
            statusOverlay.hide(1000)
            devOverlay.hide(1000)
            ticketStatusBgColor();
            statusButtons(ticketStatus,1000)
        }
    });
    e.preventDefault();
    statusOverlay.show(1000)
    devOverlay.show(1000)
    toastr.info('Trying to assign user to ticket. Please Wait')

});
$(".updateStatus").on("submit", function (e) {

    var dataString = $(this).serialize();
    var newStatus = $(this).children(".statusName")[0].value
    console.log(newStatus)
    $.ajax({
        type: "POST",
        url: "/Tickets/UpdateStatus",
        data: dataString,
        success: function (result) {
            toastr.success(`Ticket Status was updated`)
            ticketStatus = newStatus
            $("#ticketStatusText").text(ticketStatus)
            statusOverlay.hide(1000)

            ticketStatusBgColor();
            statusButtons(ticketStatus,1000)
        }
    });
    e.preventDefault();
    statusOverlay.show()

    toastr.info('Updating Ticket Status. Please Wait')
});