function charts() {//Priority Chart JS
    newDoughnutChart("#priorityChart", ['Urgent', 'High', 'Medium', 'Low'], priority)
    newDoughnutChart("#typeChart", ['Maintenance', 'UI', 'Runtime', 'New Development'], types)
    newDoughnutChart("#statusChart", ['Archived', 'Resolved', 'Testing', 'Development', 'Unassigned', 'New'], statuses)
}
console.log("CHARTING")
charts();