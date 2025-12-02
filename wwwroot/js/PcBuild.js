// =====================
// STATE QUẢN LÝ
// =====================
let currentBuild = {};
// mySlots và pcCategories đã được nạp từ View

const modalState = {
    products: [], currentPage: 1, productsPerPage: 10, totalProducts: 0,
    category: '', currentFilters: {}
};

const dom = {
    // UI Chính
    slotButtons: document.querySelectorAll('.slot-btn'),
    slotIdInput: document.getElementById('current-slot-id'),
    buildList: document.getElementById('build-pc-categories'),
    totalPrice: document.getElementById('build-total-price'),
    addBtn: document.getElementById('add-build-to-cart-btn'),

    // UI Modal
    modal: document.getElementById('product-modal'),
    title: document.getElementById('modal-title'),
    list: document.getElementById('modal-product-list'),
    filters: document.getElementById('modal-filter-options'),
    priceFilters: document.getElementById('modal-price-filter-options'),
    search: document.getElementById('modal-search-input'),
    pagination: document.getElementById('modal-pagination')
};

let bsModal = null;
const formatCurrency = num => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(num);

// =====================
// 1. LOGIC KHỞI ĐỘNG
// =====================
document.addEventListener('DOMContentLoaded', () => {
    document.body.appendChild(dom.modal);
    bsModal = new bootstrap.Modal(dom.modal);

    if (typeof mySlots !== 'undefined' && mySlots.length > 0) {
        selectSlot(mySlots[0].pcBuildID);
    } else {
        renderBuildPC();
    }

    setupEventListeners();
});

// =====================
// [MỚI] HÀM RELOAD DỮ LIỆU TỪ SERVER
// =====================
async function reloadGlobalData() {
    try {
        // 1. Gọi API lấy dữ liệu mới nhất
        const res = await fetch('/api/pcbuild/my-slots');
        if (!res.ok) return;

        // 2. Cập nhật biến toàn cục mySlots
        mySlots = await res.json();

        // 3. Cập nhật giao diện Nút bấm (Giá tiền mới)
        mySlots.forEach(slot => {
            const btn = document.querySelector(`.slot-btn[data-id="${slot.pcBuildID}"]`);
            if (btn) {
                const smallTag = btn.querySelector('small');
                if (smallTag) smallTag.textContent = `(${formatCurrency(slot.totalPrice)})`;
            }
        });

        // 4. Render lại cấu hình đang chọn
        const currentId = parseInt(dom.slotIdInput.value);
        if (currentId) {
            selectSlot(currentId);
        }

    } catch (e) {
        console.error("Lỗi reload data:", e);
    }
}

// =====================
// 2. LOGIC CHỌN SLOT & RENDER
// =====================
function selectSlot(slotId) {
    if (dom.slotIdInput) dom.slotIdInput.value = slotId;

    // Tìm dữ liệu mới nhất trong mySlots (vừa được reload)
    const slot = mySlots.find(s => s.pcBuildID === slotId);
    if (!slot) return;

    // Highlight nút
    dom.slotButtons.forEach(btn => {
        if (parseInt(btn.dataset.id) === slotId) {
            btn.classList.remove('btn-outline-secondary');
            btn.classList.add('btn-primary');
        } else {
            btn.classList.remove('btn-primary');
            btn.classList.add('btn-outline-secondary');
        }
    });

    // Map dữ liệu
    currentBuild = {};
    if (slot.components) {
        slot.components.forEach(c => {
            currentBuild[c.componentType] = {
                id: c.productID,
                name: c.productName,
                price: c.unitPrice,
                img: c.imageURL,
                quantity: c.quantity
            };
        });
    }

    renderBuildPC();
}

