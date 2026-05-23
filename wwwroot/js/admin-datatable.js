// Standard DataTables init for all Admin listing pages.
// Usage: initDataTable('#tableId', actionsColumnIndex)
function initDataTable(tableSelector, actionsColumnIndex) {
    $(tableSelector).DataTable({
        pageLength: 25,
        order: [[0, 'asc']],
        columnDefs: [
            { orderable: false, targets: actionsColumnIndex }
        ],
        language: {
            search:     'Search:',
            lengthMenu: 'Show _MENU_ entries',
            info:       'Showing _START_ to _END_ of _TOTAL_ entries',
            paginate: {
                first:    'First',
                last:     'Last',
                next:     'Next',
                previous: 'Previous'
            }
        }
    });
}
