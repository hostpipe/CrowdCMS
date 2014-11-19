/// <reference path="../../../_references.js" />

HostPipe.UI.POIsManager = function () {
    this.element = $('#POIsContent');
}

HostPipe.UI.POIsManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            isEdit: ko.observable(false),
            isFileEdit: ko.observable(false),
            sectionHeader: ko.observable('Add Point Of Interest'),

            categoryID: ko.observable(0),
            description: ko.observable(''),
            latitude: ko.observable(''),
            longitude: ko.observable(''),
            phone: ko.observable(''),
            poiID: ko.observable(0),
            title: ko.observable(''),
            tags: ko.observableArray([]),
            files: ko.observableArray([]),

            domains: ko.observableArray([]),
            domainID: ko.observable(0),
            sitemaps: ko.observableArray([]),
            sitemapID: ko.observable(0),

            addressID: ko.observable(0),
            address1: ko.observable(''),
            address2: ko.observable(''),
            address3: ko.observable(''),
            town: ko.observable(''),
            postcode: ko.observable(''),
            county: ko.observable(''),
            country: ko.observable(''),

            onDelete: $.proxy(this._onDelete, this),
            onDeleteFile: $.proxy(this._onDeleteFile, this),
            onEdit: $.proxy(this._onEdit, this),
            onEditImage: $.proxy(this._onEditImage, this),
            onNewPOI: $.proxy(this._onNewPOI, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.domainID.subscribe(function (value) {
            if (!value || value == 0) {
                this.viewModel.sitemapID(0);
                this.viewModel.sitemaps([]);
            }
            else {
                this._getSitemaps(value, 0);
            }
        }, this);

        this.viewModel.poiID.subscribe(function (value) {
            if (value == 0) {
                this.isEdit(false);
                this.sectionHeader('Add Point Of Interest');
            }
            else {
                this.isEdit(true);
                this.sectionHeader('Update Point Of Interest');
            }
        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addPOIUrl: this.element.attr('data-addPOI-url'),
            deletePOIUrl: this.element.attr('data-deletePOI-url'),
            deletePOIFileUrl: this.element.attr('data-deletePOIFile-url'),
            getCustomEditorObjects: this.element.attr('data-getCustomEditorObjects-url'),
            getDomainsUrl: this.element.attr('data-getDomains-url'),
            getSitemapsUrl: this.element.attr('data-getSitemaps-url'),
            getPOIUrl: this.element.attr('data-getPOI-url'),
            getPOIFilesUrl: this.element.attr('data-getPOIFiles-url'),
            getPOITagsUrl: this.element.attr('data-getPOITags-url'),
            updatePOIUrl: this.element.attr('data-updatePOI-url')
        }

        this.cssFile = this.element.attr('data-domainCSSFile');
    },
    _getDomains: function () {
        $.post(this.settings.getDomainsUrl, $.proxy(this._getDomainsCompleted, this));
    },
    _getDomainsCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.domains(data.domains);
            this.viewModel.domainID(data.selected);
        }
    },
    _getFiles: function () {
        $.post(this.settings.getPOIFilesUrl, { poiID: this.viewModel.poiID() }, $.proxy(this._getFilesCompleted, this));
    },
    _getFilesCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.poiID(data.poiID);
            this.viewModel.files(data.files);
        }
    },
    _getSitemaps: function (domainID, sitemapID) {
        $.post(this.settings.getSitemapsUrl, { domainID: domainID, sitemapID: sitemapID }, $.proxy(this._getSitemapsCompleted, this));
    },
    _getSitemapsCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.sitemaps(data.sections);
            this.viewModel.sitemapID(data.sitemapID);
        }
    },
    _getTags: function () {
        $.post(this.settings.getPOITagsUrl, { poiID: this.viewModel.poiID() }, $.proxy(this._getTagsCompleted, this));
    },
    _getTagsCompleted: function (data) {
        if (data && data.tags)
            this.viewModel.tags(data.tags);
    },
    _initializeFileForm: function () {
        $("#POIFilesForm").ajaxForm({
            dataType: 'json',
            beforeSubmit: $.proxy(this._onBeforeSubmitImages, this),
            success: $.proxy(this._onSubmitImagesCompleted, this),
            uploadProgress: $.proxy(this._onProgress, this)
        });
    },
    _onBeforeSubmitImages: function (arr, $form, options) {
        $('#UploadWindow').dialog({ title: "File Upload", modal: true, width: 250 });
        $('#Progressbar').progressbar();
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var poiID = menuItem.attr('data-itemID');

        if (confirm("Are you sure you wish to delete this poi? This action cannot be undone.")) {
            if (poiID == this.viewModel.poiID())
                this.reload = true;

            $.post(this.settings.deletePOIUrl, { poiID: poiID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("POI deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewPOI();
                this.reload = false;
            }
        }
    },
    _onDeleteFile: function (data, event) {
        var fileID = $(event.currentTarget).attr('data-fileID');
        var confirmationText = "Are you sure that you wish to delete this file? This action cannot be undone.";

        if (confirm(confirmationText)) {
            $.post(this.settings.deletePOIFileUrl, { fileID: fileID }, $.proxy(this._onDeleteFileCompleted, this));
        }
    },
    _onDeleteFileCompleted: function (data) {
        if (data && data.success) {
            alert("File successfully deleted");
            this._getFiles();
        }
    },
    _onEdit: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.poiID(menuItem.attr('data-itemID'));

        $.post(this.settings.getPOIUrl, { poiID: this.viewModel.poiID() }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.title(data.poi.Title);
            this.viewModel.categoryID(data.poi.CategoryID);
            this.viewModel.description(data.poi.Description);
            this.viewModel.latitude(data.poi.Latitude);
            this.viewModel.longitude(data.poi.Longitude);
            this.viewModel.phone(data.poi.Phone);
            this.viewModel.poiID(data.poi.POIID);
            this.viewModel.title(data.poi.Title);

            this.viewModel.domainID(data.poi.DomainID);
            this._getSitemaps(data.poi.DomainID, data.poi.SitemapID);

            this.viewModel.addressID(data.poi.AddressID);
            this.viewModel.address1(data.poi.Address1);
            this.viewModel.address2(data.poi.Address2);
            this.viewModel.address3(data.poi.Address3);
            this.viewModel.town(data.poi.Town);
            this.viewModel.postcode(data.poi.Postcode);
            this.viewModel.county(data.poi.County);
            this.viewModel.country(data.poi.Country);

            this._getTags();
            this.viewModel.isFileEdit(false);
        }
    },
    _onEditImage: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.poiID(menuItem.attr('data-itemID'));

        this.viewModel.isFileEdit(true);
        this._getFiles();
    },
    _onNewPOI: function () {
        this.viewModel.isFileEdit(false);

        this.viewModel.title('');
        this.viewModel.categoryID(0);
        this.viewModel.description('');
        this.viewModel.latitude('');
        this.viewModel.longitude('');
        this.viewModel.phone('');
        this.viewModel.poiID(0);
        this.viewModel.title('');

        this.viewModel.domainID(null);

        this.viewModel.addressID(0);
        this.viewModel.address1('');
        this.viewModel.address2('');
        this.viewModel.address3('');
        this.viewModel.town('');
        this.viewModel.postcode('');
        this.viewModel.county('');
        this.viewModel.country('');

        this._getTags();
    },
    _onProgress: function (event, position, total, percentComplete) {
        $("#Progressbar").progressbar("value", percentComplete);
    },
    _onSubmit: function () {
        form = $('#DetailsForm');
        form.validate();
        if (form.valid()) {
            var url = this.viewModel.isEdit() ? this.settings.updatePOIUrl : this.settings.addPOIUrl;

            $.ajax(url, {
                type: "POST",
                data: form.serializeArray(),
                traditional: true,
                success: $.proxy(this._onSubmitCompleted, this)
            });
        }
        return false;
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("POI successfully saved.");
                this._reloadLeftMenu();
                $.post(this.settings.getPOIUrl, { poiID: data.poiID }, $.proxy(this._onEditCompleted, this));
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onSubmitImagesCompleted: function (data) {
        $('#UploadWindow').dialog("close");
        $("#Progressbar").progressbar("destroy");
        $('#UploadWindow').dialog("destroy");

        this._getFiles();
    },
    _reloadLeftMenu: function () {
        LeftMenuManager.loadMenu(this.viewModel);
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._reloadLeftMenu();
        this._getTags();
        this._getDomains();
        this._initializeFileForm();
    }
}

$(function () {
    var POIsManager = new HostPipe.UI.POIsManager();
    POIsManager.initialize();
})