function renderBuildPC() {
    let total = 0;
    if (!pcCategories) return;

    dom.buildList.innerHTML = pcCategories.map(cat => {
        const comp = currentBuild[cat.id];
        if (comp) total += comp.price * (comp.quantity || 1);

        return `
            <li class="list-group-item d-flex justify-content-between align-items-center py-3">
                <div class="d-flex align-items-center">
                    <span class="badge bg-secondary rounded-pill me-3" style="min-width:60px">${cat.id}</span>
                    <span class="fw-semibold">${cat.name}</span>
                </div>
                <div class="d-flex align-items-center justify-content-end" style="flex:1">
                    ${comp ? `
                        <div class="d-flex align-items-center me-3">
                            <img src="${comp.img}" class="rounded border me-2" style="width:40px;height:40px;object-fit:cover;">
                            <div class="text-end">
                                <div class="fw-bold text-primary text-truncate" style="max-width:200px">${comp.name}</div>
                                <div class="text-danger small fw-bold">${formatCurrency(comp.price)}</div>
                            </div>
                        </div>
                        <button class="btn btn-sm btn-outline-warning open-modal-btn me-2" data-category="${cat.id}"><i class="bi bi-arrow-repeat"></i></button>
                        <button class="btn btn-sm btn-outline-danger remove-component-btn" data-category="${cat.id}"><i class="bi bi-x-lg"></i></button>
                    ` : `
                        <button class="btn btn-sm btn-outline-success fw-bold open-modal-btn" data-category="${cat.id}"><i class="bi bi-plus-lg"></i> Chọn</button>
                    `}
                </div>
            </li>
        `;
    }).join('');

    dom.totalPrice.textContent = formatCurrency(total);
    dom.addBtn.disabled = total === 0;
}

// =====================
// 3. AUTO-SAVE & UPDATE (ĐÃ THÊM RELOAD)
// =====================

async function selectComponentAutoSave(btn) {
    const slotId = dom.slotIdInput.value;
    const type = btn.dataset.category;
    const prodId = btn.dataset.id;

    if (!slotId || slotId === '0') { alert("Vui lòng chọn cấu hình trước!"); return; }

    const originalText = btn.innerHTML;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
    btn.disabled = true;

    try {
        const formData = new FormData();
        formData.append('slotId', slotId);
        formData.append('type', type);
        formData.append('productId', prodId);

        const res = await fetch('/api/pcbuild/update-item', { method: 'POST', body: formData });
        const data = await res.json();

        if (res.ok && data.success) {
            bsModal.hide();

            // [THAY ĐỔI] Thay vì update tay, gọi reload để lấy dữ liệu chuẩn từ Server
            await reloadGlobalData();

        } else {
            alert('Lỗi: ' + (data.message || 'Không thể lưu'));
        }
    } catch (e) {
        console.error(e);
    } finally {
        btn.innerHTML = originalText;
        btn.disabled = false;
    }
}

async function removeComponentServer(type) {
    if (!confirm('Xóa linh kiện này?')) return;
    const slotId = dom.slotIdInput.value;

    const formData = new FormData();
    formData.append('slotId', slotId);
    formData.append('type', type);

    const res = await fetch('/api/pcbuild/remove-item', { method: 'POST', body: formData });

    if (res.ok) {
        // [THAY ĐỔI] Reload lại dữ liệu từ Server
        await reloadGlobalData();
    }
}

