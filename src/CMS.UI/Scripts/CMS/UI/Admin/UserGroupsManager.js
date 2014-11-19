/// <reference path="../../../_references.js" />

HostPipe.UI.UserGroupsManager = function () {
    this.element = $('#UserGroupContent');
}

HostPipe.UI.UserGroupsManager.prototype = {
    _applyViewModel: function () {
        ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            groupID: ko.observable(0),
            groupName: ko.observable(''),
            isNewGroup: ko.observable(true),
            menuItems: ko.observableArray([]),
            permissions: ko.observableArray([]),
            sectionHeader: ko.observable('Add Group'),

            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onNewUserGroup: $.proxy(this._onNewUserGroup, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.groupID.subscribe(function (value) {
            if (value == 0) {
                this.isNewGroup(true);
                this.sectionHeader('Add Group');
            }
            else {
                this.isNewGroup(false);
                this.sectionHeader('Update Group');
            }

        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addGroupUrl: this.element.attr('data-addGroup-url'),
            deleteGroupUrl: this.element.attr('data-deleteGroup-url'),
            getGroupUrl: this.element.attr('data-getGroup-url'),
            updateGroupUrl: this.element.attr('data-updateGroup-url')
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var groupID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            this._delete(groupID, false);
        }
    },
    _delete: function (groupID, force) {
        if (groupID == this.viewModel.groupID())
            this.reload = true;

        $.post(this.settings.deleteGroupUrl, { groupID: groupID, force: force }, $.proxy(this._onDeleteCompleted, this));
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("User group deleted successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                if (this.reload)
                    this._onNewUserGroup();
                this.reload = false;
            }

            if (data.error) {
                var message = "This group contains " + data.users + " admin user" + (data.users > 1 ? "s." : ".") + " Are you sure you wish to delete this group with all users? This action cannot be undone.";
                if (confirm(message)) {
                    this._delete(data.groupID, true);
                }
            }
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var groupID = menuItem.attr('data-itemID');

        $.post(this.settings.getGroupUrl, { groupID: groupID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.groupID(data.GroupID);
            this.viewModel.groupName(data.GroupName);
            this.viewModel.menuItems(data.MenuItems);
            this.viewModel.permissions(data.Permissions);
        }
    },
    _onNewUserGroup: function () {
        $.post(this.settings.getGroupUrl, { groupID: 0 }, $.proxy(this._onEditCompleted, this));
    },
    _onSubmit: function (form) {
        form = $(form);
        var selectedAdminMenu = form.find('#MenuItemsList input:checked').map(function () {
            return parseInt(this.value);
        }).get();

        var selectedPermissions = this.element.find('#PermissionsList input:checked').map(function () {
            return parseInt(this.value);
        }).get();

        form.validate();
        if (form.valid()) {
            var url = this.viewModel.isNewGroup() ? this.settings.addGroupUrl : this.settings.updateGroupUrl;
            $.ajax(url, {
                type: "POST",
                data: {
                    GroupName: this.viewModel.groupName(),
                    UserGroupID: this.viewModel.groupID(),
                    menuItems: selectedAdminMenu,
                    permissions: selectedPermissions
                },
                traditional: true,
                success: $.proxy(this._onSubmitCompleted, this)
            });
        }
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("User group was successfuly saved.");
                LeftMenuManager.loadMenu(this.viewModel);
                $.post(this.settings.getGroupUrl, { groupID: data.userGroupID }, $.proxy(this._onEditCompleted, this));
            }
        }
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();

        this._onNewUserGroup();
    }
}

$(function () {
    var UserGroupsManager = new HostPipe.UI.UserGroupsManager();
    UserGroupsManager.initialize();
})