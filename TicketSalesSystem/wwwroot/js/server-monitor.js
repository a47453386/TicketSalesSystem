// wwwroot/js/server-monitor.js

function initServerMonitor() {
    console.log("Monitor System: Initializing...");

    // 🚩 1. 真實數據抓取 (人流)
    function fetchTraffic() {
        const loadEl = document.getElementById('server-load');
        const dotEl = document.getElementById('load-dot');

        // 如果頁面上沒這些元素，就跳過這次抓取
        if (!loadEl || !dotEl) return;

        $.get('/Home/GetPublicServerStatus', function (data) {
            const statusMap = {
                "HEAVY": { text: "繁忙", color: "#ff3333", class: "text-danger" },
                "MODERATE": { text: "適中", color: "#ffcc00", class: "text-warning" },
                "STABLE": { text: "穩定", color: "#00ffcc", class: "text-success" }
            };

            const config = statusMap[data.load] || statusMap["STABLE"];
            loadEl.textContent = config.text;
            loadEl.className = config.class;
            dotEl.style.color = config.color;
        }).fail(() => {
            console.error("Monitor System: Connection Lost.");
        });
    }

    // 🚩 2. 模擬即時跳動 (在線率)
    function flickerUptime() {
        const uptimeEl = document.getElementById('uptime-val');
        if (uptimeEl) {
            const base = 99.97;
            const jitter = (Math.random() * 0.029).toFixed(3);
            uptimeEl.textContent = (base + parseFloat(jitter)).toFixed(2);
        }
    }

    // 🚩 3. 系統時鐘
    function updateClock() {
        const clockEl = document.getElementById('sync-clock');
        if (clockEl) {
            const now = new Date();
            clockEl.textContent = now.toTimeString().split(' ')[0];
        }
    }

    // 設定循環任務
    setInterval(updateClock, 1000);
    setInterval(fetchTraffic, 30000);
    setInterval(flickerUptime, 3000);

    // 立即執行一次初始化
    updateClock();
    fetchTraffic();
    flickerUptime();
}

// 頁面載入後啟動
$(document).ready(initServerMonitor);