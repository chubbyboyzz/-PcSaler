// ---- Cấu hình ----
const priceRanges = [
    { id: 'all', name: 'Tất cả', min: 0, max: Infinity },
    { id: 'lt-5m', name: 'Dưới 5 triệu', min: 0, max: 4999999 },
    { id: '5m-10m', name: '5 - 10 triệu', min: 5000000, max: 10000000 },
    { id: '10m-20m', name: '10 - 20 triệu', min: 10000001, max: 20000000 },
    { id: 'gt-20m', name: 'Trên 20 triệu', min: 20000001, max: Infinity }
];

const pcCategories = [
    { id: 'CPU', name: 'CPU - Vi Xử Lý' },
    { id: 'Mainboard', name: 'Mainboard - Bo Mạch Chủ' },
    { id: 'RAM', name: 'RAM - Bộ Nhớ Trong' },
    { id: 'SSD', name: 'Ổ Cứng SSD' },
    { id: 'VGA', name: 'VGA - Card Đồ Họa' },
    { id: 'Nguồn', name: 'PSU - Nguồn Máy Tính' },
    { id: 'Case', name: 'Case - Vỏ Máy Tính' },
    { id: 'Tản Nhiệt', name: 'Tản Nhiệt CPU' },
];

// ---- State ----
let currentBuild = {};
let modalState = {
    allProducts: [],
    filteredProducts: [],
    currentPage: 1,
    productsPerPage: 10,
    category: '',
};

// ---- DOM ----
const buildPCCategoriesContainer = document.getElementById('build-pc-categories');
const buildTotalPrice = document.getElementById('build-total-price');
const addBuildToCartBtn = document.getElementById('add-build-to-cart-btn');

const productModal = document.getElementById('product-modal'); // Phần tử DOM của Modal
const modalTitle = document.getElementById('modal-title');
const modalProductList = document.getElementById('modal-product-list');
const modalFilterOptions = document.getElementById('modal-filter-options');
const modalPriceFilterOptions = document.getElementById('modal-price-filter-options');
const modalSearchInput = document.getElementById('modal-search-input');
const modalPagination = document.getElementById('modal-pagination');


// ---- Hàm hỗ trợ ----
const formatCurrency = (number) => {
    if (!Number.isFinite(number)) return '';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(number);
};

// **LƯU Ý:** showProductSelectionModal và closeModal đã được định nghĩa lại trong Index.cshtml
// để sử dụng API Bootstrap.Modal. Các hàm này chỉ cần xử lý logic data/render.

// ---- Render filter (Chuyển đổi sang cấu trúc form Bootstrap) ----
const renderModalFilters = () => {
    // Lọc hãng
    const brands = [...new Set(modalState.allProducts.map(p => p.brand))].filter(b => b);
    const brandHTML = brands.map(brand => `
        <div class="form-check">
            <input type="checkbox" class="form-check-input modal-filter-checkbox" id="brand-${brand}" value="${brand}">
            <label class="form-check-label" for="brand-${brand}">${brand}</label>
        </div>
    `).join('');
    modalFilterOptions.innerHTML = brandHTML;

    // Lọc giá
    const priceHTML = priceRanges.map(range => `
        <div class="form-check">
            <input type="radio" name="price-range" class="form-check-input modal-price-filter-radio" 
                   value="${range.id}" id="price-${range.id}" ${range.id === 'all' ? 'checked' : ''}>
            <label class="form-check-label" for="price-${range.id}">${range.name}</label>
        </div>
    `).join('');
    modalPriceFilterOptions.innerHTML = priceHTML;
};

