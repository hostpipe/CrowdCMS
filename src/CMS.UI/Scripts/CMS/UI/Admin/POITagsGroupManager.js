/// <reference path="../../../_references.js" />

HostPipe.UI.POITagsGroupsManager = function () {
    this.element = $('#POITagsGroupContent');
}

HostPipe.UI.POITagsGroupsManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            isEdit: ko.observable(false),
            poiTagsGroupID: ko.observable(0),
            title: ko.observable(''),
            sectionHeader: ko.observable('Add Group'),

            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onNewTagsGroup: $.proxy(this._onNewTagsGroup, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.poiTagsGroupID.subscribe(function (value) {
            if (value == 0) {
                this.isEdit(false);
                this.sectionHeader('Add Group');
            }
            else {
                this.isEdit(true);
                this.sectionHeader('Update Group');
            }
        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addTagsGroupUrl: this.element.attr('data-addTagsGroup-url'),
            deleteTagsGroupUrl: this.element.attr('data-deleteTagsGroup-url'),
            getTagsGroupUrl: this.element.attr('data-getTagsGroup-url'),
            updateTagsGroupUrl: this.element.attr('data-updateTagsGroup-url')
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var groupID = menuItem.attr('data-itemID');

        if (confirm("Are you sure you wish to delete this group? This will DELETE ALL RELATED TAGS and this action cannot be undone.")) {
            if (groupID == this.viewModel.poiTagsGroupID())
                this.reload = true;

            $.post(this.settings.deleteTagsGroupUrl, { poiTagsGroupID: groupID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("POI Tag Group deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewTagsGroup();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.poiTagsGroupID(menuItem.attr('data-itemID'));

        $.post(this.settings.getTagsGroupUrl, { poiTagsGroupID: this.viewModel.poiTagsGroupID() }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.title(data.group.Title);
            this.viewModel.poiTagsGroupID(data.group.POITagsGroupID);
        }
    },
    _onNewTagsGroup: function () {
        this.viewModel.title('');
        this.viewModel.poiTagsGroupID(0);
    },
    _onSubmit: function () {
        form = $('#DetailsForm');
        form.validate();
        if (form.valid()) {
            var url = this.viewModel.isEdit() ? this.settings.updateTagsGroupUrl : this.settings.addTagsGroupUrl;

            $.post(url, {
                Title: this.viewModel.title(),
                POITagsGroupID: this.viewModel.poiTagsGroupID()
            }, $.proxy(this._onSubmitCompleted, this));
        }
        return false;
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("POI group successfully saved.");
                this._reloadLeftMenu();
                $.post(this.settings.getTagsGroupUrl, { poiTagsGroupID: data.poiTagsGroupID }, $.proxy(this._onEditCompleted, this));
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
    var POITagsGroupsManager = new HostPipe.UI.POITagsGroupsManager();
    POITagsGroupsManager.initialize();
})