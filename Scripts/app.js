/// <reference path="mapstyles.js" />
//-------------------
// Set global variables
var map;
var bounds;
var mapOptions;
var isIdle = false;
var tilesLoaded = false;
var locationsLoaded = false;
var mainContainerId = "#widgetsdpd-pickuppoints-map-container";
var mapLoadingId = "#map-loading-window";
var targetDiv = "#widgetsdpd-pickuppoints-map-container";
var pickupLocationsContainer = "#pickup-locations .show-for-medium-up";
var errorWrapper = "#pickup-notification";
var errorContainer = ".inform-message-wrap";
var mapTargetDiv = "#dpd-map";
var infoWindow = null;
var defaultZoom = 12; // The minimum zoom level
var mapElementId = 'dpd-map'
var mapElement = document.getElementById(mapElementId);
var globalAjaxResult;
var locations = []; // Locations Array
var pickupPointsRankedList = []; // Pickuppoints List Array
var markers = [];
var errorMessages = [];
var useGeocoding = false; // true: geocode marker from street address, false: set markers using lat/lng
// Set marker attributes. If you need unique values for each marker,
// you can update the values directly in the locations array.
var markerWidth = 24;
var markerHeight = 44;
var markerScale = 1; // Scale the image, if you can't control the source file.

var even_odd = false;
var locationCount;
var locationsContentCount;

var mapStyle = '';

function loadGoogleMapsApiReady() {
    $("body").trigger("gmap_loaded");
}

