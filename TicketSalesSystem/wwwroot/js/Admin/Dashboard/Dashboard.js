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
    fetch('/Admin/GetOrderStatusDistribution').then(res => res.json()).then(data => {
        new Chart(document.getElementById('orderStatusChart'), {
            type: 'doughnut',
            data: {
                labels: data.map(d => d.status),
                datasets: [{
                    data: data.map(d => d.count),
                    backgroundColor: ['#00f2ff', '#7000ff', '#ff0055', '#39ff14'],
                    borderColor: '#000b1a',
                    borderWidth: 3
                }]
            },
            options: {
                maintainAspectRatio: false,
                cutout: '75%',
                plugins: { legend: { position: 'right', labels: { color: '#00f2ff' } } }
            }
        });
    });

    // 4. Alerts
    function loadLiveAlerts() {
        fetch('/Admin/GetLiveAlerts').then(res => res.json()).then(data => {
            const container = document.getElementById('alertsContainer');
            container.innerHTML = data.map(alert => `
                        <div class="p-2 border-start border-${alert.level} border-4 mb-2"
                             style="background: rgba(255, 255, 255, 0.03); border-color: var(--tech-${alert.level}) !important;">
                            <div class="tech-label text-${alert.level}" style="font-size:0.7rem; color: var(--tech-${alert.level}) !important;">
                                [ ALERT_${alert.level.toUpperCase()} ]
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
    fetch('/Admin/GetLiveBackgroundLogs')
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.json();
        })
        .then(data => {
            // 1. 更新日誌內容 (對應小寫 logs)
            const logContainer = document.getElementById('log-container');
            if (logContainer && data.logs) {
                logContainer.innerHTML = data.logs.map(log =>
                    `<div class="mb-1"><span style="color: #0f0; opacity: 0.6;">[LOG]</span> ${log}</div>`
                ).join('');
            }

            // 2. 更新狀態文字與 Spinner (對應小寫 status)
            const statusText = document.getElementById('current-status-text');
            const statusSpinner = document.getElementById('status-spinner');

            if (statusText) statusText.innerText = data.status;

            if (statusSpinner) {
                // 使用邏輯更清晰的切換方式
                if (data.status === "待機中") {
                    statusSpinner.className = "spinner-grow text-secondary mb-2";
                } else {
                    statusSpinner.className = "spinner-grow text-success mb-2 shadow-glow";
                }
            }
        })
        .catch(err => {
            // 🚩 當連線失敗時（例如伺服器重啟中），給管理員一個提示
            const statusText = document.getElementById('current-status-text');
            if (statusText) statusText.innerHTML = '<span class="text-danger">連線中斷...</span>';
            console.warn("監控服務暫時無法連線:", err);
        });
}

// 🚩 設定每 5 秒刷新一次
setInterval(refreshBackgroundLogs, 5000);

// 🚩 頁面載入後立即執行一次，不要等 5 秒才出現
document.addEventListener('DOMContentLoaded', refreshBackgroundLogs);