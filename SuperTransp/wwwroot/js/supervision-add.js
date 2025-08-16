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
const getMakesUrl = wrapper.dataset.getMakesUrl;
const getModelsUrl = wrapper.dataset.getModelsUrl;
const partnerNumber = wrapper.dataset.partnerNumber;
const publicTransportGroupRif = wrapper.dataset.publicTransportGroupRif;
const driverId = wrapper.dataset.driverId;
const publicTransportGroupId = wrapper.dataset.publicTransportGroupId;
const isTotalAccess = wrapper.dataset.isTotalAccess === "true";

const stateName = document.querySelector('[name="StateName"]').value;
const driverIdentityDocument = document.querySelector('[name="DriverIdentityDocument"]').value;

Dropzone.autoDiscover = false;
const dropzone = new Dropzone("#myDropzone", {
    url: saveFilesUrl,
    autoProcessQueue: true,
    paramName: "file",
    maxFilesize: 10,
    acceptedFiles: ".jpg,.jpeg,.png,.gif,.webp",
    dictDefaultMessage: "Arrastra las imágenes aquí o haga clic para agregarlas.",
    addRemoveLinks: true,
    maxFiles: 4,
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
    },

    init: function () {
        this.on("addedfile", function (file) {
            if (this.files.length > 4) {
                this.removeFile(file);
                alert("Solo se pueden subir hasta 4 imágenes.");
                return;
            }
        });

        this.on("removedfile", function (file) {});

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

    handleInput("[name='Remarks']", function (value) {
        return value.toUpperCase();
    });
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
                showMsg(data);
            },
            cache: false
        });
    }
});

function isOkToSave() {
    let firstInvalidField = null;
    let message = "";

    const dzInstance = Dropzone.forElement("#myDropzone");
    const uploaded = dzInstance.getAcceptedFiles().filter(f => f.status === Dropzone.SUCCESS);

    if ($("#DriverWithVehicle").val() == "" && $("#DriverWithVehicle").val() != "False" && !firstInvalidField) {
        firstInvalidField = "#DriverWithVehicle";
        message = "Debe indicar si el socio posee o no vehículo";
    }

    if ($("#DriverWithVehicle option:selected").text() == "Sí") {
        if ($("#InPerson").val() == "" && !firstInvalidField) {
            firstInvalidField = "#InPerson";
            message = "Debe indicar si el socio se presentó o no a la supervisión";
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
            message = "Debe seleccionar la cantidad de puestos";
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

        if ($("#FingerprintTrouble").val() == "" && !firstInvalidField) {
            firstInvalidField = "#FingerprintTrouble";
            message = "Debe seleccionar si tiene o no problemas con la huella";
        }

        if ($("#FailureTypeId").val() == "" && !firstInvalidField) {
            firstInvalidField = "#FailureTypeId";
            message = "Debe seleccionar si tiene o no alguna falla";
        }

        if (uploaded.length > 0) {
            var modeId = $("#ModeId").val();

            if (modeId === "4") {
                if ((uploaded.length < 2 || uploaded.length > 2) && !firstInvalidField) {
                    firstInvalidField = "#myDropzone";
                    var staticMessage = "El vehículo que está actualizando es una moto, debe agregar 2 imágenes para ese tipo de vehículo, está intentando agregar ";
                    var quantityMessage = "";

                    if (uploaded.length > 2) {
                        quantityMessage = uploaded.length + ", presione el botón: Eliminar imágenes de la lista, y agregue solo 2";
                    } else if (uploaded.length < 2) {
                        quantityMessage = uploaded.length + ", haga clic en el recuadro: Cargar imágenes, y agregue otra.";
                    }

                    message = staticMessage + quantityMessage;
                }
            } else {
                if ((uploaded.length < 4 || uploaded.length > 4) && !firstInvalidField) {
                    firstInvalidField = "#myDropzone";
                    var staticMessage = "El vehículo que está actualizando requiere de 4 imágenes para poder ser actualizado, está agregando solo ";
                    var quantityMessage = "";

                    if (uploaded.length < 4) {
                        quantityMessage = uploaded.length + ", haga clic en el recuadro: Cargar imágenes, y agregue la(s) restante(s).";
                    }

                    message = staticMessage + quantityMessage;
                }
            }
        } else {
            firstInvalidField = "#myDropzone";
            message = "Debe agregar imágenes del vehículo para poder actualizar";
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

    $('#Remarks').on('input', function () {
        const value = $(this).val();
        const sanitizedValue = value.replace(/[^a-zA-Z\s]/g, '');
        $(this).val(sanitizedValue);
    });

    setTransportTypeDefaultValues($("#ModeId").val());

    $('#DriverWithVehicle').change(function (event) {
        if ($(this).val() === "True") {
            $('#additionalFields').css({ visibility: 'visible', opacity: '1', height: 'auto', overflow: 'visible' });
            $('#InPerson').val("");
        } else if ($(this).val() === "False") {
            $('#additionalFields').css({ opacity: '0', visibility: 'hidden', height: '0', overflow: 'hidden' });
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