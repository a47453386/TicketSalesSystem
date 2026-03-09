window.onload = function () {
    const qrContainers = document.querySelectorAll('.qrcode-container');
    qrContainers.forEach(container => {
        const code = container.getAttribute('data-code');
        if (code) {
            container.innerHTML = ""; // 確保乾淨
            new QRCode(container, {
                text: code,
                width: 140, // 依需求調整
                height: 140,
                colorDark: "#000000",
                colorLight: "#ffffff",
                correctLevel: QRCode.CorrectLevel.H
            });
        }
    });
};