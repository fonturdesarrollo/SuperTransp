// Routes of the organization: place search (suggestions while typing), route calculation and the list of added routes.
// The map is shown by RouteMapViewer. Place search and route calculation go through the controller (window.searchPlacesUrl / window.getRouteUrl).
// Added routes live in the table rows (row data); window.appendRouteInputs writes them as Routes[i].* fields before the form is submitted.
(function () {
    var SEARCH_DELAY_MS = 350;
    var MIN_SEARCH_LENGTH = 3;

    var searchState = {
        Start: { timer: null, requestId: 0 },
        End: { timer: null, requestId: 0 }
    };

    var mapIcon = '<svg width="20" height="20" viewBox="0 0 16 16" fill="currentColor" xmlns="http://www.w3.org/2000/svg">' +
        '<path fill-rule="evenodd" d="M15.817.113A.5.5 0 0 1 16 .5v14a.5.5 0 0 1-.402.49l-5 1a.502.502 0 0 1-.196 0L5.5 15.01l-4.902.98A.5.5 0 0 1 0 15.5v-14a.5.5 0 0 1 .402-.49l5-1a.5.5 0 0 1 .196 0L10.5.99l4.902-.98a.5.5 0 0 1 .415.103zM10 1.91l-4-.8v12.98l4 .8V1.91zm1 12.98 4-.8V1.11l-4 .8v12.98zm-6-.8V1.11l-4 .8v12.98l4-.8z"/>' +
        '</svg>';

    var trashIcon = '<svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor" xmlns="http://www.w3.org/2000/svg">' +
        '<path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z"/>' +
        '<path fill-rule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z"/>' +
        '</svg>';

    function hasPoint(prefix) {
        return $('#Route' + prefix + 'Lat').val() !== '' && $('#Route' + prefix + 'Lng').val() !== '';
    }

    function hideSuggestions(prefix) {
        $('#route' + prefix + 'Suggestions').hide().empty();
    }

    function updateClearButton(prefix) {
        $('#route' + prefix + 'Clear').toggle($('#route' + prefix + 'Input').val() !== '');
    }

    // Empties the text and the selected place
    function clearPoint(prefix) {
        var state = searchState[prefix];

        clearTimeout(state.timer);
        state.requestId++;

        $('#route' + prefix + 'Input').val('').removeClass('is-invalid');
        $('#Route' + prefix + 'Name').val('');
        $('#Route' + prefix + 'Lat').val('');
        $('#Route' + prefix + 'Lng').val('');

        hideSuggestions(prefix);
        updateClearButton(prefix);
        $('#route' + prefix + 'Input').focus();
    }

    function selectPlace(prefix, place) {
        var label = place.label.substring(0, 250);

        $('#route' + prefix + 'Input').val(label);
        $('#Route' + prefix + 'Name').val(label);
        $('#Route' + prefix + 'Lat').val(place.lat.toFixed(6));
        $('#Route' + prefix + 'Lng').val(place.lng.toFixed(6));
        $('#route' + prefix + 'Input').removeClass('is-invalid');

        hideSuggestions(prefix);
        updateClearButton(prefix);
    }

    function renderSuggestions(prefix, places) {
        var $list = $('#route' + prefix + 'Suggestions').empty();

        if (places.length === 0) {
            $list.append($('<li class="route-suggestion-empty">').text('No se encontraron lugares'));
        }

        places.forEach(function (place) {
            $('<li>').text(place.label).on('mousedown', function (event) {
                // mousedown (not click) so it runs before the input loses focus
                event.preventDefault();
                selectPlace(prefix, place);
            }).appendTo($list);
        });

        $list.show();
    }

    function searchPlaces(prefix, query) {
        var state = searchState[prefix];
        var requestId = ++state.requestId;

        $.getJSON(window.searchPlacesUrl, { q: query })
            .done(function (places) {
                // Ignore answers that arrive after a newer search was already sent
                if (requestId === state.requestId) {
                    renderSuggestions(prefix, places);
                }
            })
            .fail(function () {
                if (requestId === state.requestId) {
                    $('#route' + prefix + 'Suggestions').empty()
                        .append($('<li class="route-suggestion-empty">').text('No se pudo buscar lugares, intente de nuevo'))
                        .show();
                }
            });
    }

    function setupPoint(prefix) {
        var $input = $('#route' + prefix + 'Input');

        $input.on('input', function () {
            var query = $input.val().trim();
            var state = searchState[prefix];

            // Typing again invalidates the selected place
            $('#Route' + prefix + 'Name').val('');
            $('#Route' + prefix + 'Lat').val('');
            $('#Route' + prefix + 'Lng').val('');
            updateClearButton(prefix);

            clearTimeout(state.timer);
            state.requestId++;

            if (query.length < MIN_SEARCH_LENGTH) {
                hideSuggestions(prefix);
                return;
            }

            state.timer = setTimeout(function () {
                searchPlaces(prefix, query);
            }, SEARCH_DELAY_MS);
        });

        $input.on('keydown', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
            } else if (event.key === 'Escape') {
                hideSuggestions(prefix);
            }
        });

        $('#route' + prefix + 'Clear').on('click', function () {
            clearPoint(prefix);
        });

        updateClearButton(prefix);

        $input.on('blur', function () {
            hideSuggestions(prefix);
        });
    }

    function setupMidPoint(number) {
        var $midPoint = $('#RouteMidPoint' + number);
        var $clear = $('#routeMidPoint' + number + 'Clear');

        $midPoint.on('input', function () {
            $clear.toggle($midPoint.val() !== '');
        });

        $midPoint.on('keydown', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
            }
        });

        $clear.on('click', function () {
            $midPoint.val('').trigger('input').focus();
        });

        $clear.toggle($midPoint.val() !== '');
    }

    function clearDraft() {
        ['Start', 'End'].forEach(function (prefix) {
            $('#route' + prefix + 'Input').val('').removeClass('is-invalid');
            $('#Route' + prefix + 'Name').val('');
            $('#Route' + prefix + 'Lat').val('');
            $('#Route' + prefix + 'Lng').val('');
            hideSuggestions(prefix);
            updateClearButton(prefix);
        });

        $('#RouteMidPoint1, #RouteMidPoint2').val('').trigger('input');
    }

    function hasDraftContent() {
        return $('#routeStartInput').val().trim() !== ''
            || $('#routeEndInput').val().trim() !== ''
            || $('#RouteMidPoint1').val().trim() !== ''
            || $('#RouteMidPoint2').val().trim() !== '';
    }

    // The back button cannot show a custom text through beforeunload, so while there are unsaved routes an extra history entry
    // is kept: going back first lands on it (popstate) and the confirmation is asked there.
    var backGuardPushed = false;

    function updateBackGuard() {
        if (initialRoutesSnapshot !== '' && !backGuardPushed && hasUnsavedRoutes()) {
            history.pushState({ routeGuard: true }, '');
            backGuardPushed = true;
        }
    }

    function updateTableVisibility() {
        $('#routeTableContainer').toggle($('#routeTableBody tr').length > 0);
        updateBackGuard();
    }

    // route: { startName, startLat, startLng, endName, endLat, endLng, midPoint1, midPoint2, geometry, distanceMeters, durationSeconds }
    function appendRouteRow(route) {
        var $row = $('<tr>').data('route', route);

        $('<td style="padding:8px 12px; border-bottom:1px solid #eee;">').text(route.startName).appendTo($row);
        $('<td style="padding:8px 12px; border-bottom:1px solid #eee;">').text(route.endName).appendTo($row);

        $('<td style="padding:8px; text-align:center; border-bottom:1px solid #eee;">').append(
            $('<button type="button" class="btnViewRoute" style="background:none; border:none; cursor:pointer; color:#667eea; padding:2px 6px;" title="Ver en mapa" aria-label="Ver en mapa">').html(mapIcon)
        ).appendTo($row);

        $('<td style="padding:8px; text-align:center; border-bottom:1px solid #eee;">').append(
            $('<button type="button" class="btnRemoveRoute" style="background:none; border:none; cursor:pointer; color:#c0392b; padding:2px 6px;" title="Eliminar" aria-label="Eliminar">').html(trashIcon)
        ).appendTo($row);

        $('#routeTableBody').append($row);
        updateTableVisibility();
    }

    function addRoute() {
        if (!hasPoint('Start')) {
            alert('Debe seleccionar el inicio de la ruta de la lista de sugerencias.');
            $('#routeStartInput').focus();
            return;
        }

        if (!hasPoint('End')) {
            alert('Debe seleccionar la última parada de la ruta de la lista de sugerencias.');
            $('#routeEndInput').focus();
            return;
        }

        var draft = {
            startName: $('#RouteStartName').val(),
            startLat: $('#RouteStartLat').val(),
            startLng: $('#RouteStartLng').val(),
            endName: $('#RouteEndName').val(),
            endLat: $('#RouteEndLat').val(),
            endLng: $('#RouteEndLng').val(),
            midPoint1: $('#RouteMidPoint1').val().trim(),
            midPoint2: $('#RouteMidPoint2').val().trim()
        };

        var $btn = $('#btnAddRoute');
        $btn.prop('disabled', true).text('Calculando ruta...');

        $.getJSON(window.getRouteUrl, {
            startLat: draft.startLat,
            startLng: draft.startLng,
            endLat: draft.endLat,
            endLng: draft.endLng
        })
            .done(function (route) {
                draft.geometry = route.geometry;
                draft.distanceMeters = route.distanceMeters;
                draft.durationSeconds = route.durationSeconds;

                appendRouteRow(draft);
                clearDraft();
            })
            .fail(function (xhr) {
                var message = xhr.responseJSON && xhr.responseJSON.error
                    ? xhr.responseJSON.error
                    : 'No se pudo calcular la ruta, intente de nuevo.';

                alert(message);
            })
            .always(function () {
                $btn.prop('disabled', false).text('Agregar ruta a la lista');
            });
    }

    var UNSAVED_MESSAGE = 'Se dispone a salir del módulo y aún existen datos pendientes por guardar que se van a perder, ¿está seguro?';
    var initialRoutesSnapshot = '';
    var leavingConfirmed = false;

    function routesSnapshot() {
        return JSON.stringify($('#routeTableBody tr').map(function () {
            return [$(this).data('route')];
        }).get());
    }

    function hasUnsavedRoutes() {
        return modeHasRoute() && routesSnapshot() !== initialRoutesSnapshot;
    }

    // Writes the added routes as Routes[i].* hidden fields of the form (call right before the native submit)
    window.appendRouteInputs = function (form) {
        // The routes are being saved: do not warn about leaving the page
        leavingConfirmed = true;

        $(form).find('input[name^="Routes["]').remove();

        $('#routeTableBody tr').each(function (index) {
            var route = $(this).data('route');
            var fields = {
                RouteStartName: route.startName,
                RouteStartLat: route.startLat,
                RouteStartLng: route.startLng,
                RouteEndName: route.endName,
                RouteEndLat: route.endLat,
                RouteEndLng: route.endLng,
                RouteMidPoint1: route.midPoint1,
                RouteMidPoint2: route.midPoint2,
                RouteGeometry: route.geometry,
                RouteDistanceMeters: route.distanceMeters,
                RouteDurationSeconds: route.durationSeconds
            };

            $.each(fields, function (name, value) {
                $('<input>').attr({
                    type: 'hidden',
                    name: 'Routes[' + index + '].' + name,
                    value: value == null ? '' : value
                }).appendTo(form);
            });
        });
    };

    // Taxi (3) and moto taxi (4) units are not assigned a route
    function modeHasRoute() {
        var modeId = $('#ModeId').val();
        return modeId !== '3' && modeId !== '4';
    }

    function toggleRouteSection() {
        $('.route-section').toggle(modeHasRoute());
    }

    // Returns the first missing piece of the routes as { message, selector }, or null when the routes are complete
    window.validateRouteFields = function () {
        if (!modeHasRoute()) {
            return null;
        }

        if (hasDraftContent()) {
            return { message: 'Hay una ruta sin agregar a la lista. Presione "Agregar ruta a la lista" o borre sus campos.', selector: '#btnAddRoute' };
        }

        if ($('#routeTableBody tr').length === 0) {
            return { message: 'Debe agregar al menos una ruta a la lista.', selector: '#routeStartInput' };
        }

        return null;
    };

    $(document).ready(function () {
        $('#ModeId').on('change', toggleRouteSection);

        setupPoint('Start');
        setupPoint('End');
        setupMidPoint('1');
        setupMidPoint('2');

        $('#btnAddRoute').on('click', addRoute);

        $('#routeTableBody').on('click', '.btnViewRoute', function () {
            var $row = $(this).closest('tr');

            RouteMapViewer.show($.extend({ title: 'Ruta ' + ($row.index() + 1) }, $row.data('route')));
        });

        $('#routeTableBody').on('click', '.btnRemoveRoute', function () {
            $(this).closest('tr').remove();
            updateTableVisibility();
        });

        // Saved routes (edit): same shape the map viewer uses
        (window.initialRoutes || []).forEach(function (route) {
            appendRouteRow({
                startName: route.RouteStartName,
                startLat: route.RouteStartLat,
                startLng: route.RouteStartLng,
                endName: route.RouteEndName,
                endLat: route.RouteEndLat,
                endLng: route.RouteEndLng,
                midPoint1: route.RouteMidPoint1,
                midPoint2: route.RouteMidPoint2,
                geometry: route.RouteGeometry,
                distanceMeters: route.RouteDistanceMeters,
                durationSeconds: route.RouteDurationSeconds
            });
        });

        toggleRouteSection();
        initialRoutesSnapshot = routesSnapshot();

        // Links (menu): custom confirmation. Capture phase so it runs before any handler of the menu
        document.addEventListener('click', function (event) {
            var link = event.target.closest ? event.target.closest('a[href]') : null;

            if (!link) {
                return;
            }

            var href = link.getAttribute('href');

            if (!href || href.charAt(0) === '#' || href.toLowerCase().indexOf('javascript:') === 0 || link.target === '_blank') {
                return;
            }

            if (hasUnsavedRoutes() && !leavingConfirmed) {
                if (confirm(UNSAVED_MESSAGE)) {
                    leavingConfirmed = true;
                } else {
                    event.preventDefault();
                    event.stopImmediatePropagation();
                }
            }
        }, true);

        // Back button
        window.addEventListener('popstate', function () {
            if (!backGuardPushed || leavingConfirmed) {
                return;
            }

            if (!hasUnsavedRoutes()) {
                // Nothing pending anymore: skip the extra history entry
                backGuardPushed = false;
                history.back();
            } else if (confirm(UNSAVED_MESSAGE)) {
                leavingConfirmed = true;
                backGuardPushed = false;
                history.back();
            } else {
                history.pushState({ routeGuard: true }, '');
            }
        });

        // Reload or closing the tab: the browser only allows its own generic text here
        $(window).on('beforeunload', function (event) {
            if (hasUnsavedRoutes() && !leavingConfirmed) {
                event.preventDefault();
                event.returnValue = '';
                return '';
            }
        });
    });
})();
