/// <reference path="../../../_references.js" />

function BasketManager() {
    this.element = $('#Basket');
}

BasketManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            basketItems: ko.observableArray([]),
            countries: ko.observableArray([{ Text: "Select country", Value: "0"}]),
            countryID: ko.observable("0"),
            deliveryCost: ko.observable(''),
            discountAmount: ko.observable(''),
            euVAT: ko.observable(''),
            isBasket: ko.observable(false),
            isDeliverable: ko.observable(true),
            isSingleCountry: ko.observable(false),
            isSinglePostage: ko.observable(false),
            postages: ko.observableArray([{ Text: "Select delivery", Value: "0"}]),
            postageID: ko.observable("0"),
            totalPrice: ko.observable(''),
            totalTax: ko.observable(''),

            onAmountChanged: $.proxy(this._onNewAmount, this),
            onProceed: $.proxy(this._onProceed, this),
            onRemoveItem: $.proxy(this._onRemoveFromBasket, this),

            //discount partial
            isDiscount: ko.observable(false),
            promotionalCode: ko.observable(''),

            onAddDiscount: $.proxy(this._onAddDiscount, this),
            onRemoveDiscount: $.proxy(this._onRemoveDiscount, this)
        }

        this.viewModel.countryID.subscribe(function (value) {
            if (value && this.viewModel.isDeliverable())
                this._getAvailablePostage(value);
        }, this);

        this.viewModel.postageID.subscribe(function (value) {
            if (value && this.viewModel.isDeliverable())
                this._updateBasketPostage(value);
        }, this);

        this.viewModel.countries.subscribe(function (value) {
            if (value.length == 2) {
                this.countryID(value[1].Value);
                this.isSingleCountry(true);
            }
            else {
                this.isSingleCountry(false);
            }
        }, this.viewModel);

        this.viewModel.postages.subscribe(function (value) {
            if (value.length == 2) {
                this.postageID(value[1].Value);
                this.isSinglePostage(true);
            }
            else {
                this.isSinglePostage(false);
            }
        }, this.viewModel);
    },
    _initializeSettings: function () {
        this.settings = {
            addDiscountUrl: this.element.attr('data-addDiscount-url'),
            canProceedToCheckoutUrl: this.element.attr('data-canProceedToCheckout-url'),
            getAvailablePostageUrl: this.element.attr('data-getAvailablePostage-url'),
            getBasketDataUrl: this.element.attr('data-GetBasketData-url'),
            getPricesUrl: this.element.attr('data-getPrices-url'),
            onProceedUrl: this.element.attr('data-proceed-url'),
            removeDiscountUrl: this.element.attr('data-removeDiscount-url'),
            removeFromBasketUrl: this.element.attr('data-removeFromBasket-url'),
            updateBasketPostageUrl: this.element.attr('data-updateBasketPostage-url'),
            updateProductAmountUrl: this.element.attr('data-updateProductAmount-url')
        }
    },
    _getAvailablePostage: function (countryID) {
        $.post(this.settings.getAvailablePostageUrl, { countryID: countryID }, $.proxy(this._getAvailablePostageCompleted, this));
    },
    _getAvailablePostageCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.postages(data.postages);
            if (data.postages.length > 2) // insert postage id from server if array has more postages then one
                this.viewModel.postageID(data.postageID.toString());
        } else {
            this.viewModel.postages([{ Text: "Select delivery", Value: "0"}]);
            this.viewModel.postageID("0");
        }
    },
    _getBasket: function () {
        Core.isBusy(true, this.element);
        $.ajax(this.settings.getBasketDataUrl, {
            cache: false,
            type: "POST",
            data: { date: new Date() }, // not used in server, only in order to prevent cashing post request
            success: $.proxy(this._getBasketCompleted, this)
        });
    },
    _getBasketCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.isBasket(true);
                this.viewModel.basketItems(ko.mapping.fromJSON(JSON.stringify(data.basketItems))());
                this.viewModel.isDiscount(data.isDiscount);
                this.viewModel.promotionalCode(data.promotionalCode);
                this.viewModel.discountAmount(data.discountAmount);
                this.viewModel.isDeliverable(data.isDeliverable);
                this.viewModel.deliveryCost(data.deliveryCost);
                this.viewModel.euVAT(data.euVAT);
                this.viewModel.totalTax(data.totalTax);
                this.viewModel.totalPrice(data.totalPrice);
                this.viewModel.countries(data.countries);
                this.viewModel.countryID(data.countryID);
            }
            else {
                this.viewModel.isBasket(false);
            }
        }
        Core.isBusy(false, this.element);
    },
    _getPrices: function () {
        $.ajax(this.settings.getPricesUrl, {
            cache: false,
            type: "POST",
            data: { date: new Date() }, // not used in server, only in order to prevent cashing post request
            success: $.proxy(this._getPricesCompleted, this)
        });
    },
    _getPricesCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.deliveryCost(data.deliveryCost);
            this.viewModel.discountAmount(data.discountAmount);
            this.viewModel.totalPrice(data.totalPrice);
            this.viewModel.totalTax(data.totalTax);
        }
    },
    _onAddDiscount: function () {
        $.post(this.settings.addDiscountUrl, { code: this.viewModel.promotionalCode() }, $.proxy(this._onAddDiscountCompleted, this));
    },
    _onAddDiscountCompleted: function (data) {
        if (data && data.success)
            this._getBasket();
        else
            alert("Your code is incorrect or has expired.");
    },
    _onNewAmount: function (data, event) {
        var contentID = data.BasketContentID();
        var amount = parseInt(data.Quantity());
        var maxAmount = parseInt(data.Max());
        if (maxAmount < amount) {
            alert("Quantity exceeded. Maximum quantity of product in stock is " + maxAmount);
            data.Quantity(maxAmount);
            amount = maxAmount;
        }
        else if (amount < 1) {
            alert("Can not order less then 1 product.");
            data.Quantity(1); // row.find('input[name="tbAmount"]').val(1);
            amount = 1;
        }
        $.post(this.settings.updateProductAmountUrl, { contentID: contentID, amount: isNaN(amount) ? 1 : amount }, $.proxy(this._onNewAmountCompleted, this));
    },
    _onNewAmountCompleted: function (data) {
        if (data && data.success) {
            this._getBasket();
        }
    },
    _onProceed: function (event) {
        if (this.viewModel.isDeliverable() && (!this.viewModel.countryID() || this.viewModel.countryID() == 0)) {
            alert("Please select delivery country.");
            return;
        }

        if (this.viewModel.isDeliverable() && (!this.viewModel.postageID() || this.viewModel.postageID() == 0)) {
            alert("Please select delivery option.");
            return;
        }

        $.post(this.settings.canProceedToCheckoutUrl, { EUVAT: this.viewModel.euVAT() }, $.proxy(this._onProceedCompleted, this));
    },
    _onProceedCompleted: function (data) {
        if (data) {
            if (data.success)
                window.location.assign(this.settings.onProceedUrl);
            else if (data.error)
                alert(data.error);
        }
    },
    _onRemoveDiscount: function (data, event) {
        $.post(this.settings.removeDiscountUrl, $.proxy(this._onRemoveDiscountCompleted, this));
    },
    _onRemoveDiscountCompleted: function (data) {
        if (data && data.success)
            this._getBasket();
    },
    _onRemoveFromBasket: function (data, event) {
        var contentID = data.BasketContentID();

        if (confirm("Are you sure you want to remove this product from basket?"))
            $.post(this.settings.removeFromBasketUrl, { contentID: contentID }, $.proxy(this._onRemoveFromBasketCompleted, this));
    },
    _onRemoveFromBasketCompleted: function (data) {
        if (data && data.success) {
            alert('Product successfully removed from basket');
            this._getBasket();
        }
    },
    _updateBasketPostage: function (postageID) {
        $.post(this.settings.updateBasketPostageUrl, { postageID: postageID }, $.proxy(this._updateBasketPostageCompleted, this));
    },
    _updateBasketPostageCompleted: function (data) {
        if (data && data.success) {
            this._getPrices();
        }
    },
    initialize: function () {
        this._initializeSettings();
        this._createViewModel();
        this._applyViewModel();
        this._getBasket();
    }
}

$(function () {
    var manager = new BasketManager();
    manager.initialize();
})