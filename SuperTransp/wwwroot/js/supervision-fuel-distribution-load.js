// Fuel distribution load screen: cascading combos state -> municipality -> district -> gas station,
// plus edit (click a row) and delete (trash icon) of the loaded records.

var numericFields = ['#TrucksReceived', '#LitersReceived', '#LitersDispatched', '#UrbanAttended', '#SubUrbanAttended', '#TaxiAttended', '#MotoTaxiAttended'];

// Thousands separator is "." and there are no decimals; the digits are stripped again before submitting
// Maximum allowed per box (urban, suburban, taxi and moto taxi attended); other boxes only use the INT range
var maxAttended = 10000;
var limitedFields = ['#UrbanAttended', '#SubUrbanAttended', '#TaxiAttended', '#MotoTaxiAttended'];

function formatThousands(digits) {
	return digits.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}

function unformatNumber(value) {
	return String(value || '').replace(/\./g, '');
}

function numericValue(selector) {
	return unformatNumber($(selector).val());
}

function setNumericValue(selector, value) {
	var digits = unformatNumber(value);
	$(selector).val(selector === '#TrucksReceived' ? digits : formatThousands(digits));
}

function resetCombo(selector, placeholder) {
	$(selector).empty().append($('<option>').val('').text(placeholder));
}

function resetGasStations() {
	resetCombo('#GasStationId', '-- Seleccione la estación de servicio --');
}

function resetDistricts() {
	resetCombo('#DistrictId', '-- Seleccione la parroquia --');
	resetGasStations();
}

function resetMunicipalities() {
	resetCombo('#MunicipalityId', '-- Seleccione el municipio --');
	resetDistricts();
}

function fillCombo(selector, items, idField, nameField) {
	var $combo = $(selector);
	$.each(items || [], function (i, item) {
		$combo.append($('<option>').val(item[idField]).text(item[nameField]));
	});
}

// The optional "selected" arguments preselect each level (used when a row is loaded for editing)
function loadMunicipalities(stateId, selectedMunicipalityId, selectedDistrictId, selectedGasStationId) {
	resetMunicipalities();
	if (!stateId) return;

	$.getJSON(municipalitiesUrl, { stateId: stateId }, function (data) {
		fillCombo('#MunicipalityId', data, 'municipalityId', 'municipalityName');
		if (selectedMunicipalityId) {
			$('#MunicipalityId').val(selectedMunicipalityId);
			loadDistricts(selectedMunicipalityId, selectedDistrictId, selectedGasStationId);
		}
	});
}

function loadDistricts(municipalityId, selectedDistrictId, selectedGasStationId) {
	resetDistricts();
	if (!municipalityId) return;

	$.getJSON(districtsUrl, { municipalityId: municipalityId }, function (data) {
		fillCombo('#DistrictId', data, 'districtId', 'districtName');
		if (selectedDistrictId) {
			$('#DistrictId').val(selectedDistrictId);
			loadGasStations(selectedDistrictId, selectedGasStationId);
		}
	});
}

function loadGasStations(districtId, selectedGasStationId) {
	resetGasStations();
	if (!districtId) return;

	$.getJSON(gasStationsUrl, { districtId: districtId }, function (data) {
		fillCombo('#GasStationId', data, 'gasStationId', 'gasStationName');
		if (selectedGasStationId) {
			$('#GasStationId').val(selectedGasStationId);
		}
	});
}

function showAlert(message, selector) {
	alert(message);
	var $field = $(selector);
	$field.addClass('is-invalid');
	$field.on('input change', function () {
		$(this).removeClass('is-invalid');
	});
	$field.focus();
	$field[0].scrollIntoView({ behavior: 'smooth', block: 'center' });
}

