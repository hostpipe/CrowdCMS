/// <reference path="../../../_references.js" />

HostPipe.UI.TaxesManager = function () {
    this.element = $('#TaxContent');
}

HostPipe.UI.TaxesManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            isEdit: ko.observable(false),
            percentage: ko.observable(0),
            taxID: ko.observable(0),
            title: ko.observable(''),
            sectionHeader: ko.observable('Add Tax'),

            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onNewTax: $.proxy(this._onNewTax, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.taxID.subscribe(function (value) {
            if (value == 0) {
                this.isEdit(false);
                this.sectionHeader('Add Tax');
            }
            else {
                this.isEdit(true);
                this.sectionHeader('Update Tax');
            }
        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addTaxUrl: this.element.attr('data-addTax-url'),
            deleteTaxUrl: this.element.attr('data-deleteTax-url'),
            getTaxUrl: this.element.attr('data-getTax-url'),
            updateTaxUrl: this.element.attr('data-updateTax-url')
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var taxID = menuItem.attr('data-itemID');

        if (confirm("Are you sure you wish to delete this tax? This action cannot be undone.")) {
            if (taxID == this.viewModel.taxID())
                this.reload = true;

            $.post(this.settings.deleteTaxUrl, { taxID: taxID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Tax deleted successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                if (this.reload)
                    this._onNewTax();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.taxID(menuItem.attr('data-itemID'));

        $.post(this.settings.getTaxUrl, { taxID: this.viewModel.taxID() }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.title(data.title);
            this.viewModel.percentage(data.percentage);
            this.viewModel.taxID(data.taxID);
        }
    },
    _onNewTax: function () {
        this.viewModel.title('');
        this.viewModel.percentage('');
        this.viewModel.taxID('');
    },
    _onSubmit: function () {
        form = $('#TaxForm');
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.viewModel.isEdit() ? this.settings.updateTaxUrl : this.settings.addTaxUrl;

            $.post(url, {
                taxID: this.viewModel.taxID(),
                title: this.viewModel.title(),
                percentage: this.viewModel.percentage()
            }, $.proxy(this._onSubmitCompleted, this));
        }
        return false;
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Tax successfully saved.");
                LeftMenuManager.loadMenu(this.viewModel);
                $.post(this.settings.getTaxUrl, { taxID: data.taxID }, $.proxy(this._onEditCompleted, this));
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
    var TaxesManager = new HostPipe.UI.TaxesManager();
    TaxesManager.initialize();
})