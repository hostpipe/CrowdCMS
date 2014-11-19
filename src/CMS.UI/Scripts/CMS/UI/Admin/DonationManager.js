HostPipe.UI.DonationManager = function () {
    this.element = $("#DonationContent");
}

HostPipe.UI.DonationManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createSettings: function () {
        this.settings = {
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            getDonationUrl: this.element.attr('data-getDonation-url'),
            searchDonationUrl: this.element.attr('data-searchDonations-url')
        };
    },
    _createViewModel: function () {
        this.viewModel = {
            lDomainID: ko.observable(),
            domains: ko.observableArray([]),
            search: ko.observable(),
            allDates: ko.observable(false),
            startDate: ko.observable(),
            endDate: ko.observable(),

            onEdit: $.proxy(this._onEdit, this),
            onLDomainChange: $.proxy(this._refreshLeftMenu, this),
            onFullList: $.proxy(this._onFullList, this),
            onSearch: $.proxy(this._onSearch, this),
            onSearchKeyUp: $.proxy(this._onSearchKeyUp, this),
            onSearchDonation: $.proxy(this._onSearchDonation, this),
        };
    },
        _onChangeStatus: function (data, event) {
        var statusID = parseInt($(event.currentTarget).attr('data-status'));

        if (confirm("Are you sure you want to change the status of this order?")) {
            $.post(this.settings.updateOrderStatusUrl, { orderID: this.orderID, statusID: statusID }, $.proxy(this._onChangeStatusCompleted, this));
        }
    },
    _onChangeStatusCompleted: function (data) {
        if (data && data.success) {
            alert("Status updated successfully.");
            $.post(this.settings.getOrderUrl, { orderID: this.orderID }, $.proxy(this._onEditCompleted, this));
            this._onSearchOrders();
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        if (menuItem && menuItem.length > 0)
            this.orderID = menuItem.attr('data-itemID');

        $.post(this.settings.getDonationUrl, { orderID: this.orderID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.element.find('.mainContainer').html(data);
            this._applyViewModel(this.element.find('.mainContainer'));
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        }
    },
    _getDomainsList: function (data, event) {
        $.post(this.settings.getDomainsListUrl, { currentDomain: true }, $.proxy(this._getDomainsListCompleted, this));
    },
    _getDomainsListCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.domains(data.domains);
                if (data.selected)
                    this.viewModel.lDomainID(data.selected);
                this._refreshLeftMenu();
            }
        }
    },
    _onFullList: function (data, event) {
        this.viewModel.endDate('');
        this.viewModel.startDate('');
        this.viewModel.allDates(false);
        this.viewModel.search('');

        this._refreshLeftMenu();
    },
    _onSearch: function (data, event) {
        $('[data-input-type="date"]').datepicker("option", "disabled", true);
        $('#SearchDialog').dialog("open");
        $('[data-input-type="date"]').datepicker("option", "disabled", false);
    },
    _onSearchKeyUp: function (data, event) {
        if (event.keyCode == 13)
            this._onSearchOrders();
    },
    _onSearchDonation: function (data, event) {
        $('#SearchDialog').dialog("close");

        var startDate = this.viewModel.allDates() ? null : this.viewModel.startDate();
        var endDate = this.viewModel.allDates() ? null : this.viewModel.endDate();

        this._refreshLeftMenu(startDate, endDate);
    },
    _initializeSearchDialog: function () {
        $('#SearchDialog').dialog({
            modal: true,
            autoOpen: false,
            closeOnEscape: true,
            title: "Search Orders",
            width: "auto",
            open: function () {
                $('#tbSearch').focus();
            }
        });
    },
    _insertPartialView: function (data) {
        this.element.find('.mainContainer').html(data);
        this._applyViewModel(this.element.find('.mainContainer'));
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        $.validator.unobtrusive.parse(this.element);
    },
    _refreshLeftMenu: function (startDate, endDate) {
        LeftMenuManager.loadMenu(this.viewModel, this.settings.searchDonationUrl, {
            search: this.viewModel.search(),
            startDate: startDate,
            endDate: endDate,
            domainID: this.viewModel.lDomainID()
        });
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._getDomainsList();

        this._initializeSearchDialog();
    }
}

$(function () {
    var DonationManager = new HostPipe.UI.DonationManager();
    DonationManager.initialize();
})