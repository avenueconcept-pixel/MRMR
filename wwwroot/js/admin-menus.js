/**
 * admin-menus.js
 * Handles: SortableJS tree drag-drop, sort save, icon picker, delete confirm.
 * Depends on: SortableJS, SweetAlert2, jQuery, pageMsg object declared by page.
 */

let sortChanged = false;

document.addEventListener('DOMContentLoaded', () => {
    initSortables();
    buildIconGrid();
});

function initSortables() {
    document.querySelectorAll('.sortable-list').forEach(list => {
        Sortable.create(list, {
            group:      { name: 'menus', put: true },
            handle:     '.drag-handle',
            animation:  150,
            ghostClass: 'opacity-25',
            onEnd: (evt) => {
                const item      = evt.item;
                const toList    = evt.to;
                const newLevel  = parseInt(toList.dataset.level);
                const newParent = toList.dataset.parentId || '';

                item.dataset.level    = newLevel;
                item.dataset.parentId = newParent;

                item.classList.remove('menu-group', 'menu-parent', 'menu-child', 'menu-leaf');
                if (newLevel === 1)      item.classList.add('menu-parent');
                else if (newLevel === 2) item.classList.add('menu-child');
                else                    item.classList.add('menu-leaf');

                sortChanged = true;
                const btn = document.getElementById('btnSaveOrder');
                if (btn) btn.classList.remove('d-none');
            }
        });
    });
}

async function saveSortOrder() {
    const items = [];

    document.querySelectorAll('.sortable-list').forEach(list => {
        const level    = parseInt(list.dataset.level);
        const parentId = list.dataset.parentId ? parseInt(list.dataset.parentId) : null;

        list.querySelectorAll(':scope > .menu-row').forEach((row, idx) => {
            items.push({
                id:        parseInt(row.dataset.id),
                sortOrder: idx,
                level:     parseInt(row.dataset.level),
                parentId:  row.dataset.parentId ? parseInt(row.dataset.parentId) : null
            });
        });
    });

    const token = document.querySelector('#formAjax input[name="__RequestVerificationToken"]').value;

    try {
        const res  = await fetch('?handler=SaveSort', {
            method:  'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body:    JSON.stringify(items)
        });
        const data = await res.json();

        if (data.success) {
            Swal.fire({ icon: 'success', timer: 1500, showConfirmButton: false, title: pageMsg.sortSuccess });
            sortChanged = false;
            const btn = document.getElementById('btnSaveOrder');
            if (btn) btn.classList.add('d-none');
        } else {
            Swal.fire({ icon: 'error', title: pageMsg.sortError, text: data.message });
        }
    } catch (err) {
        Swal.fire({ icon: 'error', title: 'Error', text: err.message });
    }
}

window.addEventListener('beforeunload', (e) => {
    if (sortChanged) { e.preventDefault(); e.returnValue = ''; }
});

async function confirmDelete(id, title, text, confirmBtn, cancelBtn) {
    const result = await Swal.fire({
        icon:               'warning',
        title:              title,
        text:               text,
        showCancelButton:   true,
        confirmButtonText:  confirmBtn,
        cancelButtonText:   cancelBtn,
        confirmButtonColor: '#dc3545'
    });

    if (!result.isConfirmed) return;

    const token = document.querySelector('#formAjax input[name="__RequestVerificationToken"]').value;
    const formData = new FormData();
    formData.append('__RequestVerificationToken', token);

    try {
        const res  = await fetch('?handler=SoftDelete&id=' + id, { method: 'POST', body: formData });
        const data = await res.json();

        if (data.success) {
            Swal.fire({ icon: 'success', timer: 1500, showConfirmButton: false, title: pageMsg.deleteSuccess })
                .then(() => window.location.reload());
        } else {
            Swal.fire({ icon: 'error', text: data.message || pageMsg.deleteError });
        }
    } catch (err) {
        Swal.fire({ icon: 'error', text: err.message });
    }
}

