var backgroundColor = ['#f56954', '#00a65a', '#f39c12', '#00c0ef', '#3c8dbc', '#d2d6de', '#6f42c1']// defaults
var textColor
//used to tell dark mode to use darkswap
page = "ProjectDetails"
function darkSwap() {
    var isDark = document.getElementById("layoutBody").classList.contains("dark-mode")
    if (isDark) {
        //Dark colors RED Orange YELLOW Green blue (primary) 
        backgroundColor = ['#e74b3c', '#fd7e14', '#f39c12', '#06BC8C', '#3f6791', '#6f42c1']
        textColor = '#ffffff'
    } else {
        //Light colors
        backgroundColor = ['#dc3545', '#fd7e14', '#ffc108', '#27a745', '#1385ff', '#6f42c1',]
        textColor = '#202529'

    }
    charts()
}
function newDoughnutChart(id, labels, data) {
    var donutChartCanvas = $(id).get(0).getContext('2d')
    var donutData = {
        labels: labels,
        datasets: [
            {
                data: data,
                backgroundColor: backgroundColor,
            }
        ]
    }
    new Chart(donutChartCanvas, {
        type: 'doughnut',
        data: donutData,
        options: {
            maintainAspectRatio: false,
            responsive: true,
                legend: {
                    labels: {
                        fontColor: textColor
                    }
            }
        }
    })
}