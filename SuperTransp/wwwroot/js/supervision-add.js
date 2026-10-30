function confirmDeletion(url) {
    const userConfirmed = confirm("¿Está seguro de que desea eliminar este registro?");
    if (userConfirmed) {
        window.location.href = url;
    }
}

const wrapper = document.getElementById('wrapper');
const saveFilesUrl = wrapper.dataset.saveFilesUrl;
const deletePicturesUrl = wrapper.dataset.deletePicturesUrl;
const checkPlateUrl = wrapper.dataset.checkPlateUrl;
const checkVehicleIdentifiersUrl = wrapper.dataset.checkVehicleIdentifiersUrl;
const getMakesUrl = wrapper.dataset.getMakesUrl;
const getModelsUrl = wrapper.dataset.getModelsUrl;
const partnerNumber = wrapper.dataset.partnerNumber;
const publicTransportGroupRif = wrapper.dataset.publicTransportGroupRif;
const driverId = wrapper.dataset.driverId;
const driverPublicTransportGroupId = wrapper.dataset.driverPublicTransportGroupId;
const publicTransportGroupId = wrapper.dataset.publicTransportGroupId;
const isTotalAccess = wrapper.dataset.isTotalAccess === "true";

const stateName = document.querySelector('[name="StateName"]').value;
const driverIdentityDocument = document.querySelector('[name="DriverIdentityDocument"]').value;

// Pictures required per vehicle: 3 for moto taxi (mode 4), 5 for the other modes (taxi included)
const requiredPictures = document.querySelector('[name="ModeId"]').value === "4" ? 3 : 5;