var widgetsdpd = {

    autoCompleteSearch: function () {

        var postalCodeData = $('#postal_code').val();
        var countryCodeData = $('#country').val();

        $('.ajax-loading-window').show();

        $.ajax({
            cache: false,
            url: '/DPDShipToShop/PickupPointsMapJson',
            type: "POST",
            data: { postalCode: postalCodeData, countryCode: countryCodeData },
            dataType: "json",
            success: function (data) {

                if (data.error) {
                    $('.ajax-loading-window').hide();
                    handleErrors(data);
                } else {
                    //Clear the markers
                    deleteMarkers();
                    globalAjaxResult = data;

                    renderMap();
                    $("#pickup-locations-container").animate({
                        scrollTop: 0
                    }, 'fast');
                    $('.ajax-loading-window').hide();
                }

            },
            error: function (data) {
                $(targetDiv).html("There was an error...");
            },
            failure: function (data) {
                $(targetDiv).html("There was a failure...");
            }
        });
    },

    searchAddressChange: function () {


        var IS_UK_POSTCODE_REGEX = /^(GIR[ ]?0AA)|([a-zA-Z]{1,2}(([0-9]{1,2})|([0-9][a-zA-Z]))[ ]?[0-9][a-zA-Z]{2})$/;

        var postalCodeData = $('#sender_postcode').val();
        var countryCodeData = $('#country').val();

        if (!postalCodeData) {
            event.preventDefault();
            //errorContainer.html("Please enter a postcode").show();
            //displayBarNotification("Please enter a postcode", 'error', 2000);
            ShowPostcodeNotification("Please enter a postcode", 'error', 2000);
            return false;
        }

        if (postalCodeData && !IS_UK_POSTCODE_REGEX.test(postalCodeData)) {
            event.preventDefault();
            //errorContainer.html("Please enter a valid postcode").show();
            //displayBarNotification("Please enter a valid postcode", 'error', 2000);
            ShowPostcodeNotification("Please enter a valid postcode", 'error', 2000);
            return false;
        }

        $('.ajax-loading-window').show();

        $.ajax({
            cache: false,
            url: '/DPDShipToShop/PickupPointsMapJson',
            type: "POST",
            data: { postalCode: postalCodeData, countryCode: countryCodeData },
            dataType: "json",
            success: function (data) {

                if (data.error) {
                    $('.ajax-loading-window').hide();
                    handleErrors(data);
                } else {
                    //clear the markers
                    deleteMarkers();
                    globalAjaxResult = data;

                    renderMap();
                    $("#pickup-locations-container").animate({
                        scrollTop: 0
                    }, 'fast');
                    $('.ajax-loading-window').hide();
                }

            },
            error: function (data) {
                $(targetDiv).html("There was an error...");
            },
            failure: function (data) {
                $(targetDiv).html("There was a failure...");
            }
        });

        event.preventDefault();
        return false;
    },

    initMap: function () {

        //Load the external map styles
        $.get('/Plugins/Widgets.DPDShipToShop/Scripts/mapstyles.js', function (data) {

            // Customize look of the map.
            // https://www.mapbuildr.com/
            //mapStyle = JSON.parse(data);

            mapOptions = {
                zoom: defaultZoom,
                zoomControl: true,
                zoomControlOptions: {
                    style: google.maps.ZoomControlStyle.SMALL,
                },
                disableDoubleClickZoom: false,
                mapTypeControl: false,
                panControl: false,
                scaleControl: false,
                scrollwheel: false,
                streetViewControl: false,
                draggable: true,
                overviewMapControl: false,
                mapTypeId: google.maps.MapTypeId.ROADMAP/*,
                styles: mapStyle*/
            }

            // Create new map object
            map = new google.maps.Map(document.getElementById('dpd-map'), mapOptions);

            renderMap();
        });

    },

    reinitMap: function () {
        $(mapLoadingId).hide();
        $(mainContainerId).show();
        $('#pickup-locations-container').height($(mapTargetDiv).height() - $('.widgetsdpd-pickup-showpickuplocationsranked .search').innerHeight());

        $('.pickup-location:first-of-type').find('.pickup-location-details').slideDown('fast');
        var center = map.getCenter();
        google.maps.event.trigger(map, "resize");
        map.setCenter(center);
        map.fitBounds(bounds, 0);
        var currentZoom = map.getZoom();
        if (currentZoom > mapOptions.zoom) {
            map.setZoom(mapOptions.zoom);
        }
    },

    initAutocomplete: function () {

        const input = document.querySelector('[id=sender_postcode]');
        const europeBounds = { north: 71.38619630099946, south: 34.22199866164511, west: -11.216666773880648, east: 40.92464277524735 };

        const options = {
            bounds: europeBounds,
            strictBounds: true,
            types: ['(regions)']
        };

        const autocomplete = new google.maps.places.Autocomplete(
            input,
            options
        );

        autocomplete.addListener('place_changed', () => {

            var componentForm = {
                country: 'short_name',
                postal_code: 'short_name'
            };

            const place = autocomplete.getPlace();

            for (var i = 0; i < place.address_components.length; i++) {
                var addressType = place.address_components[i].types[0];
                if (componentForm[addressType]) {
                    var val = place.address_components[i][componentForm[addressType]];
                    document.getElementById(addressType).value = val;
                }
            }

            widgetsdpd.autoCompleteSearch();
        });
    },

    selectPickupLocation: function () {

    }
}


