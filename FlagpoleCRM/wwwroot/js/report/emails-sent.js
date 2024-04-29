
function loadEmailsSentChart(data) {
    Highcharts.stockChart('emails-sent-chart', {
        rangeSelector: {
            selected: 1
        },

        title: {
            text: 'Emails Sent'
        },

        series: [{
            name: 'Emails',
            data: data,
            type: 'line',
            color: '#f55fc3',
        }]
    });
}

