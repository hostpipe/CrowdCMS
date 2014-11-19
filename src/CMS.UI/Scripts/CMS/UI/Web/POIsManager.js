/// <reference path="../../../_references.js" />

function POIsManager() {
    this.element = $('#POIsContainer');
    this.map = null;
    this.latitude = 0;
    this.longitude = 0;
    this.markers = [];
    this.infoWindow = null;
    this.currentMarker = null;
}

POIsManager.prototype = {
    _applyViewModel: function () {
        ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            categories: ko.observableArray([]),
            categoryID: ko.observable(null),
            pois: ko.observableArray([]),
            search: ko.observable(''),
            tagGroups: ko.observableArray([]),

            onReset: $.proxy(this._onReset, this),
            onSearch: $.proxy(this._onSearch, this)
        }
    },
    _initializeMap: function () {
        var mapOptions = {
            center: new google.maps.LatLng(this.latitude, this.longitude),
            zoom: 5
        };

        this.map = new google.maps.Map($('#MapContainer').get(0), mapOptions);

        this.infoWindow = new google.maps.InfoWindow({});
    },
    _initializeSettings: function () {
        this.settings = {
            getPOIUrl: this.element.attr('data-getPOI-url'),
            getTagsUrl: this.element.attr('data-getTags-url'),
            searchPOIsUrl: this.element.attr('data-searchPOIs-url')
        }
    },
    _getLocation: function (position) {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                $.proxy(function (position) {
                    this.latitude = position.coords.latitude;
                    this.longitude = position.coords.longitude;

                    this._initializeMap();
                }, this),
                $.proxy(function (error) {
                    //London
                    this.latitude = 51.5;
                    this.longitude = -0.116667;

                    this._initializeMap();
                }, this)
            );
        }
        else {
            //London
            this.latitude = 51.5;
            this.longitude = -0.116667;

            this._initializeMap();
        }
    },
    _getMarkerData: function (marker) {
        var poiID = marker.poiID;
        this.currentMarker = marker;
        this.infoWindow.close();
        $.post(this.settings.getPOIUrl, { poiID: poiID }, $.proxy(this._getMarkerDataCompleted, this));
    },
    _getMarkerDataCompleted: function (data) {
        if (data) {
            this.infoWindow.setContent(data);
            this.infoWindow.open(this.map, this.currentMarker);
        }
    },
    _getTags: function () {
        $.post(this.settings.getTagsUrl, $.proxy(this._getTagsCompleted, this));
    },
    _getTagsCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.tagGroups(data.tagGroups);
            this.viewModel.categories(data.categories);
        }
    },
    _onReset: function () {
        this.viewModel.categoryID(0);
        this.viewModel.search('');
        this.element.find('input:checked').prop('checked', false);

        this._removeMarkers();
    },
    _onSearch: function () {
        var tagIDs = this.element.find('input:checked').map(function (index, element) {
            return $(element).val();
        }).get();

        $.ajax(this.settings.searchPOIsUrl, {
            data: {
                search: this.viewModel.search(),
                categoryID: this.viewModel.categoryID(),
                tagIDs: tagIDs
            },
            type: 'POST',
            success: $.proxy(this._onSearchCompleted, this),
            traditional: true
        });
    },
    _onSearchCompleted: function (data) {
        if (data) {
            if (data.success && this.map) {
                this._removeMarkers();
                this._setMarkers(data.tags);
            }
        }
    },
    _removeMarkers: function () {
        for (var marker in this.markers) {
            this.markers[marker].setMap(null);
        }
        this.markers = [];
    },
    _setMarkers: function (tags) {
        for (var tag in tags) {
            var marker = new google.maps.Marker({
                position: new google.maps.LatLng(tags[tag].Latitude, tags[tag].Longitude),
                title: tags[tag].Title,
                map: this.map,
                poiID: tags[tag].POIID
            });

            var self = this;
            google.maps.event.addListener(marker, 'click', function () {
                self._getMarkerData(this);
            });

            this.markers.push(marker);
        }
    },
    initialize: function () {
        this._initializeSettings();
        this._createViewModel();
        this._applyViewModel();
        this._getTags();
        this._getLocation();
    }
}

$(function () {
    var manager = new POIsManager();
    manager.initialize();
})