// Init the map
function renderMap() {

    createPickupRankedList();

    locationsContentCount = 1;

    $.each(JSON.parse(globalAjaxResult.results), function (i, item) {

        content = [
            item.PickupLocationOrganisation,
            item.PickupLocationStreet,
            item.PickupLocationLocality,
            item.PickupLocationTown,
            item.PickupLocationCounty,
            item.PickupLocationPostcode,
            item.PickupLocationLatitude,
            item.PickupLocationLongitude,
            {
                // Marker icon config object
                url: 'Plugins/Widgets.DPDShipToShop/Themes/DefaultClean/Content/images/markers/dpd_pickup_map-pin_' + locationsContentCount + '.fw.png',
                size: new google.maps.Size(markerWidth, markerHeight),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(markerWidth * (markerScale / 2), markerHeight * markerScale),
                scaledSize: new google.maps.Size(markerWidth * markerScale, markerHeight * markerScale)
            },
            new google.maps.Size((markerWidth * (markerScale / 4)) * -1, markerHeight * markerScale),
        ],

            locationsContentCount++;
        //Push the content to the array
        locations.push(content);
    });


    // Create new map object
    //map = new google.maps.Map(mapElement, mapOptions);


    // OPTIONAL: Set listener to tell when map is idle
    // Can be useful during dev
    google.maps.event.addListener(map, "idle", function () {
        //console.log("map is idle");
    });


    //Tiles are loaded
    google.maps.event.addListener(map, 'tilesloaded', function () {
        //console.log("tiles are loaded");
    });

    var geocoder = new google.maps.Geocoder();
    bounds = new google.maps.LatLngBounds();
    locationCount = 0;

    // Init InfoWindow and leave it
    // for use when user clicks marker
    infoWindow = new google.maps.InfoWindow({ content: "Loading content..." });

    // Loop through locations and set markers
    for (i = 0; i < locations.length; i++) {

        // street+city,state+zip
        var address = locations[i][1] + ', ' + locations[i][2] + ',' + locations[i][3] + '+' + locations[i][4];
        //Get latitude and longitude from address
        if (useGeocoding) {
            geocoder.geocode({ 'address': address }, onGeocodeComplete(i));
        } else {
            setMarkers(i);
        }

    }

    // Re-center map on window resize
    google.maps.event.addDomListener(window, 'resize', function () {
        var center = map.getCenter();
        google.maps.event.trigger(map, "resize");
        map.setCenter(center);
    });

    $("#pickup-locations-container").scrollTop(300);

}
// END init()

// Create the individual pickup location markers
function createpickupPointsLocations() {

    $.each(JSON.parse(globalAjaxResult.results), function (i, item) {
        content = [
            "" + item.PickupLocationOrganisation + "",
            "",
            "" + item.PickupLocationStreet + "",
            "" + item.PickupLocationTown + "",
            "" + item.PickupLocationCounty + "",
            "" + item.PickupLocationPostcode + "",
            "" + item.PickupLocationLatitude + "",
            "" + item.PickupLocationLogitude + "",
            {
                url: 'Plugins/Widgets.DPDShipToShop/Themes/DefaultClean/Content/images/markers/dpd_pickup_map-pin_' + locationsCount + '.fw.png',
                size: new google.maps.Size(markerWidth, markerHeight),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(markerWidth * (markerScale / 2), markerHeight * markerScale),
                scaledSize: new google.maps.Size(markerWidth * markerScale, markerHeight * markerScale)
            },
            new google.maps.Size((markerWidth * (markerScale / 4)) * -1, markerHeight * markerScale),
        ],

            locationsCount++;
        //Push the content to the array
        locations.push(content);
    });

}
//END Create the individual pickup location markers

// Triggered as the geocode callback
function onGeocodeComplete(i) {

    // Callback function for geocode on response from Google.
    // We wrap it in 'onGeocodeComplete' so we can send the
    // location index through to the marker to establish
    // content.
    var geocodeCallBack = function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {

            // The HTML content for the InfoWindow.
            // Includes a form to allow the user to
            // get directions.
            var windowContent =
                '<p><strong>' + locations[i][0] + '</strong><br>' +
                locations[i][1] + ', ' + locations[i][2] + '<br>' +
                locations[i][3] + ', ' + locations[i][4] + ' ' + locations[i][5] + '</p>';

            // Create the marker for the location
            // We use 'html' key to attach the
            // InfoWindow content to the marker.
            var marker = new google.maps.Marker({
                icon: locations[i][8],
                position: results[0].geometry.location,
                map: map,
                window_offset: locations[i][9],
                html: windowContent
            });

            markers.push(marker);

            // Set event to display the InfoWindow anchored
            // to the marker when the marker is clicked.
            google.maps.event.addListener(marker, 'click', function () {
                showInfoWindow(this);
            });

            // Add this marker to the map bounds
            extendBounds(results[0].geometry.location);

        } else {
            window.log('Location geocoding has failed: ' + google.maps.GeocoderStatus);

            // Hide empty map element on error
            mapElement.style.display = 'none';
        }
    } // END geocodeCallBack()

    return geocodeCallBack;

}
// END onGeocodeComplete()