// =====================
// 4. LOGIC MODAL & FETCH PRODUCT (Giữ nguyên)
// =====================
async function loadData() {
    const filters = getFilters();
    let dynamicFilterQuery = Object.entries(filters.dynamic)
        .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`)
        .join('&');

    const url = new URL('/api/pcbuild/search', window.location.origin);
    url.searchParams.append('category', modalState.category);
    url.searchParams.append('page', modalState.currentPage);
    url.searchParams.append('pageSize', modalState.productsPerPage);
    url.searchParams.append('search', filters.search);
    url.searchParams.append('minPrice', filters.price.min);
    if (isFinite(filters.price.max)) url.searchParams.append('maxPrice', filters.price.max);

    let fullUrl = url.toString();
    if (dynamicFilterQuery) fullUrl += `&${dynamicFilterQuery}`;

    try {
        dom.list.innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary"></div></div>';
        const res = await fetch(fullUrl);
        if (!res.ok) throw new Error('Err');
        const data = await res.json();

        modalState.products = data.products.map(p => ({
            id: p.productID, name: p.productName, brand: p.brand, price: p.price, img: p.imageURL
        }));
        modalState.totalProducts = data.totalCount;

        if (modalState.currentPage === 1) {
            renderPriceFilters(data.priceRanges);
            renderDynamicFilters(data.dynamicFilters);
        }
        renderProductPage();
        renderPagination();
    } catch (err) { console.error(err); }
}

// ... Helper Functions (getFilters, renderPriceFilters... Giữ nguyên không đổi) ...
function getFilters() {
    const dynamicFilters = {};
    document.querySelectorAll('.modal-filter-checkbox:checked').forEach(cb => {
        dynamicFilters[cb.dataset.group] = cb.value;
    });
    const selectedPriceRangeId = document.querySelector('.modal-price-filter-radio:checked')?.value || 'all';
    const priceRange = priceRanges.find(r => r.id === selectedPriceRangeId) || { id: 'all', min: 0, max: Infinity };
    return { search: dom.search.value.toLowerCase(), price: priceRange, dynamic: dynamicFilters };
}
function handleFilterChange() { modalState.currentPage = 1; loadData(); }
function renderPriceFilters(ranges) {
    priceRanges = (ranges || []).map(r => ({ ...r, max: r.max === null ? Infinity : r.max }));
    dom.priceFilters.innerHTML = priceRanges.map(r => `<div class="form-check"><input type="radio" name="pr" class="form-check-input modal-price-filter-radio" value="${r.id}" ${r.id === 'all' ? 'checked' : ''}><label class="form-check-label">${r.name}</label></div>`).join('');
}
function renderDynamicFilters(filters) {
    dom.filters.innerHTML = (filters || []).map(g => `<h6 class="fw-bold mt-2">${g.name}</h6>` + g.values.map(v => `<div class="form-check"><input type="checkbox" class="form-check-input modal-filter-checkbox" value="${v}" data-group="${g.name}"><label class="form-check-label">${v}</label></div>`).join('')).join('');
}
function renderProductPage() {
    const items = modalState.products;
    dom.list.innerHTML = items.length === 0 ? '<p class="text-center p-3">Không có sản phẩm</p>' : items.map(p => `
        <div class="card p-2 mb-2 shadow-sm">
            <div class="d-flex justify-content-between align-items-center">
                <div class="d-flex align-items-center">
                    <img src="${p.img || '/images/no-img.png'}" class="me-3 rounded" style="width:50px;height:50px;object-fit:cover;">
                    <div><h6 class="mb-0 small fw-bold">${p.name}</h6><span class="text-danger fw-bold small">${formatCurrency(p.price)}</span></div>
                </div>
                <button class="btn btn-primary btn-sm select-component-btn" data-id="${p.id}" data-category="${modalState.category}" data-name="${p.name}" data-price="${p.price}" data-img="${p.img || '/images/no-img.png'}">Chọn</button>
            </div>
        </div>
    `).join('');
}
function renderPagination() {
    const total = Math.ceil(modalState.totalProducts / modalState.productsPerPage);
    if (total <= 1) return dom.pagination.innerHTML = '';
    let html = `<nav><ul class="pagination justify-content-center pagination-sm">`;
    for (let i = 1; i <= total; i++) html += `<li class="page-item ${i === modalState.currentPage ? 'active' : ''}"><button class="page-link modal-page-btn" data-page="${i}">${i}</button></li>`;
    html += `</ul></nav>`;
    dom.pagination.innerHTML = html;
}

// =====================
// 5. EVENT HANDLERS
// =====================
function setupEventListeners() {
    dom.buildList.addEventListener('click', e => {
        const openBtn = e.target.closest('.open-modal-btn');
        if (openBtn) {
            modalState.category = openBtn.dataset.category;
            modalState.currentPage = 1;
            dom.title.textContent = "Chọn " + (pcCategories.find(c => c.id === modalState.category)?.name || modalState.category);
            dom.filters.innerHTML = ""; dom.priceFilters.innerHTML = ""; dom.list.innerHTML = ""; dom.pagination.innerHTML = "";
            dom.search.value = "";
            bsModal.show();
            loadData();
        }
        const removeBtn = e.target.closest('.remove-component-btn');
        if (removeBtn) removeComponentServer(removeBtn.dataset.category);
    });

    dom.list.addEventListener('click', e => {
        const select = e.target.closest('.select-component-btn');
        if (select) selectComponentAutoSave(select);
    });

    dom.filters.addEventListener('change', handleFilterChange);
    dom.priceFilters.addEventListener('change', handleFilterChange);
    let searchTimeout;
    dom.search.addEventListener('input', () => { clearTimeout(searchTimeout); searchTimeout = setTimeout(handleFilterChange, 300); });
    dom.pagination.addEventListener('click', e => { const btn = e.target.closest('.modal-page-btn'); if (btn) { modalState.currentPage = parseInt(btn.dataset.page); loadData(); } });

    dom.addBtn.addEventListener('click', async () => {
        if (!confirm('Thêm vào giỏ hàng?')) return;
        const slotId = dom.slotIdInput.value;
        const formData = new FormData();
        formData.append('slotId', slotId);

        // Sau khi thêm vào giỏ thành công -> Reload data để thấy slot trống
        const res = await fetch('/api/pcbuild/add-to-cart', { method: 'POST', body: formData });
        if (res.ok) {
            window.location.href = '/Cart';
        }
    });
}