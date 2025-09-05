(function () {
	// Acceso a datos y rutas desde la vista
	const data = window.supervisionEditData || {};

	function confirmDeletion(url) {
		const userConfirmed = confirm("¿Está seguro de que desea eliminar este registro?");
		if (userConfirmed) {
			window.location.href = url;
		}
	}

	Dropzone.autoDiscover = false;
	const dropzone = new Dropzone("#myDropzone", {
		url: data.saveFilesUrl,
		autoProcessQueue: true,
		paramName: "file",
		maxFilesize: 10,
		acceptedFiles: ".jpg,.jpeg,.png,.gif,.webp",
		dictDefaultMessage: "Arrastra las imágenes aquí o haga clic para agregarlas.",
		addRemoveLinks: false,
		maxFiles: 4,
		headers: {
			"RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
		},
		params: {
			stateName: document.querySelector('[name="StateName"]').value,
			driverIdentityDocument: document.querySelector('[name="DriverIdentityDocument"]').value,
			partnerNumber: data.partnerNumber,
			publicTransportGroupRif: data.publicTransportGroupRif,
			driverId: data.driverId,
			driverPublicTransportGroupId: data.driverPublicTransportGroupId,
		},
		init: function () {
			this.on("addedfile", function (file) {
				if (this.files.length > 4) {
					this.removeFile(file);
					alert("Solo se pueden subir hasta 4 imágenes.");
					return;
				}
				console.log("Archivo agregado:", file);
			});
			this.on("removedfile", function (file) {
				console.log("Archivo eliminado:", file);
			});
			this.on("queuecomplete", function () {
				let uploadedFiles = this.getAcceptedFiles().filter(f => f.status === Dropzone.SUCCESS);
				if (uploadedFiles.length > 0 && uploadedFiles.length <= 4) {
					alert("Todas las imágenes han sido cargadas correctamente.");
				}
			});
			this.on("error", function (file, errorMessage) {
				alert("Hubo un error al subir la imagen: " + errorMessage);
			});
		}
	});

	$("#btnDeleteImages").on("click", function () {
		if (confirm("¿Está seguro de que desea eliminar todas las imágenes?")) {
			$.ajax({
				url: data.deleteAllPicturesUrl,
				type: "POST",
				headers: {
					'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
				},
				data: {
					stateName: document.querySelector('[name="StateName"]').value,
					publicTransportGroupRif: data.publicTransportGroupRif,
					partnerNumber: data.partnerNumber,
					publicTransportGroupId: data.publicTransportGroupId,
					driverId: data.driverId,
				},
				success: function () {
					alert("Las imágenes fueron eliminadas correctamente.");
					if (Dropzone.instances.length > 0) {
						Dropzone.instances[0].removeAllFiles(true);
					}
				},
				error: function () {
					alert("Ocurrió un error al intentar eliminar las imágenes.");
				}
			});
		}
	});

	const fileUrls = data.fileUrls || [];
	const container = document.getElementById('thumbnail-container');

	function isImage(url) {
		return /\.(jpg|jpeg|png|gif|bmp|webp)$/i.test(url);
	}

	function getFileName(url) {
		return url.split('/').pop();
	}

	if (container && Array.isArray(fileUrls)) {
		fileUrls.forEach(url => {
			const element = document.createElement('div');
			element.style.display = 'inline-block';
			element.style.margin = '5px';
			element.style.cursor = 'pointer';

			if (isImage(url)) {
				const img = document.createElement('img');
				img.src = url;
				img.alt = 'Thumbnail';
				img.style.width = '100px';
				img.style.height = 'auto';
				element.appendChild(img);
			} else if (/\.pdf$/i.test(url)) {
				const pdfIcon = document.createElement('img');
				pdfIcon.src = data.pdfIconUrl;
				pdfIcon.alt = 'PDF';
				pdfIcon.style.width = '100px';
				pdfIcon.style.height = 'auto';
				pdfIcon.title = getFileName(url);
				element.appendChild(pdfIcon);
			}

			element.addEventListener('click', () => {
				window.open(url, '_blank');
			});

			container.appendChild(element);
		});
	}

	document.addEventListener("DOMContentLoaded", function () {
		function handleInput(selector, transformFunction) {
			var inputElement = document.querySelector(selector);
			if (inputElement) {
				inputElement.addEventListener("input", function () {
					inputElement.value = transformFunction(inputElement.value);
				});
				inputElement.addEventListener("keydown", function (event) {
					if (event.key === "Tab" || event.key === " " || event.code === "Space") {
						event.preventDefault();
					}
				});
			}
		}
		handleInput("[name='Plate']", function (value) {
			return value.toUpperCase().replace(/\s+/g, "");
		});
	});

	document.addEventListener("DOMContentLoaded", function () {
		function handleInputRemarks(selector, transformFunction) {
			const inputElement = document.querySelector(selector);
			if (inputElement) {
				inputElement.addEventListener("input", function () {
					const start = inputElement.selectionStart;
					const end = inputElement.selectionEnd;

					const transformed = transformFunction(inputElement.value);

					if (inputElement.value !== transformed) {
						inputElement.value = transformed;
						inputElement.setSelectionRange(start, end);
					}
				});
			}
		}

		handleInputRemarks("[name='Remarks']", sanitizeInputValue);
	});

	function handleSaveRequest(event) {
		if (event) {
			event.preventDefault();
		}
		if (!isOkToSave()) {
			return false;
		}
		if (!confirm("¿Está seguro que desea actualizar los registros?")) {
			return false;
		}
		$("#saveRequest").closest("form").trigger("submit");
	}

	$("body").on("click", "#saveRequest", function (event) {
		handleSaveRequest(event);
	});

	$("form").on("submit", function (e) {
		e.preventDefault();
		if ($(this).valid()) {
			var paramValue1 = $('#DriverId').val();
			var paramValue2 = $('#Plate').val();
			$.ajax({
				url: data.checkExistingPlateUrl,
				data: {
					paramValue1: paramValue1,
					paramValue2: paramValue2
				},
				success: function (data) {
					showMsg(data);
				},
				cache: false
			});
		}
	});

	$(window).on("beforeunload", function () {
		$("#loadingOverlay").show();
	});

	function isOkToSave() {
		let firstInvalidField = null;
		let message = "";
		const dz = Dropzone.forElement("#myDropzone");
		const uploaded = dz.getAcceptedFiles().filter(f => f.status === Dropzone.SUCCESS);
		const val = id => $(`#${id}`).val();
		const isEmpty = id => val(id) === "";

		const validateField = (condition, fieldId, msg) => {
			if (condition && !firstInvalidField) {
				firstInvalidField = `#${fieldId}`;
				message = msg;
			}
		};

		validateField(isEmpty("DriverWithVehicle") && val("DriverWithVehicle") !== "False", "DriverWithVehicle", "Debe indicar si el socio posee o no vehículo");

		if ($("#DriverWithVehicle option:selected").text() === "Sí") {
			const requiredFields = [
				["InPerson", "Debe indicar si el socio se presentó o no a la supervisión"],
				["WorkingVehicle", "Debe indicar si el vehículo está operativo o no"],
				["FailureTypeId", "Debe indicar el tipo de falla", () => val("WorkingVehicle") === "False"],
				["Plate", "Debe colocar la placa"],
				["Year", "Debe seleccionar el año del vehículo"],
				["Make", "Debe seleccionar la marca del vehículo"],
				["VehicleDataId", "Debe seleccionar el modelo del vehículo"],
				["Passengers", "Debe seleccionar la cantidad de puestos"],
				["RimId", "Debe seleccionar el tipo de neumáticos"],
				["Wheels", "Debe seleccionar la cantidad de neumáticos"],
				["FuelTypeId", "Debe seleccionar el tipo de combustible"],
				["TankCapacity", "Debe seleccionar la capacidad del tanque de combustible"],
				["BatteryId", "Debe seleccionar el tipo de batería"],
				["NumberOfBatteries", "Debe seleccionar la cantidad de baterías"],
				["MotorOilId", "Debe seleccionar el tipo de aceite"],
				["Liters", "Debe seleccionar los litros de aceite"],
				["FingerprintTrouble", "Debe seleccionar si tiene o no problemas con la huella"],
				["FailureTypeId", "Debe seleccionar si tiene o no alguna falla"]
			];

			requiredFields.forEach(([id, msg, cond]) => {
				validateField(isEmpty(id) && (!cond || cond()), id, msg);
			});

			const pictures = data.pictures || [];
			const totalPics = pictures.length;
			const modeId = val("ModeId");

			if (uploaded.length > 0) {
				const modeMessage = validateMode(modeId, uploaded, totalPics);
				if (modeMessage && !firstInvalidField) {
					firstInvalidField = "#myDropzone";
					message = modeMessage;
				}
			} else if (totalPics === 0) {
				firstInvalidField = "#myDropzone";
				message = "Debe agregar imágenes del vehículo para poder actualizar";
			} else {
				const modeMessage = validateMode(modeId, uploaded, totalPics);
				if (modeMessage && !firstInvalidField) {
					firstInvalidField = "#myDropzone";
					message = modeMessage;
				}
			}
		}

		if (firstInvalidField && message !== "") {
			const el = $(firstInvalidField);
			if (el.length) {
				el.addClass("is-invalid");
				el.on("input", function () {
					$(this).removeClass("is-invalid");
				});
				el[0].scrollIntoView({ behavior: "smooth", block: "center" });
				el.focus();
			}
			alert(message);
			return false;
		}
		return true;
	}

	function validateMode(modeId, uploaded, currentTotalPictures) {
		var message = "";
		if (modeId === "4") {
			if (currentTotalPictures != 2 || uploaded.length > 0) {
				if (uploaded.length < 2 || uploaded.length > 2) {
					var staticMessage = "El vehículo que está actualizando es una moto y requiere de 2 imágenes para poder ser actualizado, actualmente tiene ";
					var quantityMessage = "";
					if (uploaded.length > 2) {
						quantityMessage = uploaded.length + ", haga clic en el recuadro: Cargar imágenes, y agregue 2 imágenes (las actuales serán sustituidas)";
					} else if (uploaded.length < 2) {
						quantityMessage = uploaded.length + ", haga clic en el recuadro: Cargar imágenes, y agregue 2 imágenes (las actuales serán sustituidas).";
					}
					message = staticMessage + quantityMessage;
				}
			}
		} else {
			if (currentTotalPictures != 4 || uploaded.length > 0) {
				if (uploaded.length < 4 || uploaded.length > 4) {
					var staticMessage = "El vehículo que está actualizando requiere de 4 imágenes para poder ser actualizado, actualmente tiene ";
					var quantityMessage = "";
					if (uploaded.length < 4) {
						quantityMessage = uploaded.length + ", haga clic en el recuadro: Cargar imágenes, y agregue 4 imágenes (las actuales serán sustituidas).";
					}
					message = staticMessage + quantityMessage;
				}
			}
		}
		return message;
	}

	function showMsg(hasCurrentJob) {
		if (hasCurrentJob != "OK") {
			alert(hasCurrentJob);
			return false;
		} else {
			$("form").unbind('submit').submit();
		}
	}

	function setTransportTypeDefaultValues(modeId) {
		if (data.driverWithVehicle === 'False') {
			switch (modeId) {
				case "3":
					$('#Passengers').val("4");
					$('#Wheels').val("4");
					$('#FuelTypeId').val("1");
					$('#BatteryId').val("1");
					$('#TankCapacity').val("50");
					$('#NumberOfBatteries').val("1");
					$('#Liters').val("4");
					break;
				case "4":
					$('#Passengers').val("2");
					$('#Wheels').val("2");
					$('#FuelTypeId').val("1");
					$('#BatteryId').val("9");
					$('#TankCapacity').val("10");
					$('#NumberOfBatteries').val("1");
					$('#Liters').val("1");
					break;
			}
		}
	}

	$(document).ready(function () {
		setTimeout(function () {
			$("#successMessage").fadeOut("slow");
		}, 2500);

		$('#Plate').on('keydown', function (e) {
			if (e.key === "Enter") {
				e.preventDefault();
				return false;
			}
		});

		setTransportTypeDefaultValues($("#ModeId").val());

		$('#Plate').on('input', function () {
			const value = $(this).val();
			const sanitizedValue = value.replace(/[^a-zA-Z0-9\s]/g, '');
			$(this).val(sanitizedValue);
		});

		if ($('#DriverWithVehicle').val() == "True") {
			$('#additionalFields').css({ visibility: 'visible', opacity: '1', height: 'auto', overflow: 'visible' });
		}

		$('#DriverWithVehicle').change(function (event) {
			if ($(this).val() === "True") {
				$('#additionalFields').css({ visibility: 'visible', opacity: '1', height: 'auto', overflow: 'visible' });
			} else if ($(this).val() === "False") {
				$('#additionalFields').css({ opacity: '0', visibility: 'hidden', height: '0', overflow: 'hidden' });
				if (!data.isTotalAccess) {
					alert("No tiene permiso de modificar registros.");
				} else {
					if (confirm("¿Está seguro de que desea actualizar los registros?")) {
						$(this).closest("form")[0].submit();
					} else {
						$('#DriverWithVehicle').val("");
					}
				}
			}
		});

		$('#WorkingVehicle').change(function (event) {
			if ($(this).val() === "False") {
				$('#FailureTypeId').val("").prop("disabled", false);
			} else if ($(this).val() === "True") {
				$('#FailureTypeId').val("1").prop("disabled", true);
			}
		});

		$('#FailureTypeId').change(function (event) {
			if ($(this).val() === "1") {
				$('#WorkingVehicle').val("True");
			} else {
				$('#WorkingVehicle').val("False");
			}
		});

		$('#Year').change(function () {
			var yearId = $(this).val();
			$('#Make').empty().append('<option value="">Seleccione</option>');
			$('#VehicleDataId').empty().append('<option value="">Seleccione</option>');
			if (yearId) {
				$.getJSON(data.getMakesUrl, { yearId: yearId }, function (dataMakes) {
					$.each(dataMakes, function (index, makes) {
						$('#Make').append($('<option>', {
							value: makes.vehicleDataId,
							text: makes.make
						}));
					});
				});
			}
		});

		$('#Make').change(function () {
			var yearId = $('#Year').val();
			var make = $('#Make option:selected').text();
			$('#VehicleDataId').empty().append('<option value="">Seleccione</option>');
			if (yearId) {
				$.getJSON(data.getModelsUrl, { yearId: yearId, make: make }, function (dataModels) {
					$.each(dataModels, function (index, models) {
						$('#VehicleDataId').append($('<option>', {
							value: models.vehicleDataId,
							text: models.modelName
						}));
					});
				});
			}
		});
	});
})();