// Using Lat / lng
function setMarkers(i) {

    // create map position
    var position = new google.maps.LatLng(parseFloat(locations[i][6]), parseFloat(locations[i][7]));

    // Create the marker for the location
    // We use 'html' key to attach the
    // InfoWindow content to the marker.
    var marker = new google.maps.Marker({
        icon: locations[i][8],
        position: position,
        map: map,
        window_offset: locations[i][9],
        html: getInfoWindowContent(i)
    });

    markers.push(marker);

    // Set event to display the InfoWindow anchored
    // to the marker when the marker is clicked.
    google.maps.event.addListener(marker, 'click', function () {
        showInfoWindow(this);
        scrollToSummary(i + 1)
    });

    google.maps.event.addListener(marker, 'dblclick', function () {
        //map.setZoom(8);
        //map.setCenter(marker.getPosition());
    });

    // Add this marker to the map bounds
    extendBounds(position);

}
//END Using Lat / lng

// The HTML content for the InfoWindow.
function getInfoWindowContent(i) {
    var windowContent = '<p><strong>' + locations[i][0] + '</strong><br>\
		' + locations[i][1] + ', ' + locations[i][2] + '<br>\
		' + locations[i][3] + ', ' + locations[i][4] + ' ' + locations[i][5] + '</p>';
    return windowContent;
}
// END The HTML content for the InfoWindow.

// Show info window marker
function showInfoWindow(marker) {
    // Updates the InfoWindow content with
    // the HTML held in the marker ('this').
    infoWindow.setOptions({
        content: marker.html,
        pixelOffset: marker.window_offset

    });
    infoWindow.open(map, marker);
}
// END Show info window marker

// Establishes the bounds for all the markers
// then centers and zooms the map to show all.
function extendBounds(latlng) {
    ++locationCount;

    bounds.extend(latlng);

    if (locationCount == locations.length) {
        map.fitBounds(bounds, 0);
        var currentZoom = map.getZoom();
        if (currentZoom > mapOptions.zoom) {
            map.setZoom(mapOptions.zoom);
        }
    }
}
// END extendBounds()

