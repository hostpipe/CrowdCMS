/// <reference path="../../../_references.js" />

HostPipe.UI.DiscountManager = function () {
    this.element = $('#DiscountContent');
}

HostPipe.UI.DiscountManager.prototype = {
    _applyViewModel: function () {
        ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            code: ko.observable(''),
            discountID: ko.observable(0),
            desc: ko.observable(''),
            domains: ko.observableArray(),
            domainID: ko.observable(),
            expire: ko.observable(''),
            isEdit: ko.observable(false),
            isPercentage: ko.observable(false),
            lDomainID: ko.observable(),
            sectionHeader: ko.observable("Add Discount"),
            start: ko.observable(''),
            title: ko.observable(''),
            value: ko.observable(0),

            onDelete: $.proxy(this._onDelete, this),
            onEdit: $.proxy(this._onEdit, this),
            onLDomainChange: $.proxy(this._onLDomainChange, this),
            onNewDiscount: $.proxy(this._onNewDiscount, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.discountID.subscribe(function (value) {
            if (value == 0) {
                this.sectionHeader("Add Discount");
                this.isEdit(false);
            }
            else {
                this.sectionHeader("Update Discount");
                this.isEdit(true);
            }
        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addDiscountUrl: this.element.attr("data-addDiscount-url"),
            deleteDiscountUrl: this.element.attr("data-deleteDiscount-url"),
            getDiscountUrl: this.element.attr("data-getDiscount-url"),
            getDomainsUrl: this.element.attr("data-getDomains-url"),
            updateDiscountUrl: this.element.attr("data-updateDiscount-url")
        };
    },
    _getDomains: function (data, event) {
        $.post(this.settings.getDomainsUrl, { currentDomain: true }, $.proxy(this._getDomainsCompleted, this));
    },
    _getDomainsCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.domains(data.domains);
                if (data.selected)
                    this.viewModel.lDomainID(data.selected);
                this._refreshLeftMenu();
            }
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var discountID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (discountID == this.viewModel.discountID())
                this.reload = true;

            $.post(this.settings.deleteDiscountUrl, { discountID: discountID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Discount deleted successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                if (this.reload)
                    this._onNewDiscount();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.discountID(menuItem.attr('data-itemID'));
        this._onEditDiscount(this.viewModel.discountID());
    },
    _onEditDiscount: function (discountID) {
        $.post(this.settings.getDiscountUrl, { discountID: discountID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.discountID(data.DiscountID);
            this.viewModel.domainID(data.DomainID);
            this.viewModel.code(data.Code);
            this.viewModel.title(data.Title);
            this.viewModel.value(data.Value);
            this.viewModel.isPercentage(data.IsPercentage);
            this.viewModel.desc(data.Description);
            this.viewModel.expire(data.Expire);
            this.viewModel.start(data.Start);
        }
    },
    _onLDomainChange: function (data, event) {
        this._refreshLeftMenu();
    },
    _onNewDiscount: function () {
        this.viewModel.discountID(0);
        this.viewModel.domainID(null);
        this.viewModel.code('');
        this.viewModel.title('');
        this.viewModel.value(0);
        this.viewModel.isPercentage(false);
        this.viewModel.desc('');
        this.viewModel.expire('');
        this.viewModel.start('');
    },
    _onSubmit: function () {
        var form = $('#DiscountForm');
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.viewModel.isEdit() ? this.settings.updateDiscountUrl : this.settings.addDiscountUrl;

            $.post(url, data, $.proxy(this._onSubmitCompleted, this));
        }
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Discount saved successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                this._onEditDiscount(data.discountID);
            }
        }
    },
    _refreshLeftMenu: function (search) {
        LeftMenuManager.loadMenu(this.viewModel, null, { domainID: this.viewModel.lDomainID() });
    },
    initialize: function () {
        this._createViewModel();
        this._applyViewModel();
        this._createSettings();
        this._getDomains();
    }
}

$(function () {
    var DiscountManager = new HostPipe.UI.DiscountManager();
    DiscountManager.initialize();
})