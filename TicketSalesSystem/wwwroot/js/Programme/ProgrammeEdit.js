

// 1. 全域變數初始化
// 注意：window.getVenuesUrl 必須先在 Razor View 中定義
const getVenuesUrl = window.getVenuesUrl;
let cachedVenues = [];

// --- 2. 圖片預覽功能 ---
function previewImage(input, previewId) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#' + previewId).attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

// --- 3. 物理容量驗證與總量計算 ---
function validateRow($row) {
    const $select = $row.find(".venue-select");
    const $selectedOpt = $select.find("option:selected");

    // 抓取該區域的物理極限 (來自 data-屬性)
    const maxR = parseInt($selectedOpt.data("max-row")) || 0;
    const maxS = parseInt($selectedOpt.data("max-seat")) || 0;

    // 抓取目前輸入的值
    const currR = parseInt($row.find(".row-count-input").val()) || 0;
    const currS = parseInt($row.find(".seat-count-input").val()) || 0;

    // 計算總量 (Capacity)
    const total = currR * currS;
    $row.find(".capacity-display").val(total);

    // 如果是新票區 (尚未有 ID)，剩餘票數通常等於總量
    const hasId = $row.find(".area-id-input").val();
    if (!hasId) {
        $row.find(".remaining-input").val(total);
    }

    let errorMsg = "";
    if (maxR > 0 && currR > maxR) {
        errorMsg = `排數超限 (區域上限: ${maxR})`;
    } else if (maxS > 0 && currS > maxS) {
        errorMsg = `座數超限 (區域上限: ${maxS})`;
    }

    // 視覺回饋：若錯誤則變紅並顯示提示
    if (errorMsg !== "") {
        $row.css("background", "rgba(220, 53, 69, 0.15)");
        if (!$row.find(".error-hint").length) {
            $row.find(".input-group").after(`<div class="error-hint text-danger fw-bold animate__animated animate__shakeX" style="font-size: 0.65rem; margin-top:4px;">${errorMsg}</div>`);
        } else {
            $row.find(".error-hint").text(errorMsg);
        }
    } else {
        $row.css("background", "transparent");
        $row.find(".error-hint").remove();
    }
}

// --- 4. 更新下拉選單 (含 GUID 正規化與顏色綁定) ---
function updateVenueSelects(isInitial) {
    $(".venue-select").each(function () {
        const $select = $(this);
        // 正規化原始值 (處理大小寫與空白)
        let originalValue = ($select.attr("data-init-value") || "").toLowerCase().trim();

        let opt = '<option value="">-- 選取區域 --</option>';
        if (cachedVenues && cachedVenues.length > 0) {
            cachedVenues.forEach(v => {
                const vID = String(v.venueID || v.VenueID || "").toLowerCase().trim();
                const vName = v.venueName || v.VenueName;
                const mR = v.rowCount || v.RowCount;
                const mS = v.seatCount || v.SeatCount;
                const vColor = v.areaColor || v.AreaColor || "#00f2ff";

                opt += `<option value="${vID}" 
                                data-max-row="${mR}" 
                                data-max-seat="${mS}" 
                                data-color="${vColor}">
                            ${vName}
                        </option>`;
            });
        }

        $select.html(opt);

        // 初始化時還原選取狀態
        if (isInitial && originalValue) {
            $select.val(originalValue);

            // 補強：若 val() 因 DOM 渲染延遲未選中，手動選取
            if (!$select.val()) {
                $select.find("option").each(function () {
                    if ($(this).val().toLowerCase().trim() === originalValue) {
                        $(this).prop("selected", true);
                    }
                });
            }

            // 選中後立刻更新顏色色塊與執行驗證
            const $selectedOpt = $select.find("option:selected");
            const venueColor = $selectedOpt.data("color");
            $select.closest("tr").find(".area-color-preview").css("background-color", venueColor || "#00f2ff");
            validateRow($select.closest("tr"));
        }
    });
}

// --- 5. 索引重整：這是防止 POST 數據遺失的核心 ---
function reIndexAll() {
    $(".session-item").each(function (sIdx) {
        const $session = $(this);
        $session.attr("data-index", sIdx);
        $session.find(".session-num").text(sIdx + 1);

        // 遍歷該場次內所有表單元素
        $session.find("input, select, textarea").each(function () {
            let name = $(this).attr("name");
            if (name) {
                // 更新場次索引 [i]
                let newName = name.replace(/Session\[\d+\]/g, `Session[${sIdx}]`);

                // 更新票區索引 [j]
                if (newName.includes("TicketsArea")) {
                    const $tr = $(this).closest("tr");
                    if ($tr.length > 0) {
                        const tIdx = $tr.index();
                        newName = newName.replace(/TicketsArea\[\d+\]/g, `TicketsArea[${tIdx}]`);
                    }
                }
                $(this).attr("name", newName);
            }
        });
    });
}

