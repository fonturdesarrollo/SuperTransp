document.addEventListener("DOMContentLoaded", function() {
    new AirDatepicker('#Birthdate', {
        dateFormat: 'dd/MM/yyyy',
        locale: {
            days: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
            daysShort: ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'],
            daysMin: ['D', 'L', 'M', 'Mi', 'J', 'V', 'S'],
            months: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
            monthsShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
            today: 'Hoy',
            clear: 'Borrar',
            dateFormat: 'dd/MM/yyyy',
            firstDay: 1
        },
        autoClose: true,
        minDate: new Date(1920, 0, 1),
        maxDate: new Date(2030, 11, 31)
    });
});

document.addEventListener("DOMContentLoaded", function() {
    var input = document.getElementById("Birthdate");
    input.addEventListener("change", function() {
        var regex = /^(0[1-9]|[12][0-9]|3[01])\/(0[1-9]|1[0-2])\/[0-9]{4}$/;
        if (!regex.test(this.value)) {
            alert("Formato de fecha inválido, debe ser DD/MM/YYYY.");
            this.value = "";
        }
    });
});

document.addEventListener("DOMContentLoaded", function () {
    function handleInput(selector, transformFunction) {
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

    handleInput("[name='DriverIdentityDocument']", function (value) {
        return value.replace(/[^0-9]/g, "");
    });

    handleInput("[name='DriverFullName']", sanitizeInputValue);

    handleInput("[name='PartnerNumber']", function (value) {
        return value.replace(/[^0-9]/g, "");
    });

    handleInput("[name='DriverPhone']", function (value) {
        return value.replace(/[^0-9]/g, "");
    });
});

$("body").on("click", "#saveRequest", function (event) {
    event.preventDefault();

    if (!isOkToSave()) return false;

    if (!confirm("¿Está seguro de que desea realizar el cambio de socio?")) return false;

    $(this).closest("form").trigger("submit");
});

$("form").on("submit", function (e) {
    e.preventDefault();

    if ($(this).valid()) {
        let paramValue2 = $('#PublicTransportGroupId').val();
        let paramValue4 = $('#PartnerNumber').val();
        let paramValue5 = $('#DriverId').val();
        let pTGCompleteName = $('#PTGCompleteName').val();

        $("#saveRequest").prop("disabled", true);

        $.ajax({
            url: window.checkExistingValuesOnEditUrl,
            data: {
                paramValue2,
                paramValue4,
                paramValue5
            },
            success: function (response) {
                if (response.redirectUrl) {
                    $("#saveRequest").prop("disabled", false);
                    showMsg(response.message || "Redireccionando...");
                    setTimeout(function () {
                        window.location.href = response.redirectUrl;
                    }, 1000);
                    return;
                }

                if (response.canContinue) {
                    let formData = $("form").serialize();

                    $.ajax({
                        url: window.changePlateWithAjaxUrl,
                        type: 'POST',
                        data: formData,
                        success: function (data) {
                            if (data.redirectUrl) {
                                showMsg(data.message || "Datos actualizados correctamente");
                                window.location.href = data.redirectUrl
                            } else {
                                showMsg(data.message || "Datos actualizados correctamente");
                                window.location.href = `${window.redirectToAddBaseUrl}?publicTransportGroupId=${paramValue2}&pTGCompleteName=${encodeURIComponent(pTGCompleteName)}`;
                            }
                        }
                    });
                } else {
                    showMsg(response.message);
                    $("#saveRequest").prop("disabled", false);
                }
            }
        });
    }
});

function isOkToSave() {
    let firstInvalidField = null;
    let message = "";

    if ($("#DriverIdentityDocument").val() == "" && !firstInvalidField) {
        firstInvalidField = "#DriverIdentityDocument";
        message = "Debe colocar la cédula del socio";
    }

    if (!firstInvalidField) {
        const identityValue = $("#DriverIdentityDocument").val();

        if (identityValue == "" || identityValue.length < 5) {
            firstInvalidField = "#DriverIdentityDocument";
            message = "Debe colocar un número de cédula válido";
        }
    }

    if ($("#DriverFullName").val() == "" && !firstInvalidField) {
        firstInvalidField = "#DriverFullName";
        message = "Debe colocar el nombre del socio";
    } else {
        const driverFullName = $("#DriverFullName").val().trim();
        const regex = /^[a-zA-ZñÑ\s]{5,}$/;

        if (!regex.test(driverFullName)) {
            if (!firstInvalidField) {
                firstInvalidField = "#DriverFullName";
            }
            message = "El nombre debe contener solo letras y espacios, y al menos 5 caracteres.";
        }
    }

    if ($("#SexId").val() == ""  &&  !firstInvalidField) {
        firstInvalidField = "#SexId";
        message = "Debe indicar el sexo";
    }

    if ($("#Birthdate").val() == ""  &&  !firstInvalidField) {
        firstInvalidField = "#Birthdate";
        message = "Debe indicar la fecha de nacimiento";
    }

    if ($("#PartnerNumber").val() != "" &&  $("#PartnerNumber").val() == "0" && !firstInvalidField) {
        firstInvalidField = "#PartnerNumber";
        message = "El número de socio no puede ser cero";
    }

    if ($("#PartnerNumber").val() == "" && !firstInvalidField) {
        firstInvalidField = "#PartnerNumber";
        message = "Debe colocar el número de socio";
    }

    if (!firstInvalidField) {
        const identityValue = $("#DriverPhone").val();

        if (identityValue == "" || identityValue.length < 11) {
            firstInvalidField = "#DriverPhone";
            message = "Debe colocar un número de télefono válido";
        }
    }

    if (firstInvalidField) {
        showAlert(message, firstInvalidField);
        return false;
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

    $(selector)[0].scrollIntoView({behavior: "smooth", block: "center" });
}

function showMsg(hasCurrentJob) {
    if (hasCurrentJob != "OK") {
        alert(hasCurrentJob);
        return false;
    } else {
        $("form").unbind('submit').submit();
    }
}

$(document).ready(function () {
    setTimeout(function () {
        $("#successMessage").fadeOut("slow");
    }, 2500);

    $("#DriverIdentityDocument").on("blur keypress", function (event) {
        if (event.type === "blur" || (event.type === "keypress" && event.which === 13)) {
            event.preventDefault();
            let inputValue = $(this).val();
            GetDriverByIdentifierIdNumber(inputValue);
        }
    });	

    function GetDriverByIdentifierIdNumber(value) {
        $.ajax({
            url: window.getDriverDataByIdDocumentUrl,
            type: 'POST',
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            data: { driverIdentityDocument: value },
            success: function (response) {
                if (response && response.driverFullName && response.driverPhone) {
                    window.newPlateDriverId = response.driverId;

                    if (response.driverId == $("#DriverId").val()) {
                        $("#saveRequest").prop("disabled", true);
                    } else {
                        $("#saveRequest").prop("disabled", false);
                    }

                    $("#NewPlateDriverId").val(response.driverId);
                    $("#DriverFullName").val(response.driverFullName);
                    $("#DriverPhone").val(response.driverPhone);
                    $("#SexId").val(response.driverSexId);
                    $("#Birthdate").val(response.driverBirthDate);

                    $("#SexId").addClass("readonly-style");
                    $("#SexId").on("click", e => e.preventDefault());

                    $("#DriverFullName").prop("readonly", true);
                    $("#DriverPhone").prop("readonly", true);
                    $("#Birthdate").prop("readonly", true);

                    $("#PartnerNumber").focus();
                }
                else {
                    window.newPlateDriverId = 0;
                    $("#NewPlateDriverId").val(0);
                    $("#saveRequest").prop("disabled", false);
                    $('#SexId').removeClass('readonly-style');
                    $("#DriverFullName").prop("readonly", false);
                    $("#DriverPhone").prop("readonly", false);

                    $("#DriverFullName").val("");
                    $("#DriverPhone").val("");

                    $("#DriverFullName").focus();
                }
            },
            error: function (xhr, status, error) {
                console.error("Error en la petición:", error);

                $("#DriverFullName").prop("readonly", false);
                $("#DriverPhone").prop("readonly", false);

                $("#DriverFullName").val("");
                $("#DriverPhone").val("");
            },
        });
    }

    var controllerUrl = window.getDriverByPTGDriverIdAndPartnerNumber +
        `?publicTransportGroupId=${window.publicTransportGroupId}&driverId=${window.driverId}&partnerNumber=${window.partnerNumber}&stateId=${window.stateId}`;

    $('#inventory').DataTable({
        ajax: {
            url: controllerUrl,
            dataSrc: 'data'
        },
        columns: [
            { data: 'nombre' },
            { data: 'cedula' },
            { data: 'socio' },
            { data: 'telefono' },
            { data: 'sexo' },
            { data: 'nacimiento' },
            { data: 'año' },
            { data: 'marca' },
            { data: 'modelo' },
            { data: 'placa' },
        ],
        stateSave: true,
        language: {
            sProcessing: "Procesando...",
            sLengthMenu: "Mostrar _MENU_ registros",
            sZeroRecords: "No se encontraron resultados",
            sEmptyTable: "Ningún dato disponible en esta tabla",
            sInfo: "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
            sInfoEmpty: "Mostrando registros del 0 al 0 de un total de 0 registros",
            sInfoFiltered: "(filtrado de un total de _MAX_ registros)",
            sSearch: "Buscar:",
            oPaginate: {
                sFirst: "Primero",
                sLast: "Último",
                sNext: "Siguiente",
                sPrevious: "Anterior"
            }
        }
    });
});