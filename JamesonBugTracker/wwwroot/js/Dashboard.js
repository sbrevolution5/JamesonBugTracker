
function charts() {//Priority Chart JS
    newDoughnutChart("#devPriorityChart", ['Urgent', 'High', 'Medium', 'Low'], devPriority)
    newDoughnutChart("#devTypeChart", ['New Development', 'UI', 'Runtime', 'Maintenance'], devTypes)
    newDoughnutChart("#devStatusChart", ['Archived', 'Resolved', 'Testing', 'Development', 'Unassigned', 'New'], devStatuses)
    newDoughnutChart("#subPriorityChart", ['Urgent', 'High', 'Medium', 'Low'], subPriority)
    newDoughnutChart("#subTypeChart", ['New Development', 'UI', 'Runtime', 'Maintenance'], subTypes)
    newDoughnutChart("#subStatusChart", ['Archived', 'Resolved', 'Testing', 'Development', 'Unassigned','New'], subStatuses)
}
charts();