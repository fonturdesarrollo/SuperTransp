document.addEventListener("DOMContentLoaded", function () {
    var inputElement = document.querySelector("[name='PublicTransportGroupRif']");
    if (inputElement) {
        inputElement.addEventListener("input", function () {
            var value = inputElement.value;

            if (value.length > 0 && !/^[JjVvCc]/.test(value[0])) {
                value = "J" + value;
            }

            value = value.replace(/[^JjVvCc0-9]/g, "");

            if (/^[Jj]/.test(value)) {
                value = "J" + value.slice(1);
            } else if (/^[Vv]/.test(value)) {
                value = "V" + value.slice(1);
            } else if (/^[Cc]/.test(value)) {
                value = "C" + value.slice(1);
            }

            if (value.length > 1 && /^[JVC]/.test(value[0])) {
                value = value[0] + value.slice(1).replace(/[JVC]/gi, "");
            }

            inputElement.value = value;
        });
    }
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

    handleInput("[name='PublicTransportGroupName']", sanitizeInputValue);

    handleInput("[name='RepresentativeName']", sanitizeInputValue);

    handleInput("[name='RepresentativeIdentityDocument']", function (value) {
        return value.replace(/[^0-9]/g, "");
    });

    handleInput("[name='RepresentativePhone']", function (value) {
        return value.replace(/[^0-9]/g, "");
    });

    handleInput("[name='Partners']", function (value) {
        return value.replace(/[^0-9]/g, "");
    });
});

$("body").on("click", "#saveRequest", function (event) {
    event.preventDefault();

    if (!isOkToSave()) {
        return false;
    }

    if (!confirm("¿Está seguro de que desea actualizar los registros?")) {
        return false;
    }

    var $btn = $(this);
    var nativeForm = $btn.closest("form")[0];
    var paramValue1 = $('#PublicTransportGroupRif').val();
    var paramValue2 = $('#PublicTransportGroupId').val();
    var paramValue3 = $('#Partners').val();

    $btn.prop("disabled", true);

    $.ajax({
        url: window.checkExistingUrl,
        data: {
            paramValue1: paramValue1,
            paramValue2: paramValue2,
            paramValue3: paramValue3,
        },
        success: function (data) {
            $btn.prop("disabled", false);
            if (data !== "OK") {
                alert(data);
                return;
            }
            // Inyectar GasStationIds antes del submit nativo
            $('input[name="GasStationIds"]').remove();
            $('#gasStationTableBody tr').each(function () {
                var id = $(this).data('gasStationId');
                $('<input>').attr({
                    type: 'hidden',
                    name: 'GasStationIds',
                    value: id
                }).appendTo(nativeForm);
            });
            nativeForm.submit();
        },
        cache: false
    });
});