// ---- Cập nhật danh sách sản phẩm ----
const updateModalProductList = () => {
    const searchTerm = modalSearchInput.value.toLowerCase();

    const selectedBrands = Array.from(document.querySelectorAll('.modal-filter-checkbox:checked')).map(b => b.value);

    const selectedPriceRangeId = document.querySelector('.modal-price-filter-radio:checked')?.value || 'all';
    const selectedRange = priceRanges.find(r => r.id === selectedPriceRangeId);

    modalState.filteredProducts = modalState.allProducts.filter(p => {
        const matchSearch = p.name.toLowerCase().includes(searchTerm);
        const matchBrand = selectedBrands.length === 0 || selectedBrands.includes(p.brand);
        const matchPrice = p.price >= selectedRange.min && (selectedRange.max === Infinity || p.price <= selectedRange.max);

        return matchSearch && matchBrand && matchPrice;
    });

    renderModalProductPage();
};

// ---- Render sản phẩm (Cấu trúc list item Bootstrap) ----
const renderModalProductPage = () => {
    const start = (modalState.currentPage - 1) * modalState.productsPerPage;
    const end = start + modalState.productsPerPage;
    const productsToShow = modalState.filteredProducts.slice(start, end);

    if (productsToShow.length === 0) {
        modalProductList.innerHTML = '<p class="text-secondary text-center">Không tìm thấy sản phẩm phù hợp.</p>';
    } else {
        const productsHTML = productsToShow.map(p => `
            <div class="card p-2 mb-2 shadow-sm">
                <div class="d-flex justify-content-between align-items-center">
                    <div class="d-flex align-items-center">
                        <img src="${p.img}" alt="${p.name}" class="me-3 rounded" style="width: 60px; height: 60px; object-fit: cover;">
                        <div>
                            <h6 class="card-title fw-semibold mb-0">${p.name}</h6>
                            <span class="text-danger fw-bold">${formatCurrency(p.price)}</span>
                        </div>
                    </div>
                    <button class="btn btn-primary btn-sm select-component-btn" 
                            data-id="${p.id}" data-category="${modalState.category}">
                        Chọn
                    </button>
                </div>
            </div>
        `).join('');
        modalProductList.innerHTML = productsHTML;
    }
    renderModalPagination();
};

// ---- Render phân trang (Cấu trúc Pagination Bootstrap) ----
const renderModalPagination = () => {
    modalPagination.innerHTML = '';
    const totalPages = Math.ceil(modalState.filteredProducts.length / modalState.productsPerPage);
    if (totalPages <= 1) return;

    const pageButtons = [];
    pageButtons.push(`
        <li class="page-item ${modalState.currentPage === 1 ? 'disabled' : ''}">
            <button class="page-link modal-page-btn" data-page="${modalState.currentPage - 1}">&laquo;</button>
        </li>
    `);

    for (let i = 1; i <= totalPages; i++) {
        pageButtons.push(`
            <li class="page-item ${i === modalState.currentPage ? 'active' : ''}">
                <button class="page-link modal-page-btn" data-page="${i}">${i}</button>
            </li>
        `);
    }

    pageButtons.push(`
        <li class="page-item ${modalState.currentPage === totalPages ? 'disabled' : ''}">
            <button class="page-link modal-page-btn" data-page="${modalState.currentPage + 1}">&raquo;</button>
        </li>
    `);

    modalPagination.innerHTML = `
        <nav>
            <ul class="pagination justify-content-center">
                ${pageButtons.join('')}
            </ul>
        </nav>
    `;
};

