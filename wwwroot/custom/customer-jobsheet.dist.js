/**
 * Page Detail overview
 */

'use strict';

// Datatable (jquery)
$(function () {
  // Variable declaration for table
  var dt_customer_order = $('.datatables-customer-order'),
    statusObj = {
      "Draft": { title: 'Draft', class: 'text-danger' },
      "InDesign": { title: 'InDesign', class: 'bg-label-warning' },
      "Completed": { title: 'Completed', class: 'bg-label-success' },
      "InProduction": { title: 'InProduction', class: 'bg-label-primary' },
      "CompletedInstallation": { title: 'CompletedInstallation', class: 'bg-label-info' },
      "WaitInstallation": { title: 'WaitInstallation', class: 'bg-label-info' },
      "WaitPickup": { title: 'WaitPickup', class: 'bg-label-info' },
      "WaitShip": { title: 'WaitShip', class: 'bg-label-info' },
      "FollowupPayment": { title: 'FollowupPayment', class: 'text-warning' },
      "FollowupPaymentEP": { title: 'FollowupPaymentEP', class: 'text-warning' },
      "FollowupPaymentLO": { title: 'FollowupPaymentLO', class: 'text-warning' },
      "Cancel": { title: 'Cancel', class: 'text-danger' },
      "KIV": { title: 'KIV', class: 'text-danger' }
    },
    jobTypeObj = {
      "PrintingType": { title: 'PrintingType', class: 'text-success' },
      "SignBoardType": { title: 'SignBoardType', class: 'text-warning' },
      "UniformType": { title: 'UniformType', class: 'text-danger' },
      "OtherType": { title: 'OtherType', class: 'text-secondary' }
    },
    priorityObj = {
      1: { title: 'Top', class: 'text-danger' },
      2: { title: 'High', class: 'text-warning' },
      3: { title: 'Medium', class: 'text-success' },
      4: { title: 'Low', class: 'text-secondary' }
    };

  // orders datatable
  if (dt_customer_order.length) {
    var dt_order = dt_customer_order.DataTable({
      ajax: {
        url: '/CustomerF/CustomerOverview?handler=JobsheetData', // Replace with your actual handler path
        type: 'GET', // Or 'GET', depending on your handler
        dataSrc: 'data'   // Use '' if your JSON is a flat array, or 'data' if it's wrapped
      },
      columns: [
        // columns according to JSON
        { data: 'jobId' },
        { data: 'jobNo' },
        { data: 'entryDate' },
       
        { data: 'totalAmount' },
        { data: 'jobStatus' },
        { data: 'jobType' },
        { data: 'priority' }, //method_number
      ],
      columnDefs: [
        {
          // For Responsive
          className: 'control',
          searchable: false,
          orderable: false,
          responsivePriority: 2,
          targets: 0,
          render: function (data, type, full, meta) {
            return '';
          }
        },
        {
          // order order number
          targets: 1,
          render: function (data, type, full, meta) {
            var $order_id = full['jobNo'];
            // Creates full output for row
            var $row_output = '<div class="d-flex flex-column">' +
              '<span class="text-nowrap"><a target="_blank" href="/JobsheetF/Preview/' + full['jobId'] + '"><span>#' + $order_id + '</span></a></span>' +
              '<small class="text-truncate"> ' +
              full['branchName'] +
              '</small>' +
              '</div>'
            return $row_output;
          }
        },
        {
          // date
          targets: 2,
          render: function (data, type, full, meta) {
            var date = new Date(full['entryDate']); // convert the date string to a Date object
            var timeX = "";
            //var formattedDate = date.toLocaleDateString('en-US', {
            //  month: 'short',
            //  day: 'numeric',
            //  year: 'numeric',
            //  time: 'numeric'
            //});

            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-indexed
            const day = String(date.getDate()).padStart(2, '0');

            const formattedDate = `${year}-${month}-${day}`;


            var $row_output = '<div class="d-flex flex-column">' +
              '<span class="text-nowrap">' + formattedDate + ' [<font color=red>' + full['processDay'] + '</font>]</span>' +
              '<small class="text-truncate">Inv: ' +
              full['invoiceNo'] +
              '</small>' +
              '</div>'

            return $row_output;
          }
        },
        {
          // status
          targets: 5,
          render: function (data, type, full, meta) {
            var $columnValue = full['jobType'];

            var $row_output = '<div class="d-flex flex-column">' +
              '<span class="text-nowrap"><span class="badge px-2 rounded-pill ' +
              jobTypeObj[$columnValue].class +
              '" text-capitalized>' +
              full['jobTypeText'] +
              '</span></span>' +
              '<small class="text-truncate"> A:' +
              full['adminUsername'] +
              '</small>' +
              '<small class="text-truncate"> D:' +
              full['designerUsername'] +
              '</small>' +
              '</div>';

            return $row_output;
          }
        },
        {
          // spent
          targets: 3,
          render: function (data, type, full, meta) {
            var $columnValue = full['totalAmount'];

            return '<span class="text-nowrap">' + full['totalAmount'] + ' [<font color=red>' + full['balanceAmount'] + '</font>]</span>';
          }
        },
        {
          // Status
          targets: 4,
          render: function (data, type, full, meta) {
            var $columnValue = full['jobStatus'];

            return (
              '<span class="badge px-2 rounded-pill ' +
              statusObj[$columnValue].class +
              '" text-capitalized>' +
              full['jobStatusText'] +
              '</span>'
            );
          }
        },
        {
          // Priority
          targets: 6,
          render: function (data, type, full, meta) {
            var $priority = full['priority'],
              $Obj = priorityObj[$priority];
            if ($Obj) {
              return (
                '<h6 class="mb-0 w-px-100 d-flex align-items-center ' +
                priorityObj.class +
                '">' +
                '<i class="ri-circle-fill ri-10px me-1"></i>' +
                full['priorityText'] +
                '</h6>'
              );
            }
            return data;
          }
        },

        //{
        //  // Actions
        //  targets: -1,
        //  title: 'Actions',
        //  searchable: false,
        //  orderable: false,
        //  render: function (data, type, full, meta) {
        //    return (
        //      '<div>' +
        //      '<button class="btn btn-sm btn-icon btn-text-secondary waves-effect waves-light rounded-pill dropdown-toggle hide-arrow" data-bs-toggle="dropdown"><i class="ri-more-2-line ri-20px"></i></button>' +
        //      '<div class="dropdown-menu dropdown-menu-end m-0">' +
        //      '<a href="javascript:;" class="dropdown-item">View</a>' +
        //      '<a href="javascript:;" class="dropdown-item  delete-record">Delete</a>' +
        //      '</div>' +
        //      '</div>'
        //    );
        //  }
        //}
      ],
      order: [[1, 'desc']],
      dom:
        '<"card-header d-flex flex-wrap py-0 pt-5 pt-sm-0 flex-column flex-sm-row"<"head-label text-center me-4 ms-1">f' +
        '>t' +
        '<"row mx-4"' +
        '<"col-md-12 col-xxl-6 text-center text-xxl-start pb-2 pb-xxl-0 pe-0"i>' +
        '<"col-md-12 col-xxl-6"p>' +
        '>',
      lengthMenu: [6, 30, 50, 70, 100],
      language: {
        sLengthMenu: '_MENU_',
        search: '',
        searchPlaceholder: 'Search',
        paginate: {
          next: '<i class="ri-arrow-right-s-line"></i>',
          previous: '<i class="ri-arrow-left-s-line"></i>'
        }
      },
      // Buttons with Dropdown

      // For responsive popup
      responsive: {
        details: {
          display: $.fn.dataTable.Responsive.display.modal({
            header: function (row) {
              var data = row.data();
              return 'Details of Jobsheet: ' + data['jobNo'];
            }
          }),
          type: 'column',
          renderer: function (api, rowIdx, columns) {
            var data = $.map(columns, function (col, i) {
              return col.title !== '' // ? Do not show row in modal popup if title is blank (for check box)
                ? '<tr data-dt-row="' +
                col.rowIndex +
                '" data-dt-column="' +
                col.columnIndex +
                '">' +
                '<td>' +
                col.title +
                ':' +
                '</td> ' +
                '<td>' +
                col.data +
                '</td>' +
                '</tr>'
                : '';
            }).join('');

            return data ? $('<table class="table"/><tbody />').append(data) : false;
          }
        }
      }
    });
    $('div.head-label').html('<h5 class="card-title mb-0 text-nowrap">Jobsheets</h5>');
    $('.pagination').addClass('justify-content-xxl-end justify-content-center');
  }

  // Delete Record
  $('.datatables-orders tbody').on('click', '.delete-record', function () {
    dt_order.row($(this).parents('tr')).remove().draw();
  });
});

// Validation & Phone mask
