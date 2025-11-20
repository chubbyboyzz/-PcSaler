// =====================
// CONFIG - Sẽ được nạp từ Server
// =====================

// QUAN TRỌNG: Hai biến này bây giờ sẽ trống.
// Chúng sẽ được nạp từ file .cshtml (xem BƯỚC 4 ở cuối)

// =====================
// STATE
// =====================
let currentBuild = {};

// State của Modal đã thay đổi RẤT NHIỀU
const modalState = {
    // Không còn allProducts hay filteredProducts
    // Chỉ lưu trạng thái hiện tại
    products: [],      // Chỉ 10 sản phẩm của trang hiện tại
    currentPage: 1,
    productsPerPage: 10,
    totalProducts: 0,  // Tổng số sản phẩm (để tính phân trang)
    category: '',      // CategoryID/ComponentType (VD: 'CPU')
    currentFilters: {} // Lưu các bộ lọc đang chọn (VD: { Brand: 'Intel', Socket: 'LGA1700' })
};

// =====================
// DOM CACHE (Giữ nguyên)
// =====================
const dom = {
    buildList: document.getElementById('build-pc-categories'),
    totalPrice: document.getElementById('build-total-price'),
    addBtn: document.getElementById('add-build-to-cart-btn'),

    modal: document.getElementById('product-modal'),
    title: document.getElementById('modal-title'),
    list: document.getElementById('modal-product-list'),
    filters: document.getElementById('modal-filter-options'), // Sẽ chứa các bộ lọc động
    priceFilters: document.getElementById('modal-price-filter-options'),
    search: document.getElementById('modal-search-input'),
    pagination: document.getElementById('modal-pagination')
};

let bsModal = null;

// =====================
// UTILS (Giữ nguyên)
// =====================
const formatCurrency = num =>
    Number.isFinite(num)
        ? new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(num)
        : '';

// =====================
// FETCH PRODUCTS (Logic thay đổi hoàn toàn)
// =====================

