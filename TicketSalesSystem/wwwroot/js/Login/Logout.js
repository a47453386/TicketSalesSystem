(function () {
    const timeoutMs = (5 * 60 * 1000) - 10000;
    let logoutTimer;
    let lastPingTime = Date.now();

    function performLogout() {
        console.warn("[SYSTEM]: 連線逾時，執行安全登出協定...");
        const currentPath = encodeURIComponent(window.location.pathname + window.location.search);

        // 🚩 動態判斷跳轉路徑
        // 如果現在的網址包含 admin 或 employee，就跳轉到員工登入頁面
        const isBackend = window.location.pathname.toLowerCase().includes('/admin') ||
            window.location.pathname.toLowerCase().includes('/employees');

        if (isBackend) {
            window.location.href = '/Login/EmployeeLogin?returnUrl=' + currentPath;
        } else {
            window.location.href = '/Login/MemberLogin?returnUrl=' + currentPath;
        }
    }

    function resetTimer() {
        clearTimeout(logoutTimer);
        logoutTimer = setTimeout(performLogout, timeoutMs);

        const now = Date.now();
        if (now - lastPingTime > 60000) {
            fetch('/Home/Ping', { method: 'HEAD' });
            lastPingTime = now;
        }
    }

    // 初始化監聽
    logoutTimer = setTimeout(performLogout, timeoutMs);
    window.addEventListener('mousemove', resetTimer);
    window.addEventListener('keypress', resetTimer);
    window.addEventListener('mousedown', resetTimer);
})();