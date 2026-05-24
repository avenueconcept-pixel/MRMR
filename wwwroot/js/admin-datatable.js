// Standard DataTables init for all Admin listing pages.
// Usage: initDataTable('#tableId', actionsColumnIndex, order?)
function initDataTable(tableSelector, actionsColumnIndex, order = [[0, 'asc']]) {
    $(tableSelector).DataTable({
        pageLength: 25,
        order: order,
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