// Hàm này giờ là trung tâm, nó sẽ gọi API mỗi khi lọc hoặc phân trang
async function loadData() {
    // 1. Thu thập tất cả các bộ lọc
    const filters = getFilters();

    // 2. Tạo chuỗi query (VD: &Brand=Intel&Socket=AM5)
    // Lưu ý: Code này hiện tại chỉ hỗ trợ 1 giá trị/nhóm (sẽ cần nâng cấp nếu muốn chọn nhiều brand)
    let dynamicFilterQuery = Object.entries(filters.dynamic)
        .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`)
        .join('&');

    // 3. Xây dựng URL API
    const url = new URL('/api/products/search', window.location.origin);
    url.searchParams.append('category', modalState.category); // 'category' này là ComponentType, VD: 'CPU'
    url.searchParams.append('page', modalState.currentPage);
    url.searchParams.append('pageSize', modalState.productsPerPage);
    url.searchParams.append('search', filters.search);
    url.searchParams.append('minPrice', filters.price.min);
    if (isFinite(filters.price.max)) { // Chỉ gửi maxPrice nếu nó không phải Infinity
        url.searchParams.append('maxPrice', filters.price.max);
    }

    // Nối chuỗi query động vào
    let fullUrl = url.toString();
    if (dynamicFilterQuery) {
        fullUrl += `&${dynamicFilterQuery}`;
    }

    // 4. Gọi API
    try {
        dom.list.innerHTML = '<p class="text-center text-secondary py-5">Đang tải...</p>'; // Thêm trạng thái loading
        const res = await fetch(fullUrl);
        if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);

        // API mới trả về object { products, totalCount, dynamicFilters, priceRanges }
        const data = await res.json();

        // 5. Cập nhật State
        modalState.products = data.products.map(p => ({ // Ánh xạ dữ liệu trả về
            id: p.productID,
            name: p.productName,
            brand: p.brand,
            price: p.price,
            img: p.imageURL
        }));
        modalState.totalProducts = data.totalCount;

        // 6. Render
        // Chỉ render bộ lọc động *lần đầu tiên* tải category (khi page=1)
        if (modalState.currentPage === 1) {
            // API trả về luôn cả priceRanges và dynamicFilters cho category này
            renderPriceFilters(data.priceRanges); // Render lọc giá động
            renderDynamicFilters(data.dynamicFilters); // Render lọc thuộc tính động
        }
        renderProductPage(); // Render 10 sản phẩm
        renderPagination();  // Render phân trang dựa trên totalCount

    } catch (err) {
        console.error("Fetch lỗi:", err);
        dom.list.innerHTML = '<p class="text-danger text-center py-5">Lỗi khi tải sản phẩm. Vui lòng thử lại.</p>';
    }
}

// =====================
// FILTERS (Logic thay đổi hoàn toàn)
// =====================
function getFilters() {
    // Lấy các bộ lọc động (Brand, Socket, Chipset...)
    const dynamicFilters = {};
    document.querySelectorAll('.modal-filter-checkbox:checked').forEach(cb => {
        const group = cb.dataset.group; // VD: 'Brand'
        const value = cb.value;         // VD: 'Intel'
        // TODO: Nâng cấp để hỗ trợ chọn nhiều giá trị (VD: Brand=Intel,AMD)
        dynamicFilters[group] = value;
    });

    // Lấy giá trị radio được chọn
    const selectedPriceRangeId = document.querySelector('.modal-price-filter-radio:checked')?.value || 'all';
    // Tìm object priceRange tương ứng (lưu ý: priceRanges giờ nằm trong data trả về từ API)
    const priceRange = priceRanges.find(r => r.id === selectedPriceRangeId) || { id: 'all', min: 0, max: Infinity };

    return {
        search: dom.search.value.toLowerCase(),
        price: priceRange,
        dynamic: dynamicFilters // Trả về object các filter động
    };
}

// Hàm updateProductList cũ đã bị xóa.
// Thay vào đó, ta gọi loadData() mỗi khi có thay đổi
function handleFilterChange() {
    modalState.currentPage = 1; // Luôn reset về trang 1 khi lọc
    loadData(); // Gọi API với bộ lọc mới
}


// =====================
// RENDER FILTERS (Logic thay đổi hoàn toàn)
// =====================

// Hàm này render bộ lọc TĨNH (Khoảng giá)
// Nó sẽ được gọi MỖI KHI tải modal, vì khoảng giá giờ đã ĐỘNG
function renderPriceFilters(ranges) {
    // Cập nhật biến global 'priceRanges' để hàm getFilters() có thể dùng
    priceRanges = (ranges || []).map(r => ({ ...r, max: r.max === null ? Infinity : r.max }));

    dom.priceFilters.innerHTML = priceRanges.map(r => `
        <div class="form-check">
            <input type="radio" name="price-range" class="form-check-input modal-price-filter-radio"
                   value="${r.id}" ${r.id === 'all' ? 'checked' : ''}>
            <label class="form-check-label">${r.name}</label>
        </div>
    `).join('');

    // Đảm bảo radio 'all' luôn được chọn nếu không có gì khác
    if (!document.querySelector('.modal-price-filter-radio:checked')) {
        const allRadio = document.querySelector('.modal-price-filter-radio[value="all"]');
        if (allRadio) allRadio.checked = true;
    }
}

// Hàm này render các bộ lọc ĐỘNG (từ API)
// VD: { name: "Socket", values: ["LGA1700", "AM5"] }
function renderDynamicFilters(filters) {
    dom.filters.innerHTML = (filters || []).map(group => `
        <h6 class="fw-semibold mt-3">${group.name}</h6>
        ${group.values.map(val => `
            <div class="form-check">
                <input type="checkbox" class="form-check-input modal-filter-checkbox" 
                       value="${val}" data-group="${group.name}">
                <label class="form-check-label">${val}</label>
            </div>
        `).join('')}
    `).join('');
}


// =====================
// RENDER PRODUCTS (Logic thay đổi nhỏ)
// =====================
function renderProductPage() {
    // Không cần 'slice' nữa, vì server đã trả về đúng số sản phẩm
    const items = modalState.products;

    dom.list.innerHTML = items.length === 0
        ? `<p class="text-secondary text-center py-5">Không tìm thấy sản phẩm.</p>`
        : items.map(p => `
            <div class="card p-2 mb-2 shadow-sm">
                <div class="d-flex justify-content-between">
                    <div class="d-flex">
                        <img src="${p.img || '/images/placeholder.png'}" class="me-3 rounded" style="width:60px;height:60px;object-fit:cover;">
                        <div>
                            <h6 class="fw-semibold mb-0">${p.name}</h6>
                            <span class="text-danger fw-bold">${formatCurrency(p.price)}</span>
                        </div>
                    </div>
                    <button class="btn btn-primary btn-sm select-component-btn"
                        data-id="${p.id}" data-category="${modalState.category}"
                        data-name="${p.name}" data-price="${p.price}" data-img="${p.img || '/images/placeholder.png'}">
                        Chọn
                    </button>
                </div>
            </div>
        `).join(''); // Cập nhật: Thêm data- attributes cho nút chọn
}

// =====================
// PAGINATION (Logic thay đổi)
// =====================
function renderPagination() {
    // Tính tổng số trang dựa trên 'totalProducts' từ server
    const total = Math.ceil(modalState.totalProducts / modalState.productsPerPage);
    if (total <= 1) return dom.pagination.innerHTML = '';

    // Code render HTML giữ nguyên, chỉ thay đổi logic tính 'total'
    let html = `
        <nav><ul class="pagination justify-content-center">
            <li class="page-item ${modalState.currentPage === 1 ? 'disabled' : ''}">
                <button class="page-link modal-page-btn" data-page="${modalState.currentPage - 1}">&laquo;</button>
            </li>
    `;
    // Logic render số trang (tối ưu hơn để không render quá nhiều số)
    // ... (Tạm thời giữ logic cũ, render tất cả)
    for (let i = 1; i <= total; i++) {
        html += `
            <li class="page-item ${i === modalState.currentPage ? 'active' : ''}">
                <button class="page-link modal-page-btn" data-page="${i}">${i}</button>
            </li>
        `;
    }
    html += `
            <li class="page-item ${modalState.currentPage === total ? 'disabled' : ''}">
                <button class="page-link modal-page-btn" data-page="${modalState.currentPage + 1}">&raquo;</button>
            </li>
        </ul></nav>
    `;
    dom.pagination.innerHTML = html;
}

// =====================
// BUILD PC VIEW (Thay đổi nhỏ)
// =====================
function renderBuildPC() {
    let total = 0;
    // Quan trọng: Phải đảm bảo 'pcCategories' đã được tải
    if (!pcCategories || pcCategories.length === 0) {
        console.warn("pcCategories chưa được tải!");
        return;
    }

    dom.buildList.innerHTML = pcCategories.map(cat => {
        const comp = currentBuild[cat.id]; // Dùng cat.id (VD: 'CPU')
        if (comp) total += comp.price;

        return `
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <div class="d-flex align-items-center">
                    <span class="badge bg-secondary rounded-pill me-3">${cat.id}</span>
                    <span class="fw-semibold">${cat.name}</span>
                </div>
                <div class="d-flex align-items-center">
                    ${comp ? `
                        <img src="${comp.img}" class="me-3 rounded" style="width:50px;height:50px;">
                        <div class="me-4">
                            <h6 class="text-primary fw-semibold mb-0">${comp.name}</h6>
                            <span class="text-danger fw-bold">${formatCurrency(comp.price)}</span>
                        </div>
                        <button class="btn btn-warning btn-sm open-modal-btn me-2" data-category="${cat.id}">Thay đổi</button>
                        <button class="btn btn-outline-danger btn-sm remove-component-btn" data-category="${cat.id}">
                            <i class="bi bi-x-lg"></i>
                        </button>
                    ` : `
                        <button class="btn btn-success open-modal-btn" data-category="${cat.id}">Chọn</button>
                    `}
                </div>
            </li>
        `;
    }).join('');
    dom.totalPrice.textContent = formatCurrency(total);
    dom.addBtn.disabled = total === 0;
}

// =====================
// EVENT HANDLERS (Thay đổi)
// =====================
function selectComponent(btn) {
    // Lấy data từ attributes của nút, không cần tìm trong mảng state
    const comp = {
        id: +btn.dataset.id,
        name: btn.dataset.name,
        price: +btn.dataset.price,
        img: btn.dataset.img
    };
    const category = btn.dataset.category;

    if (comp) currentBuild[category] = comp;
    bsModal.hide();
    renderBuildPC();
}

// =====================
// STARTUP
// =====================
document.addEventListener('DOMContentLoaded', () => {

    bsModal = new bootstrap.Modal(dom.modal);

    // BƯỚC 4: NẠP DATA TỪ SERVER (BẮT BUỘC)
    // File .cshtml của bạn phải có đoạn script này, NẰM TRƯỚC khi
    // bạn gọi file .js này, để 2 biến 'pcCategories' được gán giá trị
    /*
    <script>
        // Dữ liệu được "in" ra từ Controller (ViewBag/ViewModel)
        pcCategories = @Html.Raw(Json.Serialize(ViewBag.Categories));
        
        // Chúng ta không cần nạp priceRanges ở đây nữa, 
        // vì API 'loadData()' sẽ tự động nạp nó theo đúng category.
    </script>
    */

    // Khởi tạo giao diện lần đầu
    renderBuildPC(); // Render danh sách build pc (dùng pcCategories từ server)

    // --- EVENT DELEGATION ---
    document.addEventListener('click', e => {
        const btn = e.target.closest('.open-modal-btn');
        if (btn) {
            modalState.category = btn.dataset.category;
            modalState.currentPage = 1; // Luôn reset khi mở modal
            modalState.currentFilters = {}; // Reset bộ lọc

            dom.title.textContent = "Chọn " + (pcCategories.find(c => c.id === modalState.category)?.name || modalState.category);
            dom.search.value = "";

            // Xóa bộ lọc cũ trước khi gọi API
            dom.filters.innerHTML = "";
            dom.priceFilters.innerHTML = "";
            dom.list.innerHTML = "";
            dom.pagination.innerHTML = "";

            bsModal.show();
            loadData(); // Tải dữ liệu trang 1
        }

        const remove = e.target.closest('.remove-component-btn');
        if (remove) {
            delete currentBuild[remove.dataset.category];
            renderBuildPC();
        }

        const select = e.target.closest('.select-component-btn');
        if (select) {
            selectComponent(select); // Truyền cả cái nút vào
        }

        const page = e.target.closest('.modal-page-btn');
        if (page) {
            const newPage = Number(page.dataset.page);
            if (newPage > 0 && newPage !== modalState.currentPage) {
                // Không cần kiểm tra 'max' ở đây vì nút 'disabled' đã xử lý
                modalState.currentPage = newPage;
                loadData(); // Tải trang mới
            }
        }
    });

    // Các event listener cho lọc
    // Giờ tất cả đều gọi hàm handleFilterChange()
    dom.filters.addEventListener('change', handleFilterChange);
    dom.priceFilters.addEventListener('change', handleFilterChange);

    // Thêm 'debounce' để không gọi API liên tục khi gõ
    let searchTimeout;
    dom.search.addEventListener('input', () => {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            handleFilterChange();
        }, 300); // Chờ 300ms sau khi người dùng ngừng gõ
    });

    // Nút "Add to Cart" (Giữ nguyên)
    dom.addBtn.addEventListener('click', () => {
        let total = Object.values(currentBuild).reduce((s, c) => s + c.price, 0);
        alert(`Đã lưu cấu hình trị giá ${formatCurrency(total)}! (Demo)`);
        // Logic thật: Gửi 'currentBuild' về server
        // Ví dụ: post('/api/custompc/save', currentBuild)
    });
});