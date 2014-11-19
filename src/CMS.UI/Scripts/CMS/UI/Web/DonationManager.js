/// <reference path="../../../_references.js" />

var HostPipe = {};
HostPipe.UI = {};
HostPipe.UI.DonationManager = function () {
    this.element = $("#Donation");
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

        };

        var attrDirect = this.element.attr('data-isDirect');
        this.isDirect = attrDirect ? attrDirect.toLowerCase() === "true" : false;
        this.sagePay = this.element.attr('data-sagePay');
    },
    _createViewModel: function () {
        this.viewModel = {
            amount: ko.observable($('#Amount').val()),
            donationAmount: ko.observable($('[name="DonationAmount"]:checked').val()),
            isCreditCard: ko.observable($('#PaymentDomainID').val() == this.sagePay && this.isDirect),
            donationType: ko.observable("Single"),
            paymentOption: ko.observable($('#PaymentDomainID').val()),

            onCustomAmount: $.proxy(this._onCustomAmount, this),
            onUpdateValue: $.proxy(this._onUpdateValue, this)
        };

        this.viewModel.paymentOption.subscribe(function (value) {
            if (value == this.sagePay && this.isDirect)
                this.viewModel.isCreditCard(true);
            else
                this.viewModel.isCreditCard(false);
        }, this);

        this.viewModel.donationAmount.subscribe(function (value) {
            if (value && value != 0)
                this.viewModel.amount(value);
        }, this);
    },
    _initializeDatePicker: function () {
        if ($('[data-input-type="date"]').length > 0) {
            $('[data-input-type="date"]').datepicker({
                dateFormat: "dd/mm/yy"
            });
        }

        if ($('[data-datepicker]').length > 0) {
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
    _onCustomAmount: function (data, event) {
        var link = $('#btnDonate');
        var url = link.attr("href");
        var amount = parseFloat($('#iCustomAmount').val());
        url = url.substring(0, url.lastIndexOf('/') + 1) + (isNaN(amount) ? "0" : amount);
        link.attr("href", url);
    },
    _onUpdateValue: function (data, event) {
        var value = $(event.currentTarget).val();
        this.viewModel.donationAmount(null);
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._initializeDatePicker();
    }
}
$(function () {
    var DonationManager = new HostPipe.UI.DonationManager();
    DonationManager.initialize();
})