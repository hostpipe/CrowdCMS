/// <reference path="../../../_references.js" />

HostPipe.UI.ProdAttributesManager = function () {
    this.element = $('#ProdAttributesContent');
}

HostPipe.UI.ProdAttributesManager.prototype = {
    _applyViewModel: function () {
        ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            attributeID: ko.observable(0),
            isEdit: ko.observable(false),
            title: ko.observable(''),
            sectionHeader: ko.observable("Add Attribute"),
            attrValues: ko.observableArray([]),

            onDelete: $.proxy(this._onDelete, this),
            onDeleteValue: $.proxy(this._onDeleteValue, this),
            onEdit: $.proxy(this._onEdit, this),
            onNewAttribute: $.proxy(this._onNewAttribute, this),
            onNewValue: $.proxy(this._onNewValue, this),
            onSaveValue: $.proxy(this._onSaveValue, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.attributeID.subscribe(function (value) {
            if (value == 0) {
                this.sectionHeader("Add Attribute");
                this.isEdit(false);
            }
            else {
                this.sectionHeader("Update Attribute");
                this.isEdit(true);
            }
        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addAttributeUrl: this.element.attr("data-addAttribute-url"),
            addAttValueUrl: this.element.attr("data-addAttValue-url"),
            deleteAttributeUrl: this.element.attr("data-deleteAttribute-url"),
            deleteAttValueUrl: this.element.attr("data-deleteAttValue-url"),
            getAttributeUrl: this.element.attr("data-getAttribute-url"),
            updateAttributeUrl: this.element.attr("data-updateAttribute-url"),
            updateAttValueUrl: this.element.attr("data-updateAttValue-url")
        };
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var attributeID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (attributeID == this.viewModel.attributeID())
                this.reload = true;

            $.post(this.settings.deleteAttributeUrl, { attributeID: attributeID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Attribute deleted successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                if (this.reload)
                    this._onNewAttribute();
                this.reload = false;
            }
        }
    },
    _onDeleteValue: function (data, event) {
        var attValueID = $(event.currentTarget).parent().attr('data-valueID');
        $.post(this.settings.deleteAttValueUrl, { attributeValueID: attValueID }, $.proxy(this._onDeleteValueCompleted, this));
    },
    _onDeleteValueCompleted: function (data) {
        if (data && data.success) {
            alert("Value deleted successfully.");
            this._onEditAttribute(this.viewModel.attributeID());
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.attributeID(menuItem.attr('data-itemID'));
        this._onEditAttribute(this.viewModel.attributeID());
    },
    _onEditAttribute: function (attributeID) {
        $.post(this.settings.getAttributeUrl, { attributeID: attributeID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.attributeID(data.AttributeID);
            this.viewModel.title(data.Title);
            this.viewModel.attrValues(data.Values);
        }
    },
    _onNewAttribute: function () {
        this.viewModel.attributeID(0);
        this.viewModel.title('');
        this.viewModel.attrValues([]);
    },
    _onNewValue: function (data, event) {
        $('#NewValue').dialog({ width: 450, title: "Add Attribute Value", close: $.proxy(this._onNewValueClosed, this), modal: true });
        this.isNewValue = true;
    },
    _onNewValueClosed: function (data) {
        this.isNewValue = false;
    },
    _onSaveValue: function (data, event) {
        var form = $(event.currentTarget).parent();
        form.validate();
        if (form.valid()) {
            var attValueID = form.attr('data-valueID');
            var value = $(event.currentTarget).siblings('input').val();
            var url = this.isNewValue ? this.settings.addAttValueUrl : this.settings.updateAttValueUrl;

            $.post(url, {
                attributeValueID: attValueID,
                value: value,
                attributeID: this.viewModel.attributeID()
            }, $.proxy(this._onSaveValueCompleted, this));
        }
    },
    _onSaveValueCompleted: function (data) {
        if (data && data.success) {
            alert("Value saved successfully.");

            if (this.isNewValue) {
                $('#tbNewValue').val('');
                $("#NewValue").dialog("close");
                this._onEditAttribute(this.viewModel.attributeID());
            }
        }
    },
    _onSubmit: function () {
        var form = $('#DetailsForm');
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.viewModel.isEdit() ? this.settings.updateAttributeUrl : this.settings.addAttributeUrl;

            $.post(url, data, $.proxy(this._onSubmitCompleted, this));
        }
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Attribute saved successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                this._onEditAttribute(data.attributeID);
            }
        }
    },
    initialize: function () {
        this._createViewModel();
        this._applyViewModel();
        this._createSettings();
    }
}

$(function () {
    var ProdAttributesManager = new HostPipe.UI.ProdAttributesManager();
    ProdAttributesManager.initialize();
})