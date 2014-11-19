/// <reference path="../../../_references.js" />

HostPipe.UI.POITagsManager = function () {
    this.element = $('#POITagsContent');
}

HostPipe.UI.POITagsManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            isEdit: ko.observable(false),
            poiTagID: ko.observable(0),
            poiTagsGroupID: ko.observable(0),
            title: ko.observable(''),
            sectionHeader: ko.observable('Add POI Tag'),

            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onNewTag: $.proxy(this._onNewTag, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.poiTagID.subscribe(function (value) {
            if (value == 0) {
                this.isEdit(false);
                this.sectionHeader('Add POI Tag');
            }
            else {
                this.isEdit(true);
                this.sectionHeader('Update POI Tag');
            }
        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addTagUrl: this.element.attr('data-addTag-url'),
            deleteTagUrl: this.element.attr('data-deleteTag-url'),
            getTagUrl: this.element.attr('data-getTag-url'),
            updateTagUrl: this.element.attr('data-updateTag-url')
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var tagID = menuItem.attr('data-itemID');

        if (confirm("Are you sure you wish to delete this tag? This action cannot be undone.")) {
            if (tagID == this.viewModel.poiTagID())
                this.reload = true;

            $.post(this.settings.deleteTagUrl, { poiTagID: tagID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("POI Tag deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewTag();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.poiTagID(menuItem.attr('data-itemID'));

        $.post(this.settings.getTagUrl, { poiTagID: this.viewModel.poiTagID() }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.title(data.tag.Title);
            this.viewModel.poiTagsGroupID(data.tag.POITagsGroupID);
            this.viewModel.poiTagID(data.tag.POITagID);
        }
    },
    _onNewTag: function () {
        this.viewModel.title('');
        this.viewModel.poiTagsGroupID(0);
        this.viewModel.poiTagID(0);
    },
    _onSubmit: function () {
        form = $('#DetailsForm');
        form.validate();
        if (form.valid()) {
            var url = this.viewModel.isEdit() ? this.settings.updateTagUrl : this.settings.addTagUrl;

            $.post(url, {
                Title: this.viewModel.title(),
                POITagsGroupID: this.viewModel.poiTagsGroupID(),
                POITagID: this.viewModel.poiTagID()
            }, $.proxy(this._onSubmitCompleted, this));
        }
        return false;
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("POI tag successfully saved.");
                this._reloadLeftMenu();
                $.post(this.settings.getTagUrl, { poiTagID: data.poiTagID }, $.proxy(this._onEditCompleted, this));
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _reloadLeftMenu: function () {
        LeftMenuManager.loadMenu(this.viewModel);
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._reloadLeftMenu();
    }
}

$(function () {
    var POITagsManager = new HostPipe.UI.POITagsManager();
    POITagsManager.initialize();
})