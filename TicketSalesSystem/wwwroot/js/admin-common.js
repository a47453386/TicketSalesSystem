// wwwroot/js/admin-common.js

/**
 * 全域刪除函數
 * @param {string} id - 資料主鍵
 * @param {string} displayName - 顯示名稱
 * @param {string} controller - 控制器名稱
 * @param {string} action - Action名稱
 */
function globalDelete(id, displayName, controller, action = "Delete") {
    const targetUrl = `/${controller}/${action}`;

    Swal.fire({
        title: '確定要刪除嗎？',
        text: `項目：${displayName}\n此動作將無法復原！`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: '確定刪除',
        cancelButtonText: '取消'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: targetUrl,
                type: 'POST',
                data: {
                    id: id,
                    // 抓取全域防偽標籤
                    "__RequestVerificationToken": $('#antiforgery-token-holder input[name="__RequestVerificationToken"]').val()
                },
                success: function (res) {
                    if (res.success) {
                        Swal.fire('已刪除', `${displayName} 已從系統移除。`, 'success');

                        // 自動移除該列
                        $(`button[data-delete-id="${id}"]`).closest('tr').fadeOut(400, function () {
                            $(this).remove();
                        });
                    } else {
                        Swal.fire('刪除失敗', res.message || '發生未知錯誤', 'error');
                    }
                },
                error: function (xhr) {
                    console.error("Delete Error:", xhr.responseText);
                    Swal.fire('錯誤', '伺服器連線失敗 (400/500)', 'error');
                }
            });
        }
    });
}