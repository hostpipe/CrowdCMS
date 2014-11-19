/// <reference path="../../../_references.js" />

HostPipe.UI.CustomersManager = function () {
    this.element = $('#CustomerContent');
    this.filterDialog = $("#SearchDialog");
    this.addressTable = null;
}

HostPipe.UI.CustomersManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            domains: ko.observableArray([]),
            isSearch: ko.observable(false),
            lDomainID: ko.observable(),
            character: ko.observable('A'),
            characters: ko.observableArray(['A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','W','X','Y','Z']),
            noOrders: ko.observable(true),
            orders: ko.observableArray([]),
            details: ko.observable(),
            sectionHeader: ko.observable(''),
            visibleMain: ko.observable(false),

            btnText: ko.observable('Mark as Dormant'),
            btnClass: ko.observable('btnGreen'),

            showRegistered: ko.observable(false),
            showUnRegistered: ko.observable(false),
            isNewAddress: ko.observable(false),
            countries: ko.observableArray([]),
            isDormant: ko.observable(false),

            onAddAddress: $.proxy(this._onAddAddress, this),
            onCharacterChange: $.proxy(this._onCharacterChange, this),
            onEdit: $.proxy(this._onEdit, this),
            onFilter: $.proxy(this._onFilter, this),
            onFullList: $.proxy(this._onFullList, this),
            onLDomainChange: $.proxy(this._onLDomainChange, this),
            onSaveCustomer: $.proxy(this._onSaveCustomer, this),
            onSearch: $.proxy(this._onSearch, this),
            onSearchCustomers: $.proxy(this._onSearchCustomers, this),
            onSearchKeyUp: $.proxy(this._onSearchKeyUp, this),
            onShowDonationDetails: $.proxy(this._onShowDonationDetails, this),
            onShowOrderDetails: $.proxy(this._onShowOrderDetails, this),
            onUpdateCustomer: $.proxy(this._onUpdateCustomer, this),
            onToggleDormant: $.proxy(this._onToggleDormant, this),
            toggleNewAddressForm: $.proxy(this._toggleNewAddressForm, this),
            onGetCustomerCSV: $.proxy(this._onGetCustomerCSV, this)
        }

        this.viewModel.sectionHeader.subscribe(function (value) {
            if (value.search("Viewing Customer ") == -1)
                this.viewModel.sectionHeader("Viewing Customer " + value);
        }, this);

        this.viewModel.orders.subscribe(function (value) {
            if (value.length > 0)
                this.viewModel.noOrders(false);
            else
                this.viewModel.noOrders(true);
        }, this);
        this.viewModel.details.subscribe(function (value) {
            if (value) {
                if (value.IsDormant()) {
                    this.viewModel.btnText("Remove 'Dormant' flag");
                    this.viewModel.btnClass('btnRed');
                }
                else {
                    this.viewModel.btnText("Mark as 'Dormant'");
                    this.viewModel.btnClass('btnGreen');
                }

                this.viewModel.visibleMain(true);
            }
            else
                this.viewModel.visibleMain(false);
        }, this);
    },
    _createSettings: function () {
        this.settings = {
            addAddressUrl: this.element.attr('data-addAddress-url'),
            getAddressesUrl: this.element.attr('data-getAddresses-url'),
            getCustomerUrl: this.element.attr('data-getCustomer-url'),
            getCountriesUrl: this.element.attr('data-getCountries-url'),
            getOrderDetailsUrl: this.element.attr('data-getOrderDetails-url'),
            getDomainsUrl: this.element.attr('data-getDomains-url'),
            searchCustomersUrl: this.element.attr('data-searchCustomers-url'),
            updateAddressUrl: this.element.attr('data-updateAddress-url'),
            updateCustomerUrl: this.element.attr('data-updateCustomer-url'),
            toggleDormantFlagUrl: this.element.attr('data-toggleDormantFlag-url'),
            getcustomerstocsv: this.element.attr('data-getcustomerstocsv-url')
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
    _initializeDataTable: function () {
        this.addressTable = $('#AddressTable').dataTable({
            "sAjaxSource": this.settings.getAddressesUrl,
            "sServerMethod": "POST",
            "fnServerParams": $.proxy(function (aoData) {
                aoData.push({ "name": "customerID", "value": (this.viewModel.details() ? this.viewModel.details().CustomerID() : 0) });
            }, this),
            "aoColumnDefs": [
                { "bVisible": false, "aTargets": ["indexCol"] },
                { "sClass": "editText", "aTargets": ["editText"] },
                { "sClass": "editList", "aTargets": ["editList"] }
            ],
            "bFilter": false,
            "bInfo": false,
            "bPaginate": false,
            "bSort": false,
            "bSortClasses": false,
            "fnDrawCallback": $.proxy(this._tableRendered, this)
        });
    },
    _initializeSearchDialog: function () {
        this.filterDialog.dialog({
            modal: true,
            autoOpen: false,
            closeOnEscape: true,
            title: "Search Customers",
            minWidth: 300
        });
    },
    _onAddAddress: function (data, event) {
        var form = $('#CustomerAddressForm');
        form.validate();

        if (form.valid()) {
            var data = form.serializeArray();
            data.push({ name: "customerID", value: this.viewModel.details().CustomerID() });

            $.post(this.settings.addAddressUrl, data, $.proxy(this._onAddAddressCompleted, this));
        }
    },
    _onAddAddressCompleted: function (data) {
        if (data) {
            if (data.success) {
                this._reloadTable();
                this._resetNewAddressForm();
            }
        }
    },
    _onCharacterChange: function (data, event) {
        this._refreshLeftMenu();
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.customerID = menuItem.attr('data-itemID');

        $.post(this.settings.getCustomerUrl, { customerID: this.customerID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data && data.success) {
            if (data.customer) {
                this.viewModel.details(ko.mapping.fromJS(data.customer.Details));
                this.viewModel.countries(data.customer.Countries);
                this.viewModel.sectionHeader(data.customer.Name);
                this.viewModel.orders(data.customer.Orders);
                this._resetNewAddressForm();
            }
            this._reloadTable();
        }
    },
    _onFilter: function () {
        this.filterDialog.dialog("open");
    },
    _onFullList: function () {
        this.viewModel.isSearch(false);
        this.viewModel.showRegistered(false);
        this.viewModel.showUnRegistered(false);
        this.viewModel.isDormant(false);
        this.viewModel.character(null);
        $('#tbSearch').val('');
        this._refreshLeftMenu();
    },
    _onLDomainChange: function (data, event) {
        this._refreshLeftMenu();
    },
    _onSaveCustomer: function (data, event) {
        var form = $('#CustomerForm');
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            $.post(this.settings.updateCustomerUrl, data, $.proxy(this._onSaveCustomerCompleted, this));
        }

        return false;
    },
    _onSaveCustomerCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Customer saved successfully.");
                this._refreshLeftMenu();
            }
            else
                alert("Customer was not saved. " + data.error);
        }
    },
    _onSearch: function () {
        this.viewModel.isSearch(true);
        this._refreshLeftMenu();
    },
    _onSearchCustomers: function () {
        this.filterDialog.dialog("close");
        this._onSearch();
    },
    _onSearchKeyUp: function (data, event) {
        if (event.keyCode == 13)
            this._onSearch();
    },
    _onShowDonationDetails: function (data, event) {
        var orderID = $(event.currentTarget).attr('data-orderID');
        $.post(this.settings.getOrderDetailsUrl, { orderID: orderID, isDonation: true }, $.proxy(this._onShowOrderDetailsCompleted, this));
    },
    _onShowOrderDetails: function (data, event) {
        var orderID = $(event.currentTarget).attr('data-orderID');
        $.post(this.settings.getOrderDetailsUrl, { orderID: orderID }, $.proxy(this._onShowOrderDetailsCompleted, this));
    },
    _onShowOrderDetailsCompleted: function (data) {
        if (data) {
            $("#OrderDetailsWindow").html(data).dialog("open");
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this, { element: $("#OrderDetailsWindow") });
        }
    },
    _onToggleDormant: function (data, event) {
        $.post(this.settings.toggleDormantFlagUrl, { customerID: this.viewModel.details().CustomerID() }, $.proxy(this._onToggleDormantCompleted, this));
    },
    _onToggleDormantCompleted: function (data) {
        if (data && data.success) {
            $.post(this.settings.getCustomerUrl, { customerID: this.viewModel.details().CustomerID() }, $.proxy(this._onEditCompleted, this));
        }
    },
    _refreshLeftMenu: function () {
        LeftMenuManager.loadMenu(this.viewModel, this.settings.searchCustomersUrl, {
            search: $('#tbSearch').val(),
            character: this.viewModel.character(),
            isRegistered: this.viewModel.showRegistered(),
            isUnRegistered: this.viewModel.showUnRegistered(),
            domainID: this.viewModel.lDomainID(),
            isDormant: this.viewModel.isDormant()
        });
    },
    _reloadTable: function () {
        this.addressTable.fnReloadAjax();
    },
    _resetNewAddressForm: function () {
        $('#CustomerAddressForm').resetForm();
        this.viewModel.isNewAddress(false);
    },
    _saveTableRow: function (rowID) {
        var data = this.addressTable.fnGetData(rowID);

        $.post(this.settings.updateAddressUrl, { 
            AddressID: data[0],
            Title: data[1],
            FirstName: data[2],
            Surname: data[3],
            Phone: data[4],
            Address1: data[5],
            Address2: data[6],
            Address3: data[7],
            Town: data[8],
            County: data[9],
            Postcode: data[10],
            Country: data[11],
            CountryID: 0, 
            customerID: this.viewModel.details().CustomerID()
        }, $.proxy(this._saveTableRowCompleted, this));
    },
    _saveTableRowCompleted: function (data) {
        if (data) {
            if (!data.success) {
                alert("Address was not saved. " + data.error);
            }
        }
    },
    _tableRendered: function () {
        var table = this.addressTable;
        var manager = this;

        if (table) {
            table.$('td.editText').editable(function (value, settings) {
                var aPos = table.fnGetPosition(this);
                table.fnUpdate(value, aPos[0], aPos[2]);
                manager._saveTableRow(aPos[0]);
                return (value);
            }, {
                "placeholder": "",
                "onblur": "submit",
                "event": "dblclick"
            });

            table.$('td.editList').editable(function (value, settings) {
                var aPos = table.fnGetPosition(this);
                table.fnUpdate(value, aPos[0], aPos[2]);
                manager._saveTableRow(aPos[0]);
                return (value);
            }, {
                "loadurl": this.settings.getCountriesUrl,
                "loaddata" : function (value, settings) {
                    return { domainID: manager.viewModel.details().DomainID() };
                },
                "loadtype": "POST",
                "type": "select",
                "placeholder": "",
                "onblur": "submit",
                "event": "dblclick"
            });
        }
    },
    _toggleNewAddressForm: function () {
        this.viewModel.isNewAddress(!this.viewModel.isNewAddress());
    },
    _onGetCustomerCSV: function(data, event){

        //var url = this.settings.getcustomerstocsv;

        //var customParams ={
        //        search: $('#tbSearch').val(),
        //        isRegistered: this.viewModel.showRegistered(),
        //        isUnRegistered: this.viewModel.showUnRegistered(),
        //        domainID: this.viewModel.lDomainID(),
        //        isDormant: this.viewModel.isDormant()
        //    };

        //$(event.currentTarget).attr('href', url);

        //var params = {};
        //params.url = window.location.pathname;

        //$.extend(params, customParams);
        //$.post(url, params);

        // return true;

        var url = this.settings.getcustomerstocsv + "?" + this._serialize(
            {
                search: $('#tbSearch').val(),
                isRegistered: this.viewModel.showRegistered() ,
                isUnRegistered: this.viewModel.showUnRegistered() ,
                domainID: this.viewModel.lDomainID(),
                isDormant: this.viewModel.isDormant() 
            });

        $(event.currentTarget).attr('href', url);
        return true;
    },
    _serialize: function (obj) {
        var str = [];
        for (var p in obj) {
            if (obj.hasOwnProperty(p)) {
                str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
            }
        }
        return str.join("&");
    },
    initialize: function () {
        this._createViewModel();
        this._applyViewModel();
        this._createSettings();
        this._getDomains();
        this._initializeSearchDialog();
        this._initializeDataTable();

        $("#OrderDetailsWindow").dialog({
            title: "Details",
            modal: true,
            closeOnEscape: true,
            autoOpen: false,
            width: 1300
        });
    }
}

$(function () {
    var CustomersManager = new HostPipe.UI.CustomersManager();
    CustomersManager.initialize();
})