// Modal with a Leaflet map that shows a saved or calculated route (markup: Views/PublicTransportGroup/_RouteModal.cshtml).
// Usage: RouteMapViewer.show({ title, startName, startLat, startLng, endName, endLat, endLng, midPoint1, midPoint2, geometry, distanceMeters, durationSeconds })
window.RouteMapViewer = (function () {
    var DEFAULT_TITLE = 'Ruta de la organización';

    var map = null;
    var routeLayer = null;
    var markerLayer = null;

    function ensureMap() {
        if (map) {
            return;
        }

        map = L.map('routeMap').setView([7.0, -66.0], 6);

        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(map);

        markerLayer = L.layerGroup().addTo(map);
    }

    function decodePolyline(encoded) {
        var points = [];
        var index = 0;
        var lat = 0;
        var lng = 0;

        while (index < encoded.length) {
            var result = 0;
            var shift = 0;
            var byte;

            do {
                byte = encoded.charCodeAt(index++) - 63;
                result |= (byte & 0x1f) << shift;
                shift += 5;
            } while (byte >= 0x20);

            lat += (result & 1) ? ~(result >> 1) : (result >> 1);

            result = 0;
            shift = 0;

            do {
                byte = encoded.charCodeAt(index++) - 63;
                result |= (byte & 0x1f) << shift;
                shift += 5;
            } while (byte >= 0x20);

            lng += (result & 1) ? ~(result >> 1) : (result >> 1);

            points.push([lat / 1e5, lng / 1e5]);
        }

        return points;
    }

    function formatSummary(distanceMeters, durationSeconds) {
        var km = (distanceMeters / 1000).toFixed(1).replace('.', ',');
        var totalMinutes = Math.round(durationSeconds / 60);
        var hours = Math.floor(totalMinutes / 60);
        var minutes = totalMinutes % 60;
        var time = hours > 0 ? hours + ' h ' + minutes + ' min' : minutes + ' min';

        return 'Distancia: ' + km + ' km · Tiempo estimado: ' + time;
    }

    function render(route) {
        markerLayer.clearLayers();

        if (routeLayer) {
            map.removeLayer(routeLayer);
            routeLayer = null;
        }

        [
            { label: 'Inicio', name: route.startName, lat: route.startLat, lng: route.startLng },
            { label: 'Última parada', name: route.endName, lat: route.endLat, lng: route.endLng }
        ].forEach(function (point) {
            L.marker([parseFloat(point.lat), parseFloat(point.lng)])
                .bindTooltip(point.label + ': ' + point.name)
                .addTo(markerLayer);
        });

        if (route.geometry) {
            routeLayer = L.polyline(decodePolyline(route.geometry), { color: '#667eea', weight: 5, opacity: 0.9 }).addTo(map);
            map.fitBounds(routeLayer.getBounds(), { padding: [30, 30] });
        } else {
            map.fitBounds(markerLayer.getLayers().map(function (marker) { return marker.getLatLng(); }), { padding: [60, 60] });
        }
    }

    function display(route) {
        $('#routeModalTitle').text(route.title || DEFAULT_TITLE);
        $('#routeModalStart').text(route.startName);
        $('#routeModalEnd').text(route.endName);
        $('#routeModalMid1').text(route.midPoint1 || '');
        $('#routeModalMid1Row').toggle(!!route.midPoint1);
        $('#routeModalMid2').text(route.midPoint2 || '');
        $('#routeModalMid2Row').toggle(!!route.midPoint2);
        $('#routeModalSummary').text(route.geometry ? formatSummary(route.distanceMeters, route.durationSeconds) : '');
        $('#routeModal').css('display', 'flex');

        // The map needs its container visible to measure its size
        ensureMap();
        map.invalidateSize();
        render(route);
    }

    function show(route) {
        display(route);
    }

    function close() {
        $('#routeModal').hide();
    }

    $(document).ready(function () {
        $('#btnCloseRouteModal').on('click', close);

        $('#routeModal').on('mousedown', function (event) {
            if (event.target === this) {
                close();
            }
        });

        $(document).on('keydown', function (event) {
            if (event.key === 'Escape' && $('#routeModal').is(':visible')) {
                close();
                // Pages with another modal below this one (route list) must not close it with the same key press
                event.stopImmediatePropagation();
            }
        });
    });

    return { show: show, close: close, formatSummary: formatSummary };
})();
