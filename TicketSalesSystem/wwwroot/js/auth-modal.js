/**
 * 會員登入 AJAX 彈窗邏輯 (同步更新版)
 */

function openLoginModal() {
    // 1. 抓取當前網址中的 returnUrl (如果有)
    // 例如：/Login/MemberIndex?ReturnUrl=/Seats/Index/10
    const urlParams = new URLSearchParams(window.location.search);
    const returnUrl = urlParams.get('ReturnUrl') || window.location.pathname;

    // 2. 顯示彈窗殼子
    var modalEl = document.getElementById('loginModal');
    var myModal = new bootstrap.Modal(modalEl);
    myModal.show();

    $('#loginModalBody').html('<div class="text-center py-5"><div class="spinner-border text-info"></div><div class="mt-3 text-white">LOADING...</div></div>');
    myModal.show();

    // 3. 使用 AJAX 抓取內容，並把 returnUrl 帶過去
    $.get('/Login/MemberLogin', { returnUrl: returnUrl }, function (data) {
        $('#loginModalBody').hide().html(data).fadeIn(200);

        // 重新解析驗證標籤
        if (typeof $.validator !== 'undefined' && typeof $.validator.unobtrusive !== 'undefined') {
            $.validator.unobtrusive.parse('#loginModalBody');
        }
    });
}

// 處理登入表單的 AJAX 提交
$(document).on('submit', '#loginModalBody form', function (e) {
    e.preventDefault();
    var $form = $(this);
    var submitBtn = $form.find('button[type="submit"]');

    // 防止重複點擊
    submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> 處理中...');

    $.ajax({
        url: $form.attr('action'),
        type: 'POST',
        data: $form.serialize(),
        success: function (response) {
            if (response.success) {
                
                window.location.href = response.redirectUrl;
            } else {
                // 登入失敗：後端回傳的是 _LoginPartial 的 HTML 片段
                $('#loginModalBody').html(response);

                if (typeof $.validator !== 'undefined' && typeof $.validator.unobtrusive !== 'undefined') {
                    $.validator.unobtrusive.parse('#loginModalBody');
                }
            }
        },
        error: function () {
            alert("系統連線異常，請稍後再試。");
            submitBtn.prop('disabled', false).html('立即登入');
        }
    });
});

function initServerMonitor() {
    // 🚩 1. 真實數據抓取 (人流)
    function fetchTraffic() {
        $.get('/Home/GetPublicServerStatus', function (data) {
            const loadEl = document.getElementById('server-load');
            const dotEl = document.getElementById('load-dot');

            // 轉換中文顯示
            const statusMap = {
                "HEAVY": { text: "繁忙", color: "#ff3333", class: "text-danger" },
                "MODERATE": { text: "適中", color: "#ffcc00", class: "text-warning" },
                "STABLE": { text: "穩定", color: "#00ffcc", class: "text-success" }
            };

            const config = statusMap[data.load] || statusMap["STABLE"];
            loadEl.textContent = config.text;
            loadEl.className = config.class;
            dotEl.style.color = config.color;
        });
    }

    // 🚩 2. 模擬即時跳動 (在線率)
    function flickerUptime() {
        const uptimeEl = document.getElementById('uptime-val');
        if (uptimeEl) {
            // 在 99.970 與 99.999 之間隨機產生一個數值
            const base = 99.97;
            const jitter = (Math.random() * 0.029).toFixed(3);
            uptimeEl.textContent = (base + parseFloat(jitter)).toFixed(2);
        }
    }

    // 🚩 3. 系統時鐘
    setInterval(() => {
        const now = new Date();
        document.getElementById('sync-clock').textContent = now.toTimeString().split(' ')[0];
    }, 1000);

    // 設定循環
    setInterval(fetchTraffic, 30000); // 30秒更新一次真實人流
    setInterval(flickerUptime, 3000); // 3秒跳動一次百分比

    fetchTraffic();
    flickerUptime();
}

// 頁面載入後啟動
$(document).ready(initServerMonitor);