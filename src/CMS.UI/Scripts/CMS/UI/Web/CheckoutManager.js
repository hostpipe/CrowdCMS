/// <reference path="../../../_references.js" />

function CheckoutManager() {
    this.element = $("#AddressForm");
    this.isSummary = false;

    if (!this.element || this.element.length == 0) {
        this.element = $("#Summary");
        this.isSummary = true;
    }
}

CheckoutManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _clearFields: function () {
        this.viewModel.postCode("");
        this.viewModel.town("");
        this.viewModel.address1("");
        this.viewModel.address2("");
        this.viewModel.address3("");
        this.viewModel.phone("");
        this.viewModel.surName("");
        this.viewModel.firstName("");
        this.viewModel.title("");
        this.viewModel.county("");
    },
    _clearBillingFields: function () {
        this.viewModel.b_countryID(0);
        this.viewModel.b_postCode("");
        this.viewModel.b_town("");
        this.viewModel.b_address1("");
        this.viewModel.b_address2("");
        this.viewModel.b_address3("");
        this.viewModel.b_phone("");
        this.viewModel.b_surName("");
        this.viewModel.b_firstName("");
        this.viewModel.b_title("");
        this.viewModel.b_county("");
    },
    _createSettings: function () {
        this.settings = {
            addDiscountUrl: this.element.attr('data-addDiscount-url'),
            addOrderUrl: this.element.attr('data-addOrder-url'),
            getAddressListUrl: this.element.attr('data-getAddressList-url'),
            getSelectedAddressUrl: this.element.attr('data-getSelectedAddress-url'),
            removeDiscountUrl: this.element.attr('data-removeDiscount-url')
        };
        
        if (this.isSummary) {
            this.isDirect = this.element.attr('data-isDirect').toLowerCase() === "true";
            this.sagePay = this.element.attr('data-sagePay');
        }
    },
    _createViewModel: function () {
        this.viewModel = {
            addresses: ko.observableArray([]),
            addressID: ko.observable(),
            b_addresses: ko.observableArray([]),
            billingAddressID: ko.observable(),

            paymentOption: ko.observable($('#PaymentDomainID').val()),
            isCreditCard: ko.observable($('#PaymentDomainID').val() == this.sagePay && this.isDirect),

            postCode: ko.observable($("#DeliveryPostcode").val()),
            town: ko.observable($("#DeliveryCity").val()),
            address1: ko.observable($("#DeliveryAddress1").val()),
            address2: ko.observable($("#DeliveryAddress2").val()),
            address3: ko.observable($("#DeliveryAddress3").val()),
            phone: ko.observable($("#DeliveryPhone").val()),
            surName: ko.observable($("#DeliverySurname").val()),
            firstName: ko.observable($("#DeliveryFirstName").val()),
            title: ko.observable($("#DeliveryTitle").val()),
            county: ko.observable($("#DeliveryState").val()),

            b_countryID: ko.observable(),
            b_postCode: ko.observable($("#BillingPostcode").val()),
            b_town: ko.observable($("#BillingCity").val()),
            b_address1: ko.observable($("#BillingAddress1").val()),
            b_address2: ko.observable($("#BillingAddress2").val()),
            b_address3: ko.observable($("#BillingAddress3").val()),
            b_phone: ko.observable($("#BillingPhone").val()),
            b_surName: ko.observable($("#BillingSurname").val()),
            b_firstName: ko.observable($("#BillingFirstName").val()),
            b_title: ko.observable($("#BillingTitle").val()),
            b_county: ko.observable($("#BillingState").val()),

            onAddressSelected: $.proxy(this._getSelectedAddress, this),
            onBillingAddressSelected: $.proxy(this._getBillingSelectedAddress, this),

            //discount partial
            isDiscount: ko.observable(this.element.attr('data-promotionalCode') != ""),
            promotionalCode: ko.observable(this.element.attr('data-promotionalCode')),

            onAddDiscount: $.proxy(this._onAddDiscount, this),
            onRemoveDiscount: $.proxy(this._onRemoveDiscount, this)
        };

        this.viewModel.paymentOption.subscribe(function (value) {
            if (value == this.sagePay && this.isDirect)
                this.viewModel.isCreditCard(true);
            else
                this.viewModel.isCreditCard(false);
        }, this);
    },
    _getAddressList: function (event) {
        if (!this.isSummary)
            $.post(this.settings.getAddressListUrl, $.proxy(this._getAddressListCompleted, this));
    },
    _getAddressListCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.addresses(data.d_addresses);
            this.viewModel.b_addresses(data.addresses);
        }
    },
    _getSelectedAddress: function (data, event) {
        if (!this.viewModel.addressID())
            this._clearFields();
        else
            $.post(this.settings.getSelectedAddressUrl, { addressID: this.viewModel.addressID() }, $.proxy(this._getSelectedAddressCompleted, this));
    },
    _getSelectedAddressCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.postCode(data.address.Postcode);
            this.viewModel.town(data.address.Town);
            this.viewModel.address1(data.address.Address1);
            this.viewModel.address2(data.address.Address2);
            this.viewModel.address3(data.address.Address3);
            this.viewModel.phone(data.address.Phone);
            this.viewModel.surName(data.address.Surname);
            this.viewModel.firstName(data.address.FirstName);
            this.viewModel.title(data.address.Title);
            this.viewModel.county(data.address.County);
        }
    },
    _getBillingSelectedAddress: function (data, event) {
        if (!this.viewModel.billingAddressID())
            this._clearBillingFields();
        else
            $.post(this.settings.getSelectedAddressUrl, { addressID: this.viewModel.billingAddressID() }, $.proxy(this._getSelectedBillingAddressCompleted, this));
    },
    _getSelectedBillingAddressCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.b_countryID(data.address.CountryID);
            this.viewModel.b_postCode(data.address.Postcode);
            this.viewModel.b_town(data.address.Town);
            this.viewModel.b_address1(data.address.Address1);
            this.viewModel.b_address2(data.address.Address2);
            this.viewModel.b_address3(data.address.Address3);
            this.viewModel.b_phone(data.address.Phone);
            this.viewModel.b_surName(data.address.Surname);
            this.viewModel.b_firstName(data.address.FirstName);
            this.viewModel.b_title(data.address.Title);
            this.viewModel.b_county(data.address.County);
        }
    },
    _initializeDatePicker: function () {
        if (this.isSummary) {
            $('[data-datepicker]').datepicker({
                changeMonth: true,
                changeYear: true,
                showButtonPanel: true,
                dateFormat: 'mmy',
                yearRange: "-5:+5",
                shortYearCutoff: 99,
                onClose: function (dateText, inst) {
                    var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
                    var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();
                    $(this).datepicker('setDate', new Date(year, month, 1));
                },
                beforeShow: function (input, inst) {
                    if ((datestr = $(this).val()).length > 0) {
                        year = datestr.substring(datestr.length - 2, datestr.length);
                        month = parseInt(datestr.substring(0, datestr.length - 2)) - 1;
                        $(this).datepicker('option', 'defaultDate', new Date('20' + year, month, 1));
                        $(this).datepicker('setDate', new Date("20" + year, month, 1));
                    }
                    $('#ui-datepicker-div').addClass("hideCalendar");
                }
            });
        }
    },
    _onAddDiscount: function () {
        $.post(this.settings.addDiscountUrl, { code: this.viewModel.promotionalCode() }, $.proxy(this._onAddDiscountCompleted, this));
    },
    _onAddDiscountCompleted: function (data) {
        if (data && data.success)
            window.location.reload();
        else
            alert("Your code is incorrect or has expired.");
    },
    _onAddressChecked: function (event) {
        if ($('#BillingAddressTheSame').prop("checked"))
            $('#billingAddress').slideUp();
        else
            $('#billingAddress').slideDown();
    },
    _onRemoveDiscount: function (data, event) {
        $.post(this.settings.removeDiscountUrl, $.proxy(this._onRemoveDiscountCompleted, this));
    },
    _onRemoveDiscountCompleted: function (data) {
        if (data && data.success)
            window.location.reload();
    },
    initialize: function () {
        $('#btnAddDiscount').click($.proxy(this._onAddDiscount, this));
        $('#BillingAddressTheSame').change($.proxy(this._onAddressChecked, this));
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._initializeDatePicker();

        if (!this.isSummary) {
            this._onAddressChecked();
            this._getAddressList();
        }
    }
}

$(function () {
    var manager = new CheckoutManager();
    manager.initialize();
})