// ---- Build PC view (Cấu trúc List Group Bootstrap) ----
const renderBuildPCView = () => {
    buildPCCategoriesContainer.innerHTML = '';
    let total = 0;
    const categoryItemsHTML = pcCategories.map(cat => {
        const comp = currentBuild[cat.id];
        let compHTML = '';

        if (comp) {
            total += comp.price;
            compHTML = `
                <div class="d-flex align-items-center me-4">
                    <img src="${comp.img}" alt="${comp.name}" class="me-3 rounded" style="width: 50px; height: 50px; object-fit: cover;">
                    <div>
                        <h6 class="text-primary fw-semibold mb-0">${comp.name}</h6>
                        <span class="text-danger fw-bold">${formatCurrency(comp.price)}</span>
                    </div>
                </div>`;
        }

        const actionButtons = comp ? `
            <div class="d-flex align-items-center">
                <button class="btn btn-warning btn-sm open-modal-btn me-2" data-category="${cat.id}">Thay đổi</button>
                <button class="btn btn-outline-danger btn-sm remove-component-btn" data-category="${cat.id}">
                    <i class="bi bi-x-lg"></i>
                </button>
            </div>` : `
            <button class="btn btn-success open-modal-btn" data-category="${cat.id}">Chọn</button>`;

        return `
            <li class="list-group-item d-flex justify-content-between align-items-center">
            <th>
            <tr>
                <div class="d-flex align-items-center">
                    <span class="badge bg-secondary rounded-pill me-3">${cat.id}</span>
                    <span class="fw-semibold">${cat.name}</span>
                </div>
            </tr>
            <td>
             <div class="d-flex align-items-center">
                    ${comp ? compHTML : ''}
                    ${actionButtons}
                </div>
            </td>
            </th>
               
            </li>
        `;
    }).join('');

    buildPCCategoriesContainer.innerHTML = categoryItemsHTML;
    buildTotalPrice.textContent = formatCurrency(total);
    addBuildToCartBtn.disabled = total === 0;
};

// ---- Xử lý sự kiện & Ủy quyền sự kiện ----
const handleSelectComponent = (productId, category) => {
    const product = modalState.allProducts.find(p => p.id === productId);
    if (product) currentBuild[category] = product;
    window.closeModal(); // Dùng hàm đã được định nghĩa trong Index.cshtml
    renderBuildPCView();
};
const handleRemoveComponent = (category) => {
    delete currentBuild[category];
    renderBuildPCView();
};
const handleAddBuildToCart = () => {
    let total = 0; Object.values(currentBuild).forEach(c => total += c.price);
    if (total === 0) return;

    alert(`Đã lưu cấu hình trị giá ${formatCurrency(total)}! (Demo)`);
    currentBuild = {};
    renderBuildPCView();
};

// Ủy quyền sự kiện (Event Delegation) cho các bộ lọc
modalFilterOptions.addEventListener('change', e => {
    if (e.target.classList.contains('modal-filter-checkbox')) {
        modalState.currentPage = 1;
        updateModalProductList();
    }
});

modalPriceFilterOptions.addEventListener('change', e => {
    if (e.target.classList.contains('modal-price-filter-radio')) {
        modalState.currentPage = 1;
        updateModalProductList();
    }
});

modalPagination.addEventListener('click', e => {
    const btn = e.target.closest('.modal-page-btn');
    if (btn) {
        const page = parseInt(btn.dataset.page);
        const totalPages = Math.ceil(modalState.filteredProducts.length / modalState.productsPerPage);
        if (page && page > 0 && page <= totalPages) {
            modalState.currentPage = page;
            renderModalProductPage();
        }
    }
});

// Xử lý sự kiện click chung (chỉ cần chạy 1 lần)
document.addEventListener('click', e => {
    const target = e.target;
    // Mở Modal
    if (target.closest('.open-modal-btn')) {
        const cat = target.closest('.open-modal-btn').dataset.category;
        window.showProductSelectionModal(cat); // Dùng hàm đã ghi đè
    }
    // Xóa linh kiện
    if (target.closest('.remove-component-btn')) {
        const cat = target.closest('.remove-component-btn').dataset.category;
        handleRemoveComponent(cat);
    }
    // Chọn linh kiện
    if (target.closest('.select-component-btn')) {
        const id = parseInt(target.closest('.select-component-btn').dataset.id);
        const cat = target.closest('.select-component-btn').dataset.category;
        handleSelectComponent(id, cat);
    }
});

addBuildToCartBtn.addEventListener('click', handleAddBuildToCart);

modalSearchInput.addEventListener('input', () => {
    modalState.currentPage = 1;
    updateModalProductList();
});

// ---- Khởi tạo ----
document.addEventListener('DOMContentLoaded', renderBuildPCView);