/*
Name: 			Tables / Advanced - Examples
Written by: 	Okler Themes - (http://www.okler.net)
Theme Version: 	1.7.0
*/

(function($) {

	'use strict';

	var datatableInit = function() {
        var $table = $('#datatable');

		// format function for row details
		var fnFormatDetails = function( datatable, tr ) {
            var data = datatable.fnGetData(tr);

			return [
                '<div class="divTable greyGridTable">',
                    '<div class="divTableHeading">',
                        '<div class="divTableRow">',
                            '<div class="divTableHead">C&oacute;digo QR</div>',
                            '<div class="divTableHead">Correlativo</div>',
                            '<div class="divTableHead">Tipo</div>',
                            '<div class="divTableHead">Cantidad Paletizada</div>',
                            '<div class="divTableHead">Fecha</div>',
                            '<div class="divTableHead">Acciones</div>',
                        '</div>',
                    '</div>',
                    data[8],
                '</div>',
			].join('');
		};

		// insert the expand/collapse column
		var th = document.createElement( 'th' );
		var td = document.createElement( 'td' );
		td.innerHTML = '<i data-toggle class="fa fa-plus-square-o text-primary h5 m-none" style="cursor: pointer;"></i>';
		td.className = "text-center";

		$table
			.find( 'thead tr' ).each(function() {
				this.insertBefore( th, this.childNodes[0] );
			});

		$table
			.find( 'tbody tr' ).each(function() {
				this.insertBefore(  td.cloneNode( true ), this.childNodes[0] );
            });

        // initialize
        var datatable = $table.dataTable({
            aoColumnDefs: [{
                bSortable: false,
                aTargets: [0]
            }],
            aaSorting: [
                [1, 'asc']
            ],
            "ordering": true,
            "searching": true,
            "lengthChange": true,
            "language": {
                "emptyTable": "No hay registros"
            }
        });

		// add a listener
		$table.on('click', 'i[data-toggle]', function() {
			var $this = $(this),
				tr = $(this).closest( 'tr' ).get(0);

			if ( datatable.fnIsOpen(tr) ) {
				$this.removeClass( 'fa-minus-square-o' ).addClass( 'fa-plus-square-o' );
				datatable.fnClose( tr );
			} else {
				$this.removeClass( 'fa-plus-square-o' ).addClass( 'fa-minus-square-o' );
				datatable.fnOpen( tr, fnFormatDetails( datatable, tr), 'details' );
			}
		});
	};

	$(function() {
		datatableInit();
	});

}).apply(this, [jQuery]);