function onLevelChange(val) {
    const level     = parseInt(val);
    const rowGroup  = document.getElementById('rowGroup');
    const rowParent = document.getElementById('rowParent');
    const rowUrl    = document.getElementById('rowUrl');

    if (rowGroup)  rowGroup.style.display  = level === 1 ? '' : 'none';
    if (rowParent) rowParent.style.display = level === 2 ? '' : 'none';
    if (rowUrl)    rowUrl.style.display    = level === 0 ? 'none' : '';
}

function previewIcon(val) {
    const el = document.getElementById('iconPreviewEl');
    if (!el) return;
    el.className = val.trim() ? 'ri ' + val.trim() : 'ri ri-question-line';
}

const REMIX_ICONS = [
    'ri-home-line','ri-dashboard-line','ri-settings-line','ri-user-line',
    'ri-team-line','ri-admin-line','ri-lock-line','ri-shield-line',
    'ri-global-line','ri-map-pin-line','ri-map-2-line','ri-earth-line',
    'ri-building-line','ri-store-line','ri-hotel-line',
    'ri-money-dollar-circle-line','ri-bank-card-line','ri-wallet-line',
    'ri-file-list-line','ri-file-text-line','ri-clipboard-line',
    'ri-bar-chart-line','ri-pie-chart-line','ri-line-chart-line',
    'ri-calendar-line','ri-time-line','ri-history-line',
    'ri-notification-line','ri-mail-line','ri-message-line',
    'ri-search-line','ri-filter-line','ri-sort-asc','ri-sort-desc',
    'ri-add-line','ri-edit-line','ri-delete-bin-line','ri-save-line',
    'ri-refresh-line','ri-download-line','ri-upload-line',
    'ri-eye-line','ri-eye-off-line','ri-key-line','ri-link-line',
    'ri-checkbox-circle-line','ri-close-circle-line','ri-information-line',
    'ri-star-line','ri-heart-line','ri-bookmark-line','ri-flag-line',
    'ri-tag-line','ri-price-tag-line','ri-coupon-line',
    'ri-truck-line','ri-car-line','ri-flight-takeoff-line',
    'ri-customer-service-line','ri-question-line','ri-error-warning-line',
    'ri-list-check','ri-survey-line','ri-discuss-line',
    'ri-restaurant-line','ri-shopping-cart-line','ri-gift-line',
    'ri-image-line','ri-video-line','ri-music-line','ri-mic-line',
    'ri-code-line','ri-terminal-line','ri-database-line','ri-server-line',
    'ri-cloud-line','ri-wifi-line','ri-bluetooth-line','ri-smartphone-line',
    'ri-computer-line','ri-printer-line','ri-shield-star-line',
    'ri-group-line','ri-contacts-line','ri-profile-line'
];

function buildIconGrid(filter = '') {
    const grid = document.getElementById('iconGrid');
    if (!grid) return;
    const filtered = filter
        ? REMIX_ICONS.filter(ic => ic.includes(filter.toLowerCase()))
        : REMIX_ICONS;
    grid.innerHTML = filtered.map(ic => `
        <div class="col">
            <button type="button"
                    class="btn btn-outline-secondary d-flex flex-column align-items-center p-2"
                    style="width:80px;font-size:11px;" title="${ic}"
                    onclick="selectIcon('${ic.replace('ri-', '')}')">
                <i class="ri ${ic} fs-5 mb-1"></i>
                <span class="text-truncate w-100 text-center" style="font-size:9px;">
                    ${ic.replace('ri-','').replace('-line','')}
                </span>
            </button>
        </div>
    `).join('');
}

function filterIcons(val) { buildIconGrid(val); }

function selectIcon(iconClass) {
    const input = document.getElementById('iconInput');
    if (input) { input.value = iconClass; previewIcon(iconClass); }
    const modal = bootstrap.Modal.getInstance(document.getElementById('iconModal'));
    if (modal) modal.hide();
}