// --- 6. 事件監聽主程式 ---
$(document).ready(function () {

    // A. 頁面載入初始化
    const initialPlaceId = $("#PlaceID").val();
    if (initialPlaceId) {
        $.get(getVenuesUrl, { placeId: initialPlaceId }, function (data) {
            cachedVenues = data;
            updateVenueSelects(true);
        });
    }

    // B. 地區切換監聽 (連動場館列表)
    $("#PlaceID").on("change", function () {
        const newPlaceId = $(this).val();
        if (confirm("切換地區將導致現有票區設定失效，是否確認執行重構？")) {
            $.get(getVenuesUrl, { placeId: newPlaceId }, function (data) {
                cachedVenues = data;
                updateVenueSelects(false); // 重設選單，不保留舊選取值
            });
        }
    });

    // C. 場館下拉選單切換：自動連動顏色與驗證
    $(document).on("change", ".venue-select", function () {
        const $row = $(this).closest("tr");
        const venueColor = $(this).find("option:selected").data("color");

        // 更新預覽色塊
        $row.find(".area-color-preview").css("background-color", venueColor || "#00f2ff");

        validateRow($row);
    });

    // D. 新增場次 (Dynamic HTML)
    $("#add-session").on("click", function (e) {
        e.preventDefault();
        const sIdx = $(".session-item").length;
        const html = `
        <div class="session-item border-bottom border-info border-opacity-10 p-4 animate__animated animate__fadeIn" data-index="${sIdx}">
            <input type="hidden" name="Session[${sIdx}].SessionID" value="" />
            <div class="d-flex justify-content-between align-items-center mb-3">
                <span class="badge bg-info text-dark font-monospace">NEW_SESSION_NODE #<span class="session-num">${sIdx + 1}</span></span>
                <button type="button" class="btn btn-outline-danger btn-sm remove-session">終止場次</button>
            </div>
            <div class="row g-3 mb-4 font-monospace">
                <div class="col-md-4"><label class="small text-info">開演時間</label><input type="datetime-local" name="Session[${sIdx}].StartTime" class="form-control" required /></div>
                <div class="col-md-4"><label class="small text-info">開賣時間</label><input type="datetime-local" name="Session[${sIdx}].SaleStartTime" class="form-control" required /></div>
                <div class="col-md-4"><label class="small text-info">停售時間</label><input type="datetime-local" name="Session[${sIdx}].SaleEndTime" class="form-control" required /></div>
            </div>
            <table class="cyber-table-edit text-center">
                <thead>
                    <tr>
                        <th style="width:20%">場館區域</th>
                        <th>票區名稱</th>
                        <th style="width:80px">顏色</th>
                        <th style="width:120px">價格</th>
                        <th style="width:140px">規格(RxS)</th>
                        <th style="width:100px">總量</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody class="area-tbody"></tbody>
            </table>
            <button type="button" class="btn btn-outline-info btn-sm mt-3 add-ticket-area">+ 新增票區</button>
        </div>`;
        $("#session-container").append(html);
        reIndexAll();
    });

    // E. 新增票區 (Dynamic Table Row)
    $(document).on("click", ".add-ticket-area", function (e) {
        e.preventDefault();
        const $session = $(this).closest(".session-item");
        const sIdx = $session.attr("data-index");
        const $tbody = $session.find(".area-tbody");
        const tIdx = $tbody.find("tr").length;

        let vOpts = '<option value="">-- 選取區域 --</option>';
        cachedVenues.forEach(v => {
            const vID = String(v.venueID || v.VenueID || "").toLowerCase().trim();
            vOpts += `<option value="${vID}" data-max-row="${v.rowCount}" data-max-seat="${v.seatCount}" data-color="${v.areaColor}">${v.venueName}</option>`;
        });

        const row = `<tr>
            <input type="hidden" name="Session[${sIdx}].TicketsArea[${tIdx}].TicketsAreaID" value="" class="area-id-input" />
            <td><select name="Session[${sIdx}].TicketsArea[${tIdx}].VenueID" class="form-select form-select-sm venue-select" required>${vOpts}</select></td>
            <td><input name="Session[${sIdx}].TicketsArea[${tIdx}].TicketsAreaName" class="form-control form-control-sm" required /></td>
            <td><div class="area-color-preview mx-auto shadow-sm" style="background-color: #00f2ff; width: 25px; height: 25px; border-radius: 4px; border: 1px solid rgba(255,255,255,0.2);"></div></td>
            <td><input name="Session[${sIdx}].TicketsArea[${tIdx}].Price" type="number" class="form-control form-control-sm text-end text-info fw-bold" value="0" /></td>
            <td>
                <div class="input-group input-group-sm">
                    <input name="Session[${sIdx}].TicketsArea[${tIdx}].RowCount" type="number" class="form-control text-center row-count-input" value="0" />
                    <input name="Session[${sIdx}].TicketsArea[${tIdx}].SeatCount" type="number" class="form-control text-center seat-count-input" value="0" />
                </div>
            </td>
            <td><input class="form-control form-control-sm text-center bg-transparent border-0 capacity-display text-white" value="0" readonly /></td>
            <td><button type="button" class="btn btn-link text-danger remove-area p-0">×</button></td>
        </tr>`;
        $tbody.append(row);
        reIndexAll();
    });

    // F. 移除邏輯 (場次與票區)
    $(document).on("click", ".remove-area, .remove-session", function () {
        const $target = $(this).closest($(this).hasClass("remove-area") ? "tr" : ".session-item");
        $target.addClass("animate__animated animate__fadeOutRight");
        setTimeout(() => {
            $target.remove();
            reIndexAll();
        }, 400);
    });

    // G. 規格輸入即時監聽
    $(document).on("input change", ".row-count-input, .seat-count-input", function () {
        validateRow($(this).closest("tr"));
    });

    // H. 提交前執行最後重整，確保索引連續性
    $('#editForm').on('submit', function () {
        reIndexAll();
    });
});