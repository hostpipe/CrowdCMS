/// <reference path="../../../_references.js" />

HostPipe.UI.AdminMenuManager = function () {
    this.element = $('#ModuleContent');
}

HostPipe.UI.AdminMenuManager.prototype = {
    _applyViewModel: function () {
        ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            isNewMenuItem: ko.observable(true),
            menuText: ko.observable(''),
            menuItemID: ko.observable(0),
            menuItems: ko.observable([]),
            sectionHeader: ko.observable("Add Menu Item"),
            url: ko.observable(''),
            parentID: ko.observable(0),

            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onNewModule: $.proxy(this._onNewModule, this),
            onSaveOrder: $.proxy(this._onSaveOrder, this),
            onSubmit: $.proxy(this._onSubmit, this),
            onToggleVisibility: $.proxy(this._onToggleVisibility, this)
        }

        this.viewModel.menuItemID.subscribe(function (value) {
            if (value == 0) {
                this.isNewMenuItem(true);
                this.sectionHeader('Add Menu Item');
            }
            else {
                this.isNewMenuItem(false);
                this.sectionHeader('Update Menu Item');
            }

        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addModuleUrl: this.element.attr('data-addModule-url'),
            deleteModuleUrl: this.element.attr('data-deleteModule-url'),
            getModuleUrl: this.element.attr('data-getModule-url'),
            getMenuItemsUrl: this.element.attr('data-getMenuItems-url'),
            saveOrderUrl: this.element.attr('data-saveOrder-url'),
            saveVisibilityUrl: this.element.attr('data-saveVisibility-url'),
            updateModuleUrl: this.element.attr('data-updateModule-url')
        }
    },
    _getMenuItems: function (data, event) {
        $.post(this.settings.getMenuItemsUrl, $.proxy(this._getMenuItemsCompleted, this));
    },
    _getMenuItemsCompleted: function (data) {
        if (data && data.menuItems) {
            this.viewModel.menuItems(data.menuItems);
            this.viewModel.parentID(parseInt(this.viewModel.parentID()));
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) +"? This action cannot be undone.";
        var menuItemID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (menuItemID == this.viewModel.menuItemID())
                this.reload = true;

            $.post(this.settings.deleteModuleUrl, { menuItemID: menuItemID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Menu item deleted successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                this._getMenuItems();
                if (this.reload)
                    this._onNewModule();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var menuItemID = menuItem.attr('data-itemID');

        $.post(this.settings.getModuleUrl, { menuItemID: menuItemID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.menuText(data.MenuText);
            this.viewModel.menuItemID(data.AdminMenuID);
            this.viewModel.url(data.URL);
            this.viewModel.parentID(data.ParentID);
        }
    },
    _onNewModule: function () {
        this.viewModel.menuText('');
        this.viewModel.menuItemID(0);
        this.viewModel.url('');
        this.viewModel.parentID(0);
    },
    _onSaveOrder: function () {
        var orderedItems = $('.leftMenu .menuItem').map(function () {
            return $(this).attr('data-itemID')
        }).get();

        $.ajax(this.settings.saveOrderUrl, {
            type: "POST",
            data: { orderedMenuItemIDs: orderedItems },
            traditional: true,
            success: $.proxy(this._onSaveOrderCompleted, this)
        });
    },
    _onSaveOrderCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Menu items order was successfuly saved.");
                $('#orderIndicator').removeClass('icon-exclamation').addClass('icon-ok');
            }
        }
    },
    _onSubmit: function (form) {
        form = $(form);
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.viewModel.isNewMenuItem() ? this.settings.addModuleUrl : this.settings.updateModuleUrl;

            $.post(url, data, $.proxy(this._onSubmitCompleted, this));
        }
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Menu item was successfuly saved.");
                LeftMenuManager.loadMenu(this.viewModel);
                $.post(this.settings.getModuleUrl, { menuItemID: data.adminMenuID }, $.proxy(this._onEditCompleted, this));
                this._getMenuItems();
            }
        }
    },
    _onToggleVisibility: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var menuItemID = menuItem.attr('data-itemID');

        $.post(this.settings.saveVisibilityUrl, { menuItemID: menuItemID }, $.proxy(this._onToggleVisibilityCompleted, this));
    },
    _onToggleVisibilityCompleted: function (data) {
        if (data && data.success) {
            LeftMenuManager.loadMenu(this.viewModel);
        }
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._getMenuItems();
    }
}

$(function () {
    var AdminMenuManager = new HostPipe.UI.AdminMenuManager();
    AdminMenuManager.initialize();
})