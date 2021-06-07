var backgroundColor = ['#f56954', '#00a65a', '#f39c12', '#00c0ef', '#3c8dbc', '#d2d6de']// defaults
var textColor
//used to tell dark mode to use darkswap
page = "ProjectDetails"
function darkSwap() {
    var isDark = document.getElementById("layoutBody").classList.contains("dark-mode")
    if (isDark) {
        //Dark colors RED Orange YELLOW Green blue (primary) 
        backgroundColor = ['#e74b3c', '#fd7e14', '#f39c12', '#06BC8C', '#3f6791']
        textColor = '#ffffff'
    } else {
        //Light colors
        backgroundColor = ['#dc3545', '#fd7e14', '#ffc108', '#27a745', '#1385ff', '#3c8dbc', '#fd7e14']
        textColor = '#202529'

    }
    charts()
}
function charts() {//Priority Chart JS
    var donutChartCanvas = $('#priorityChart').get(0).getContext('2d')
    var donutData = {
        labels: [
            'Urgent',
            'High',
            'Medium',
            'Low'
        ],
        datasets: [
            {
                data: priority,
                backgroundColor: backgroundColor,
            }
        ]
    }
    var donutOptions = {
        maintainAspectRatio: false,
        responsive: true
    }
    new Chart(donutChartCanvas, {
        type: 'doughnut',
        data: donutData,
        options: donutOptions
    })


    //Type Chart JS
    var donutChartCanvas = $('#typeChart').get(0).getContext('2d')
    var donutData = {
        labels: [
            'New Development',
            'Runtime',
            'UI',
            'Maintenance'
        ],
        datasets: [
            {
                data: types,
                backgroundColor: backgroundColor,
            }
        ]
    }
    var donutOptions = {
        maintainAspectRatio: false,
        responsive: true,
    }
    new Chart(donutChartCanvas, {
        type: 'doughnut',
        data: donutData,
        options: donutOptions
    })


    //Status Chart JS
    var donutChartCanvas = $('#statusChart').get(0).getContext('2d')
    var donutData = {
        labels: [
            'New',
            'Unassigned',
            'Testing',
            'Development',
            'Resolved',
        ],
        datasets: [
            {
                data: statuses,
                backgroundColor: backgroundColor,
            }
        ]
    }
    var donutOptions = {
        maintainAspectRatio: false,
        responsive: true,
    }
    new Chart(donutChartCanvas, {
        type: 'doughnut',
        data: donutData,
        options: donutOptions
    })
}
charts();