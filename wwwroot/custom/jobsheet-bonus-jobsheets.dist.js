/**
 * app-ecommerce-order-list Script
 */

'use strict';

// Datatable (jquery)

$(function () {
  let borderColor, bodyBg, headingColor;

  if (isDarkStyle) {
    borderColor = config.colors_dark.borderColor;
    bodyBg = config.colors_dark.bodyBg;
    headingColor = config.colors_dark.headingColor;
  } else {
    borderColor = config.colors.borderColor;
    bodyBg = config.colors.bodyBg;
    headingColor = config.colors.headingColor;
  }

  // Variable declaration for table

  var dt_order_table = $('.datatables-order-jobsheets'),
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
  // E-commerce Products datatable

  pdfMake.fonts = {
    FZYTK: {
      normal: 'FZYTK.TTF',
      bold: 'FZYTK.TTF',
      italics: 'FZYTK.TTF',
      bolditalics: 'FZYTK.TTF'
    }
  };


  if (dt_order_table.length) {
    var dt_products = dt_order_table.DataTable({
      ajax: {
        url: '/JobsheetF/BonusByUser?handler=JobsheetData', // Replace with your actual handler path
        type: 'GET', // Or 'GET', depending on your handler
        dataSrc: 'data'   // Use '' if your JSON is a flat array, or 'data' if it's wrapped
      },
      columns: [
        // columns according to JSON
        { data: 'userId' },
        { data: 'userId' },
        { data: 'username' },
        { data: 'Name' },
        { data: 'DepartmentName' },
        { data: 'JobNo' }, //email //avatar
        { data: 'CompletedDate' },
        { data: 'TotalAmount' },  
        { data: 'bonusAmount' },    
     
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
          // For Checkboxes
          targets: 1,
          orderable: false,
          checkboxes: {
            /*selectAllRender: '<input type="checkbox" class="form-check-input">'*/
            selectAllRender: ''
          },
          render: function () {
            /*return '<input type="checkbox" class="dt-checkboxes form-check-input" >';*/
            return '';
          },
          searchable: false
        },
        {
          // Order ID
          targets: 2,
          render: function (data, type, full, meta) {
            var $order_id = full['username'];
            // Creates full output for row
            var $row_output = '<span>' + $order_id + '</span>';
            return $row_output;
          }
        },
        {
          // Order ID
          targets: 3,
          render: function (data, type, full, meta) {
            var $order_id = full['fullName'];
            // Creates full output for row
            var $row_output = '<span>' + $order_id + '</span>';
            return $row_output;
          }
        },
        {
          // Date and Time
          targets: 4,
          render: function (data, type, full, meta) {
            var $order_id = full['departmentName'];
            // Creates full output for row
           
          

            return '<span class="text-nowrap">' + $order_id + ' </span>';
          }
        },
       
        {
          // Job Type
          targets: 5,
          render: function (data, type, full, meta) {
            var $columnValue = full['jobNo'];

            var $row_output = '<div class="d-flex flex-column">' +
              '<span class="text-nowrap"><a target="_blank" href="/JobsheetF/Preview/' + full['jobId'] + '"><span>#' + $columnValue + '</span></a></span>' +
              '<small class="text-truncate"> ' +
              full['branchName'] +
              '</small>' +
              '</div>';
            return $row_output;
          }
        },

        {
          // Date and Time
          targets: 6,
          render: function (data, type, full, meta) {
            var date = new Date(full['jobCompleteDate']); // convert the date string to a Date object
            var timeX = "";


            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-indexed
            const day = String(date.getDate()).padStart(2, '0');

            const formattedDate = `${year}-${month}-${day}`;


            var $row_output = '<div class="d-flex flex-column">' +
              '<span class="text-nowrap">' + formattedDate + '</span>' +
              '<small class="text-truncate">Inv: ' +
              full['invoiceNo'] +
              '</small>' +
              '</div>';

            return $row_output;
          }
        },

        {
          // Total Amount
          targets: 7,
          render: function (data, type, full, meta) {
            var $columnValue = full['totalAmount'];

            return '<span class="text-nowrap">' + full['totalAmount'] + ' </span>';
          }
        },

        {
          // Total Amount
          targets: 8,
          render: function (data, type, full, meta) {
            var $columnValue = full['bonusAmount'];

            return '<span class="text-nowrap">' + $columnValue + ' </span>';
          }
        },
                     
      ],
      order: [3, 'asc'], //set any columns order asc/desc
      dom:
        '<"card-header d-flex flex-column flex-md-row align-items-start align-items-md-center pb-md-0 pt-0"<f><"d-flex align-items-md-center justify-content-md-end gap-4"l<"dt-action-buttons"B>>' +
        '>t' +
        '<"row mx-1"' +
        '<"col-sm-12 col-md-6"i>' +
        '<"col-sm-12 col-md-6"p>' +
        '>',
      lengthMenu: [10, 40, 60, 80, 100], //for length of menu
      language: {
        sLengthMenu: '_MENU_',
        search: '',
        searchPlaceholder: 'Search',
        info: 'Displaying _START_ to _END_ of _TOTAL_ entries',
        paginate: {
          next: '<i class="ri-arrow-right-s-line"></i>',
          previous: '<i class="ri-arrow-left-s-line"></i>'
        }
      },
      // Buttons with Dropdown
      buttons: [
        {
          extend: 'collection',
          className: 'btn btn-outline-secondary dropdown-toggle waves-effect waves-light',
          text: '<i class="ri-upload-2-line ri-16px me-2"></i> <span class="d-none d-sm-inline-block">Export</span>',
          buttons: [
            {
              extend: 'print',
              text: '<i class="ri-printer-line me-1" ></i>Print',
              className: 'dropdown-item',
              exportOptions: {
                columns: [1, 2, 3, 4, 5, 6, 7, 8],
                // prevent avatar to be print
                format: {
                  body: function (inner, coldex, rowdex) {
                    if (inner.length <= 0) return inner;
                    var el = $.parseHTML(inner);
                    var result = '';
                    $.each(el, function (index, item) {
                      if (item.classList !== undefined && item.classList.contains('user-name')) {
                        result = result + item.lastChild.firstChild.textContent;
                      } else if (item.innerText === undefined) {
                        result = result + item.textContent;
                      } else result = result + item.innerText;
                    });
                    return result;
                  }
                }
              },
              customize: function (win) {
                //customize print view for dark
                $(win.document.body)
                  .css('color', headingColor)
                  .css('border-color', borderColor)
                  .css('background-color', bodyBg);
                $(win.document.body)
                  .find('table')
                  .addClass('compact')
                  .css('color', 'inherit')
                  .css('border-color', 'inherit')
                  .css('background-color', 'inherit');
              }
            },
            //{
            //  extend: 'csv',
            //  text: '<i class="ri-file-text-line me-1" ></i>Csv',
            //  className: 'dropdown-item',
            //  exportOptions: {
            //    columns: [1, 2, 3, 4, 5, 6, 7, 8],
            //    // prevent avatar to be display
            //    format: {
            //      body: function (inner, coldex, rowdex) {
            //        if (inner.length <= 0) return inner;
            //        var el = $.parseHTML(inner);
            //        var result = '';
            //        $.each(el, function (index, item) {
            //          if (item.classList !== undefined && item.classList.contains('user-name')) {
            //            result = result + item.lastChild.firstChild.textContent;
            //          } else if (item.innerText === undefined) {
            //            result = result + item.textContent;
            //          } else result = result + item.innerText;
            //        });
            //        return result;
            //      }
            //    }
            //  }
            //},
            {
              extend: 'excel',
              text: '<i class="ri-file-excel-line me-1"></i>Excel',
              className: 'dropdown-item',
              exportOptions: {
                columns: [1, 2, 3, 4, 5, 6, 7, 8],
                // prevent avatar to be display
                format: {
                  body: function (inner, coldex, rowdex) {
                    if (inner.length <= 0) return inner;
                    var el = $.parseHTML(inner);
                    var result = '';
                    $.each(el, function (index, item) {
                      if (item.classList !== undefined && item.classList.contains('user-name')) {
                        result = result + item.lastChild.firstChild.textContent;
                      } else if (item.innerText === undefined) {
                        result = result + item.textContent;
                      } else result = result + item.innerText;
                    });
                    return result;
                  }
                }
              }
            },
            {
              extend: 'pdf',
              text: '<i class="ri-file-pdf-line me-1"></i>Pdf',
              className: 'dropdown-item',
              exportOptions: {
                columns: [1, 2, 3, 4, 5, 6, 7, 8],
                // prevent avatar to be display
                format: {
                  body: function (inner, coldex, rowdex) {
                    if (inner.length <= 0) return inner;
                    var el = $.parseHTML(inner);
                    var result = '';
                    $.each(el, function (index, item) {
                      if (item.classList !== undefined && item.classList.contains('user-name')) {
                        result = result + item.lastChild.firstChild.textContent;
                      } else if (item.innerText === undefined) {
                        result = result + item.textContent;
                      } else result = result + item.innerText;
                    });
                    return result;
                  }
                }
              },
              customize: function (doc) {
                doc.defaultStyle = {
                  font: 'FZYTK' // 👈 The font name you registered earlier
                };
              }

            },
            {
              extend: 'copy',
              text: '<i class="ri-file-copy-line me-1"></i>Copy',
              className: 'dropdown-item',
              exportOptions: {
                columns: [1, 2, 3, 4, 5, 6, 7, 8],
                // prevent avatar to be display
                format: {
                  body: function (inner, coldex, rowdex) {
                    if (inner.length <= 0) return inner;
                    var el = $.parseHTML(inner);
                    var result = '';
                    $.each(el, function (index, item) {
                      if (item.classList !== undefined && item.classList.contains('user-name')) {
                        result = result + item.lastChild.firstChild.textContent;
                      } else if (item.innerText === undefined) {
                        result = result + item.textContent;
                      } else result = result + item.innerText;
                    });
                    return result;
                  }
                }
              }
            }
          ]
        }
      ],
      // For responsive popup
      responsive: {
        details: {
          display: $.fn.dataTable.Responsive.display.modal({
            header: function (row) {
              var data = row.data();
              return 'Details of ' + data['username'];
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
    $('.dt-action-buttons').addClass('pt-0');
  }

  // Delete Record
  $('.datatables-order-summary tbody').on('click', '.delete-record', function () {
    dt_products.row($(this).parents('tr')).remove().draw();
  });

  // Filter form control to default size
  // ? setTimeout used for multilingual table initialization
  setTimeout(() => {
    $('.dataTables_filter .form-control').addClass('ms-0');
  }, 300);
});
