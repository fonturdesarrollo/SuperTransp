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

function confirmDeletion(deleteUrl) {
    if (!confirm("¿Está seguro de que desea eliminar este registro? Al hacerlo, también se borrarán los datos de supervisión (vehículos e imágenes) si están cargados.")) {
        return;
    }

    const params = new URLSearchParams(deleteUrl.split('?')[1]);
    const driverId = params.get("driverId");
    const driverPublicTransportGroupId = params.get("driverPublicTransportGroupId");
    const partnerNumber = params.get("partnerNumber");

    $.ajax({
        url: window.deleteDriverAjaxUrl,
        type: 'POST',
        data: { driverId: driverId, driverPublicTransportGroupId: driverPublicTransportGroupId, partnerNumber: partnerNumber },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                alert(response.message || 'Registro eliminado.');
                $('#inventory').DataTable().ajax.reload(null, false);
            } else {
                alert(response.message || 'Error al eliminar el conductor.');
            }
        },
        error: function () {
            alert('Error en el servidor al intentar eliminar el registro.');
        }
    });
}

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

$("#saveRequest").on("click", function (event) {
    event.preventDefault();

    if (!isOkToSave()) return false;

    if (!confirm("¿Está seguro de que desea actualizar los registros?")) return false;

    $("#saveRequest").prop("disabled", true);

    let paramValue2 = $('#PublicTransportGroupId').val();
    let paramValue4 = $('#PartnerNumber').val();

    $.ajax({
        url: window.checkExistingValuesUrl,
        type: "GET",
        data: {
            paramValue2: paramValue2,
            paramValue4: paramValue4
        },
        success: function (validationResponse) {
            if (validationResponse !== "OK") {
                alert(validationResponse);
                $("#saveRequest").prop("disabled", false);
                return;
            }

            let formData = $("form").serialize();
            formData += "&DriverId=" + (Number.isInteger(window.driverId) ? window.driverId : 0);

            $.ajax({
                url: window.addWithAjaxUrl,
                type: "POST",
                data: formData,
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        alert("Registro actualizado correctamente.");

                        $("form")[0].reset();
                        $("#DriverFullName, #DriverPhone").prop("readonly", false);
                        $("#SexId").removeClass("readonly-style").off("click");
                        $("#publicTransportGroupId").val(window.publicTransportGroupId);
                        $("#PTGCompleteName").val(window.ptgCompleteName);
                        $("#Birthdate").val("");
                        $('#inventory').DataTable().ajax.reload();
                        $("#DriverIdentityDocument").focus();
                    } else if (response.redirect) {
                        window.location.href = response.redirect;
                        return;
                    } else {
                        alert(response.message);
                    }

                    $("#saveRequest").prop("disabled", false);
                },
                error: function () {
                    alert("Error en el servidor al guardar el socio.");
                    $("#saveRequest").prop("disabled", false);
                }
            });
        },
        error: function () {
            alert("Error al validar los datos del socio.");
            $("#saveRequest").prop("disabled", false);
        }
    });
});

$("form").on("submit", function (e) {
    e.preventDefault();

    if ($(this).valid()) {
        var paramValue2 = $('#PublicTransportGroupId').val();
        var paramValue4 = $('#PartnerNumber').val();
        $.ajax({
            url: window.checkExistingValuesUrl,
            data: {
                paramValue2: paramValue2,
                paramValue4: paramValue4
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

    if ($("#DriverIdentityDocument").val() == "" && !firstInvalidField) {
        firstInvalidField = "#DriverIdentityDocument";
        message = "Debe colocar la cédula del socio";
    }

    if (!firstInvalidField) {
        const identityValue = $("#DriverIdentityDocument").val();

        if (identityValue == "" || identityValue.length < 7) {
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

    if ($("#Birthdate").val() == ""  && !firstInvalidField) {
        firstInvalidField = "#Birthdate";
        message = "Debe colocar la fecha de nacimiento";
    }

    if ($("#PartnerNumber").val() != "" &&  $("#PartnerNumber").val() == "0" && !firstInvalidField) {
        firstInvalidField = "#PartnerNumber";
        message = "El número de socio no puede ser cero";
    }

    if ($("#PartnerNumber").val() == "" && !firstInvalidField) {
        firstInvalidField = "#PartnerNumber";
        message = "Debe colocar el número de socio";
    }

    if ($("#RepresentativePhone").val() == "" && !firstInvalidField) {
        firstInvalidField = "#RepresentativePhone";
        message = "Debe colocar el número de teléfono del representante";
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

$(document).ready(function () {
    setTimeout(function () {
        $("#successMessage").fadeOut("slow");
    }, 2500);

    $("#Birthdate").val("");
    $(document).on("click", ".generateQR", function () {
        var ptgGUID = $(this).closest("tr").data("ptgguid");

        $.ajax({
            url: window.generateQRUrl,
            type: "POST",
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            contentType: "application/json",
            data: JSON.stringify({ ptgGUID: ptgGUID }),
            dataType: "json",
            success: function (response) {
                $("#qrImage").attr("src", response.qrImage);
                var myModal = new bootstrap.Modal(document.getElementById('qrModal'));
                myModal.show();
            },
            error: function () {
                alert("Error al generar el código QR.");
            }
        });
    });

    $("#DriverIdentityDocument").on("blur keypress", function (event) {
        if (event.type === "blur" || (event.type === "keypress" && event.which === 13)) {
            event.preventDefault();
            let inputValue = $(this).val();
            llamarControlador(inputValue);
        }
    });	

    function llamarControlador(value) {
        $.ajax({
            url: window.getDriverDataByIdDocumentUrl,
            type: 'POST',
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            data: { driverIdentityDocument: value },
            success: function (response) {
                if (response && response.driverFullName && response.driverPhone) {
                    window.driverId = response.driverId;
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
                    window.driverId = response.driverId;
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

    var controllerUrl = window.getDriversByPTGUrl + `?publicTransportGroupId=${window.publicTransportGroupId}`;

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
            { data: 'modificar' },
            { data: 'eliminar' },
            { data: 'qr' }
        ],
        createdRow: function (row, data, dataIndex) {
            let ptgGUID = `${data.ptgGUID}`;
            $(row).attr('data-ptgguid', ptgGUID);
        },
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