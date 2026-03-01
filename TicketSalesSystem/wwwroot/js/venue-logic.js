$(document).ready(function () {
    let isEditMode = false;

    // 1. 圖片預覽功能
    $("#imageInput").on("change", function () {
        if (this.files && this.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#imagePreview').attr('src', e.target.result).removeClass('d-none');
            }
            reader.readAsDataURL(this.files[0]);
        }
    });

    // 2. 新增區域列 (樣板化：解決沒資料時按鈕失效的問題)
    $("#btnAddVenue").on("click", function () {
        const index = $(".venue-row").length;

        // 使用樣板字串，裡面包含變數 ${index} 和 window.venueConfig 的內容
        const newRow = `
        <tr class="venue-row">
            <input type="hidden" name="VenueItem[${index}].VenueID" value="" />
            <td><input name="VenueItem[${index}].VenueName" class="form-control form-control-sm tech-input" required /></td>
            <td><input name="VenueItem[${index}].FloorName" class="form-control form-control-sm tech-input" required /></td>
            <td><input name="VenueItem[${index}].AreaColor"type="color" class="form-control tech-input p-1" style="height: 45px;" value="#00f2ff" /></td>
            <td>
                <div class="input-group input-group-sm">
                    <input name="VenueItem[${index}].RowCount" type="number" class="form-control tech-input" min="1" placeholder="ROW" />
                    <span class="input-group-text bg-transparent text-info border-info border-opacity-25">x</span>
                    <input name="VenueItem[${index}].SeatCount" type="number" class="form-control tech-input" min="1"  placeholder="SEATS"/>
                </div>
            </td>
            <td>
                <div class="input-group input-group-sm">
                    <select name="VenueItem[${index}].VenueStatusID" class="form-select status-select tech-input">
                        <option value="">-- 請選擇 --</option>
                        ${window.venueConfig.statusOptionsHtml}
                    </select>
                   <button class="btn btn-outline-info btn-status-modal" type="button" data-mode="add"><i class="bi bi-plus"></i></button>
                   <button class="btn btn-outline-warning btn-status-modal" type="button" data-mode="edit"><i class="bi bi-pencil"></i></button>
                </div>
            </td>
            <td class="text-center">
               <button type="button" class="text-danger bg-transparent border-0 remove-venue" disabled><i class="bi bi-trash3 fs-4"></i></button>
            </td>
        </tr>`;

        $("#venueBody").append(newRow);
        reOrderIndex();
    });

    // 3. 刪除區域列
    $(document).on("click", ".remove-venue", function () {
        if ($(".venue-row").length > 1) {
            $(this).closest("tr").remove();
            reOrderIndex();
        } else {
            alert("至少需保留一個區域。");
        }
    });

    // 4. 重新編排索引 (確保 Model Binding 成功)
    function reOrderIndex() {
        $(".venue-row").each(function (i) {
            $(this).find("input, select").each(function () {
                const name = $(this).attr("name");
                if (name) {
                    const newName = name.replace(/VenueItem\[\d+\]/, "VenueItem[" + i + "]");
                    $(this).attr("name", newName);
                }
            });
        });
    }

    // 5. 狀態彈窗切換 (新增/修改)
    $(document).on('click', '.btn-status-modal', function () {
        const btn = $(this);
        const mode = btn.data('mode');
        const select = btn.closest('.input-group').find('select');

        if (mode === 'edit') {
            const val = select.val();
            if (!val) { alert("請先選擇狀態"); return; }
            isEditMode = true;
            $('#statusModalTitle').text('修改狀態名稱');
            $('#newStatusID').val(val).prop('readonly', true);
            // 假設選項格式為 "A - 正常"
            const currentName = select.find("option:selected").text().split(' - ')[1] || "";
            $('#newStatusName').val(currentName);
        } else {
            isEditMode = false;
            $('#statusModalTitle').text('快速新增狀態');
            $('#newStatusID').val('').prop('readonly', false);
            $('#newStatusName').val('');
        }
        $('#statusModal').modal('show');
    });

    // 6. 彈窗確認儲存 (AJAX)
    $('#btnConfirmStatus').on("click", function () {
        const data = {
            id: $('#newStatusID').val().toUpperCase(),
            name: $('#newStatusName').val()
        };
        const url = isEditMode ? window.venueConfig.updateStatusUrl : window.venueConfig.createStatusUrl;

        $.post(url, data, function (res) {
            if (res.success) {
                const txt = `${res.id} - ${res.name}`;
                if (isEditMode) {
                    $(`.status-select option[value="${res.id}"]`).text(txt);
                } else {
                    $(".status-select").append(new Option(txt, res.id));
                    // (可選) 自動選取剛新增的這項
                }
                $('#statusModal').modal('hide');
            } else {
                alert(res.message);
            }
        });
    });
});