Dropzone.autoDiscover = false;
const dropzone = new Dropzone("#myDropzone", {
    url: saveFilesUrl,
    autoProcessQueue: true,
    paramName: "file",
    maxFilesize: 10,
    acceptedFiles: ".jpg,.jpeg,.png,.gif,.webp",
    dictDefaultMessage: "Arrastra las imágenes aquí o haga clic para agregarlas.",
    addRemoveLinks: true,
    maxFiles: requiredPictures,
    addRemoveLinks: false,

    headers: {
        "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
    },

    params: {
        stateName: stateName,
        driverIdentityDocument: driverIdentityDocument,
        partnerNumber: partnerNumber,
        publicTransportGroupRif: publicTransportGroupRif,
        driverId: driverId,
        driverPublicTransportGroupId: driverPublicTransportGroupId,
    },

    init: function () {
        this.on("addedfile", function (file) {
            if (this.files.length > requiredPictures) {
                this.removeFile(file);
                alert("Solo se pueden subir hasta " + requiredPictures + " imágenes.");
                return;
            }
        });

        this.on("removedfile", function (file) {});

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
            url: deletePicturesUrl,
            type: "POST",
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },

            data: {
                stateName: stateName,
                publicTransportGroupRif: publicTransportGroupRif,
                partnerNumber: partnerNumber,
                publicTransportGroupId: publicTransportGroupId,
                driverId: driverId,
                driverPublicTransportGroupId: driverPublicTransportGroupId,
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
            url: checkPlateUrl,
            data: {
                paramValue1: paramValue1,
                paramValue2: paramValue2
            },
            success: function (data) {
                if (data != "OK") {
                    showMsg(data);
                    return;
                }

                // Plate is free: now check the NIV / serials against other vehicles
                $.ajax({
                    url: checkVehicleIdentifiersUrl,
                    data: {
                        paramValue1: paramValue1,
                        paramValue2: $('#ModeId').val(),
                        niv: $('#Niv').val(),
                        bodySerial: $('#BodySerialNumber').val(),
                        engineSerial: $('#EngineSerialNumber').val(),
                        withVehicle: $("#DriverWithVehicle option:selected").text() == "Sí"
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

// N/D and N/A (any case) are placeholders, not real values: they count as empty
function isEmptySerial(selector) {
    return /^\s*$|^\s*N\s*\/\s*[DA]\s*$/i.test($(selector).val() || "");
}

function isOkToSave() {
    let firstInvalidField = null;
    let message = "";

    const dzInstance = Dropzone.forElement("#myDropzone");
    const uploaded = dzInstance.getAcceptedFiles().filter(f => f.status === Dropzone.SUCCESS);

    if ($("#DriverWithVehicle").val() == "" && $("#DriverWithVehicle").val() != "False" && !firstInvalidField) {
        firstInvalidField = "#DriverWithVehicle";
        message = "Debe indicar si el asociado posee o no vehículo";
    }

    if ($("#DriverWithVehicle option:selected").text() == "Sí") {
        if ($("#InPerson").val() == "" && !firstInvalidField) {
            firstInvalidField = "#InPerson";
            message = "Debe indicar si el asociado se presentó o no a la supervisión";
        }

        if ($("#WorkingVehicle").val() == "" && !firstInvalidField) {
            firstInvalidField = "#WorkingVehicle";
            message = "Debe indicar si el vehículo está operativo o no";
        }

        if ($("#WorkingVehicle").val() == "False" && $("#FailureTypeId").val() == "" && !firstInvalidField) {
            firstInvalidField = "#FailureTypeId";
            message = "Debe indicar si el tipo de falla";
        }

        if ($("#Plate").val() == "" && !firstInvalidField) {
            firstInvalidField = "#Plate";
            message = "Debe colocar la placa";
        }

        if ($("#Years").val() == "" && !firstInvalidField) {
            firstInvalidField = "#Years";
            message = "Debe seleccionar el año del vehículo";
        }

        if ($("#MakeId").val() == "" && !firstInvalidField) {
            firstInvalidField = "#MakeId";
            message = "Debe seleccionar la marca del vehículo";
        }

        if ($("#VehicleDataId").val() == "" && !firstInvalidField) {
            firstInvalidField = "#VehicleDataId";
            message = "Debe seleccionar el modelo del vehículo";
        }

        if ($("#Passengers").val() == "" && !firstInvalidField) {
            firstInvalidField = "#Passengers";
            message = "Debe seleccionar la cantidad de asientos";
        }

        if ($("#RimId").val() == "" && !firstInvalidField) {
            firstInvalidField = "#RimId";
            message = "Debe seleccionar el tipo de neumáticos";
        }

        if ($("#Wheels").val() == "" && !firstInvalidField) {
            firstInvalidField = "#Wheels";
            message = "Debe seleccionar la cantidad de neumáticos";
        }

        if ($("#FuelTypeId").val() == "" && !firstInvalidField) {
            firstInvalidField = "#FuelTypeId";
            message = "Debe seleccionar el tipo de combustible";
        }

        if ($("#TankCapacity").val() == "" && !firstInvalidField) {
            firstInvalidField = "#TankCapacity";
            message = "Debe seleccionar la capacidad del tanque de combustible";
        }

        if ($("#BatteryId").val() == "" && !firstInvalidField) {
            firstInvalidField = "#BatteryId";
            message = "Debe seleccionar el tipo de batería";
        }

        if ($("#NumberOfBatteries").val() == "" && !firstInvalidField) {
            firstInvalidField = "#NumberOfBatteries";
            message = "Debe seleccionar la cantidad de baterías";
        }

        if ($("#MotorOilId").val() == "" && !firstInvalidField) {
            firstInvalidField = "#MotorOilId";
            message = "Debe seleccionar el tipo de aceite";
        }

        if ($("#Liters").val() == "" && !firstInvalidField) {
            firstInvalidField = "#Liters";
            message = "Debe seleccionar los litros de aceite";
        }

        // The NIV only applies to modes 1, 2, 5 and 6 (the box is locked for the others)
        if (["1", "2", "5", "6"].includes($("#ModeId").val()) && isEmptySerial("#Niv") && !firstInvalidField) {
            firstInvalidField = "#Niv";
            message = "Debe colocar el NIV del vehículo";
        }

        // Body serial and engine serial are optional: the server saves N/D when they are empty

        if ($("#FingerprintTrouble").val() == "" && !firstInvalidField) {
            firstInvalidField = "#FingerprintTrouble";
            message = "Debe seleccionar si tiene o no problemas con la huella";
        }

        if ($("#FailureTypeId").val() == "" && !firstInvalidField) {
            firstInvalidField = "#FailureTypeId";
            message = "Debe seleccionar si tiene o no alguna falla";
        }

        if (uploaded.length > 0) {
            if (uploaded.length !== requiredPictures && !firstInvalidField) {
                firstInvalidField = "#myDropzone";
                var staticMessage = "El vehículo requiere de " + requiredPictures + " imágenes para poder ser agregado, está agregando ";
                var quantityMessage = "";

                if (uploaded.length > requiredPictures) {
                    quantityMessage = uploaded.length + ", presione el botón: Eliminar imágenes de la lista, y agregue solo " + requiredPictures + "." + " Recuerde agregar la foto del carnet o certificado de circulación.";
                } else {
                    quantityMessage = uploaded.length + ", haga clic en el recuadro: Cargar imágenes, y agregue la(s) restante(s)." + " Recuerde agregar la foto del carnet o certificado de circulación.";
                }

                message = staticMessage + quantityMessage;
            }
        } else if (!firstInvalidField) {
            firstInvalidField = "#myDropzone";
            message = "Debe agregar imágenes del vehículo para poder actualizar. Recuerde agregar la foto del carnet o certificado de circulación.";
        }

        if (firstInvalidField) {
            showAlert(message, firstInvalidField);
            return false;
        }
    }

    return true;
}

function showAlert(message, selector) {
    alert(message);
    highlightErrorField(selector);
    $(selector).focus();
}

function highlightErrorField(selector) {
    $(selector).addClass("is-invalid");
    $(selector).on("input", function () {
        $(this).removeClass("is-invalid");
    });

    $(selector)[0].scrollIntoView({ behavior: "smooth", block: "center" });
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

    $('#Plate').on('input', function () {
        const value = $(this).val();
        const sanitizedValue = value.replace(/[^a-zA-Z0-9\s]/g, '');
        $(this).val(sanitizedValue);
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

    $('#DriverWithVehicle').change(function (event) {
        if ($(this).val() === "True") {
            $('#additionalFieldsX').css({ visibility: 'visible', opacity: '1', height: 'auto', overflow: 'visible' });
            $('#InPerson').val("");
        } else if ($(this).val() === "False") {
            $('#additionalFieldsX').css({ opacity: '0', visibility: 'hidden', height: '0', overflow: 'hidden' });
            $('#InPerson').val("1");

            if (!isTotalAccess) {
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
            if (!isTotalAccess) {
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

    $('#Years').change(function () {
        var yearId = $(this).val();
        $('#MakeId').empty().append('<option value="">Seleccione</option>');
        $('#VehicleDataId').empty().append('<option value="">Seleccione</option>');

        if (yearId) {
            $.getJSON(getMakesUrl, { yearId: yearId }, function (data) {
                $.each(data, function (index, makes) {
                    $('#MakeId').append($('<option>', {
                        value: makes.vehicleDataId,
                        text: makes.make
                    }));
                });
            });
        }
    });

    $('#MakeId').change(function () {
        var yearId = $('#Years').val();
        var make = $('#MakeId option:selected').text();
        $('#VehicleDataId').empty().append('<option value="">Seleccione</option>');

        if (yearId) {
            $.getJSON(getModelsUrl, { yearId: yearId, make: make }, function (data) {
                $.each(data, function (index, models) {
                    $('#VehicleDataId').append($('<option>', {
                        value: models.vehicleDataId,
                        text: models.modelName
                    }));
                });
            });
        }
    });
});