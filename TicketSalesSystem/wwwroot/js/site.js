$(document).ready(function () {
    let isEditMode = false;

    // 1. 圖片預覽
    $("#imageInput").change(function () {
        if (this.files && this.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) { $('#imagePreview').attr('src', e.target.result).removeClass('d-none'); }
            reader.readAsDataURL(this.files[0]);
        }
    });

    // 2. 動態增減列
    $("#btnAddVenue").click(function () {
        const index = $(".venue-row").length;
        const newRow = $(".venue-row").first().clone();
        newRow.find("input").not("[type='color']").val("");
        newRow.find("input[type='hidden']").val(""); // 新增列的 VenueID 必須為空
        newRow.find("select").val("");
        newRow.find(".remove-venue").prop("disabled", false);
        $("#venueBody").append(newRow);
        reOrderIndex();
    });

    $(document).on("click", ".remove-venue", function () {
        if ($(".venue-row").length > 1) { $(this).closest("tr").remove(); reOrderIndex(); }
    });

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

    // 3. 狀態 Modal 切換
    $(document).on('click', '.btn-status-modal', function () {
        const btn = $(this);
        const mode = btn.data('mode');
        const select = btn.closest('.input-group').find('select');

        if (mode === 'edit') {
            if (!select.val()) { alert("請先選擇狀態"); return; }
            isEditMode = true;
            $('#statusModalTitle').text('修改狀態名稱');
            $('#newStatusID').val(select.val()).prop('readonly', true);
            $('#newStatusName').val(select.find("option:selected").text().split(' - ')[1]);
        } else {
            isEditMode = false;
            $('#statusModalTitle').text('新增狀態');
            $('#newStatusID').val('').prop('readonly', false);
            $('#newStatusName').val('');
        }
        $('#statusModal').modal('show');
    });

    $('#btnConfirmStatus').click(function () {
        const data = { id: $('#newStatusID').val(), name: $('#newStatusName').val() };
        const url = isEditMode ? '/Places/UpdateVenueStatus' : '/Places/QuickCreateVenueStatus';

        $.post(url, data, function (res) {
            if (res.success) {
                const txt = `${res.id} - ${res.name}`;
                if (isEditMode) {
                    $(`.status-select option[value="${res.id}"]`).text(txt);
                } else {
                    $(".status-select").append(new Option(txt, res.id));
                }
                $('#statusModal').modal('hide');
            } else { alert(res.message); }
        });
    });
});