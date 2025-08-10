// Obtener datos del contenedor
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

// Confirmación de borrado (si se usa)
function confirmDeletion(url) {
    const userConfirmed = confirm("¿Está seguro de que desea eliminar este registro?");
    if (userConfirmed) {
        window.location.href = url;
    }
}

Dropzone.autoDiscover = false;

const stateName = document.querySelector('[name="StateName"]').value;
const driverIdentityDocument = document.querySelector('[name="DriverIdentityDocument"]').value;

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

// ... (el resto de tu JS permanece igual, solo asegúrate de NO usar Razor ni variables C# aquí)

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

    setTransportTypeDefaultValues($("#ModeId").val())

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