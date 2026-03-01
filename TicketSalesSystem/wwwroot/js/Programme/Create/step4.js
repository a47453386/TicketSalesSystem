
    // 🚩 1. 多圖累加暫存池 (Description Images Only)
    let accumulatedDescriptionFiles = [];

    document.addEventListener("DOMContentLoaded", function() {
        // --- A. 單張圖片：封面圖監聽 ---
        const coverInput = document.getElementById('CoverImageFile');
    if (coverInput) {
        coverInput.addEventListener('change', function () {
            previewSingle(this, 'coverPreviewContainer');
        });
        }

    // --- B. 單張圖片：座位圖監聽 ---
    const seatInput = document.getElementById('SeatImageFile');
    if (seatInput) {
        seatInput.addEventListener('change', function () {
            previewSingle(this, 'seatPreviewContainer');
        });
        }

    // --- C. 多張累加：描述圖監聽 ---
    const descInput = document.getElementById('DescriptionImageFiles');
    if (descInput) {
        descInput.addEventListener('change', function (e) {
            const newFiles = Array.from(e.target.files);

            newFiles.forEach(file => {
                const isDuplicate = accumulatedDescriptionFiles.some(f => f.name === file.name && f.size === file.size);
                if (!isDuplicate) accumulatedDescriptionFiles.push(file);
            });

            syncFilesToInput(descInput); // 重要：同步回 input 讓後端收得到
            renderMultiplePreview('descPreviewContainer', descInput);
        });
        }
    });

    /**
     * 🚩 核心功能：處理單張圖片預覽 (封面、座位圖)
     */
    function previewSingle(input, containerId) {
        const container = document.getElementById(containerId);
    if (!container) return;

    if (input.files && input.files[0]) {
            const reader = new FileReader();
    reader.onload = function (e) {
        // 使用 img-preview-tech 樣式確保視覺統一
        container.innerHTML = `
                    <div class="img-preview-tech d-inline-block animate__animated animate__fadeIn">
                        <img src="${e.target.result}" style="max-height: 200px; max-width: 100%;" />
                        <div class="small text-info mt-1" style="font-size: 0.6rem;">[ SELECTED: ${input.files[0].name} ]</div>
                    </div>`;
            };
    reader.readAsDataURL(input.files[0]);
        }
    }

    /**
     * 🚩 核心功能：渲染多圖累加預覽
     */
    function renderMultiplePreview(containerId, inputElement) {
        const container = document.getElementById(containerId);
    if (!container) return;

    container.innerHTML = ""; // 先清空，重新依據暫存池繪製

        accumulatedDescriptionFiles.forEach((file, index) => {
            const reader = new FileReader();
    reader.onload = function (e) {
                const div = document.createElement("div");
    div.className = "col-md-3 mb-3 animate__animated animate__zoomIn";
    div.innerHTML = `
    <div class="img-preview-tech position-relative">
        <div class="btn-remove-img" onclick="removeFile(${index}, '${containerId}', '${inputElement.id}')">×</div>

        <img src="${e.target.result}" class="w-100" style="height: 120px; object-fit: cover;" />

        <div class="small text-truncate mt-1 text-info" style="font-size: 0.65rem;">
            [ DATA: ${file.name} ]
        </div>
    </div>`;
    container.appendChild(div);
            };
    reader.readAsDataURL(file);
        });
    }

    /**
     * 🚩 核心功能：從暫存池移除圖片
     */
    window.removeFile = function(index, containerId, inputId) {
        accumulatedDescriptionFiles.splice(index, 1);
    const inputElement = document.getElementById(inputId);
    syncFilesToInput(inputElement);
    renderMultiplePreview(containerId, inputElement);
    };

    /**
     * 🚩 核心技術：將 JS 陣列寫入 Input 物件
     */
    function syncFilesToInput(inputElement) {
        const dataTransfer = new DataTransfer();
        accumulatedDescriptionFiles.forEach(file => dataTransfer.items.add(file));
    inputElement.files = dataTransfer.files;
    }
