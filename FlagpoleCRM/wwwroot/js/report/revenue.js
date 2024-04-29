
function loadRevenueChart(data) {
    Highcharts.setOptions({
        lang: {
            decimalPoint: '.',
            thousandsSep: ',',
        }
    });

    Highcharts.stockChart('revenue-chart', {
        rangeSelector: {
            selected: 1
        },

        title: {
            text: 'Revenue Chart'
        },

        series: [{
            name: 'Revenue',
            data: data,
            type: 'line',
            color: 'green',
            tooltip: {
                valueDecimals: 2
            }
        }]
    });
}

