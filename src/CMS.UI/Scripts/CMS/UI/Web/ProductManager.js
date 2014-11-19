/// <reference path="../../../_references.js" />

function ProductManager() {
    this.element = $('.product');
}

ProductManager.prototype = {
    _addToBasket: function () {
        var amount = parseInt(this.element.find('#tbQuantity').val());
        var attrs = $('.selectAttr').map(function () {
            return $(this).val();
        }).get();
        $('.attr').each(function () {
            attrs.push($(this).attr('data-attr'));
        });
        var date = $('#eventDate').val();
        $.ajax(this.settings.addToBasketUrl, {
            data: {
                productID: this.productID,
                amount: isNaN(amount) ? 0 : amount,
                attrs: attrs,
                selectedDate: date
            },
            traditional: true,
            type: 'POST',
            success: $.proxy(this._addToBasketCompleted, this)
        });
    },
    _addToBasketCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert('Product successfully added to basket');
                $.post(this.settings.getBasketSummaryUrl, this._getBasketSummaryCompleted);
                this._getPrice();
            } else {
                alert('There was an error. Product does not exist or has been changed');
            }
        }
    },
    _getBasketSummaryCompleted: function (data) {
        if (data) {
            $('#BasketSummary').replaceWith(data);
        }
    },
    _getPrice: function () {
        var amount = parseInt(this.element.find('#tbQuantity').val());
        var attrs = $('.selectAttr').map(function () {
            return $(this).val();
        }).get();

        $('.attr').each(function () {
            attrs.push($(this).attr('data-attr'));
        });

        var date = $('#eventDate').val();

        $.ajax(this.settings.getPriceUrl, {
            data: {
                productID: this.productID,
                amount: isNaN(amount) ? 0 : amount,
                attrs: attrs,
                selectedDate: date
            },
            traditional: true,
            type: 'POST',
            success: $.proxy(this._getPriceCompleted, this)
        });
    },
    _getPriceCompleted: function (data) {
        if (data && data.success) {
            $('#lbPrice').text(data.price);
            $('#lbStock').text(data.stock);
            if (data.insufficientStock)
                $('#btnAdd').attr("title", "That quantity is not available.").prop("disabled", true);
            else
                $('#btnAdd').removeAttr("title").prop("disabled", false);
        }
        else if (data.warning) {
            $('#lbPrice').text("0");
            $('#btnAdd').attr("title", data.message).prop("disabled", true);
            alert(data.message);
        }
    },
    _initializeImageZoom: function () {
        $('#imgPrimary').elevateZoom({
            borderSize: 2,
            tint: true,
            tintColour: '#111',
            tintOpacity: 0.5,
            zoomWindowFadeIn: 500,
            zoomWindowFadeOut: 500,
            lensFadeIn: 500,
            lensFadeOut: 500
        });
    },
    _initializeSettings: function () {
        this.settings = {
            addToBasketUrl: this.element.attr('data-addToBasket-url'),
            getBasketSummaryUrl: this.element.attr('data-getBasketSummary-url'),
            getPriceUrl: this.element.attr('data-getPrice-url')
        };

        this.productID = this.element.attr('data-productID');
    },
    initialize: function () {
        this._initializeSettings();
        this._initializeImageZoom();
        this._getPrice();

        this.element.find('select').change($.proxy(this._getPrice, this));
        this.element.find('input').change($.proxy(this._getPrice, this));
        this.element.find('#btnAdd').click($.proxy(this._addToBasket, this));
    }
}

$(function () {
    var manager = new ProductManager();
    manager.initialize();
})