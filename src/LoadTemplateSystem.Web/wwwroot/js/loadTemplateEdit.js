/**
 * loadTemplateEdit.js
 * Handles Add Product Popup, Deduplication (ignores already added SKUs),
 * In-table keypress filtering, and Batch line deletion.
 */
document.addEventListener('DOMContentLoaded', function () {
    const linesTableBody = document.getElementById('linesTableBody');
    const productModalEl = document.getElementById('productPopupModal');
    const productSearchInput = document.getElementById('productSearchInput');
    const productResultsTbody = document.getElementById('productResultsTbody');
    const productSearchLoading = document.getElementById('productSearchLoading');
    const noProductsFound = document.getElementById('noProductsFound');
    const selectAllProductsCheckbox = document.getElementById('selectAllProductsCheckbox');
    const addSelectedProductsBtn = document.getElementById('addSelectedProductsBtn');
    const selectAllLinesCheckbox = document.getElementById('selectAllLinesCheckbox');
    const btnDeleteSelectedLines = document.getElementById('btnDeleteSelectedLines');
    const emptyLinesRow = document.getElementById('emptyLinesRow');
    const templateUidInput = document.querySelector('input[name="UID"]');
    const lineFilterSearchInput = document.getElementById('lineFilterSearchInput');

    let debounceTimer = null;
    let productModal = null;
    if (productModalEl) {
        productModal = new bootstrap.Modal(productModalEl);
    }

    // ==========================================
    // 1. In-Table Line Filtering on Key Press
    // ==========================================
    if (lineFilterSearchInput) {
        lineFilterSearchInput.addEventListener('input', function () {
            const filter = this.value.trim().toLowerCase();
            document.querySelectorAll('.template-line-row').forEach(row => {
                const isDeleted = row.querySelector('.line-is-deleted')?.value === 'true';
                if (isDeleted) return;

                const skuCode = (row.getAttribute('data-skucode') || '').toLowerCase();
                const skuName = (row.getAttribute('data-skuname') || '').toLowerCase();

                if (!filter || skuCode.includes(filter) || skuName.includes(filter)) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        });
    }

    // ==========================================
    // 2. Existing SKU Deduplication Helper
    // ==========================================
    function getExistingSkuCodes() {
        const skuCodes = new Set();
        document.querySelectorAll('.template-line-row').forEach(row => {
            const isDeleted = row.querySelector('.line-is-deleted')?.value === 'true';
            if (!isDeleted) {
                const skuCode = row.getAttribute('data-skucode') || row.querySelector('.line-sku-code')?.value;
                if (skuCode) skuCodes.add(skuCode.trim().toUpperCase());
            }
        });
        return skuCodes;
    }

    // ==========================================
    // 3. Product Popup Modal & Key-Press AJAX Search
    // ==========================================
    if (productModalEl) {
        productModalEl.addEventListener('shown.bs.modal', function () {
            if (productSearchInput) {
                productSearchInput.value = '';
                productSearchInput.focus();
            }
            performProductSearch('');
        });
    }

    // Search should be done on key press only
    if (productSearchInput) {
        productSearchInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            const query = this.value.trim();
            debounceTimer = setTimeout(() => {
                performProductSearch(query);
            }, 200);
        });
    }

    function performProductSearch(query) {
        if (!productResultsTbody) return;

        if (productSearchLoading) productSearchLoading.classList.remove('d-none');
        if (noProductsFound) noProductsFound.classList.add('d-none');
        productResultsTbody.innerHTML = '';
        if (selectAllProductsCheckbox) selectAllProductsCheckbox.checked = false;
        updateSelectedProductsCount();

        fetch(`/Product/Search?query=${encodeURIComponent(query)}`)
            .then(res => res.json())
            .then(data => {
                if (productSearchLoading) productSearchLoading.classList.add('d-none');
                const products = data.data || [];

                if (products.length === 0) {
                    if (noProductsFound) noProductsFound.classList.remove('d-none');
                    return;
                }

                // Check existing items in the template to identify already added products
                const existingSkuCodes = getExistingSkuCodes();

                products.forEach(p => {
                    const isAlreadyAdded = existingSkuCodes.has(p.code.toUpperCase());
                    const tr = document.createElement('tr');
                    if (isAlreadyAdded) {
                        tr.className = 'table-light text-muted opacity-75';
                    }

                    tr.innerHTML = `
                        <td class="text-center">
                            <input type="checkbox"
                                   class="form-check-input popup-product-checkbox"
                                   value="${p.code}"
                                   data-skucode="${p.code}"
                                   data-skuname="${p.name}"
                                   ${isAlreadyAdded ? 'disabled title="Product already added to template"' : ''} />
                        </td>
                        <td class="font-monospace fw-semibold">
                            ${p.code}
                            ${isAlreadyAdded ? '<span class="badge bg-secondary-subtle text-secondary ms-1 small">Already Added</span>' : ''}
                        </td>
                        <td>${p.name}</td>
                    `;
                    productResultsTbody.appendChild(tr);
                });

                document.querySelectorAll('.popup-product-checkbox:not(:disabled)').forEach(cb => {
                    cb.addEventListener('change', updateSelectedProductsCount);
                });
            })
            .catch(err => {
                if (productSearchLoading) productSearchLoading.classList.add('d-none');
                console.error('Error searching products:', err);
            });
    }

    if (selectAllProductsCheckbox) {
        selectAllProductsCheckbox.addEventListener('change', function () {
            const isChecked = this.checked;
            document.querySelectorAll('.popup-product-checkbox:not(:disabled)').forEach(cb => {
                cb.checked = isChecked;
            });
            updateSelectedProductsCount();
        });
    }

    function updateSelectedProductsCount() {
        const checkedBoxes = document.querySelectorAll('.popup-product-checkbox:checked');
        if (addSelectedProductsBtn) {
            addSelectedProductsBtn.disabled = checkedBoxes.length === 0;
        }
    }

    // ==========================================
    // 4. Add Selected Products (Deduplication Rule from PDF)
    // "If any item already added then if user selecting that item again then don't add in list and add other items ignoring that item."
    // ==========================================
    if (addSelectedProductsBtn) {
        addSelectedProductsBtn.addEventListener('click', async function () {
            const checkedBoxes = Array.from(document.querySelectorAll('.popup-product-checkbox:checked'));
            if (checkedBoxes.length === 0) return;

            addSelectedProductsBtn.disabled = true;
            addSelectedProductsBtn.textContent = 'Adding...';

            const templateUid = templateUidInput ? templateUidInput.value : '';
            const existingSkuCodes = getExistingSkuCodes();
            let addedCount = 0;
            let skippedCount = 0;

            for (const cb of checkedBoxes) {
                const skuCode = cb.value.trim().toUpperCase();

                // If already added, ignore this item and continue adding other items!
                if (existingSkuCodes.has(skuCode)) {
                    skippedCount++;
                    continue;
                }

                const nextIndex = getNextRowIndex();
                try {
                    const response = await fetch(`/LoadTemplate/GetLineRow?skuCode=${encodeURIComponent(skuCode)}&templateUid=${encodeURIComponent(templateUid)}&index=${nextIndex}`);
                    if (response.ok) {
                        const rowHtml = await response.text();
                        linesTableBody.insertAdjacentHTML('beforeend', rowHtml);
                        existingSkuCodes.add(skuCode);
                        addedCount++;
                    }
                } catch (err) {
                    console.error(`Failed to add line for SKU '${skuCode}':`, err);
                }
            }

            addSelectedProductsBtn.textContent = 'Add Selected Product';
            if (productModal) productModal.hide();

            rebindLineEvents();
            checkEmptyState();
        });
    }

    function getNextRowIndex() {
        return document.querySelectorAll('.template-line-row').length;
    }

    // ==========================================
    // 5. Line Row Event Handlers & Batch Delete
    // ==========================================
    function setupDayQtyInputs() {
        document.querySelectorAll('.day-qty').forEach(input => {
            if (input.dataset.qtyBound === 'true') return;
            input.dataset.qtyBound = 'true';

            // If initial value is '0', clear it so placeholder '0' displays cleanly
            if (input.value === '0' || input.value === '0.00' || input.value === '0.0') {
                input.value = '';
            }

            // 1. On focus: If "0", clear it; if non-zero, select all text so typing replaces it immediately
            input.addEventListener('focus', function () {
                if (this.value === '0' || this.value === '0.00' || this.value === '0.0') {
                    this.value = '';
                } else if (this.value !== '') {
                    setTimeout(() => this.select(), 20);
                }
            });

            // 2. On input: Strip leading zeros if user typed e.g. "01" -> "1"
            input.addEventListener('input', function () {
                if (this.value.length > 1 && this.value.startsWith('0') && !this.value.startsWith('0.')) {
                    this.value = this.value.replace(/^0+/, '');
                }
            });

            // 3. On blur: If "0", clear it so placeholder "0" shows; if negative, clear it
            input.addEventListener('blur', function () {
                if (this.value === '0' || this.value === '0.00' || this.value === '0.0') {
                    this.value = '';
                } else if (this.value !== '') {
                    const num = parseFloat(this.value);
                    if (isNaN(num) || num < 0) {
                        this.value = '';
                    }
                }
            });
        });
    }

    // Ensure empty day inputs default to 0 on form submit
    const editForm = document.getElementById('loadTemplateEditForm');
    if (editForm) {
        editForm.addEventListener('submit', function () {
            document.querySelectorAll('.day-qty').forEach(input => {
                if (input.value.trim() === '') {
                    input.value = '0';
                }
            });
        });
    }

    function rebindLineEvents() {
        setupDayQtyInputs();

        document.querySelectorAll('.line-select-checkbox').forEach(cb => {
            cb.onchange = function () {
                updateSelectedLinesCount();
            };
        });
    }

    function removeRow(row) {
        if (!row) return;
        const idInput = row.querySelector('input[name$=".Id"]');
        const isDeletedInput = row.querySelector('.line-is-deleted');

        if (idInput && parseInt(idInput.value, 10) > 0) {
            if (isDeletedInput) isDeletedInput.value = 'true';
            row.style.display = 'none';
        } else {
            row.remove();
        }
        checkEmptyState();
        updateSelectedLinesCount();
    }

    if (selectAllLinesCheckbox) {
        selectAllLinesCheckbox.addEventListener('change', function () {
            const isChecked = this.checked;
            document.querySelectorAll('.template-line-row').forEach(row => {
                if (row.style.display !== 'none') {
                    const cb = row.querySelector('.line-select-checkbox');
                    if (cb) cb.checked = isChecked;
                }
            });
            updateSelectedLinesCount();
        });
    }

    // Delete Selected From List button
    if (btnDeleteSelectedLines) {
        btnDeleteSelectedLines.addEventListener('click', function () {
            const checkedBoxes = Array.from(document.querySelectorAll('.line-select-checkbox:checked'));
            if (checkedBoxes.length === 0) return;

            checkedBoxes.forEach(cb => {
                const row = cb.closest('.template-line-row');
                removeRow(row);
            });

            if (selectAllLinesCheckbox) selectAllLinesCheckbox.checked = false;
            updateSelectedLinesCount();
        });
    }

    function updateSelectedLinesCount() {
        const checked = document.querySelectorAll('.line-select-checkbox:checked').length;
        if (btnDeleteSelectedLines) btnDeleteSelectedLines.disabled = checked === 0;
    }

    function checkEmptyState() {
        let activeLineCount = 0;
        document.querySelectorAll('.template-line-row').forEach(row => {
            const isDeleted = row.querySelector('.line-is-deleted')?.value === 'true' || row.style.display === 'none';
            if (!isDeleted) activeLineCount++;
        });

        if (emptyLinesRow) {
            if (activeLineCount === 0) {
                emptyLinesRow.classList.remove('d-none');
            } else {
                emptyLinesRow.classList.add('d-none');
            }
        }
    }

    // Initial setup on load
    rebindLineEvents();
    checkEmptyState();
});
