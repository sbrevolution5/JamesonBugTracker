﻿function ticketPriBgColor() {
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
        toTesting.fadeIn(num)
        returnTesting.fadeOut(num)
        reopenTicket.fadeOut(num)
        closeTicket.fadeOut(num)
    }
    else if (status == "Testing") {
        toTesting.fadeOut(num)
        returnTesting.fadeIn(num)
        reopenTicket.fadeOut(num)
        closeTicket.fadeIn(num)
    }
    else if (status == "Resolved" && canReopen) {
        toTesting.fadeOut(num)
        returnTesting.fadeOut(num)
        reopenTicket.fadeIn(num)
        closeTicket.fadeOut(num)
    }
    else{
        toTesting.fadeOut(num)
        returnTesting.fadeOut(num)
        reopenTicket.fadeOut(num)
        closeTicket.fadeOut(num)
    }
}
statusButtons(ticketStatus,0)
$(".assignForm").on("submit", function (e) {

    var dataString = $(this).serialize();
    var userFullName = $(this).children("div").children("select").children("option").filter(":selected").text()
    var formId = $(this).attr('id')
    console.log(formId)
    $(".assignBtn").attr("disabled", true)
    $.ajax({
        type: "POST",
        url: "/Tickets/AssignUser",
        data: dataString,
        success: function (result) {
            // if it was assign, fade out this button add in reassign/remove
            $(`#reassignSelect option:contains(${userFullName})`).prop('selected', true)
            if (formId == "assign") {
                $("#assign").fadeOut(600)
                $("#reassign").fadeIn(600)
                $("#unassign").fadeIn(600)
            }
            toastr.success(`${userFullName} assigned to ticket`)
            $("#developerName").text(userFullName)
            $("#ticketStatusText").text("Development")
            ticketStatus = $("#ticketStatusText").text
            statusOverlay.fadeOut(600)
            devOverlay.fadeOut(600)
            ticketStatusBgColor();
            statusButtons("Development", 600)
            $(".assignBtn").attr("disabled", false)
            $("#newHistory").fadeIn(600);

        },
        error: function (result) {
            $(".assignBtn").attr("disabled", false)

        }
    });
    e.preventDefault();
    statusOverlay.fadeIn(600)
    devOverlay.fadeIn(600)
    toastr.info('Trying to assign user to ticket. Please Wait')

});
$(".unassignForm").on("submit", function (e) {

    var dataString = $(this).serialize();
    $(".assignBtn").attr("disabled", true)
    $.ajax({
        type: "POST",
        url: "/Tickets/UnassignUser",
        data: dataString,
        success: function (result) {
            
                $("#assign").fadeIn(600)
                $("#reassign").fadeOut(600)
                $("#unassign").fadeOut(600)

            toastr.success(`Developer removed from ticket`)
            $("#ticketStatusText").text("Unassigned")
            $("#developerName").text("Unassigned")
            ticketStatus = $("#ticketStatusText").text
            statusOverlay.fadeOut(600)
            devOverlay.fadeOut(600)
            ticketStatusBgColor();
            statusButtons("Unassigned", 600)
            $(".assignBtn").attr("disabled", false)
            $("#newHistory").fadeIn(600);

        },
        error: function (result) {
            $(".assignBtn").attr("disabled", false)

        }
    });
    e.preventDefault();
    statusOverlay.fadeIn(600)
    devOverlay.fadeIn(600)
    toastr.info('Trying to unassign user to ticket. Please Wait')

});
$(".updateStatus").on("submit", function (e) {

    var dataString = $(this).serialize();
    var newStatus = $(this).children(".statusName")[0].value
    $(".updateBtn").attr("disabled", true)

    $.ajax({
        type: "POST",
        url: "/Tickets/UpdateStatus",
        data: dataString,
        success: function (result) {
            toastr.success(`Ticket Status was updated`)
            ticketStatus = newStatus
            $("#ticketStatusText").text(ticketStatus)
            statusOverlay.fadeOut(600)

            ticketStatusBgColor();
            statusButtons(ticketStatus, 600)
            $(".updateBtn").attr("disabled", false)
            $("#newHistory").fadeIn(600);

        },
        error: function (result) {
            toastr.danger("Something went wrong, status wasn't updated")
            $(".updateBtn").attr("disabled", false)

        }
    });
    e.preventDefault();
    statusOverlay.fadeIn(600)

    toastr.info('Updating Ticket Status. Please Wait')
});