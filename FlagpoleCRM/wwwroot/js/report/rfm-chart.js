function loadPieChart(data) {
    Highcharts.setOptions({
        colors: ['#63afff', '#96ffa3', '#f7cd92', '#e49afc', '#f5f390', '#f5a6c1', '#a8a232', '#c9327b', '#b85f30', '#13767d', '#cf1111', '#363636']
    });
    Highcharts.chart('rfm-piechart', {
        chart: {
            type: 'pie',
            backgroundColor: 'transparent',
        },
        tooltip: {
            valueSuffix: '%'
        },
        title: {
            text: 'RFM Analysis',
            style: {
                color: '#ffffff',
                font: 'roboto'
            }
        },
        subtitle: {},
        legend: {
            itemStyle: {
                color: '#fff',
                fontWeight: 'normal'
            }
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    enabled: false
                },
                showInLegend: true,
            }
        },
        series: [
            {
                name: 'Percentage',
                colorByPoint: true,
                data: data
            }
        ]
    });
}

