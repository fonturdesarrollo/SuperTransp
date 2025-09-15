const wrapper = document.getElementById('wrapper');
const checkValidRoundUrl = wrapper.dataset.checkValidRoundUrl;

$("body").on("click", "#sendRequest", function (event) {
	event.preventDefault();

	if (!isOkToSave()) {
		return false;
	}

	$(this).closest("form").trigger("submit");
});

$("body").on("click", "#sendOpenRoundRequest", function (event) {
	event.preventDefault();

	if (!isOkToSaveRound()) {
		return false;
	}

	if (!confirm("¿Está seguro de que desea actualizar los registros?")) return false;

	var $button = $(this);
	var controllerUrl = checkValidRoundUrl;
	var stateId = $("#stateId").val();
	var month = $("#monthValue").val();
	var year = $("#yearValue").val();

	$.ajax({
		url: controllerUrl,
		type: "GET",
		data: {
			stateId: stateId,
			month: month,
			year: year
		},
		success: function (response) {
			if (response.invalidMonth) {
				alert(response.message);
				return;
			}
			$button.closest("form").trigger("submit");
		},
		error: function () {
			alert("Error al validar permisos.");
		}
	});
});

document.addEventListener("DOMContentLoaded", function () {
	var inputElement = document.querySelector("[name='ptgRifName']");

	inputElement.addEventListener("input", function () {
		var value = inputElement.value;

		if (value.length > 0 && !/^[JjVv]/.test(value[0])) {
			value = "J" + value;
		}

		value = value.replace(/[^JjVv0-9]/g, "");

		if (/^[Jj]/.test(value)) {
			value = "J" + value.slice(1);
		} else if (/^[Vv]/.test(value)) {
			value = "V" + value.slice(1);
		}

		if (value.length > 1 && /^[JV]/.test(value[0])) {
			value = value[0] + value.slice(1).replace(/[JV]/gi, "");
		}

		inputElement.value = value;
	});
});

function isOkToSave() {
	let firstInvalidField = null;
	let message = "";

	const rifValue = $("#ptgRifId").val();
	if (!rifValue && !firstInvalidField) {
		firstInvalidField = "#ptgRifId";
		message = "Debe colocar el RIF";
	} else if (rifValue.includes("J") && rifValue.replace(/\D/g, "").length < 9 && !firstInvalidField) {
		firstInvalidField = "#ptgRifId";
		message = "Debe colocar un RIF válido";
	}

	if (firstInvalidField) {
		showAlert(message, firstInvalidField);
		return false;
	}

	return true;
}

function isOkToSaveRound() {
	let firstInvalidField = null;
	let message = "";

	const monthValue = $("#monthValue").val();
	const yearValue = $("#yearValue").val();
	const supervisionRoundDescription = $("#supervisionRoundDescription").val();

	if ($("#stateId").val() == ""  &&  !firstInvalidField) {
		firstInvalidField = "#stateId";
		message = "Debe indicar el estado";
	}

	if (!monthValue && !firstInvalidField) {
		firstInvalidField = "#monthValue";
		message = "Debe indicar el mes";
	}

	if (!yearValue && !firstInvalidField) {
		firstInvalidField = "#yearValue";
		message = "Debe indicar el año";
	}

	if (!supervisionRoundDescription && !firstInvalidField) {
		firstInvalidField = "#supervisionRoundDescription";
		message = "Debe indicar la descripción";
	}

	if (firstInvalidField) {
		showAlert(message, firstInvalidField);
		return false;
	}

	return true;
}

function showAlert(message, selector) {
	alert(message);

	if (selector) {
		const $field = $(selector);
		$field.addClass("is-invalid");

		$field.on("input", function () {
			$(this).removeClass("is-invalid");
		});

		setTimeout(function () {
			$field.focus();
		}, 100);
	}
}

$(document).ready(function () {
	setTimeout(function(){
		$("#successMessage").fadeOut("slow");
	}, 2500);

	$("#openModal").click(function (e) {
		e.preventDefault();
		$("#modalForm").fadeIn(function () {
			$("#ptgRifId").focus();
		});
	});

	$("#openRoundModal").click(function (e) {
		e.preventDefault();
		$("#modalRoundForm").fadeIn(function () {
			$("#txtFecha").focus();
		});
	});

	$(".close").click(function () {
		$("#modalForm").fadeOut();
		$("#modalRoundForm").fadeOut();
	});

	$(window).click(function (e) {
		if ($(e.target).is("#modalForm")) {
			$("#modalForm").fadeOut();
		}

		if ($(e.target).is("#modalRoundForm")) {
			$("#modalRoundForm").fadeOut();
		}
	});
});