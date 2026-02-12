@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}

    <script>
        $(document).ready(function () {
            // 當按下 Modal 裡的「確認新增」按鈕
            $('#saveTypeBtn').click(function () {
                // 1. 取得使用者輸入的名稱
                var nameValue = $('#newTypeName').val();

                if (!nameValue) {
                    alert("請輸入名稱！");
                    return;
                }

                // 2. 發送 AJAX 到你的第二個 Post Action (CreateTypeQuickly)
                $.ajax({
                    url: '@Url.Action("CreateTypeQuickly", "FAQs")', // 確保 Controller 名稱正確
                    type: 'POST',
                    // 🚩 注意：Key 名稱 'typeName' 必須跟 Controller 參數名完全一致
                    data: { typeName: nameValue },
                    success: function (response) {
                        if (response.success) {
                            // A. 動態產生一個新的 Option
                            // response.id 會是 SQL Function 算出的 'F1'
                            // response.name 會是剛才輸入的「票務相關」
                            var newOpt = new Option(response.name, response.id);
                            
                            // B. 塞入下拉選單並直接選取它
                            $('#faqTypeSelect').append(newOpt);
                            $('#faqTypeSelect').val(response.id);
                            
                            // C. 清除輸入框並關閉彈窗
                            $('#newTypeName').val('');
                            $('#addTypeModal').modal('hide');
                            
                            alert("新分類「" + response.name + "」已新增並選取！");
                        } else {
                            alert("錯誤：" + response.message);
                        }
                    },
                    error: function () {
                        alert("與伺服器連線時發生錯誤，請檢查 Controller 路徑。");
                    }
                });
            });
        });
    </script>
}