/// <reference path="../../../_references.js" />
HostPipe.UI.PostageManager = function () {
    this.element = $("#PostageContent");
}

HostPipe.UI.PostageManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createSettings: function () {
        this.settings = {
            addPostageUrl: this.element.attr('data-addPostage-url'),
            deletePostageUrl: this.element.attr('data-deletePostage-url'),
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            getPostageUrl: this.element.attr('data-getPostage-url'),
            getPostageDataUrl: this.element.attr('data-getPostageData-url'),
            getPostageSelectorsOnDomainChange: this.element.attr('data-getPostageSelectors-url'),
            updatePostageUrl: this.element.attr('data-updatePostage-url'),
            getPostageAttributesUrl: this.element.attr('data-getPostageAttributes-url'),
            getPostageAttributesSelectorsUrl: this.element.attr('data-getPostageAttributesSelectors-url'),
            savePostageBandUrl: this.element.attr('data-saveBand-url'),
            savePostageWeightUrl: this.element.attr('data-saveWeight-url'),
            savePostageZoneUrl: this.element.attr('data-saveZone-url'),
            deletePostageBandUrl: this.element.attr('data-deleteBand-url'),
            deletePostageWeightUrl: this.element.attr('data-deleteWeight-url'),
            deletePostageZoneUrl: this.element.attr('data-deleteZone-url')
        };
    },
    _createViewModel: function () {
        this.viewModel = {
            isNewPostage: ko.observable(true),
            isPostageEdit: ko.observable(true),
            sectionHeader: ko.observable(this.postageID == 0 ? "Add New Postage" : "Update Postage"),
            bandID: ko.observable(0),
            weightID: ko.observable(0),
            zoneID: ko.observable(0),
            postageBands: ko.observableArray([]),
            postageWeights: ko.observableArray([]),
            postageZones: ko.observableArray([]),
            postageID: ko.observable(0),
            bands: ko.observableArray([]),
            weights: ko.observableArray([]),
            zones: ko.observableArray([]),
            domainID: ko.observable(0),
            defaultZone: ko.observable(),
            description: ko.observable(),
            amount: ko.observable(),
            domains: ko.observableArray([]),
            lDomainID: ko.observable(),

            onLDomainChange: $.proxy(this._reloadLeftMenu, this),
            onDomainChange: $.proxy(this._onDomainChange, this),
            onDomainChangeAttributes: $.proxy(this._onDomainChangeAttributes, this),
            onDefaultZoneChange: $.proxy(this._onDefaultZoneChange, this),
            onDelete: $.proxy(this._onDeletePostage, this),
            onDeleteBand: $.proxy(this._onDeleteBand, this),
            onDeleteWeight: $.proxy(this._onDeleteWeight, this),
            onDeleteZone: $.proxy(this._onDeleteZone, this),
            onEdit: $.proxy(this._onEditPostage, this),
            onNewPostage: $.proxy(this._onNewPostage, this),
            onSavePostage: $.proxy(this._onSavePostage, this),
            onSaveBand: $.proxy(this._onSaveBand, this),
            onSaveWeight: $.proxy(this._onSaveWeight, this),
            onSaveZone: $.proxy(this._onSaveZone, this),
            onUpdateBand: $.proxy(this._onUpdateBand, this),
            onUpdateWeight: $.proxy(this._onUpdateWeight, this),
            onUpdateZone: $.proxy(this._onUpdateZone, this),
            onPostageAttributes: $.proxy(this._onPostageAttributes, this)
        }
    },
    _onDefaultZoneChange: function (data, event) {
        $(".PZ_IsDefault").prop("checked", false);
        $(event.currentTarget).prop("checked", true);
        return true;
    },
    _onDeleteBand: function (data, event) {
        if (confirm("Are you sure that you wish to delete this band?")) {
            var form = $(event.currentTarget).closest("form");
            var data = form.serializeArray();
            $.post(this.settings.deletePostageBandUrl, data, $.proxy(this._onDeleteBandCompleted, this));
        }
    },
    _onDeleteBandCompleted: function (data) {
        if (data && data.success == true) {
            this.viewModel.bands.remove(function (item) { return item.PostageBandID == data.id; });
        }
    },
    _onDeleteWeight: function (data, event) {
        if (confirm("Are you sure that you wish to delete this weight?")) {
            var form = $(event.currentTarget).closest("form");
            var data = form.serializeArray();
            $.post(this.settings.deletePostageWeightUrl, data, $.proxy(this._onDeleteWeightCompleted, this));
        }
    },
    _onDeleteWeightCompleted: function (data) {
        if (data && data.success == true) {
            this.viewModel.weights.remove(function (item) { return item.PostageWeightID == data.id; });
        }
    },
    _onDeleteZone: function (data, event) {
        if (confirm("Are you sure that you wish to delete this zone?")) {
            var form = $(event.currentTarget).closest("form");
            var data = form.serializeArray();
            var url = this.settings.deletePostageZoneUrl;
            $.post(url, data, $.proxy(this._onDeleteZoneCompleted, this));
        }
    },
    _onDeleteZoneCompleted: function (data) {
        if (data && data.success == true) {
            this.viewModel.zones.remove(function (item) { return item.PostageZoneID == data.id; });
        }
    },
    _onDeletePostage: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure that you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var postageID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (postageID == this.viewModel.postageID())
                this.reload = true;
            $.post(this.settings.deletePostageUrl, { postageID: postageID }, $.proxy(this._onDeletePostageCompleted, this));
        }
    },
    _onDeletePostageCompleted: function (data) {
        if (data && data.success == true) {
            alert("Postage successfully deleted");
            this._reloadLeftMenu();
            if (this.reload)
                this._onNewPostage();
            this.reload = false;
        }
    },
    _onDomainChange: function () {
        $.post(this.settings.getPostageSelectorsOnDomainChange, { domainID: this.viewModel.domainID }, $.proxy(this._onDomainChangeCompleted, this));
    },
    _onDomainChangeCompleted: function (data) {
        this.viewModel.postageBands(data.bands);
        this.viewModel.postageWeights(data.weights);
        this.viewModel.postageZones(data.zones);
        this.viewModel.defaultZone(data.defaultZone);
        this.viewModel.zoneID(this.viewModel.defaultZone());
        this._getPostageData();
    },
    _onDomainChangeAttributes: function () {
        $.post(this.settings.getPostageAttributesSelectorsUrl, { domainID: this.viewModel.domainID }, $.proxy(this._onDomainChangeAttributesCompleted, this));
    },
    _onDomainChangeAttributesCompleted: function (data) {
        this.viewModel.bands(data.bands);
        this.viewModel.weights(data.weights);
        this.viewModel.zones(data.zones);
    },
    _onEditPostage: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.postageID(menuItem.attr('data-itemID'));
        this.isEdit = true;
        if (this.viewModel.isPostageEdit() == false)
            $.post(this.settings.getPostageUrl, { postageID: 0 }, $.proxy(this._onLoadFormCompleted, this));
        else
            this._getPostageData();
    },
    _onLoadFormCompleted: function (data) {
        if (data) {
            this._insertPartialView(data);
            this._getPostageData();
        }
    },
    _onEditPostageCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.sectionHeader("Update Postage");
            this.viewModel.isNewPostage(false);
            this.viewModel.isPostageEdit(true);
            if (this.isEdit) {
                this.viewModel.domainID(data.postage.PST_DomainID);
                this.isEdit = false;
            }
            this.viewModel.bandID(data.postage.PST_PostageBand);
            this.viewModel.weightID(data.postage.PST_PostageWeight);
            this.viewModel.zoneID(data.postage.PST_PostageZone);
            this.viewModel.description(data.postage.PST_Description);
            this.viewModel.amount(data.postage.PST_Amount);
            this.viewModel.postageID(data.postage.PostageID);
        }
    },
    _onNewPostage: function () {
        $.post(this.settings.getPostageUrl, { postageID: 0 }, $.proxy(this._onNewPostageCompleted, this));
    },
    _onNewPostageCompleted: function (data, event) {
        if (data) {
            this._insertPartialView(data);
            this.viewModel.sectionHeader("Add New Postage");
            this.viewModel.isNewPostage(true);
            this.viewModel.isPostageEdit(true);
            this.viewModel.bandID(null);

            this.viewModel.weightID(null);
            this.viewModel.zoneID(this.viewModel.defaultZone());
            this.viewModel.description('');
            this.viewModel.amount('');
            this.viewModel.postageID(0);
        }
    },
    _onPostageAttributes: function () {
        $.post(this.settings.getPostageAttributesUrl, { domainID: 0 }, $.proxy(this._onPostageAttributesCompleted, this));
    },
    _onPostageAttributesCompleted: function (data) {
        if (data) {
            this.viewModel.postageID(0);
            this.viewModel.sectionHeader("Postage Attributes");
            this.viewModel.isPostageEdit(false);
            this._insertPartialView(data);
            this._onDomainChangeAttributes();
        }
    },
    _onSavePostage: function () {
        form = $('#PostageForm');
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.viewModel.isNewPostage() ? this.settings.addPostageUrl : this.settings.updatePostageUrl;
            $.post(url, data, $.proxy(this._onSavePostageCompleted, this));
        }
        return false;
    },
    _onSavePostageCompleted: function (data) {
        if (data && data.success) {
            alert("Postage successfully saved.");
            this._reloadLeftMenu();
            $.post(this.settings.getPostageUrl, { postageID: data.postageID }, $.proxy(this._onEditPostageCompleted, this));
        }
    },
    _onSaveBand: function (data, event) {
        form = $(event.currentTarget).closest("form");
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.settings.savePostageBandUrl;
            if (data[1].name == "PostageBandID") {
                if (data[2].value > data[3].value)
                    return;
                $.post(url, data, $.proxy(this._onUpdateBandCompleted, this));
            }
            else {
                if (data[1].value > data[2].value)
                    return;
                $.post(url, data, $.proxy(this._onSaveBandCompleted, this));
            }
        }
    },
    _onSaveBandCompleted: function (data, event) {
        if (data && data.success) {
            this.viewModel.bands.push(data.band);
        }
    },
    _onSaveWeight: function (data, event) {
        form = $(event.currentTarget).closest("form");
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.settings.savePostageWeightUrl;
            if (data[1].name == "PostageWeightID") {
                if (data[2].value > data[3].value)
                    return;
                $.post(url, data, $.proxy(this._onUpdateWeightCompleted, this));
            }
            else {
                if (data[1].value > data[2].value)
                    return;
                $.post(url, data, $.proxy(this._onSaveWeightCompleted, this));
            }
        }
    },
    _onSaveWeightCompleted: function (data, event) {
        if (data && data.success) {
            this.viewModel.weights.push(data.weight);
        }
    },
    _onSaveZone: function (data, event) {
        form = $(event.currentTarget).closest("form");
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.settings.savePostageZoneUrl;
            if (data[1].name == "PostageZoneID")
                $.post(url, data, $.proxy(this._onUpdateZoneCompleted, this));
            else
                $.post(url, data, $.proxy(this._onSaveZoneCompleted, this));
        }
    },
    _onSaveZoneCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.zones.push(data.zone);
            if (data.zone.PZ_IsDefault)
                this.viewModel.defaultZone(data.zone.PostageZoneID);
        }
    },
    _onUpdateZoneCompleted: function (data) {
        if (data && data.success) {
            if (data.zone.PZ_IsDefault)
                this.viewModel.defaultZone(data.zone.PostageZoneID);
        }
    },
    _getPostageData: function () {
        $.post(this.settings.getPostageDataUrl, { postageID: this.viewModel.postageID }, $.proxy(this._onEditPostageCompleted, this));
    },
    _getDomainsList: function () {
        $.post(this.settings.getDomainsListUrl, { currentDomain: true }, $.proxy(this._getDomainsListCompleted, this));
    },
    _getDomainsListCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.domains(data.domains);
            this.viewModel.lDomainID(data.selected);
            this._reloadLeftMenu();
        }
    },
    _reloadLeftMenu: function () {
        LeftMenuManager.loadMenu(this.viewModel, "", { domainID: this.viewModel.lDomainID() });
    },
    _insertPartialView: function (data) {
        this.element.find('.mainContainer').html(data);
        this._applyViewModel(this.element.find('.mainContainer'));
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        $.validator.unobtrusive.parse(this.element);
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._getDomainsList();
    }
}

$(function () {
    var PostageManager = new HostPipe.UI.PostageManager();
    PostageManager.initialize();
})