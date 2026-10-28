(function () {
	// Acceso a datos y rutas desde la vista
	const data = window.supervisionEditData || {};

	function confirmDeletion(url) {
		const userConfirmed = confirm("¿Está seguro de que desea eliminar este registro?");
		if (userConfirmed) {
			window.location.href = url;
		}
	}

	// Pictures required per vehicle: 3 for moto taxi (mode 4), 5 for the other modes (taxi included)
	const requiredPictures = document.querySelector('[name="ModeId"]').value === "4" ? 3 : 5;

	Dropzone.autoDiscover = false;
	const dropzone = new Dropzone("#myDropzone", {
		url: data.saveFilesUrl,
		autoProcessQueue: true,
		paramName: "file",
		maxFilesize: 10,
		acceptedFiles: ".jpg,.jpeg,.png,.gif,.webp",
		dictDefaultMessage: "Arrastra las imágenes aquí o haga clic para agregarlas.",
		addRemoveLinks: false,
		maxFiles: requiredPictures,
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
				if (this.files.length > requiredPictures) {
					this.removeFile(file);
					alert("Solo se pueden subir hasta " + requiredPictures + " imágenes.");
					return;
				}
				console.log("Archivo agregado:", file);
			});
			this.on("removedfile", function (file) {
				console.log("Archivo eliminado:", file);
			});
			this.on("queuecomplete", function () {
				let uploadedFiles = this.getAcceptedFiles().filter(f => f.status === Dropzone.SUCCESS);
				if (uploadedFiles.length > 0 && uploadedFiles.length <= requiredPictures) {
					alert("Todas las imágenes han sido cargadas correctamente.");
				}
			});
			this.on("error", function (file, errorMessage) {
				alert("Hubo un error al subir la imagen: " + errorMessage);
			});
		}
	});

	dropzone.hiddenFileInput.removeAttribute("multiple");

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
					driverPublicTransportGroupId: data.driverPublicTransportGroupId,
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

	$("body").on("click", "#saveRequestX", function (event) {
		event.preventDefault();
		if (confirm("¿Está seguro de que desea actualizar los registros?")) {
			$(this).closest("form")[0].submit();
		}
	});

	$("body").on("click", "#saveRequestXX", function (event) {
		event.preventDefault();
		if (confirm("¿Está seguro de que desea actualizar los registros?")) {
			$(this).closest("form")[0].submit();
		}
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
				success: function (plateResult) {
					if (plateResult != "OK") {
						showMsg(plateResult);
						return;
					}

					// Plate is free: now check the NIV / serials against other vehicles
					$.ajax({
						url: data.checkVehicleIdentifiersUrl,
						data: {
							paramValue1: paramValue1,
							paramValue2: $('#ModeId').val(),
							niv: $('#Niv').val(),
							bodySerial: $('#BodySerialNumber').val(),
							engineSerial: $('#EngineSerialNumber').val(),
							withVehicle: $("#DriverWithVehicle option:selected").text() === "Sí"
						},
						success: function (identifiersResult) {
							showMsg(identifiersResult);
						},
						cache: false
					});
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
		// N/D and N/A (any case) are placeholders, not real values: for these three boxes they count as empty
		const isSerialPlaceholder = id => ["Niv", "BodySerialNumber", "EngineSerialNumber"].includes(id) && /^\s*N\s*\/\s*[DA]\s*$/i.test(val(id) || "");
		const isEmpty = id => { const v = val(id); return v === "" || (typeof v === "string" && v.trim() === "") || isSerialPlaceholder(id); };

		const validateField = (condition, fieldId, msg) => {
			if (condition && !firstInvalidField) {
				firstInvalidField = `#${fieldId}`;
				message = msg;
			}
		};

		validateField(isEmpty("DriverWithVehicle") && val("DriverWithVehicle") !== "False", "DriverWithVehicle", "Debe indicar si el asociado posee o no vehículo");

		if ($("#DriverWithVehicle option:selected").text() === "Sí") {
			const requiredFields = [
				["InPerson", "Debe indicar si el asociado se presentó o no a la supervisión"],
				["WorkingVehicle", "Debe indicar si el vehículo está operativo o no"],
				["FailureTypeId", "Debe indicar el tipo de falla", () => val("WorkingVehicle") === "False"],
				["Plate", "Debe colocar la placa"],
				["Year", "Debe seleccionar el año del vehículo"],
				["Make", "Debe seleccionar la marca del vehículo"],
				["VehicleDataId", "Debe seleccionar el modelo del vehículo"],
				["Passengers", "Debe seleccionar la cantidad de asientos"],
				["RimId", "Debe seleccionar el tipo de neumáticos"],
				["Wheels", "Debe seleccionar la cantidad de neumáticos"],
				["FuelTypeId", "Debe seleccionar el tipo de combustible"],
				["TankCapacity", "Debe seleccionar la capacidad del tanque de combustible"],
				["BatteryId", "Debe seleccionar el tipo de batería"],
				["NumberOfBatteries", "Debe seleccionar la cantidad de baterías"],
				["MotorOilId", "Debe seleccionar el tipo de aceite"],
				["Liters", "Debe seleccionar los litros de aceite"],
				["Niv", "Debe colocar el NIV del vehículo", () => ["1", "2", "5", "6"].includes(val("ModeId"))],
				["BodySerialNumber", "Debe colocar el serial de carrocería del vehículo"],
				["EngineSerialNumber", "Debe colocar el serial de motor del vehículo"],
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
			} else if (totalPics === 0 && !firstInvalidField) {
				firstInvalidField = "#myDropzone";
				message = "Debe agregar imágenes del vehículo para poder actualizar. Recuerde agregar la foto del carnet o certificado de circulación.";
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

	// Pictures rule: the vehicle must end up with the required number of pictures.
	//  - nothing uploaded: the pictures it already has must reach the required number
	//  - uploading the full set replaces the current pictures
	//  - uploading fewer adds them to the current ones (the current pictures are kept) as long as the total is the required number
	function validateMode(modeId, uploaded, currentTotalPictures) {
		const required = requiredPictures;
		const uploadedCount = uploaded.length;
		const missing = required - currentTotalPictures;

		if (uploadedCount === 0) {
			if (currentTotalPictures >= required) return "";
			return "El vehículo requiere de " + required + " imágenes y actualmente tiene " + currentTotalPictures +
				". Haga clic en el recuadro: Cargar imágenes, y agregue " + missing + " imagen(es) adicional(es) (las actuales se conservan). Recuerde agregar la foto del carnet o certificado de circulación.";
		}

		if (uploadedCount === required) return "";
		if (missing > 0 && uploadedCount === missing) return "";

		let message = "El vehículo requiere de " + required + " imágenes, actualmente tiene " + currentTotalPictures + " y está agregando " + uploadedCount + ". ";
		if (missing > 0) {
			message += "Agregue " + missing + " imagen(es) adicional(es) (las actuales se conservan) o agregue " + required + " imágenes (las actuales serán sustituidas).";
		} else {
			message += "Agregue " + required + " imágenes (las actuales serán sustituidas).";
		}
		return message + " Recuerde agregar la foto del carnet o certificado de circulación.";
	}

	function showMsg(hasCurrentJob) {
		if (hasCurrentJob != "OK") {
			alert(hasCurrentJob);
			return false;
		} else {
			$("form").unbind('submit').submit();
		}
	}

	// Tank capacity 0 only applies to electric vehicles (FuelTypeId 6): for them 0 is the only option shown,
	// for any other fuel type 0 is hidden and the list works as usual.
	// Default value: 0 for electric, 5 for any other fuel type (applied when the fuel type is changed by the user,
	// and on load only if the capacity is empty or 0, so a saved capacity is never overwritten)
	function updateTankCapacityOptions(applyDefault) {
		const isElectric = $("#FuelTypeId").val() === "6";
		const $tank = $("#TankCapacity");

		$tank.find("option").each(function () {
			const optionValue = $(this).val();
			const hide = optionValue !== "" && (isElectric ? optionValue !== "0" : optionValue === "0");
			$(this).prop("hidden", hide).prop("disabled", hide);
		});

		if (isElectric) {
			$tank.val("0");
		} else if (applyDefault === true || $tank.val() === "0" || $tank.val() === "" || $tank.val() === null) {
			$tank.val("5");
		}
	}

	$("#FuelTypeId").on("change", function () { updateTankCapacityOptions(true); });

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

		// NIV, body serial and engine serial: always uppercase and at most 50 characters
		$('#Niv, #BodySerialNumber, #EngineSerialNumber').on('input', function () {
			const start = this.selectionStart;
			const end = this.selectionEnd;
			this.value = this.value.toUpperCase().slice(0, 50);
			if (start !== null) this.setSelectionRange(start, end);
		}).each(function () {
			this.value = this.value.toUpperCase();
			// A loaded N/D or N/A is not a valid value: show the box empty
			if (/^\s*N\s*\/\s*[DA]\s*$/.test(this.value)) this.value = "";
		});

		setTransportTypeDefaultValues($("#ModeId").val());
		updateTankCapacityOptions();

		$('#Plate').on('input', function () {
			const value = $(this).val();
			const sanitizedValue = value.replace(/[^a-zA-Z0-9\s]/g, '');
			$(this).val(sanitizedValue);
		});

		if ($('#DriverWithVehicle').val() == "True") {
			const btn = document.getElementById('saveRequestXX');
			btn.style.display = 'none';
			$('#additionalFieldsX').css({ visibility: 'visible', opacity: '1', height: 'auto', overflow: 'visible' });
			$('#additionalFields').css({ visibility: 'visible', opacity: '1', height: 'auto', overflow: 'visible' });
		} else {
			if ($('#InPerson').val() === "False") {
				$('#InPerson').val("");
			}
			const btn = document.getElementById('saveRequestXX');
			btn.style.display = '';
		}

		if ($('#InPerson').val() == "False") {
			const btn = document.getElementById('saveRequestX');
			if ($('#VehicleDataId').val() == "" || $('#VehicleDataId').val() == null) {
				$('#additionalFields').css({ opacity: '0', visibility: 'hidden', height: '0', overflow: 'hidden' });
				btn.style.display = '';
			} else {
				btn.style.display = 'none';
				$('#additionalFieldsX').css({ visibility: 'visible', opacity: '1', height: 'auto', overflow: 'visible' });
				$('#additionalFields').css({ visibility: 'visible', opacity: '1', height: 'auto', overflow: 'visible' });
			}
		} else {
			const btn = document.getElementById('saveRequestX');
			btn.style.display = 'none';
		}

		$('#DriverWithVehicle').change(function (event) {
			if ($(this).val() === "True") {
				const btn = document.getElementById('saveRequestXX');
				btn.style.display = 'none';
				$('#additionalFieldsX').css({ visibility: 'visible', opacity: '1', height: 'auto', overflow: 'visible' });
			} else if ($(this).val() === "False") {
				$('#additionalFieldsX').css({ opacity: '0', visibility: 'hidden', height: '0', overflow: 'hidden' });
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


		$('#InPerson').change(function (event) {
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
						$('#InPerson').val("");
					}
				}
			}
		});

		if ($('#WorkingVehicle').val() === "False") {
			if ($('#FailureTypeId').val() === "1") {
				$('#WorkingVehicle').val("True");
				$('#FailureTypeId').val("1").prop("disabled", true);
			}
		} else {
			$('#FailureTypeId').val("1").prop("disabled", true);
		}

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