function isOkToSave() {
	var required = [
		['#StateId', 'Debe seleccionar el estado'],
		['#MunicipalityId', 'Debe seleccionar el municipio'],
		['#DistrictId', 'Debe seleccionar la parroquia'],
		['#GasStationId', 'Debe seleccionar la estación de servicio'],
		['#FuelTypeId', 'Debe seleccionar el tipo de combustible']
	];

	for (var i = 0; i < required.length; i++) {
		if (!$(required[i][0]).val()) {
			showAlert(required[i][1], required[i][0]);
			return false;
		}
	}

	var trucks = numericValue('#TrucksReceived');
	if (trucks === '' || parseInt(trucks) < 1 || parseInt(trucks) > 10) {
		showAlert('La cantidad de gandolas recibidas debe estar entre 1 y 10', '#TrucksReceived');
		return false;
	}

	// A blank box is never saved as 0: the user must type the value explicitly
	var numericMessages = [
		['#LitersReceived', 'litros recibidos'],
		['#LitersDispatched', 'litros despachados'],
		['#UrbanAttended', 'urbanos atendidos'],
		['#SubUrbanAttended', 'suburbanos atendidos'],
		['#TaxiAttended', 'taxis atendidos'],
		['#MotoTaxiAttended', 'moto taxis atendidos']
	];

	for (var j = 0; j < numericMessages.length; j++) {
		var raw = numericValue(numericMessages[j][0]);
		if (raw === '') {
			showAlert('Debe ingresar la cantidad de ' + numericMessages[j][1] + ' (escriba 0 si no hubo)', numericMessages[j][0]);
			return false;
		}
		if (limitedFields.indexOf(numericMessages[j][0]) !== -1 && parseInt(raw) > maxAttended) {
			showAlert('La cantidad de ' + numericMessages[j][1] + ' no puede ser mayor a 10.000', numericMessages[j][0]);
			return false;
		}
		if (isNaN(raw) || parseInt(raw) < 0) {
			showAlert('La cantidad de ' + numericMessages[j][1] + ' debe ser un número mayor o igual a cero', numericMessages[j][0]);
			return false;
		}
	}

	return true;
}

function setEditMode(isEditing) {
	$('#saveRequest').val(isEditing ? 'Actualizar' : 'Guardar');
}

function clearForm() {
	$('#FuelDistributionId').val(0);
	$('.load-row').removeClass('selected-row');
	$('.is-invalid').removeClass('is-invalid');
	$('#FuelTypeId').val('');
	$('#TrucksRemarks, #DayRemarks').val('');
	$.each(numericFields, function (i, selector) {
		$(selector).val('');
	});
	$('#StateId').val(userStateId);
	loadMunicipalities(userStateId);
	setEditMode(false);
}

function loadRowIntoForm($row) {
	$('.load-row').removeClass('selected-row');
	$row.addClass('selected-row');

	// .attr() instead of .data() so that remarks made only of digits are not converted to numbers
	$('#FuelDistributionId').val($row.attr('data-id'));
	$('#FuelTypeId').val($row.attr('data-fuel'));
	setNumericValue('#TrucksReceived', $row.attr('data-trucks'));
	$('#TrucksRemarks').val($row.attr('data-trucks-remarks'));
	setNumericValue('#LitersReceived', $row.attr('data-liters'));
	setNumericValue('#LitersDispatched', $row.attr('data-liters-dispatched'));
	setNumericValue('#UrbanAttended', $row.attr('data-urban'));
	setNumericValue('#SubUrbanAttended', $row.attr('data-suburban'));
	setNumericValue('#TaxiAttended', $row.attr('data-taxi'));
	setNumericValue('#MotoTaxiAttended', $row.attr('data-moto'));
	$('#DayRemarks').val($row.attr('data-day-remarks'));

	var stateId = parseInt($row.attr('data-state'));
	$('#StateId').val(stateId);
	loadMunicipalities(stateId,
		parseInt($row.attr('data-municipality')) || 0,
		parseInt($row.attr('data-district')) || 0,
		parseInt($row.attr('data-station')) || 0);

	setEditMode(true);
	$('html, body').animate({ scrollTop: 0 }, 'fast');
}

function deleteLoad(fuelDistributionId) {
	$.post(deleteLoadUrl, {
		fuelDistributionId: fuelDistributionId,
		__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
	}).done(function (response) {
		if (response && response.success) {
			window.location.href = loadPageUrl;
		} else {
			alert((response && response.message) || 'No se pudo eliminar el registro');
		}
	}).fail(function () {
		alert('No se pudo eliminar el registro');
	});
}

