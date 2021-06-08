
function charts() {//Priority Chart JS
    newDoughnutChart("#devPriorityChart", ['Urgent', 'High', 'Medium', 'Low'], devPriority)
    newDoughnutChart("#devTypeChart", ['Maintenance', 'UI', 'Runtime', 'New Development'], devTypes)
    newDoughnutChart("#devStatusChart", ['Archived', 'Resolved', 'Testing', 'Development', 'Unassigned', 'New'], devStatuses)
    newDoughnutChart("#subPriorityChart", ['Urgent', 'High', 'Medium', 'Low'], subPriority)
    newDoughnutChart("#subTypeChart", ['Maintenance', 'UI', 'Runtime', 'New Development'], subTypes)
    newDoughnutChart("#subStatusChart", ['Archived', 'Resolved', 'Testing', 'Development', 'Unassigned','New'], subStatuses)
}
charts();