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
        plotOptions: {
            series: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: [{
                    enabled: true,
                    distance: 20,
                    style: {
                        color: '#ffffff'
                    }
                }, {
                    enabled: true,
                    distance: -40,
                    format: '{point.percentage:.1f}%',
                    style: {
                        fontSize: '1.2em',
                        textOutline: 'none',
                        opacity: 1
                    },
                    filter: {
                        operator: '>',
                        property: 'percentage',
                        value: 10
                    }
                }]
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

