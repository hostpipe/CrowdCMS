/// <reference path="../../../_references.js" />

HostPipe.UI.OrdersManager = function () {
    this.element = $('#OrdersContent');
    this.orderID = 0;
}

HostPipe.UI.OrdersManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            allDates: ko.observable(false),
            domains: ko.observableArray(),
            statuses: ko.observableArray([]),
            endDate: ko.observable(),
            lDomainID: ko.observable(),
            statusID: ko.observable(),
            isNewMethod: ko.observable(false),
            sectionHeader: ko.observable("Add Product"),
            search: ko.observable(),
            startDate: ko.observable(),
            searchCategories: ko.observableArray([]),
            searchCategoryID: ko.observable(null),
            searchProducts: ko.observableArray([]),
            searchProductID: ko.observable(null),
            orderType: ko.observableArray(),

            domainID: ko.observable(0),
            customerID: ko.observable(0),
            addressID: ko.observable(0),
            email: ko.observable(),
            dTitle: ko.observable(),
            dFirstName: ko.observable(),
            dSurname: ko.observable(),
            dPhone: ko.observable(),
            dAdd1: ko.observable(),
            dAdd2: ko.observable(),
            dAdd3: ko.observable(),
            dCity: ko.observable(),
            dPostcode: ko.observable(),
            dCounty: ko.observable(),
            dCountryID: ko.observable(),
            bTitle: ko.observable(),
            bFirstName: ko.observable(),
            bSurname: ko.observable(),
            bPhone: ko.observable(),
            bAdd1: ko.observable(),
            bAdd2: ko.observable(),
            bAdd3: ko.observable(),
            bCity: ko.observable(),
            bPostcode: ko.observable(),
            bCounty: ko.observable(),
            bCountryID: ko.observable(),
            addressTheSame: ko.observable(false),

            countries: ko.observableArray([]),
            customers: ko.observableArray([]),
            addresses: ko.observableArray([]),
            billingCountries: ko.observableArray([]),
            deliveryCountries: ko.observableArray([]),
            categories: ko.observableArray([]),
            products: ko.observableArray([]),
            prices: ko.observableArray([]),
            orderedProducts: ko.observableArray([]),
            postages: ko.observableArray([]),
            categoryID: ko.observable(0),
            prodID: ko.observable(0),
            priceID: ko.observable(0),
            postageID: ko.observable(),
            amount: ko.observable(1),
            totalPrice: ko.observable(0),
            deliveryCost: ko.observable(0),
            instructions: ko.observable(),
            isCustomPrice: ko.observable(false),
            customPrice: ko.observable(0),

            isPayment: ko.observable(false),
            cashPayment: ko.observable(1),
            paymentID: ko.observable(0),
            payments: ko.observableArray([]),
            isDirect: ko.observable(false),
            sagePayDomainID: ko.observable(null),

            isDiscount: ko.observable(false),
            discount: ko.observable(0),
            discountCode: ko.observable(),
            discountText: ko.observable(),

            step: ko.observable(1),
            orderTypeID: ko.observable(),

            onAddCustomer: $.proxy(this._onAddCustomer, this),
            onAddDiscount: $.proxy(this._onAddDiscount, this),
            onAddOrder: $.proxy(this._onAddOrder, this),
            onAddProduct: $.proxy(this._onAddProduct, this),
            onCancelNewMethod: $.proxy(this._onCancelNewMethod, this),
            onChangeStatus: $.proxy(this._onChangeStatus, this),
            onDeleteOldBaskets: $.proxy(this._onDeleteOldBaskets, this),
            onDeleteProduct: $.proxy(this._onDeleteProduct, this),
            onDomainChangedForCustomer: $.proxy(this._onDomainChangedForCustomer, this),
            onEdit: $.proxy(this._onEdit, this),
            onFullList: $.proxy(this._onFullList, this),
            onDelete: $.proxy(this._onDelete, this),
            onGetEmailsAsCSV: $.proxy(this._onGetEmailsAsCSV, this),
            onLDomainChange: $.proxy(this._onLDomainChange, this),
            onNewCustomer: $.proxy(this._onNewCustomer, this),
            onNewMethod: $.proxy(this._onNewMethod, this),
            onNewOrder: $.proxy(this._onNewOrder, this),
            onOrdersSummary: $.proxy(this._onOrdersSummary, this),
            onPackingList: $.proxy(this._onPackingList, this),
            onPrintPage: $.proxy(this._onPrintPage, this),
            onRemoveDiscount: $.proxy(this._onRemoveDiscount, this),
            onSaveNewMethod: $.proxy(this._onSaveNewMethod, this),
            onSearch: $.proxy(this._onSearch, this),
            onSearchKeyUp: $.proxy(this._onSearchKeyUp, this),
            onSearchOrders: $.proxy(this._onSearchOrders, this),
            onStatusChanged: $.proxy(this._onStatusChanged, this),
            onStep2: $.proxy(this._onStep2, this),
            onStep2Back: $.proxy(this._onStep2Back, this),
            onStep3: $.proxy(this._onStep3, this),
            onUpdateTracking: $.proxy(this._onUpdateTracking, this),
            onGetOrderSummaryAsCSV: $.proxy(this._onGetOrderSummaryAsCSV, this),
            onTypeChange: $.proxy(this._onTypeChange, this)
        };

        this.viewModel.isStep1 = ko.computed(function () {
            return this.step() == 1;
        }, this.viewModel);
        this.viewModel.isStep2 = ko.computed(function () {
            return this.step() == 2;
        }, this.viewModel);
        this.viewModel.isStep3 = ko.computed(function () {
            return this.step() == 3;
        }, this.viewModel);

        this.viewModel.customerID.subscribe(function (value) {
            if (value)
                this._getCustomer();
        }, this);

        this.viewModel.addressID.subscribe(function (value) {
            if (value)
                this._getAddress();
            else {
                this.viewModel.bAdd1("");
                this.viewModel.bAdd2("");
                this.viewModel.bAdd3("");
                this.viewModel.bCity("");
                this.viewModel.bPostcode("");
                this.viewModel.bCounty("");
                this.viewModel.bCountryID(null);

                this.viewModel.dAdd1("");
                this.viewModel.dAdd2("");
                this.viewModel.dAdd3("");
                this.viewModel.dCity("");
                this.viewModel.dPostcode("");
                this.viewModel.dCounty("");
                this.viewModel.dCountryID(null);
            }
        }, this);

        this.viewModel.categoryID.subscribe(function (value) {
            this.viewModel.products([]);
            this.viewModel.prices([]);

            if (value) {
                this._getProducts(value, $.proxy(this._getProductsCompleted, this));
            }
        }, this);

        this.viewModel.searchCategoryID.subscribe(function (value) {
            this.viewModel.searchProducts([]);

            if (value) {
                this._getProducts(value, $.proxy(this._getSearchProductsCompleted, this));
            }
        }, this);

        this.viewModel.prodID.subscribe(function (value) {
            this.viewModel.prices([]);

            if (value) {
                this._getPrices();
            }
        }, this);

        this.viewModel.totalPrice.subscribe(function (value) {
            if (value)
                this.viewModel.customPrice(value.substring(1, value.indexOf(' ')));
        }, this);

        this.viewModel.customPrice.subscribe(function (value) {
            var patternNumber = new RegExp(/^\d+([\.,]\d+)?$/g);
            var patternNotNumber = new RegExp(/[^\d\.,]/g);
            if (!patternNumber.test(value)) {
                alert('Price needs to be a number.');
                this.viewModel.customPrice(value.replace(patternNotNumber, ""));
            }
        }, this);

        this.viewModel.postageID.subscribe(function (value) {
            if (this.viewModel.isStep3())
                this._savePostage();
        }, this);

        this.viewModel.addressTheSame.subscribe(function (value) {
            if (value) {
                this.bTitle(this.dTitle());
                this.bFirstName(this.dFirstName());
                this.bSurname(this.dSurname());
                this.bPhone(this.dPhone());
                this.bAdd1(this.dAdd1());
                this.bAdd2(this.dAdd2());
                this.bAdd3(this.dAdd3());
                this.bCity(this.dCity());
                this.bPostcode(this.dPostcode());
                this.bCounty(this.dCounty());
                this.bCountryID(this.dCountryID());
            }
        }, this.viewModel);

        this.viewModel.step.subscribe(function (value) {
            if (value == 1) {
                this.domainID(null);
                this.customerID(null);
                this.addressID(null);
                this.email("");
                this.bTitle("");
                this.bFirstName("");
                this.bSurname("");
                this.bPhone("");
                this.bAdd1("");
                this.bAdd2("");
                this.bAdd3("");
                this.bCity("");
                this.bPostcode("");
                this.bCounty("");
                this.bCountryID(null);
                this.dTitle("");
                this.dFirstName("");
                this.dSurname("");
                this.dPhone("");
                this.dAdd1("");
                this.dAdd2("");
                this.dAdd3("");
                this.dCity("");
                this.dPostcode("");
                this.dCounty("");
                this.dCountryID(null);
                this.addressTheSame(false);

                this.customers([]);
                this.addresses([]);
                this.billingCountries([]);
                this.deliveryCountries([]);
                this.categories([]);
                this.products([]);
                this.prices([]);
                this.orderedProducts([]);
                this.postages([]);
                this.categoryID(0);
                this.prodID(0);
                this.priceID(0);
                this.postageID(0);
                this.amount(1);
                this.totalPrice(0);
                this.deliveryCost(0);
                this.instructions('');
                this.isCustomPrice(false);
                this.customPrice(0);

                this.isPayment(false);
                this.paymentID(0);
                this.payments([]);
                this.isDirect(false);
                this.sagePayDomainID(null);

                this.isDiscount(false);
                this.discount(0);
                this.discountCode();
                this.discountText();
            }
        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addCustomerInfoToOrder: this.element.attr('data-addCustomerInfo-url'),
            addDiscountOrder: this.element.attr('data-addDiscount-url'),
            addNewCustomerUrl: this.element.attr('data-addNewCustomer-url'),
            addProductUrl: this.element.attr('data-addProduct-url'),
            addOrderUrl: this.element.attr('data-addOrder-url'),
            cancelOrderUrl: this.element.attr('data-cancelOrder-url'),
            createBasketUrl: this.element.attr('data-createBasket-url'),
            deleteBasketUrl: this.element.attr('data-deleteOldBaskets-url'),
            deleteProductUrl: this.element.attr('data-deleteProduct-url'),
            getAddressUrl: this.element.attr('data-getAddress-url'),
            getCustomerUrl: this.element.attr('data-getCustomer-url'),
            getCategoriesUrl: this.element.attr('data-getCategories-url'),
            getCountriesAsSelectList: this.element.attr('data-getCountriesAsSelectList-url'),
            getDomainsUrl: this.element.attr('data-getDomains-url'),
            getEmailsAsCSV: this.element.attr('data-getEmailsAsCSV-url'),
            getOrderUrl: this.element.attr('data-getOrder-url'),
            getOrdersSummaryUrl: this.element.attr('data-getOrdersSummary-url'),
            getPricesUrl: this.element.attr('data-getPrices-url'),
            getProductsUrl: this.element.attr('data-getProducts-url'),
            getDeliveryDetailsUrl: this.element.attr('data-getDeliveryuDetails-url'),
            getStatusesUrl: this.element.attr('data-getStatuses-url'),
            newCustomerUrl: this.element.attr('data-newCustomer-url'),
            newOrderUrl: this.element.attr('data-newOrder-url'),
            removeDiscountUrl: this.element.attr('data-removeDiscount-url'),
            saveNewMethodUrl: this.element.attr('data-saveNewMethod-url'),
            saveOrderTrackingUrl: this.element.attr('data-saveOrderTracking-url'),
            savePostageUrl: this.element.attr('data-savePostage-url'),
            searchOrdersUrl: this.element.attr('data-searchOrders-url'),
            updateOrderStatusUrl: this.element.attr('data-updateOrderStatus-url'),
            getOrderSummaryCSVUrl: this.element.attr('data-getOrderSummaryAsCSV-url'),
            getOrderTypesUrl: this.element.attr('data-getOrderTypes-url')
        };
    },
    _getAddress: function () {
        $.post(this.settings.getAddressUrl, { addressID: this.viewModel.addressID() }, $.proxy(this._getAddressCompleted, this));
    },
    _getAddressCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.bAdd1(data.Address1);
                this.viewModel.bAdd2(data.Address2);
                this.viewModel.bAdd3(data.Address3);
                this.viewModel.bCity(data.Town);
                this.viewModel.bPostcode(data.PostCode);
                this.viewModel.bCounty(data.County);
                this.viewModel.bCountryID(data.CountryID);

                this.viewModel.dAdd1(data.Address1);
                this.viewModel.dAdd2(data.Address2);
                this.viewModel.dAdd3(data.Address3);
                this.viewModel.dCity(data.Town);
                this.viewModel.dPostcode(data.PostCode);
                this.viewModel.dCounty(data.County);
                this.viewModel.dCountryID(data.CountryID);
            }
        }
    },
    _getCategories: function (domainID, callback) {
        $.post(this.settings.getCategoriesUrl, { domainID: domainID }, callback);
    },
    _getCategoriesCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.categories(data.categories);
            }
        }
    },
    _getSearchCategoriesCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.searchCategories(data.categories);
            }
        }
    },
    _getCustomer: function () {
        $.post(this.settings.getCustomerUrl, { customerID: this.viewModel.customerID() }, $.proxy(this._getCustomerCompleted, this));
    },
    _getCustomerCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.email(data.Email);
            this.viewModel.addresses(data.Addresses);

            this.viewModel.dTitle(data.Title);
            this.viewModel.dFirstName(data.FirstName);
            this.viewModel.dSurname(data.Surname);
            this.viewModel.dPhone(data.Phone);

            this.viewModel.bTitle(data.Title);
            this.viewModel.bFirstName(data.FirstName);
            this.viewModel.bSurname(data.Surname);
            this.viewModel.bPhone(data.Phone);
        }
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
                this._getCategories(this.viewModel.lDomainID(), $.proxy(this._getSearchCategoriesCompleted, this));
            }
        }
    },
    _getPrices: function (data, event) {
        $.post(this.settings.getPricesUrl, { productID: this.viewModel.prodID() }, $.proxy(this._getPricesCompleted, this));
    },
    _getPricesCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.prices(data.prices);
            }
        }
    },
    _getProducts: function (categoryID, callback) {
        $.post(this.settings.getProductsUrl, { categoryID: categoryID }, callback);
    },
    _getProductsCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.products(data.products);
        }
    },
    _getSearchProductsCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.searchProducts(data.products);
        }
    },
    _getStatuses: function (data, event) {
        $.post(this.settings.getStatusesUrl, $.proxy(this._getStatusesCompleted, this));
    },
    _getStatusesCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.statuses(data.statuses);
                //this._refreshLeftMenu();
            }
        }
    },
    _initializeDatePicker: function () {
        this.element.find('[data-datepicker]').datepicker({
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
    _insertHtmlView: function (data) {
        if (data) {
            this.element.find('#OrdersView').html(data);
            this._applyViewModel(this.element.find('#OrdersView'));
            this._initializeDatePicker();
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        }
    },
    _onAddCustomer: function () {
        var form = $('#CustomerForm');
        form.validate();

        if (form.valid()) {
            $.post(this.settings.addNewCustomerUrl, form.serializeArray(), $.proxy(this._onAddCustomerCompleted, this));
        }
    },
    _onAddCustomerCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Customer successfully saved.");
                this._onNewCustomer();
            }
            else
                alert("Customer was not saved. " + data.error);
        }
    },
    _onAddDiscount: function () {
        if (!this.viewModel.discountCode()) {
            alert("Please specify discount code");
            return;
        }

        $.post(this.settings.addDiscountOrder, { discountCode: this.viewModel.discountCode() }, $.proxy(this._onAddDiscountCompleted, this));
    },
    _onAddDiscountCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.isDiscount(data.isDiscount);
                this.viewModel.discount(data.discount);
                this.viewModel.discountText(data.discountText);
                this.viewModel.totalPrice(data.totalPrice);
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onAddOrder: function () {
        if (!this.viewModel.postageID() || this.viewModel.postageID() <= 0) {
            alert("Please select delivery option.");
            return;
        }

        if (this.viewModel.isPayment()) {

            if (!this.viewModel.paymentID() || this.viewModel.paymentID() == 0) {
                alert("Please select a payment option.");
                return;
            }

            var form = $('#formCreditCard');
            form.validate();
            if (form.valid()) {
                var creditCard = {};
                $.map(form.serializeArray(), function (n, i) {
                    creditCard[n['name']] = n['value'];
                });

                $.ajax(this.settings.addOrderUrl, {
                    type: "POST",
                    contentType: "application/json",
                    data: JSON.stringify({
                        instructions: this.viewModel.instructions(),
                        isPayment: this.viewModel.isPayment(),
                        paymentDomainID: this.viewModel.paymentID(),
                        creditCardInfo: creditCard,
                        isCustomPrice: this.viewModel.isCustomPrice(),
                        customPrice: this.viewModel.customPrice(),
                        addressID:  this.viewModel.addressID()
                    }),
                    success: $.proxy(this._onAddOrderCompleted, this)
                });
            }
        }
        else {
            $.post(this.settings.addOrderUrl, {
                instructions: this.viewModel.instructions(),
                isCustomPrice: this.viewModel.isCustomPrice(),
                customPrice: this.viewModel.customPrice(),
                addressID: this.viewModel.addressID(),
                cashPayment: this.viewModel.cashPayment()
            }, $.proxy(this._onAddOrderCompleted, this));
        }
    },
    _onAddOrderCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Order successfully saved.");
                if (data.url) {
                    location.assign(data.url);
                    return;
                }

                this._onSearchOrders();
                this._onNewOrder();
            }
            else if (data.error)
                alert("Order was not saved. " + data.error);
        }
    },
    _onAddProduct: function () {
        if (!this.viewModel.priceID() || this.viewModel.priceID() <= 0) {
            alert("Please select product.");
            return;
        }

        var amount = parseInt(this.viewModel.amount());
        if (isNaN(amount) || amount <= 0) {
            alert("Please select correct amount.");
            return;
        }

        $.post(this.settings.addProductUrl, { priceID: this.viewModel.priceID(), amount: amount }, $.proxy(this._onProductCompleted, this));
    },
    _onCancelNewMethod: function (data, event) {
        this.viewModel.isNewMethod(false);
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
            $.post(this.settings.getOrderUrl, { orderID: this.orderID }, $.proxy(this._insertHtmlView, this));
            this._onSearchOrders();
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        if (menuItem && menuItem.length > 0)
            this.orderID = menuItem.attr('data-itemID');

        $("#DialogConfirm").dialog({
            resizable: false,
            height: 140,
            width: 700,
            modal: true,
            buttons: {
                "Cancel with cancellation email": $.proxy(function () {
                    $.post(this.settings.cancelOrderUrl, { orderID: this.orderID, isEmail: true }, $.proxy(this._onDeleteCompleted, this));
                    $("#DialogConfirm").dialog("close");
                }, this),
                "Cancel without cancellation email": $.proxy(function () {
                    $.post(this.settings.cancelOrderUrl, { orderID: this.orderID }, $.proxy(this._onDeleteCompleted, this));
                    $("#DialogConfirm").dialog("close");
                }, this),
                "Close window": function () {
                    $("#DialogConfirm").dialog("close");
                }
            }
        });
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Order is canceled successfully.");
                this._onSearchOrders();
            }
            else {
                alert("En Arror occurred durign order cancellation.");
                this._onSearchOrders(); // is it necessary??
            }
        }
    },
    _onDeleteOldBaskets: function (data, event) {
        if (confirm("This will delete all baskets used by non-registered customers which are older than 30 days. Do you want to continue?")) {
            $.post(this.settings.deleteBasketUrl, this._onDeleteOldBasketsCompleted);
        }
    },
    _onDeleteOldBasketsCompleted: function (data) {
        if (data && data.success) {
            alert("Baskets Deleted!");
        }
    },
    _onDeleteProduct: function (data, event) {
        var basketContentID = $(event.currentTarget).attr('data-basketContentID');

        if (basketContentID)
            $.post(this.settings.deleteProductUrl, { basketContentID: basketContentID }, $.proxy(this._onProductCompleted, this));
    },
    _onDomainChangedForCustomer: function (data, event) {
        $.post(this.settings.getCountriesAsSelectList, { domainID: $('#DomainID').val() }, $.proxy(this._onDomainChangedForCustomerCompleted, this));
    },
    _onDomainChangedForCustomerCompleted: function (data) {
        if (data) {
            this.viewModel.countries(data.countries);
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        if (menuItem && menuItem.length > 0)
            this.orderID = menuItem.attr('data-itemID');

        $.post(this.settings.getOrderUrl, { orderID: this.orderID, isDonation: this.viewModel.orderTypeID() == 2 ? true : false }, $.proxy(this._insertHtmlView, this));
    },
    _onFullList: function (data, event) {
        this.viewModel.endDate('');
        this.viewModel.startDate('');
        this.viewModel.allDates(false);
        this.viewModel.search('');
        this.viewModel.searchCategoryID(null);
        this.viewModel.searchProductID(null);

        this._onSearchOrders();
    },
    _onGetEmailsAsCSV: function (data, event) {
        var startDate = this.viewModel.allDates() ? null : this.viewModel.startDate();
        var endDate = this.viewModel.allDates() ? null : this.viewModel.endDate();

        var url = this.settings.getEmailsAsCSV + "?" + this._serialize(
            {
                search: this.viewModel.search(),
                startDate: startDate,
                endDate: endDate,
                domainID: this.viewModel.lDomainID(),
                statusID: this.viewModel.statusID(),
                categoryID: this.viewModel.searchCategoryID(),
                productID: this.viewModel.searchProductID()
            });

        $(event.currentTarget).attr('href', url);
        return true;
    },
    _onGetOrderSummaryAsCSV: function (data, event) {
        var startDate = this.viewModel.allDates() ? null : this.viewModel.startDate();
        var endDate = this.viewModel.allDates() ? null : this.viewModel.endDate();
        var type = this.viewModel.orderTypeID();

        var url = this.settings.getOrderSummaryCSVUrl + "?" + this._serialize(
            {
                search: this.viewModel.search(),
                startDate: startDate,
                endDate: endDate,
                domainID: this.viewModel.lDomainID(),
                statusID: this.viewModel.statusID(),
                categoryID: this.viewModel.searchCategoryID(),
                productID: this.viewModel.searchProductID(),
                typeID: type
            });

        $(event.currentTarget).attr('href', url);
        return true;
    },
    _onLDomainChange: function (data, event) {
        this._getCategories(this.viewModel.lDomainID(), $.proxy(this._getSearchCategoriesCompleted, this));
        this._onSearchOrders();
    },
    _onNewCustomer: function () {
        $.post(this.settings.newCustomerUrl, $.proxy(this._insertHtmlView, this));
    },
    _onNewMethod: function (data, event) {
        this.viewModel.isNewMethod(true);
    },
    _onNewOrder: function () {
        $.post(this.settings.newOrderUrl, $.proxy(this._insertHtmlView, this));
        this.viewModel.step(1);
    },
    _onOrdersSummary: function () {
        Core.isBusy(true, ".mainContainer");

        var startDate = this.viewModel.allDates() ? null : this.viewModel.startDate();
        var endDate = this.viewModel.allDates() ? null : this.viewModel.endDate();

        $.post(this.settings.getOrdersSummaryUrl, {
            search: this.viewModel.search(),
            startDate: startDate,
            endDate: endDate,
            domainID: this.viewModel.lDomainID(),
            statusID: this.viewModel.statusID(),
            categoryID: this.viewModel.searchCategoryID(),
            productID: this.viewModel.searchProductID(),
            typeID: this.viewModel.orderTypeID()
        }, $.proxy(this._onOrdersSummaryCompleted, this));
    },
    _onOrdersSummaryCompleted: function (data) {
        Core.isBusy(false, ".mainContainer");

        if (data) {
            this._insertHtmlView(data);
        }
    },
    _onPackingList: function (data, event) {
        $.post(this.settings.getDeliveryDetailsUrl, { orderID: this.orderID }, $.proxy(this._insertHtmlView, this));
    },
    _onPrintPage: function (data, event) {
        var html = "<!DOCTYPE HTML>";
        html += '<html lang="en-us">';
        html += '<head>';
        html += '<link href="/Content/reset.css" rel="stylesheet"><link href="/Content/admin/site.css" rel="stylesheet">';
        html += '</head>';
        html += "<body>";
        html += $('.mainContainer').html();
        html += "</body>";

        var printWin = window.open('', '', 'toolbar=0,status=0');
        printWin.document.write(html);
        printWin.document.close();
        printWin.print();

        var closeWindow = function () {
            if (printWin.document.readyState == "complete") {
                printWin.close();
            } else {
                setTimeout(closeWindow, 200);
            }
        }
        closeWindow();
    },
    _onProductCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.orderedProducts(data.products);
                this.viewModel.totalPrice(data.totalPrice);
                this.viewModel.discount(data.discountPrice);
                this.viewModel.deliveryCost(data.deliveryPrice);
            }
            if (data.error)
                alert(data.error);
        }
    },
    _onRemoveDiscount: function () {
        $.post(this.settings.removeDiscountUrl, $.proxy(this._onRemoveDiscountCompleted, this));
    },
    _onRemoveDiscountCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.isDiscount(false);
                this.viewModel.discount(0);
                this.viewModel.discountText('');
                this.viewModel.totalPrice(data.totalPrice);
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onSaveNewMethod: function (data, event) {
        var newMethod = $('#tbNewMethod').val();
        if (newMethod) {
            $.post(this.settings.saveNewMethodUrl, { method: newMethod }, $.proxy(this._onSaveNewMethodCompleted, this));
        }
        else
            alert("Please fill the name of new method. ");
    },
    _onSaveNewMethodCompleted: function (data) {
        if (data && data.success) {
            alert("New Despatch Method successfully saved.");
            this.viewModel.isNewMethod(false);
            $.post(this.settings.getOrderUrl, { orderID: this.orderID }, $.proxy(this._insertHtmlView, this));
        }
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
    _onSearchOrders: function (data, event) {
        $('#SearchDialog').dialog("close");

        this._refreshLeftMenu();

        if ($('[data-isSummary="true"]').length > 0) {
            this._onOrdersSummary();
        }
    },
    _onStatusChanged: function () {
        this._onSearchOrders();
    },
    _onTypeChange: function (data, event) {
        
        var v = $("#selOrderType").val();
        if (this.viewModel.orderTypeID() == "2") {
            $("#btnOrdersSummary").val('Show Donations Summary');
        } else {
            $("#btnOrdersSummary").val('Show Orders Summary');
        }

        this._refreshLeftMenu();
    },
    _onStep2: function () {
        if (this.viewModel.domainID() == null || this.viewModel.domainID() <= 0) {
            alert("Please select domain.");
            return;
        }
        this._getCategories(this.viewModel.domainID(), $.proxy(this._getCategoriesCompleted, this));
        $.post(this.settings.createBasketUrl, { domainID: this.viewModel.domainID() }, $.proxy(this._onStep2Completed, this));
    },
    _onStep2Completed: function (data) {
        if (data && data.success) {
            this.viewModel.step(2);
            this.viewModel.customers(data.customers);
            this.viewModel.billingCountries(data.billingCountries);
            this.viewModel.deliveryCountries(data.deliveryCountries);
            this.viewModel.payments(data.payments);
            this.viewModel.isDirect(data.isDirect);
            this.viewModel.sagePayDomainID(data.sagePayDomainID.toString());
        }
    },
    _onStep2Back: function () {
        this.viewModel.step(2);
    },
    _onStep3: function () {
        if (this.viewModel.orderedProducts().length <= 0) {
            alert("There is no product selected for order.");
            return;
        }

        var form = $('#OrderForm');
        form.validate();

        if (form.valid()) {
            $.post(this.settings.addCustomerInfoToOrder, form.serializeArray(), $.proxy(this._onStep3Completed, this));
        }
    },
    _onStep3Completed: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.step(3);
                this.viewModel.postages(data.postages);
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onUpdateTracking: function (data, event) {
        var despathMethod = parseInt($('#sDespatchMethod').val());
        var deliveryDate = $('#tbDeliveryDate').val();
        var trackingUrl = $('#tbTrackingURL').val();
        var trackingRef = $('#tbTrackingRef').val();

        $.post(this.settings.saveOrderTrackingUrl, {
            despathMethodID: despathMethod,
            deliveryDate: deliveryDate,
            trackingUrl: trackingUrl,
            trackingRef: trackingRef,
            orderID: this.orderID
        }, this._onUpdateTrackingCompleted);
    },
    _onUpdateTrackingCompleted: function (data) {
        if (data && data.success) {
            alert("Tracking Information successfully saved.");
        }
    },
    _refreshLeftMenu: function () {
        var startDate = this.viewModel.allDates() ? null : this.viewModel.startDate();
        var endDate = this.viewModel.allDates() ? null : this.viewModel.endDate();

        var url = '';
        if (this.settings != null && this.settings != 'undefined') {
            url = this.settings.searchOrdersUrl;
        }else {
            url = this.element.attr('data-searchOrders-url');
        }

        LeftMenuManager.loadMenu(this.viewModel, url, {
            search: this.viewModel.search(),
            startDate: startDate,
            endDate: endDate,
            domainID: this.viewModel.lDomainID(),
            statusID: this.viewModel.statusID(),
            categoryID: this.viewModel.searchCategoryID(),
            productID: this.viewModel.searchProductID(),
            typeID: this.viewModel.orderTypeID()
        });
    },
    _savePostage: function () {
        $.post(this.settings.savePostageUrl, { postageID: this.viewModel.postageID() }, $.proxy(this._savePostageCompleted, this));
    },
    _savePostageCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.totalPrice(data.totalPrice);
                this.viewModel.discount(data.discountPrice);
                this.viewModel.deliveryCost(data.deliveryPrice);
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _getOrderTypes: function () {
        $.post(this.settings.getOrderTypesUrl, { typeId: this.viewModel.orderTypeID() }, $.proxy(this._getOrderTypesCompleted, this));
    },
    _getOrderTypesCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.orderType(data.types);
                if (data.selected) {
                    this.viewModel.orderTypeID(data.selected);
                    //$("#selOrderType").change();
                }
                this._refreshLeftMenu();
            }
        }
    },
    _serialize: function (obj) {
        var str = [];
        for (var p in obj) {
            if (obj.hasOwnProperty(p) && obj[p]) {
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
        this._getStatuses();
        this._initializeSearchDialog();
        this._getOrderTypes();
    }
}

$(function () {
    var OrdersManager = new HostPipe.UI.OrdersManager();
    OrdersManager.initialize();
})