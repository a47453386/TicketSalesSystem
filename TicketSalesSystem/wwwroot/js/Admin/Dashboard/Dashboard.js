document.addEventListener("DOMContentLoaded", function () {
    // --- 🚩 Chart 全域科技風配置 ---
    Chart.defaults.color = '#fff';
    Chart.defaults.font.family = "'Share Tech Mono', monospace";

    const commonScales = {
        x: {
            ticks: { color: 'rgba(0, 242, 255, 0.7)', font: { size: 10 } },
            grid: { color: 'rgba(0, 242, 255, 0.05)' } // 🚩 降低網格線亮度
        },
        y: {
            ticks: { color: 'rgba(255, 255, 255, 0.7)', font: { size: 10 } },
            grid: { color: 'rgba(0, 242, 255, 0.05)' }
        }
    };

    // 1. Bar Chart
    fetch('/Admin/GetAreaSalesData').then(res => res.json()).then(data => {
        new Chart(document.getElementById('salesAreaChart'), {
            type: 'bar',
            data: {
                labels: data.map(d => d.label),
                datasets: [
                    { label: '已售', data: data.map(d => d.sold), backgroundColor: '#00f2ff' },
                    { label: '剩餘', data: data.map(d => d.remaining), backgroundColor: 'rgba(255,255,255,0.1)' }
                ]
            },
            options: { maintainAspectRatio: false, indexAxis: 'y', scales: commonScales }
        });
    });

    // 2. Line Chart
    fetch('/Admin/GetSalesVelocity').then(res => res.json()).then(data => {
        new Chart(document.getElementById('salesVelocityChart'), {
            type: 'line',
            data: {
                labels: data.map(d => d.hour),
                datasets: [{
                    label: '單量',
                    data: data.map(d => d.count),
                    borderColor: '#00f2ff',
                    backgroundColor: 'rgba(0, 242, 255, 0.2)',
                    fill: true,
                    tension: 0.4
                }]
            },
            options: { maintainAspectRatio: false, scales: commonScales }
        });
    });

    // 3. Doughnut Chart (圓餅圖)
    fetch('/Admin/GetOrderStatusDistribution')
        .then(res => res.json())
        .then(data => {
            const statusColors = {
                '已付款': '#00f2ff', 
                '待付款': '#7000ff', 
                '逾期付款': '#ff0055' 
            };

            new Chart(document.getElementById('orderStatusChart'), {
                type: 'doughnut',
                data: {
                    labels: data.map(d => d.status),
                    datasets: [{
                        data: data.map(d => d.count),
                        backgroundColor: data.map(d => statusColors[d.status] || '#444'),
                        borderColor: '#000b1a',
                        borderWidth: 3,
                        hoverOffset: 15 
                    }]
                },
                options: {
                    maintainAspectRatio: false,
                    cutout: '75%',
                    plugins: {
                        legend: {
                            position: 'right',
                            labels: {
                                color: '#00f2ff',
                                font: { size: 14, family: '微軟正黑體' },
                                padding: 20
                            }
                        }
                    }
                }
            });
        });

    // 4. Alerts
    function loadLiveAlerts() {
        fetch('/Admin/GetLiveAlerts').then(res => res.json()).then(data => {
            const container = document.getElementById('alertsContainer');
            if (!container) return;

            if (data.length === 0) {
                // 🚩 保持科技感：所有票區供應正常
                container.innerHTML = `
            <div class="p-2 opacity-50 small">
                <i class="bi bi-check2-circle me-1 text-success"></i>所有票區供應正常
            </div>`;
                return;
            }

            container.innerHTML = data.map(alert => `
            <div class="p-2 border-start border-${alert.level} border-4 mb-2"
                 style="background: rgba(255, 255, 255, 0.03); border-color: var(--tech-${alert.level}) !important;">
                <div class="tech-label text-${alert.level}" style="font-size:0.7rem; color: var(--tech-${alert.level}) !important;">
                   [ ${alert.type === 'SoldOut' ? '已完售' : '即將完售'} ]
                </div>
                <div class="text-white small opacity-75">${alert.message}</div>
            </div>
        `).join('');
        });
    }
    loadLiveAlerts();
    setInterval(loadLiveAlerts, 60000);

    // 5. 活動進場率統計 (Bar Chart)
    fetch('/Admin/GetAttendanceData').then(res => res.json()).then(data => {
        new Chart(document.getElementById('attendanceChart'), {
            type: 'bar',
            data: {
                labels: data.map(d => d.programmeName),
                datasets: [
                    {
                        label: '已進場',
                        data: data.map(d => d.actualEntry),
                        backgroundColor: '#39ff14' // 螢光綠代表進場
                    },
                    {
                        label: '未到場',
                        data: data.map(d => d.notShow),
                        backgroundColor: 'rgba(255, 255, 255, 0.05)'
                    }
                ]
            },
            options: {
                maintainAspectRatio: false,
                scales: {
                    x: { stacked: true, ticks: { color: '#39ff14' } }, // 堆疊顯示
                    y: { stacked: true, ticks: { color: '#fff' } }
                },
                plugins: {
                    legend: { labels: { color: '#39ff14' } }
                }
            }
        });
    });


    // 6. 即時核銷紀錄流水 (Live Scan Feed)
    function loadLiveScanLogs() {
        fetch('/Admin/GetLiveScanLogs').then(res => res.json()).then(data => {
            const container = document.getElementById('scanFeedContainer');
            if (container) {
                container.innerHTML = data.map(log => `
                <div class="d-flex justify-content-between border-bottom border-secondary py-1 mb-1" style="font-size: 0.8rem;">
                    <span style="color: #39ff14;">[OK]</span>
                    <span class="text-white-50">${log.time}</span>
                    <span style="color: #00f2ff;">${log.code}</span>
                    <span class="text-info">${log.programme}</span>
                </div>
            `).join('');
            }
        });
    }
    setInterval(loadLiveScanLogs, 10000); // 每 10 秒刷新一次進場流水
});
function refreshBackgroundLogs() {
    const statusText = document.getElementById('current-status-text');
    const statusSpinner = document.getElementById('status-spinner');
    const logContainer = document.getElementById('log-container');

    // 🚩 記錄更新前的日誌內容（用來比對有沒有新東西）
    const oldLogContent = logContainer ? logContainer.innerHTML : "";

    fetch('/Admin/GetLiveBackgroundLogs')
        .then(response => response.json())
        .then(data => {
            // 1. 更新日誌內容
            if (logContainer && data.logs) {
                const newLogHTML = data.logs.map(log =>
                    `<div class="mb-1"><span style="color: #0f0; opacity: 0.6;">[日誌]</span> ${log}</div>`
                ).join('');

                logContainer.innerHTML = newLogHTML;
                logContainer.scrollTop = logContainer.scrollHeight;

                // 🚩 判斷：內容有沒有變動
                const hasNewActivity = newLogHTML !== oldLogContent;

                if (statusSpinner && statusText) {
                    // 只要日誌有更新，或者後端回傳的不是「待機中」
                    if (hasNewActivity || (data.status && !data.status.includes("待機"))) {
                        // --- 🟢 活躍狀態 ---
                        statusSpinner.className = "spinner-grow text-success mb-2 shadow-glow";
                        statusText.innerText = "系統更新中"; // 改為中文
                        statusText.className = "small text-success fw-bold";
                    } else {
                        // --- ⚪ 待機狀態 ---
                        statusSpinner.className = "spinner-grow text-secondary mb-2";
                        statusText.innerText = data.status || "系統待機中"; // 顯示「待機中」
                        statusText.className = "small text-white-50";
                    }
                }
            }
        })
        .catch(err => {
            if (statusText) statusText.innerHTML = '<span class="text-danger">連線中斷</span>';
            if (statusSpinner) statusSpinner.className = "spinner-grow text-danger mb-2";
        });
}

// 🚩 設定每 5 秒刷新一次
setInterval(refreshBackgroundLogs, 5000);

// 🚩 頁面載入後立即執行一次，不要等 5 秒才出現
document.addEventListener('DOMContentLoaded', refreshBackgroundLogs);