function isOkToSave() {
    let firstInvalidField = null;
    let message = "";
    const isTotalAccess = window.isTotalAccess;

    if (!isTotalAccess) {
        showAlert("No tiene permiso de modificar registros.", null);
        return false;
    }

    const rifValue = $("#PublicTransportGroupRif").val();
    if (!rifValue && !firstInvalidField) {
        firstInvalidField = "#PublicTransportGroupRif";
        message = "Debe colocar el RIF";
    } else if (rifValue.includes("J") && rifValue.replace(/\D/g, "").length < 9 && !firstInvalidField) {
        firstInvalidField = "#PublicTransportGroupRif";
        message = "Debe colocar un RIF válido";
    }

    const typedName = $("#designationInput").val();
    const matchedItem = window.designations.find(item => item.label === typedName);
    if (!matchedItem && !firstInvalidField) {
        firstInvalidField = "#designationInput";
        message = "Debe seleccionar una entidad legal válida de la lista.";
    }

    const orgName = $("#PublicTransportGroupName").val().trim();
    if (!orgName && !firstInvalidField) {
        firstInvalidField = "#PublicTransportGroupName";
        message = "Debe colocar el nombre de la organización";
    } else {
        const regex = /^[a-zA-ZñÑ0-9\s]{5,}$/;
        if (!regex.test(orgName) && !firstInvalidField) {
            firstInvalidField = "#PublicTransportGroupName";
            message = "El nombre debe contener solo letras, números y espacios, y al menos 5 caracteres.";
        }
        const inputUpper = orgName.toUpperCase();
        if (inputUpper.startsWith(typedName)) {
            firstInvalidField = "#PublicTransportGroupName";
            message = "Seleccione la entidad legal desde la lista, no la escriba en el nombre.";
        }
    }

    const requiredDropdowns = [
        { id: "#StateId", msg: "Debe seleccionar el estado" },
        { id: "#MunicipalityId", msg: "Debe seleccionar el municipio" },
        { id: "#ModeId", msg: "Debe seleccionar la modalidad" },
        { id: "#UnionId", msg: "Debe seleccionar el gremio o sindicato" },
    ];

    for (const field of requiredDropdowns) {
        if ($(field.id).val() === "" && !firstInvalidField) {
            firstInvalidField = field.id;
            message = field.msg;
        }
    }

    const identityDoc = $("#RepresentativeIdentityDocument").val();
    if ((!identityDoc || identityDoc.length < 7) && !firstInvalidField) {
        firstInvalidField = "#RepresentativeIdentityDocument";
        message = "Debe colocar un número de cédula válido";
    }

    const repName = $("#RepresentativeName").val().trim();
    if (!repName && !firstInvalidField) {
        firstInvalidField = "#RepresentativeName";
        message = "Debe colocar el nombre del representante";
    } else {
        const regex = /^[a-zA-ZñÑ\s]{5,}$/;
        if (!regex.test(repName) && !firstInvalidField) {
            firstInvalidField = "#RepresentativeName";
            message = "El nombre debe contener solo letras y espacios, y al menos 5 caracteres.";
        }
    }

    const phone = $("#RepresentativePhone").val();
    if ((!phone || phone.length < 11) && !firstInvalidField) {
        firstInvalidField = "#RepresentativePhone";
        message = "Debe colocar un número de teléfono válido";
    }

    const partners = $("#Partners").val();
    if (partners === "" && !firstInvalidField) {
        firstInvalidField = "#Partners";
        message = "Debe colocar el cupo";
    } else if (partners === "0" && !firstInvalidField) {
        firstInvalidField = "#Partners";
        message = "El cupo no puede ser cero";
    }

    const gasStationCount = $('#gasStationTableBody tr').length;
    if (gasStationCount === 0 && !firstInvalidField) {
        firstInvalidField = "#GasStationId";
        message = "Debe agregar al menos una estación de combustible a la tabla";
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

        setTimeout(() => {
            $field[0].scrollIntoView({ behavior: "smooth", block: "center" });
            $field.focus();
        }, 100);
    }
}

function highlightErrorField(selector) {
    $(selector).addClass("is-invalid");
    $(selector).on("input", function () {
        $(this).removeClass("is-invalid");
    });

    $(selector)[0].scrollIntoView({ behavior: "smooth", block: "center" });
}