$(document).ready(function () {
	setTimeout(function () {
		$('#successMessage').fadeOut('slow');
	}, 2500);

	$('#fuelDistributionLoadsTable').DataTable({
		responsive: true,
		order: [[0, 'desc']],
		columnDefs: [{ targets: -1, orderable: false, searchable: false }],
		language: {
			sProcessing: 'Procesando...',
			sLengthMenu: 'Mostrar _MENU_ registros',
			sZeroRecords: 'No se encontraron resultados',
			sEmptyTable: 'Ningún registro cargado en este operativo',
			sInfo: 'Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros',
			sInfoEmpty: 'Mostrando registros del 0 al 0 de un total de 0 registros',
			sInfoFiltered: '(filtrado de un total de _MAX_ registros)',
			sSearch: 'Buscar:',
			oPaginate: { sFirst: 'Primero', sLast: 'Último', sNext: 'Siguiente', sPrevious: 'Anterior' }
		}
	});

	// Numeric boxes accept digits only; trucks received is limited to 1-10
	$(numericFields.join(',')).on('input', function () {
		var el = this;
		var isTrucks = el.id === 'TrucksReceived';
		var caret = el.selectionStart;
		var before = el.value;
		// Digits located before the caret, to put it back in the same place after formatting
		var digitsBeforeCaret = before.slice(0, caret).replace(/\D/g, '').length;
		var digits = before.replace(/\D/g, '');

		if (isTrucks) {
			digits = digits.slice(0, 2);
			// Maximum 10: a digit that would exceed it is discarded
			if (digits !== '' && parseInt(digits) > 10) digits = digits.slice(0, -1);
		} else {
			digits = digits.slice(0, 10);
			// Keep it inside the database INT range (or the box limit, when it has one)
			var maxValue = limitedFields.indexOf('#' + el.id) !== -1 ? maxAttended : 2147483647;
			if (digits !== '' && parseInt(digits) > maxValue) digits = digits.slice(0, -1);
		}

		var formatted = isTrucks ? digits : formatThousands(digits);
		if (before !== formatted) {
			el.value = formatted;
			var pos = 0, seen = 0;
			while (pos < formatted.length && seen < digitsBeforeCaret) {
				if (/\d/.test(formatted[pos])) seen++;
				pos++;
			}
			el.setSelectionRange(pos, pos);
		}
	}).on('keydown', function (e) {
		if (['e', 'E', '+', '-', '.', ','].indexOf(e.key) !== -1) e.preventDefault();
	});

	$('#StateId').on('change', function () {
		loadMunicipalities(parseInt($(this).val()) || 0);
	});

	$('#MunicipalityId').on('change', function () {
		loadDistricts(parseInt($(this).val()) || 0);
	});

	$('#DistrictId').on('change', function () {
		loadGasStations(parseInt($(this).val()) || 0);
	});

	// Click on any cell of a row: load the record into the form above
	$('#fuelDistributionLoadsTable tbody').on('click', 'tr.load-row', function (e) {
		if ($(e.target).closest('.delete-load').length) return;
		loadRowIntoForm($(this));
	});

	$('#fuelDistributionLoadsTable tbody').on('click', '.delete-load', function (e) {
		e.preventDefault();
		e.stopPropagation();

		if (!confirm('¿Está seguro de que desea eliminar este registro?')) return;

		deleteLoad($(this).closest('tr').attr('data-id'));
	});

	$('#clearForm').on('click', clearForm);

	$('#saveRequest').on('click', function (e) {
		e.preventDefault();

		if (!isOkToSave()) return;

		if (!confirm('¿Está seguro de que desea guardar los datos?')) return;

		// Send plain digits (no thousands separators)
		$.each(numericFields, function (i, selector) {
			$(selector).val(numericValue(selector));
		});

		$(this).closest('form')[0].submit();
	});

	// The state of the logged user comes preselected: load its municipalities right away
	if ($('#StateId').val()) {
		loadMunicipalities(parseInt($('#StateId').val()) || 0);
	}
});