//Create the individual pickup locations and add them to the pickupPoints array, then display on page when ready
function createPickupRankedList() {

    itemCount = 1;

    const selectButtonText = document.getElementById('select_button_text').value;
    const open_times_heading = document.getElementById('open_times_heading').value;
    const open_times_mon_fri = document.getElementById('open_times_mon_fri').value;
    const open_times_sat = document.getElementById('open_times_sat').value;
    const open_times_sun = document.getElementById('open_times_sun').value;

    $.each(JSON.parse(globalAjaxResult.results), function (i, item) {


        content = "<div id=\"summary_" + itemCount + "\"  class=\"pickup-location " + (even_odd ? "even" : "odd") + "\">";
        content += "<div class=\"row pickup-location-summary\" data-lat=\"" + item.PickupLocationLatitude + "\" data-lng=\"" + item.PickupLocationLongitude + "\">";
        content += "<div class=\"small-2 columns pickup-location-marker text-center\">";
        content += "<img src=\"Plugins/Widgets.DPDShipToShop/Themes/DefaultClean/Content/images/markers/dpd_pickup_map-pin_" + itemCount + ".fw.png\" alt=\"\" />";
        content += "</div>";
        content += "<div class=\"small-7 columns nowrap\">";
        content += "<!-- If: item.PickupLocationOrganisation -->";
        if (item.PickupLocationOrganisation) {
            content += "<strong>" + item.PickupLocationOrganisation + "</strong>";
        };
        content += "<!-- End If: item.PickupLocationOrganisation -->";
        content += "<!-- If: item.PickupLocationProperty -->";
        if (item.PickupLocationProperty) {
            content += "<br />" + item.PickupLocationProperty + "";
        };
        content += "<!-- End If: item.PickupLocationProperty -->";
        content += "<!-- If: item.PickupLocationStreet -->";
        if (item.PickupLocationStreet) {
            content += "<br />" + item.PickupLocationStreet + "";
        };
        content += "<!-- End If: item.PickupLocationStreet -->";
        content += "<!-- If: item.PickupLocationLocality -->";
        if (item.PickupLocationLocality) {
            content += "<br />" + item.PickupLocationLocality + "";
        };
        content += "<!-- End If: item.PickupLocationLocality -->";
        content += "<!-- If: item.PickupLocationTown -->";
        if (item.PickupLocationTown) {
            content += "<br />" + item.PickupLocationTown + "";
        };
        content += "<!-- End If: item.PickupLocationTown -->";
        content += "<!-- If: item.PickupLocationCounty -->";
        if (item.PickupLocationCounty) {
            content += "<br />" + item.PickupLocationCounty + "";
        };
        //if (item.PickupLocationCountryCode) {
        //    content += "<br />" + item.PickupLocationCountryCode + "";
        //};
        content += "<!-- End If: item.PickupLocationCounty -->";
        content += "<!-- If: item.PickupLocationPostcode -->";
        if (item.PickupLocationPostcode) {
            content += "<br />" + item.PickupLocationPostcode + "";
        };
        content += "<!-- End If: item.PickupLocationPostcode -->";
        content += "</div>";
        content += "<div class=\"small-3 columns pickup-location-distance text-right\">";
        content += "<img width=\"20\" src=\"Plugins/Widgets.DPDShipToShop/Themes/DefaultClean/Content/images/car.png\" class=\"ng-scope\" />";
        var pickupLocationDistance = item.PickupLocationDistance;

        var distanceindicator = ((item.PickupLocationCountryCode === 'GB') ? 'Miles' : 'Km');


        content += "<br /><span class=\"ng-binding ng-scope\">" + pickupLocationDistance.toFixed(2) + "<text> " + distanceindicator + "</text></span>";
        content += "</div>";
        content += "</div>";
        content += "<div class=\"pickup-location-details ng-hide\">";
        content += "<!-- Start Repeat: PickupLocation icons -->";
        content += "<div class=\"location-icons text-center\">";
        if (item.PickupLocationDisabledAccess) {
            content += "<div class=\"ng-scope\"><img width=\"30\" src=\"Plugins/Widgets.DPDShipToShop/Themes/DefaultClean/Content/images/wheelchair.png\" /></div>";
        }
        if (item.PickupLocationOpenLate) {
            content += "<div><img width=\"30\" src=\"Plugins/Widgets.DPDShipToShop/Themes/DefaultClean/Content/images/open-late.png\" /></div>";
        }
        if (item.PickupLocationOpenSaturday) {
            content += "<div class=\"ng-scope\"><img width=\"30\" src=\"Plugins/Widgets.DPDShipToShop/Themes/DefaultClean/Content/images/open-saturday.png\" /></div>";
        }
        if (item.PickupLocationOpenSunday) {
            content += "<div class=\"ng-scope\"><img width=\"30\" src=\"Plugins/Widgets.DPDShipToShop/Themes/DefaultClean/Content/images/open-sunday.png\" /></div>";
        }
        if (item.PickupLocationParkingAvailable) {
            content += "<div class=\"ng-scope\"><img width=\"30\" src=\"Plugins/Widgets.DPDShipToShop/Themes/DefaultClean/Content/images/parking.png\" /></div>";
        }
        content += "</div>";
        content += "<!-- End Repeat: PickupLocation icons -->";
        content += "<!-- Start: PickupLocation opening -->";
        content += "<div class=\"row location-opening\"></div>";
        content += "<div class=\"row\">";
        content += "<div class=\"small-12 columns text-right\">";
        content += "<table>";
        content += "<thead>";
        content += "<tr>";
        content += "<td colspan=\"2\" class=\"ng-scope\">" + open_times_heading + "</td>";
        content += "</tr>";
        content += "</thead>";
        content += "<tbody>";
        content += "<tr class=\"ng-scope\">";
        content += "<td class=\"ng-binding\"> " + open_times_mon_fri + ":</td>";
        content += "<td class=\"text-right ng-binding\">" + item.PickupLocationWeekdayTime + "</td>";
        content += "</tr>";
        content += "<tr class=\"ng-scope\">";
        content += "<td class=\"ng-binding\"> " + open_times_sat + ":</td>";
        content += "<td class=\"text-right ng-binding\">" + item.PickupLocationSaturdayTime + "</td>";
        content += "</tr>";
        content += "<tr class=\"ng-scope\">";
        content += "<td class=\"ng-binding\"> " + open_times_sun + ":</td>";
        content += "<td class=\"text-right ng-binding\">" + item.PickupLocationSundayTime + "</td>";
        content += "</tr>";
        content += "</tbody>";
        content += "</table>";
        content += "</div>";
        content += "</div>";
        content += "<!-- End: PickupLocation opening -->";
        content += "<!-- Start: PickupLocation Select Button -->";
        content += "<div class=\"row\">";
        content += "<div class=\"small-12 columns text-right\">";
        content += "<button type=\"button\" class=\"button ng-scope\" data-lc-id=\"" + item.PickupLocationCode + "\" translate=\"\">" + selectButtonText + "</button>"
        content += "</div>";
        content += "</div>";
        content += "<!-- End: PickupLocation Select Button -->";
        content += "</div>";
        content += "</div>";

        itemCount++;
        even_odd = !even_odd;

        //Push the content to the arrays

        pickupPointsRankedList.push(content);


    });

    locationsLoaded = true;
    //$(mapLoadingId).hide();
    //$(mainContainerId).show();
    $('#pickup-locations-container').html(pickupPointsRankedList);

};


