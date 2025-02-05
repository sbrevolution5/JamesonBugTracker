function ticketPriLabelColor(e) {
    let priority = e.innerText
    let ticketPri = "";
    if (priority == "Urgent") {
        ticketPri = "badge-danger";
    }
    else if (priority == "High") {
        ticketPri = "badge-orange";
    }
    else if (priority == "Medium") {
        ticketPri = "badge-warning";
    }
    else if (priority == "Low") {
        ticketPri = "badge-success";
    }
    e.classList.toggle(ticketPri)
}

let badges = document.getElementsByClassName("priorityBadge")
for (let item of badges) {
    ticketPriLabelColor(item)
}