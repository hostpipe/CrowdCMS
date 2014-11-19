/// <reference path="../../../_references.js" />
var testing;
HostPipe.UI.FormsManager = function () {
    this.element = $('#FormsContent');
}

HostPipe.UI.FormsManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createSettings: function () {
        this.settings = {
            addFormUrl: this.element.attr('data-addForm-url'),
            deleteFormUrl: this.element.attr('data-deleteForm-url'),
            deleteFormItemUrl: this.element.attr('data-deleteFormItem-url'),
            deleteFormItemValueUrl: this.element.attr('data-deleteFormItemValue-url'),
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            getFormUrl: this.element.attr('data-getForm-url'),
            getFormItemUrl: this.element.attr('data-getFormItem-url'),
            saveFormUrl: this.element.attr('data-saveForm-url'),
            saveFormItemUrl: this.element.attr('data-saveFormItem-url'),
            saveFormItemOrderUrl: this.element.attr('data-saveFormItemOrder-url'),
            saveFormItemVisibilityUrl: this.element.attr('data-saveFormItemVisibility-url'),
            saveFormItemValuesUrl: this.element.attr('data-saveFormItemValues-url'),
            getSectionsListUrl: this.element.attr('data-getSections-url')
        };

        this.TextBox = this.element.attr('data-textBox');
        this.TextArea = this.element.attr('data-textArea');
        this.Subscribe = this.element.attr('data-subscribe');
        this.Datebox = this.element.attr('data-datebox');
        this.Divider = this.element.attr('data-divider');
    },
    _createViewModel: function () {
        this.viewModel = {
            isEdit: ko.observable(false),
            isEditFormItem: ko.observable(false),
            formItemID: ko.observable(0),
            formID: ko.observable(0),
            sectionHeader: ko.observable(this.formID == 0 ? "Add New Form" : "Update Form"),
            sections: ko.observable([]),
            sectionID: ko.observable(),
            domains: ko.observableArray([]),
            lDomainID: ko.observable(),

            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onDeleteFormItemValue: $.proxy(this._onDeleteFormItemValue, this),
            onSubmit: $.proxy(this._onSubmit, this),
            onToggleVisibility: $.proxy(this._onToggleVisibility, this),
            onSaveOrder: $.proxy(this._onSaveOrder, this),
            onSaveFormItemValue: $.proxy(this._onSaveFormItemValue, this),
            onNewForm: $.proxy(this._onNewForm, this),
            onNewFormItem: $.proxy(this._onNewFormItem, this),
            onNewFormItemValue: $.proxy(this._onNewFormItemValue, this),
            onFormItemTypeChanged: $.proxy(this._onFormItemTypeChanged, this),
            onDomainChanged: $.proxy(this._onDomainChanged, this),
            onLDomainChange: $.proxy(this._reloadLeftMenu, this),
            onGetSections: $.proxy(this._getSections, this)
        }
    },

    _getSections: function () {
        $.post(this.settings.getSectionsListUrl, { domainID: $('#ddlDomain').val(), sectionID: this.sectionID }, $.proxy(this._getSectionsCompleted, this));
    },
    _getSectionsCompleted: function (data) {
        if (data && data.sections) {
            this.viewModel.sections(data.sections);
            this.viewModel.sectionID($('#currentSitemapID').val());
            //this.viewModel.sections.unshift("[Select Page]", "0");
        }
    },

    _isFormType: function (menuitem) {
        return ($(menuitem).attr('class').indexOf('level1') >= 0);
    },
    _initializeChangeEvent: function () {
    },
    _insertPartialView: function (data) {
        this.element.find('.mainContainer').html(data);
        this._applyViewModel(this.element.find('.mainContainer'));
        
        if ($('#FormsContent').length > 0) {
            this._initializeChangeEvent();
        }
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        $.validator.unobtrusive.parse(this.element);
    },
    _onEdit: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var id = menuItem.attr('data-itemID');

        if (this._isFormType(menuItem)) {
            this.viewModel.formID(id);
            $.post(this.settings.getFormUrl, { formID: id }, $.proxy(this._onEditFormCompleted, this));
        }
        else {
            this.viewModel.formItemID(id);
            $.post(this.settings.getFormItemUrl, { formItemID: id }, $.proxy(this._onEditFormItemCompleted, this));
        }
    },
    _onEditFormCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.viewModel.sectionHeader("Update Form");
            this._insertPartialView(data);
            this._getSections();
        }
    },
    _onEditFormItemCompleted: function (data) {
        if (data) {
            this.viewModel.isEditFormItem(true);
            this.viewModel.sectionHeader("Update Form Item");

            this._insertPartialView(data);
            this._onFormItemTypeChanged();
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var id = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (this._isFormType(menuItem)) {
                if (id == this.viewModel.formID())
                    this.reload = true;
                $.post(this.settings.deleteFormUrl, { formID: id }, $.proxy(this._onDeleteFormCompleted, this));
            }
            else {
                if (id == this.viewModel.formItemID())
                    this.reload = true;
                $.post(this.settings.deleteFormItemUrl, { formItemID: id }, $.proxy(this._onDeleteFormItemCompleted, this));
            }
        }
    },
    _onDeleteFormCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Form deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewForm();
                this.reload = false;
            }
        }
    },
    _onDeleteFormItemCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Form item deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewFormItem();
                this.reload = false;
            }
        }
    },
    _onDeleteFormItemValue: function (data, event) {
        var valueID = $(event.currentTarget).parent().attr('data-valueID');
        var itemID = $(event.currentTarget).parent().attr('data-itemID');

        if (confirm("Are you sure, you want to delete this value?")) {
            $.post(this.settings.deleteFormItemValueUrl, { formItemValueID: valueID }, $.proxy(this._onDeleteFormItemValueCompleted, this));
        }
    },
    _onDeleteFormItemValueCompleted: function (data) {
        if (data && data.success) {
            alert("Form Item Value deleted successfully.");
            $.post(this.settings.getFormItemUrl, { formItemID: this.viewModel.formItemID() }, $.proxy(this._onEditFormItemCompleted, this));
        }
    },
    _onToggleVisibility: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var formItemID = menuItem.attr('data-itemID');
        $.post(this.settings.saveFormItemVisibilityUrl, { formItemID: formItemID }, $.proxy(this._onToggleVisibilityCompleted, this));
    },
    _onToggleVisibilityCompleted: function (data) {
        if (data && data.success) {
            this._reloadLeftMenu();
        }
    },
    _onSaveFormItemValue: function (data, event) {
        var form = $(event.currentTarget).parent();
        $.validator.unobtrusive.parse(form);
        form.validate();

        if (form.valid()) {
            //this.viewModel.formItemID(form.attr('data-itemID'));

            $.post(this.settings.saveFormItemValuesUrl,
            {
                value: form.find('[name*="Value"]').val(),
                text: form.find('[name*="Text"]').val(),
                selected: form.find('[name*="Selected"]').prop('checked'),
                order: form.find('[name*="Order"]').val(),
                formItemID: this.viewModel.formItemID(),
                formItemValueID: form.attr('data-valueID')
            }, $.proxy(this._onSaveFormItemValueCompleted, this));
        }
    },
    _onSaveFormItemValueCompleted: function (data) {
        if (data && data.success) {
            alert("Form Item Value saved successfully.");

            if (this.isNewValue) {
                $(".ui-dialog-content").dialog().dialog("close");
                $.post(this.settings.getFormItemUrl, { formItemID: this.viewModel.formItemID() }, $.proxy(this._onEditFormItemCompleted, this));
            }
        }
    },
    _onSaveOrder: function () {
        var orderedItems = $('.leftMenu .menuItem').map(function () {
            return $(this).attr('data-itemID')
        }).get();

        testing = orderedItems;
        $.ajax(this.settings.saveFormItemOrderUrl, {
            type: "POST",
            data: { orderedFormItemIDs: orderedItems },
            traditional: true,
            success: $.proxy(this._onSaveOrderCompleted, this)
        });
    },
    _onSaveOrderCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Form items order was successfuly saved.");
                $('#orderIndicator').removeClass('icon-exclamation').addClass('icon-ok');
            }
        }
    },
    _onNewForm: function (data, event) {
        $.post(this.settings.getFormUrl, { formID: 0 }, $.proxy(this._onNewFormCompleted, this));
    },
    _onNewFormCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(false);
            this.viewModel.sectionHeader("Add New Form");
            this._insertPartialView(data);
            this._getSections();
        }
    },
    _onNewFormItem: function () {
        $.post(this.settings.getFormItemUrl, { formItemID: 0 }, $.proxy(this._onNewFormItemCompleted, this));
    },
    _onNewFormItemCompleted: function (data) {
        if (data) {
            this.viewModel.isEditFormItem(false);
            this.viewModel.sectionHeader("Add New Form Item");
            this._insertPartialView(data);
        }
    },
    _onNewFormItemValue: function () {
        $('#NewValue form').attr('data-valueID', 0);
        $('#NewValue form').attr('data-itemID', $('#hfFormItemID').val());
        $('#NewValue').dialog({
            width: 810,
            title: "Add Item Value",
            modal: true,
            close: $.proxy(this._onNewFormItemValueClosed, this)
        });
        this.isNewValue = true;
    },
    _onNewFormItemValueClosed: function (data) {
        this.isNewValue = false;
        $('#NewValue').find('[name="Value"]').val('');
        $('#NewValue').find('[name="Text"]').val('');
        $('#NewValue').find('[name="Selected"]').prop('checked', false);
        $('#NewValue').find('[name="Order"]').val('');
    },
    _onSubmit: function (form) {
        $.validator.unobtrusive.parse(this.element);

        var detailsForm = $('#DetailsForm');
        if (detailsForm[0] != undefined) {
            detailsForm.validate();
            if (detailsForm.valid()) {
                var data = detailsForm.serializeArray();
                $.post(this.settings.saveFormUrl, data, $.proxy(this._onSubmitFormCompleted, this));
            }
        }

        var itemsForm = $('#ItemsForm');
        if (itemsForm[0] != undefined) {
            itemsForm.validate();
            if (itemsForm.valid()) {
                var data = itemsForm.serializeArray();
                testing = data;
                $.post(this.settings.saveFormItemUrl, data, $.proxy(this._onSubmitFormItemCompleted, this));
            }
        }
    },
    _onFormItemTypeChanged: function () {
        var type = $("#ddlItemTypes").val();
        if (type == this.TextBox || type == this.TextArea || type == this.Subscribe || type == this.Datebox || type == this.Divider) {
            $("#ItemValues").hide();
        }
        else {
            $("#ItemValues").show();
        }
    },
    _onDomainChanged: function () {
        $.post(this.settings.getFormItemUrl, { formItemID: 0, formID: 0, domainID: $('#ddlDomain').val() }, $.proxy(this._onDomainChangedCompleted, this));
    },
    _onDomainChangedCompleted: function (data) {
        this._insertPartialView(data);
        this._onFormItemTypeChanged();
    },
    _onSubmitFormCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Form was successfuly saved.");
                this._reloadLeftMenu();
                $.post(this.settings.getFormUrl, { formID: data.formID }, $.proxy(this._onEditCompleted, this));
            }
        }
    },
    _onSubmitFormItemCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Form item was successfuly saved.");
                this._reloadLeftMenu();
                this.viewModel.formItemID(data.formItemID);
                $.post(this.settings.getFormItemUrl, { formItemID: data.formItemID }, $.proxy(this._onEditCompleted, this));
            }
        }
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
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._getDomainsList();
        this._onNewForm();
    }
}

$(function () {
    var FormsManager = new HostPipe.UI.FormsManager();
    FormsManager.initialize();
})