//Handle the api erros
function handleErrors(data) {
    $.each(JSON.parse(data.error), function (i, item) {

        errorContent = "We're sorry, but there seems to have been an error: Error Code: " + item.errorCode + " - Error Message: " + item.errorMessage + ".";
        errorMessages.push(errorContent);
    });
    displayBarNotification(errorContent, 'warning', 2000);
    //$(errorResponseContainer).show();
    //$(errorResponseContainer).html(errorContent);
}

// Removes the markers from the map, but keeps them in the array.
function clearMarkers() {
    //setMarkers(null);

    even_odd = false;
    locationCount = 0;
    locationsContentCount = 0;
    $('#pickup-locations-container').html('');

}

// Deletes all markers and locations list in the arrays by removing references to them.
function deleteMarkers() {
    clearMarkers();
    globalAjaxResult = null;
    locations = [];
    for (let i = 0; i < markers.length; i++) {
        markers[i].setMap(null);
    }
    pickupPointsRankedList = [];
}

//Show hide the Pickup Location Details
$(document).on("click", '.pickup-location-summary', function (e) {
    e.preventDefault();
    openCloseSummary($(this))
});

//Zoom to the corresponding marker
function changeMarkerPos(lat, lon) {
    myLatLng = new google.maps.LatLng(lat, lon)
    //marker.setPosition(myLatLng);
    map.panTo(myLatLng);
    map.setZoom(13);
}

function scrollToSummary(markerId) {
    var $selectedLocation = $('div[id*=summary_' + markerId + ']');
    var $container = $('#pickup-locations-container');
    $container.animate({
        scrollTop: $selectedLocation.offset().top - $container.offset().top + $container.scrollTop()
    }, 'fast');
}

