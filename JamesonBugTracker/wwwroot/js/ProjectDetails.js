var backgroundColor = ['#f56954', '#00a65a', '#f39c12', '#00c0ef', '#3c8dbc', '#d2d6de']
function darkSwap() {
    var isDark = document.getElementById("layoutBody").classList.contains("dark-mode")
    if (isDark) {
        backgroundColor = ['#f56954', '#00a65a', '#f39c12', '#00c0ef', '#3c8dbc', '#d2d6de']
    } else {
        backgroundColor = ['#ffffff', '#000000', '#f39c12', '#00c0ef', '#3c8dbc', '#d2d6de']
    }
    console.log("bg color changed")
    charts()
}
function charts(){//Priority Chart JS
    var donutChartCanvas = $('#priorityChart').get(0).getContext('2d')
    var donutData = {
        labels: [
            'Chrome',
            'IE',
            'FireFox',
            'Safari',
            'Opera',
            'Navigator',
        ],
        datasets: [
            {
                data: [700, 500, 400, 600, 300, 100],
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


    //Type Chart JS
    var donutChartCanvas = $('#typeChart').get(0).getContext('2d')
    var donutData = {
        labels: [
            'Chrome',
            'IE',
            'FireFox',
            'Safari',
            'Opera',
            'Navigator',
        ],
        datasets: [
            {
                data: [700, 500, 400, 600, 300, 100],
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
            'Chrome',
            'IE',
            'FireFox',
            'Safari',
            'Opera',
            'Navigator',
        ],
        datasets: [
            {
                data: [700, 500, 400, 600, 300, 100],
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