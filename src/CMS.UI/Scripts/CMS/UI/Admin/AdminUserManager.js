/// <reference path="../../../_references.js" />

HostPipe.UI.AdminUserManager = function () {
    this.element = $('#UserContent');
    this.isPassword = false;
}

HostPipe.UI.AdminUserManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            email: ko.observable(''),
            isNewUser: ko.observable(true),
            password: ko.observable(''),
            sectionButton: ko.observable("Add User +"),
            sectionHeader: ko.observable("Add User"),
            selectedGroupID: ko.observable(0),
            userName: ko.observable(''),
            userID: ko.observable(0),

            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onGetPassword: $.proxy(this._getPassword, this),
            onNewUser: $.proxy(this._onNewUser, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.userID.subscribe($.proxy(function (value) {
            this.isPassword = false;
            this.viewModel.password('');
            $('#Password').addClass("hidden");

            if (value > 0) {
                this.viewModel.isNewUser(false);
                this.viewModel.sectionButton("Update User +");
                this.viewModel.sectionHeader("Update User");
            }
            else {
                this.viewModel.isNewUser(true);
                this.viewModel.sectionButton("Add User +");
                this.viewModel.sectionHeader("Add User");
            }

        }, this));

        this.viewModel.password.subscribe($.proxy(function (value) {
            if (this.isPassword && value.length == 0) {
                $('span[data-valmsg-for="Password"]').removeClass('field-validation-valid').addClass('field-validation-error').text("Password is required.");
            }
            else {
                $('span[data-valmsg-for="Password"]').removeClass('field-validation-error').addClass('field-validation-valid').text('');
            }

        }, this));
    },
    _createSettings: function () {
        this.settings = {
            addUserUrl: this.element.attr('data-addUser-url'),
            deleteUserUrl: this.element.attr('data-deleteUser-url'),
            getUserUrl: this.element.attr('data-getUser-url'),
            getPasswordUrl: this.element.attr('data-getPassword-url'),
            updateUserUrl: this.element.attr('data-updateUser-url')
        }
    },
    _getPassword: function () {
        $.post(this.settings.getPasswordUrl, $.proxy(this._getPasswordCompleted, this));
    },
    _getPasswordCompleted: function (data) {
        if (data) {
            $('#Password').removeClass("hidden");
            this.isPassword = true;
            this.viewModel.password(data.password);
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var userID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (userID == this.viewModel.userID())
                this.reload = true;

            $.post(this.settings.deleteUserUrl, { userID: userID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("User deleted successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                if (this.reload)
                    this._onNewUser();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var userID = menuItem.attr('data-itemID');

        $.post(this.settings.getUserUrl, { userID: userID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.userID(data.UserID);
            this.viewModel.email(data.Email);
            this.viewModel.userName(data.UserName);
            this.viewModel.selectedGroupID(data.UserGroupID);
        }
    },
    _onNewUser: function () {
        this.viewModel.userID(0);
        this.viewModel.email('');
        this.viewModel.userName('');
        this.viewModel.selectedGroupID(0);
    },
    _onSubmit: function (form) {
        var form = $(form);
        form.validate();

        if (this.viewModel.isNewUser() && (!this.isPassword || this.viewModel.password().length == 0)) {
            alert("Please generate a password for new user.");
            return;
        }

        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.viewModel.isNewUser() ? this.settings.addUserUrl : this.settings.updateUserUrl;
            $.post(url, data, $.proxy(this._onSubmitCompleted, this));
        }
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("User was successfuly saved.");
                LeftMenuManager.loadMenu(this.viewModel);
                $.post(this.settings.getUserUrl, { userID: data.userID }, $.proxy(this._onEditCompleted, this));
            }
            else if (data.error)
                alert(data.error);
        }
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
    }
}

$(function () {
    var AdminUserManager = new HostPipe.UI.AdminUserManager();
    AdminUserManager.initialize();
})