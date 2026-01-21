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

    $(this).closest("form").trigger("submit");
});

$("form").on("submit", function (e) {
    e.preventDefault();

    if ($(this).valid()) {
        var paramValue1 = $('#PublicTransportGroupRif').val();
        $("#saveRequest").prop("disabled", true);
        $.ajax({
            url: window.checkExistingUrl,
            data: {
                paramValue1: paramValue1,
            },
            success: function (data) {
                showMsg(data);
                $("#saveRequest").prop("disabled", false);
            },
            cache: false
        });
    }
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
});