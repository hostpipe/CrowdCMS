/// <reference path="../../../_references.js" />

HostPipe.UI.POICategoriesManager = function () {
    this.element = $('#POICategoriesContent');
}

HostPipe.UI.POICategoriesManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            isEdit: ko.observable(false),
            isLive: ko.observable(0),
            poiCategoryID: ko.observable(0),
            title: ko.observable(''),
            sectionHeader: ko.observable('Add POI Category'),

            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onNewCategory: $.proxy(this._onNewCategory, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.poiCategoryID.subscribe(function (value) {
            if (value == 0) {
                this.isEdit(false);
                this.sectionHeader('Add POI Category');
            }
            else {
                this.isEdit(true);
                this.sectionHeader('Update POI Category');
            }
        }, this.viewModel);

        this.viewModel.isLive.subscribe(function (value) {
            if (value != 0 || value != 1) {
                this.isLive(value ? 1 : 0);
            }
        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addCategoryUrl: this.element.attr('data-addCategory-url'),
            deleteCategoryUrl: this.element.attr('data-deleteCategory-url'),
            getCategoryUrl: this.element.attr('data-getCategory-url'),
            updateCategoryUrl: this.element.attr('data-updateCategory-url')
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var categoryID = menuItem.attr('data-itemID');

        if (confirm("Are you sure you wish to delete this category? This action cannot be undone.")) {
            if (categoryID == this.viewModel.poiCategoryID())
                this.reload = true;

            $.post(this.settings.deleteCategoryUrl, { poiCategoryID: categoryID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("POI Category deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewCategory();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.poiCategoryID(menuItem.attr('data-itemID'));

        $.post(this.settings.getCategoryUrl, { poiCategoryID: this.viewModel.poiCategoryID() }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.title(data.category.Title);
            this.viewModel.isLive(data.category.IsLive);
            this.viewModel.poiCategoryID(data.category.POICategoryID);
        }
    },
    _onNewCategory: function () {
        this.viewModel.title('');
        this.viewModel.isLive(false);
        this.viewModel.poiCategoryID(0);
    },
    _onSubmit: function () {
        form = $('#DetailsForm');
        form.validate();
        if (form.valid()) {
            var url = this.viewModel.isEdit() ? this.settings.updateCategoryUrl : this.settings.addCategoryUrl;

            $.post(url, {
                Title: this.viewModel.title(),
                IsLive: this.viewModel.isLive() == 1 ? true : false,
                POICategoryID: this.viewModel.poiCategoryID()
            }, $.proxy(this._onSubmitCompleted, this));
        }
        return false;
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("POI category successfully saved.");
                this._reloadLeftMenu();
                $.post(this.settings.getCategoryUrl, { poiCategoryID: data.poiCategoryID }, $.proxy(this._onEditCompleted, this));
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
    var POICategoriesManager = new HostPipe.UI.POICategoriesManager();
    POICategoriesManager.initialize();
})