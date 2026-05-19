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

  var dt_order_table = $('.datatables-customers'),
    statusObj = {
      "Draft": { title: 'Draft', class: 'text-danger' },
      "InDesign": { title: 'InDesign', class: 'bg-label-warning' },
      "Completed": { title: 'Completed', class: 'bg-label-success' },
      "InProduction": { title: 'InProduction', class: 'bg-label-primary' },
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
        url: '/Index?handler=DataList', // Replace with your actual handler path
        type: 'GET', // Or 'GET', depending on your handler
        dataSrc: 'data'   // Use '' if your JSON is a flat array, or 'data' if it's wrapped
      },
      columns: [
        // columns according to JSON
        { data: 'jobId' },
        { data: 'jobId' },
        { data: 'jobNo' },
        { data: 'entryDate' },
        { data: 'customerName' }, //email //avatar
        { data: 'jobType' },
        { data: 'totalAmount' },
        { data: 'jobStatus' },
        { data: 'priority' }, //method_number
        /*{ data: '' }*/
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
          // Date and Time
          targets: 3,
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
          // Customers
          targets: 4,
          responsivePriority: 1,
          render: function (data, type, full, meta) {
            var $name = full['customerName'],
              $pic = full['contactPICName'],
              $email = full['contactPICEmail'],
              $mobile = full['contactPICMobile'],
              $avatar = "";
            if ($avatar) {
              // For Avatar image
              var $output =
                '<img src="' + assetsPath + 'img/avatars/' + $avatar + '" alt="Avatar" class="rounded-circle">';
            } else {
              // For Avatar badge
              var stateNum = Math.floor(Math.random() * 6);
              var states = ['success', 'danger', 'warning', 'info', 'dark', 'primary', 'secondary'];
              var $state = states[stateNum],
                $name = full['customerName'],
                $initials = $name.match(/\b\w/g) || [];
              $initials = (($initials.shift() || '') + ($initials.pop() || '')).toUpperCase();
              $output = '<span class="avatar-initial rounded-circle bg-label-' + $state + '">' + $initials + '</span>';
            }
            // Creates full output for row
            var $row_output =
              '<div class="d-flex justify-content-start align-items-center user-name">' +
              '<div class="avatar-wrapper me-3">' +
              '<div class="avatar avatar-sm">' +
              $output +
              '</div>' +
              '</div>' +
              '<div class="d-flex flex-column">' +
              '<a target="_blank" href="/CustomerF/CustomerOverView/?CustomerId=' + full['customerId'] + '" class="text-truncate text-heading">' +
              '<span class="fw-medium" title="' + $name + '"> ' +
              full['customerNameShort'] +
              '</span></a>' +
              '<small class="text-truncate">' +
              $pic +
              '</small>' +
              '<small class="text-truncate"><a target="_blank" href="https://wa.me/' + $mobile + '"><img width=20 src="../../img/whatsapp.png"/> ' +
              $mobile +
              '</a></small>' +
              '</div>' +
              '</div>';
            return $row_output;
          }
        },
        {
          // Job Type
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
          // Total Amount
          targets: 6,
          render: function (data, type, full, meta) {
            var $columnValue = full['totalAmount'];

            return '<span class="text-nowrap">' + full['totalAmount'] + ' [<font color=red>' + full['balanceAmount'] + '</font>]</span>';
          }
        },

        {
          // Status
          targets: 7,
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
          targets: 8,
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
        }
        //,
        //{
        //  // Actions
        //  targets: -1,
        //  title: '***',
        //  searchable: false,
        //  orderable: false,
        //  render: function (data, type, full, meta) {
        //    return (
        //      '<div>' +
        //      '<button class="btn btn-sm btn-icon btn-text-secondary waves-effect waves-light rounded-pill dropdown-toggle hide-arrow" data-bs-toggle="dropdown"><i class="ri-more-2-line ri-20px"></i></button>' +
        //      '<div class="dropdown-menu dropdown-menu-end m-0">' +
        //      '<a href="/Apps/Ecommerce/Order/OrderDetails" class="dropdown-item">View</a>' +
        //      '<a href="javascript:0;" class="dropdown-item delete-record">' +
        //      'Delete' +
        //      '</a>' +
        //      '</div>' +
        //      '</div>'
        //    );
        //  }
        //}
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
              return 'Details of ' + data['customerName'];
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
  $('.datatables-order tbody').on('click', '.delete-record', function () {
    dt_products.row($(this).parents('tr')).remove().draw();
  });

  // Filter form control to default size
  // ? setTimeout used for multilingual table initialization
  setTimeout(() => {
    $('.dataTables_filter .form-control').addClass('ms-0');
  }, 300);
});