function openCloseSummary(summaryId) {
    if (summaryId.parent().find('.pickup-location-details').hasClass("ng-open")) {
        summaryId.parent().find('.pickup-location-details').slideUp('fast').removeClass("ng-open");
        summaryId.parent().find('.pickup-location-details').addClass('ng-hide');
    } else {
        var elem = summaryId.parent().find('.pickup-location-details');
        $('.pickup-location-details').not(elem).slideUp('fast').removeClass("ng-open");
        elem.slideDown('fast').addClass("ng-open");
        changeMarkerPos(summaryId.data("lat"), summaryId.data("lng"));
    }
}

function closeAllSummary() {
    var elem = $('.pickup-location-details');
    elem.slideUp('fast').removeClass("ng-open");
}

$(document).on("click", '.pickup-location .button', function (e) {

    var elem = $(this);
    var dpdpickupointlocationcodeinput = $("input#dpd-pickupoint-location-code");
    var pickupLocationCodeId = elem.data('lc-id');
    $('.pickup-location .button').not(elem).removeClass('selected').html('Select').prop('disabled', false);
    elem.html('Please wait...').prop('disabled', true);

    //Find the index for the corresponding PickupLocationCode == pickupLocationCodeId
    var pickupPointData = $.grep(JSON.parse(globalAjaxResult.results), function (pickupoint, index) {
        return pickupoint.PickupLocationCode == pickupLocationCodeId;
    });

    //Create the model
    var model = {};

    model.PickupLocationCode = pickupPointData[0].PickupLocationCode,
        model.Organisation = pickupPointData[0].PickupLocationOrganisation,
        model.Property = pickupPointData[0].PickupLocationProperty,
        model.Street = pickupPointData[0].PickupLocationStreet,
        model.Locality = pickupPointData[0].PickupLocationLocality,
        model.Town = pickupPointData[0].PickupLocationTown,
        model.County = pickupPointData[0].PickupLocationCounty,
        model.Postcode = pickupPointData[0].PickupLocationPostcode,
        model.CountryCode = pickupPointData[0].PickupLocationCountryCode,
        model.Latitude = pickupPointData[0].PickupLocationLatitude,
        model.Longitude = pickupPointData[0].PickupLocationLongitude


    //Post the model to the controller and return a status if valid
    $.ajax({
        cache: false,
        url: '/DPDShipToShop/SelectPickupLocation',
        type: "POST",
        data: JSON.stringify(model),
        contentType: 'application/json; charset=utf-8',
        success: function (data) {

            if (data.success) {
                if (data.result) {
                    elem.addClass('selected').html('Selected!').prop('disabled', true);
                    $(dpdpickupointlocationcodeinput).val(pickupPointData[0].PickupLocationCode);
                    $('.shipping-method-next-step-button').prop('disabled', false).css("opacity", "1");
                    //console.log("Result is: "+ data.result);
                }
            } else {
                elem.html('Error...');
                //console.log(data.result);
                setTimeout(function () {
                    elem.html('Select').prop('disabled', false);
                }, 2000)
            }

        },
        error: function (data) {
            elem.html('Error...');
            //console.log("There was an error...");
        },
        failure: function (data) {
            elem.html('Failure...');
            //console.log("There was a failure...");
        }
    });

});

function ShowPostcodeNotification(text, type, timeout) {
    $('#postcode-notification').html(text);
    $('#postcode-notification').show();
    setTimeout(function () {
        $('#postcode-notification').fadeOut('slow', function () { $('#postcode-notification').html('') });
    }, timeout);

}

function ShowLocationNotification(text, type, timeout) {
    $('#location-notification').html(text);
    $('#location-notification').show();
    setTimeout(function () {
        $('#location-notification').fadeOut('slow', function () { $('#location-notification').html('') });
    }, timeout);

}
