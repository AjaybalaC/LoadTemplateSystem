/**
 * loadTemplateIndex.js
 * Handles live search on key press only, AJAX sorting/pagination, and Delete Confirm Modal.
 */
document.addEventListener('DOMContentLoaded', function () {
    const searchTermInput = document.getElementById('searchTermInput');
    const clearSearchBtn = document.getElementById('clearSearchBtn');
    const pageSizeSelect = document.getElementById('pageSizeSelect');
    const templatesContainer = document.getElementById('templatesContainer');
    const sortColumnInput = document.getElementById('sortColumnInput');
    const sortDirectionInput = document.getElementById('sortDirectionInput');
    const pageNumberInput = document.getElementById('pageNumberInput');

    const deleteModalEl = document.getElementById('deleteConfirmModal');
    let deleteModal = null;
    if (deleteModalEl) {
        deleteModal = new bootstrap.Modal(deleteModalEl);
    }
    const deleteIdInput = document.getElementById('deleteTemplateId');
    const deleteCodeSpan = document.getElementById('deleteTemplateCode');

    let debounceTimer = null;

    // ==============================================================
    // 1. Live Search on Key Press Only
    // ==============================================================
    if (searchTermInput) {
        searchTermInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            const query = this.value;

            // Toggle clear button
            if (clearSearchBtn) {
                if (query.trim().length > 0) {
                    clearSearchBtn.classList.remove('d-none');
                } else {
                    clearSearchBtn.classList.add('d-none');
                }
            }

            // Debounced AJAX search on key press (250ms)
            debounceTimer = setTimeout(() => {
                if (pageNumberInput) pageNumberInput.value = '1';
                loadTemplatesAjax();
            }, 250);
        });

        // Prevent accidental form submission on Enter key
        searchTermInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                clearTimeout(debounceTimer);
                loadTemplatesAjax();
            }
        });
    }

    // Clear search button
    if (clearSearchBtn) {
        clearSearchBtn.addEventListener('click', function () {
            if (searchTermInput) {
                searchTermInput.value = '';
                searchTermInput.focus();
            }
            clearSearchBtn.classList.add('d-none');
            if (pageNumberInput) pageNumberInput.value = '1';
            loadTemplatesAjax();
        });
    }

    // Page size dropdown change
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener('change', function () {
            if (pageNumberInput) pageNumberInput.value = '1';
            loadTemplatesAjax();
        });
    }

    // ==============================================================
    // 2. Fetch Templates Partial via AJAX
    // ==============================================================
    function loadTemplatesAjax() {
        if (!templatesContainer) return;

        const searchTerm = searchTermInput ? searchTermInput.value.trim() : '';
        const sortColumn = sortColumnInput ? sortColumnInput.value : 'TemplateName';
        const sortDirection = sortDirectionInput ? sortDirectionInput.value : 'asc';
        const page = pageNumberInput ? pageNumberInput.value : '1';
        const pageSize = pageSizeSelect ? pageSizeSelect.value : '10';

        const params = new URLSearchParams({
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortDirection: sortDirection,
            page: page,
            pageSize: pageSize
        });

        const url = `/LoadTemplate/Index?${params.toString()}`;

        // Add subtle loading opacity
        templatesContainer.style.opacity = '0.5';

        fetch(url, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(res => {
            if (!res.ok) throw new Error('Network response was not ok');
            return res.text();
        })
        .then(html => {
            templatesContainer.innerHTML = html;
            templatesContainer.style.opacity = '1';

            // Update browser URL without full reload
            window.history.replaceState(null, '', url);

            // Re-bind click handlers for dynamic elements
            bindDynamicTableEvents();
        })
        .catch(err => {
            templatesContainer.style.opacity = '1';
            console.error('Error loading templates:', err);
        });
    }

    // ==============================================================
    // 3. Bind Dynamic Table Events (Sort, Pager, Delete Modal)
    // ==============================================================
    function bindDynamicTableEvents() {
        // Handle Delete button clicks
        document.querySelectorAll('.btn-delete-template').forEach(button => {
            button.onclick = function (e) {
                e.preventDefault();
                const templateId = this.getAttribute('data-id');
                const templateCode = this.getAttribute('data-code');

                if (deleteIdInput) deleteIdInput.value = templateId;
                if (deleteCodeSpan) deleteCodeSpan.textContent = templateCode;

                if (deleteModal) deleteModal.show();
            };
        });

        // Intercept Sort column clicks for AJAX loading
        document.querySelectorAll('.sort-link').forEach(link => {
            link.onclick = function (e) {
                e.preventDefault();
                const col = this.getAttribute('data-col');
                const dir = this.getAttribute('data-dir');

                if (sortColumnInput) sortColumnInput.value = col;
                if (sortDirectionInput) sortDirectionInput.value = dir;

                loadTemplatesAjax();
            };
        });

        // Intercept Pager links for AJAX loading
        document.querySelectorAll('.pagination .page-link').forEach(link => {
            link.onclick = function (e) {
                const href = this.getAttribute('href');
                if (href && href !== '#' && !this.closest('.page-item').classList.contains('disabled')) {
                    e.preventDefault();
                    try {
                        const urlObj = new URL(href, window.location.origin);
                        const page = urlObj.searchParams.get('page') || '1';
                        if (pageNumberInput) pageNumberInput.value = page;
                        loadTemplatesAjax();
                    } catch (ex) {
                        window.location.href = href;
                    }
                }
            };
        });
    }

    // Initial binding
    bindDynamicTableEvents();
});