$(document).ready(function () {
    setTimeout(function () {
        $("#successMessage").fadeOut("slow");
    }, 2500);

    $('#StateId').change(function () {
        var stateId = $(this).val();

        $('#MunicipalityId').empty().append('<option value="">Seleccione un municipio</option>');
        $('#UnionId').empty().append('<option value="">Seleccione un gremio</option>');

        if (stateId) {
            $.getJSON(window.getMunicipalityUrl, { stateId: stateId }, function (data) {
                $.each(data, function (index, municipality) {
                    $('#MunicipalityId').append($('<option>', {
                        value: municipality.municipalityId,
                        text: municipality.municipalityName
                    }));
                });
            });

            $.getJSON(window.getUnionUrl, { stateId: stateId }, function (data) {
                $.each(data, function (index, union) {
                    $('#UnionId').append($('<option>', {
                        value: union.unionId,
                        text: union.unionName
                    }));
                });
            });
        }
    });

    $("#generateQR").click(function () {
        var ptgGUID = $('#PublicTransportGroupGUID').val();

        $.ajax({
            url: window.generateQRUrl,
            type: "POST",
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            contentType: "application/json",
            data: JSON.stringify({ ptgGUID: ptgGUID }),
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

    $("#designationInput").autocomplete({
        source: window.designations,
        minLength: 0,
        autoFocus: true,
        select: function (event, ui) {
            $("#designationInput").val(ui.item.label);
            $("#DesignationId").val(ui.item.value);
            return false;
        }
    });

    $("#designationInput").on("focus click", function () {
        $(this).autocomplete("search", "");
    });

    $("#designationInput").on("input", function () {
        const typed = $(this).val();
        const match = window.designations.find(item => item.label === typed);
        if (!match) {
            $("#DesignationId").val("");
        }
    });

    var trashIcon = '<svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor" xmlns="http://www.w3.org/2000/svg">' +
        '<path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z"/>' +
        '<path fill-rule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z"/>' +
        '</svg>';

    // Precargar estaciones existentes de la organización
    if (window.initialGasStations && window.initialGasStations.length > 0) {
        $.each(window.initialGasStations, function (i, station) {
            var row = '<tr data-gas-station-id="' + station.GasStationId + '">' +
                '<td style="padding:8px 12px; border-bottom:1px solid #eee;">' + (station.MunicipalityName || '') + '</td>' +
                '<td style="padding:8px 12px; border-bottom:1px solid #eee;">' + station.GasStationName + '</td>' +
                '<td style="padding:8px; text-align:center; border-bottom:1px solid #eee;">' +
                '<button type="button" class="btnRemoveStation" style="background:none; border:none; cursor:pointer; color:#c0392b; padding:2px 6px;" title="Eliminar">' +
                trashIcon + '</button>' +
                '</td></tr>';
            $('#gasStationTableBody').append(row);
        });
        $('#gasStationTableContainer').show();
    }

    $('#btnAddGasStation').on('click', function () {
        var stationId = $('#GasStationId').val();
        var stationName = $('#GasStationId option:selected').text();

        if (!stationId) {
            alert('Debe seleccionar una estación de la lista.');
            return;
        }

        var duplicate = false;
        $('#gasStationTableBody tr').each(function () {
            if ($(this).data('gasStationId') == stationId) {
                duplicate = true;
                return false;
            }
        });

        if (duplicate) {
            alert('Esta estación ya fue agregada.');
            return;
        }

        var municipalityName = '';
        if (window.gasStationsFull) {
            var found = window.gasStationsFull.find(function (s) { return s.GasStationId == stationId; });
            if (found) municipalityName = found.MunicipalityName || '';
        }

        var row = '<tr data-gas-station-id="' + stationId + '">' +
            '<td style="padding:8px 12px; border-bottom:1px solid #eee;">' + municipalityName + '</td>' +
            '<td style="padding:8px 12px; border-bottom:1px solid #eee;">' + stationName + '</td>' +
            '<td style="padding:8px; text-align:center; border-bottom:1px solid #eee;">' +
            '<button type="button" class="btnRemoveStation" style="background:none; border:none; cursor:pointer; color:#c0392b; padding:2px 6px;" title="Eliminar">' +
            trashIcon + '</button>' +
            '</td></tr>';

        $('#gasStationTableBody').append(row);
        $('#gasStationTableContainer').show();
        $('#GasStationId').val('');
    });

    $(document).on('click', '.btnRemoveStation', function () {
        $(this).closest('tr').remove();
        if ($('#gasStationTableBody tr').length === 0) {
            $('#gasStationTableContainer').hide();
        }
    });
});