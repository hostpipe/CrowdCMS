/// <reference path="../../../_references.js" />
HostPipe.UI.ProductsManager = function () {
    this.element = $('#ProductsContent');
    this.productID = 0;
    this.supressEventID = 0;

    this.productString = $.proxy(function () {
        return this.productType == "Event" ? "Event" : "Product";
    }, this);

    this.emptyEventType = {
        Title: "",
        Description: "",
        EventTypeID: 0
    };
}

HostPipe.UI.ProductsManager.prototype = {
    _addAttribute: function (data, event) {
        var attrID = $('#ddlAttributes').val();
        $.post(this.settings.addAttributeUrl, { productID: this.productID, attributeID: attrID }, $.proxy(this._addAttributeCompleted, this));
    },
    _addAttributeCompleted: function (data) {
        if (data && data.success) {
            $('#AddAttributeWindow').dialog('close');
            $('#AddAttributeWindow').dialog('destroy');
            this._getStockInfo();
        }
    },
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            attributes: ko.observableArray([]),
            categories: ko.observableArray([]),
            categoryID: ko.observable(),
            domainID: ko.observable(),
            isEdit: ko.observable(false),
            isSearch: ko.observable(false),
            sectionHeader: ko.observable("Add " + this.productString()),
            prodGroups: ko.observableArray([]),

            eventTypeID: ko.observable(),
            eventTypes: ko.observableArray([]),

            currentEventType: {
                Title: ko.observable(),
                Description: ko.observable(),
                EventTypeID: ko.observable(0)
            },

            onEditEventType: $.proxy(this._onEditEventType, this),
            onViewEventType: $.proxy(this._onViewEventType, this),
            onSubmitEventType: $.proxy(this._onSubmitEventType, this),
            onCloseEventTypeDialog: $.proxy(this._onCloseEventTypeDialog, this),

            addAttribute: $.proxy(this._addAttribute, this),
            createMatrix: $.proxy(this._createMatrix, this),
            onAddAssociation: $.proxy(this._onAddAssociation, this),
            onAddAttribute: $.proxy(this._onAddAttribute, this),
            onAddNewSalePrice: $.proxy(this._onAddNewSalePrice, this),
            onAddNewTimeWindow: $.proxy(this._onAddNewTimeWindow, this),
            onApprove: $.proxy(this._onApprove, this),
            onAssociate: $.proxy(this._onAssociate, this),
            onCheckAll: $.proxy(this._onCheckAll, this),
            onDelete: $.proxy(this._onDelete, this),
            onDeleteAssociation: $.proxy(this._onDeleteAssociation, this),
            onDeleteAllUnits: $.proxy(this._onDeleteAllUnits, this),
            onDeleteContent: $.proxy(this._onDeleteContent, this),
            onDeleteImage: $.proxy(this._onDeleteImage, this),
            onDeleteSelectedStockUnit: $.proxy(this._onDeleteSelectedStockUnit, this),
            onDeleteStockUnit: $.proxy(this._onDeleteStockUnit, this),
            onDeleteTimeWindow: $.proxy(this._onDeleteTimeWindow, this),
            onDomainChange: $.proxy(this._onDomainChange, this),
            onEdit: $.proxy(this._onEdit, this),
            onEditImage: $.proxy(this._onEditImage, this),
            onFullList: $.proxy(this._onFullList, this),
            onLoadContent: $.proxy(this._onLoadContent, this),
            onManageSaleTime: $.proxy(this._onManageSaleTime, this),
            onNewProduct: $.proxy(this._onNewProduct, this),
            onNewStockUnit: $.proxy(this._onNewStockUnit, this),
            onPreviewContent: $.proxy(this._onPreviewContent, this),
            onRemoveAttribute: $.proxy(this._onRemoveAttribute, this),
            onSaveOrder: $.proxy(this._onSaveOrder, this),
            onSaveSelected: $.proxy(this._onSaveSelected, this),
            onSaveStockUnit: $.proxy(this._onSaveStockUnit, this),
            onSearch: $.proxy(this._onSearch, this),
            onSearchKeyUp: $.proxy(this._onSearchKeyUp, this),
            onStock: $.proxy(this._onStock, this),
            onStockMartix: $.proxy(this._onStockMartix, this),
            onSubmit: $.proxy(this._onSubmit, this),
            onUpdateTimeWindow: $.proxy(this._onUpdateTimeWindow, this)
        }
    },
    _createMatrix: function (data, event) {
        var selectedValues = $('#CreateMatrixWindow').find('input:checked');
        var attrValuesIDs = Array();
        $(selectedValues).each(function () {
            attrValuesIDs.push($(this).val());
        });

        $.ajax(this.settings.createMatrixUrl, {
            type: "POST",
            data: { productID: this.productID, prodAttValueIDs: attrValuesIDs },
            traditional: true,
            success: $.proxy(this._createMatrixCompleted, this)
        });
    },
    _createMatrixCompleted: function (data) {
        if (data && data.success) {
            $('#CreateMatrixWindow').dialog('close');
            $('#CreateMatrixWindow').dialog('destroy');
            this._getStockInfo();
        }
    },
    _createSettings: function () {
        this.settings = {
            addAttributeUrl: this.element.attr('data-addAttrbiute-url'),
            addProductUrl: this.element.attr('data-addproduct-url'),
            addNewTimeWindowUrl: this.element.attr('data-addNewTimeWindow-url'),
            addStockUnitUrl: this.element.attr('data-addStockUnit-url'),
            approveContentUrl: this.element.attr('data-approveProduct-url'),
            createMatrixUrl: this.element.attr('data-createMatrix-url'),
            deleteAllStockUnitsUrl: this.element.attr('data-deleteAllStockUnits-url'),
            deleteSelectedStockUnitsUrl: this.element.attr('data-deleteSelectedStockUnits-url'),
            deleteImageUrl: this.element.attr('data-deleteProductImage-url'),
            deleteProductAssociationUrl: this.element.attr('data-deleteProductAssociation-url'),
            deleteProductUrl: this.element.attr('data-deleteProduct-url'),
            deleteProductPriceUrl: this.element.attr('data-deleteProductPrice-url'),
            deleteVersionUrl: this.element.attr('data-deleteProductVersion-url'),
            deleteTimeWindowUrl: this.element.attr('data-deleteTimeWindow-url'),
            getAttributesUrl: this.element.attr('data-getAttributes-url'),
            getCategoriesListUrl: this.element.attr('data-getCategoriesList-url'),
            getEventTypesUrl: this.element.attr('data-getEventTypes-Url'),
            getEventTypeUrl: this.element.attr('data-getEventType-Url'),
            getImagesSectionUrl: this.element.attr('data-getProductImages-url'),
            getProductUrl: this.element.attr('data-getProduct-url'),
            getProductAssociationUrl: this.element.attr('data-getProductAssociations-url'),
            getProductStockUrl: this.element.attr('data-getProductStock-url'),
            getCustomEditorObjects: this.element.attr('data-getCustomEditorObjects-url'),
            getSaleDatesUrl: this.element.attr('data-getSaleDates-url'),
            getTimeWindowUrl: this.element.attr('data-getTimeWindow-url'),
            previewContentUrl: this.element.attr('data-previewContent-url'),
            removeAttributeUrl: this.element.attr('data-removeAttribute-url'),
            saveProductAssociationUrl: this.element.attr('data-saveProductAssociation-url'),
            saveProductOrderUrl: this.element.attr('data-saveProductsOrder-url'),
            searchProductUrl: this.element.attr('data-searchProduct-url'),
            updateProductUrl: this.element.attr('data-updateProduct-url'),
            updateProductPriceUrl: this.element.attr('data-updateProductPrice-url'),
            updateTimeWindowUrl: this.element.attr('data-updateTimeWindow-url'),
            saveEventTypeUrl: this.element.attr('data-saveeventtype-url')
        }

        this.cssFile = this.element.attr('data-productCSSFile');
        this.productType = this.element.attr('data-productType');
    },
    _getAttributes: function (data, event) {
        $.post(this.settings.getAttributesUrl, $.proxy(this._getAttributesCompleted, this));
    },
    _getAttributesCompleted: function (data) {
        if (data && data.attributes) {
            this.viewModel.attributes(data.attributes);
        }
    },
    _onDomainChange: function() {
        this._getSelectors();
        MessageManager.dispatchMessage(UIMessage.DomainChanged, this.viewModel.domainID());
    },
    _getSelectors: function () {
        this._getCategories();
        this._getEventTypes();
    },
    _getCategories: function (data, event) {
        $.post(this.settings.getCategoriesListUrl, { productID: this.productID, type: this.productString(), domainID: this.viewModel.domainID() }, $.proxy(this._getCategoriesCompleted, this));
    },
    _getCategoriesCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.categories(data.categoriesList);
            this.viewModel.categoryID(data.selectedCategoryID);
        }
    },
    _getEventTypes: function () {
        $.post(this.settings.getEventTypesUrl, { productID: this.productID, contentID: $('#ContentID').val() }, $.proxy(this._getEventTypesCompleted, this));
    },
    _getEventTypesCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.eventTypes(data.eventTypes);
            if (this.supressEventID != 0) {
                this.viewModel.eventTypeID(this.supressEventID);
                this.supressEventID = 0;
            }
            else {
                this.viewModel.eventTypeID(data.selected);
            }
        }
    },
    _onSubmitEventType: function (data, event) {
        $.ajax({
            url: this.settings.saveEventTypeUrl,
            type: 'POST',
            data: ko.toJSON(data),
            contentType: 'application/json; charset=utf-8',
            success: $.proxy(this._onSubmitEventTypeCompleted, this),
            error: this._errorMessage
        });
    },
    _onSubmitEventTypeCompleted: function (data) {
        if (data) {
            if (data.success) {
                $("#EventTypeDialog").dialog("close");
                var model = ko.mapping.toJS(this.viewModel.currentEventType);
                this._getEventTypes();
                this.supressEventID = data.eventTypeID;
            }
        }
    },
    _getSaleDates: function (productPriceID) {
        $.post(this.settings.getSaleDatesUrl, { productPriceID: productPriceID }, $.proxy(this._getSaleDatesCompleted, this));
    },
    _getSaleDatesCompleted: function (data) {
        if (data) {
            $('#TimeWindowPopup').html(data);
            this._applyViewModel($('#TimeWindowPopup'));
            $.validator.unobtrusive.parse($('#TimeWindowPopup'));
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);

            if (!$('#TimeWindowPopup').dialog("isOpen"))
                $('#TimeWindowPopup').dialog("open");
        }
    },
    _getStockInfo: function (data, event) {
        $.post(this.settings.getProductStockUrl, { productID: this.productID }, $.proxy(this._getStockInfoCompleted, this));
    },
    _getStockInfoCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this._insertPartialView(data);
            this._initializePopupWindow();
        }
    },
    _initializeChangeEvent: function () {
        $('input:not([type="button"])').change($.proxy(this._onChange, this));
        $('textarea').change($.proxy(this._onChange, this));
        $('select:not(#sVersions):not(#CategoryID)').change($.proxy(this._onChange, this));
    },
    _initializeImageForm: function () {
        $("#ImagesUpload").ajaxForm({
            dataType: 'json',
            beforeSubmit: $.proxy(this._onBeforeSubmitImages, this),
            success: $.proxy(this._onSubmitImagesCompleted, this),
            uploadProgress: $.proxy(this._onProgress, this)
        });
    },
    _insertPartialView: function (data) {
        this.element.find('.mainContainer').html(data);
        this._applyViewModel(this.element.find('.mainContainer'));
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        if ($('#ProductContent').length > 0) {
            this._initializeChangeEvent();
            MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
                elementSelector: "#ProductContent",
                url: this.settings.getCustomEditorObjects,
                domainID: $('#DomainID').val(),
                cssFile: this.cssFile
            });
        }
        this._initializeImageForm();
        $.validator.unobtrusive.parse(this.element);
    },
    _initializePopupWindow: function () {
        $("#TimeWindowPopup").dialog({
            autoOpen: false,
            closeOnEscape: true,
            modal: true,
            title: "Prices by time",
            width: "950px",
            close: $.proxy(function () {
                this._getStockInfo();
            }, this)
        });
    },
    _onAddAssociation: function (data, event) {
        var productID = $('#ddlProducts').val();
        $.post(this.settings.saveProductAssociationUrl, { productID: this.productID, associatedProductID: productID }, $.proxy(this._onAddAssociationCompleted, this));
    },
    _onAddAssociationCompleted: function (data) {
        if (data && data.success) {
            alert("Association saved successfully.");
            $.post(this.settings.getProductAssociationUrl, { productID: this.productID }, $.proxy(this._onAssociateCompleted, this));
        }
    },
    _onAddAttribute: function (data, event) {
        $('#AddAttributeWindow').dialog({ title: "Add Attrbiute", modal: true });
    },
    _onAddNewSalePrice: function (data, event) {
        var priceID = $(event.currentTarget).siblings('#TimeWindows').attr('data-priceID');
        $.post(this.settings.getTimeWindowUrl, { productPriceTimeWindowID: 0, productPriceID: priceID }, $.proxy(this._onAddNewSalePriceCompleted, this));
    },
    _onAddNewSalePriceCompleted: function (data) {
        if (data) {
            $('#TimeWindows').append(data);
            this._applyViewModel($('#TimeWindows > form:last-child'));
            $.validator.unobtrusive.parse($('#TimeWindows'));
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        }
    },
    _onAddNewTimeWindow: function (data, event) {
        var form = $(event.currentTarget).closest('form');
        this._saveTimeWindow(this.settings.addNewTimeWindowUrl, form);
    },
    _onApprove: function (data, event) {
        event.preventDefault();
        var contentID = 0;
        var productID = 0;
        if ($(event.currentTarget).get(0).tagName === "A") {
            var menuItem = $(event.currentTarget).closest('.menuItem');
            if (menuItem)
                productID = menuItem.attr('data-itemID');

            this.reload = productID == this.productID;
        }
        else if ($(event.currentTarget).get(0).tagName === "INPUT") {
            this.reload = true;
            contentID = $('#ContentID').val();
            productID = $('#SitemapID').val();
        }

        $.post(this.settings.approveContentUrl, { productID: productID, contentID: contentID }, $.proxy(this._onApproveCompleted, this));
    },
    _onApproveCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Content approved successfully.");
                this._onFullList();
                if (this.reload)
                    $.post(this.settings.getProductUrl, { type: this.productType, productID: data.productID }, $.proxy(this._onEditCompleted, this));

                this.reload = false;
            }
        }
    },
    _onAssociate: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.productID = menuItem.attr('data-itemID');

        $.post(this.settings.getProductAssociationUrl, { productID: this.productID }, $.proxy(this._onAssociateCompleted, this));
    },
    _onAssociateCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this._insertPartialView(data);
        }
    },
    _onBeforeSubmitImages: function (arr, $form, options) {
        $('#UploadWindow').dialog({ title: "Image Upload", modal: true, width: 250 });
        $('#Progressbar').progressbar();
    },
    _onChange: function (data, event) {
        $('#btnApproveProduct').prop("disabled", true).attr("title", "Page settings changed, please save new changes first.");
    },
    _onCheckAll: function (data, event) {
        if ($(".selectAll").is(":checked")) {
            $('.selected').prop('checked', true);
        }
        else {
            $('.selected').prop('checked', false);
        }
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone. This will remove all pricing, stock and attribute information from the system";
        var productID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (productID == this.productID)
                this.reload = true;

            $.post(this.settings.deleteProductUrl, { productID: productID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert(this.productString() + " deleted successfully.");
                this._onFullList();
                if (this.reload)
                    this._onNewProduct();
                this.reload = false;
            }
        }
    },
    _onDeleteAssociation: function (data, event) {
        if (confirm("Are you sure you want to delete this association?")) {
            var productID = $(event.currentTarget).attr('data-productID');
            $.post(this.settings.deleteProductAssociationUrl, { productID: this.productID, associatedProductID: productID }, $.proxy(this._onDeleteAssociationCompleted, this));
        }
    },
    _onDeleteAssociationCompleted: function (data) {
        if (data && data.success) {
            alert("Association deleted successfully.");
            $.post(this.settings.getProductAssociationUrl, { productID: this.productID }, $.proxy(this._onAssociateCompleted, this));
        }
    },
    _onDeleteAllUnits: function (data, event) {
        if (confirm("Are you sure you want to delete all stock units?")) {
            $.post(this.settings.deleteAllStockUnitsUrl, { productID: this.productID }, $.proxy(this._onDeleteAllUnitsCompleted, this));
        }
    },
    _onDeleteAllUnitsCompleted: function (data) {
        if (data && data.success) {
            alert("All Stock Units deleted successfully.");
            this._getStockInfo();
        }
    },
    _onDeleteImage: function (data, event) {
        var imageID = $(event.currentTarget).attr('data-imageID');

        if (confirm("Are you sure you wish to delete this image? This action cannot be undone.")) {
            $.post(this.settings.deleteImageUrl, { imageID: imageID }, $.proxy(this._onDeleteImageCompleted, this));
        }
    },
    _onDeleteImageCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Image deleted successfully.");
                $.post(this.settings.getImagesSectionUrl, { productID: this.productID }, $.proxy(this._onEditImageCompleted, this));
            }
        }
    },
    _onDeleteContent: function (data, event) {
        var contentDate = _.str.trim($('#sVersions').find('option:selected').text());
        if (confirm("Are you sure you want to delete content created at " + contentDate)) {
            if ($('#sVersions').val() == $('#ContentID').val())
                this.reload = true;

            $.post(this.settings.deleteVersionUrl, { contentID: $('#sVersions').val() }, $.proxy(this._onDeleteContentCompleted, this));
        }
    },
    _onDeleteContentCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Page content version deleted successfully.");
                this._onFullList();
                if (this.reload)
                    $.post(this.settings.getProductUrl, { type: this.productType, productID: this.productID }, $.proxy(this._onEditCompleted, this));
                else
                    $('#sVersions').find('option:selected').remove();
                this.reload = false;
            }
        }
    },
    _onDeleteSelectedStockUnit: function (data, event) {
        var array = $("tbody input:checkbox[name=selected]:checked");
        if (array.length > 0) {
            if (confirm("Are you sure you want to delete selected items?")) {
                var priceID = new Array();
                $("tbody input:checkbox[name=selected]:checked").each(function () {
                    priceID.push($(this).closest('tr').attr('data-priceID'));
                });
                $.ajax(this.settings.deleteSelectedStockUnitsUrl, {
                    type: "POST",
                    data: { priceID: priceID },
                    traditional: true,
                    success: $.proxy(this._onDeleteStockUnitCompleted, this)
                });
            }
        }
    },
    _onDeleteStockUnit: function (data, event) {
        var priceID = $(event.currentTarget).closest('tr').attr('data-priceID');

        if (confirm("Are you sure you want to delete this Stock Unit?")) {
            $.post(this.settings.deleteProductPriceUrl, { priceID: priceID }, $.proxy(this._onDeleteStockUnitCompleted, this));
        }
    },
    _onDeleteStockUnitCompleted: function (data) {
        if (data && data.success) {
            this._getStockInfo();
        }
    },
    _onDeleteTimeWindow: function (data, event) {
        if (confirm("Are you sure you want to delete this price? This action can not be undone.")) {
            var timewindowID = $(event.currentTarget).siblings('[name="ProductPriceTimeWindowID"]').val();
            $.post(this.settings.deleteTimeWindowUrl, { timeWindowID: timewindowID }, $.proxy(this._onDeleteTimeWindowCompleted, this));
        }
    },
    _onDeleteTimeWindowCompleted: function (data) {
        if (data && data.success) {
            var priceID = $('#TimeWindows').attr('data-priceID');
            this._getSaleDates(priceID);
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.productID = menuItem.attr('data-itemID');

        $.post(this.settings.getProductUrl, { type: this.productType, productID: this.productID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.viewModel.sectionHeader("Update Product");
            this.viewModel.domainID($(data).find("#DomainID").val());
            this._insertPartialView(data);
            this._getSelectors();
        }
    },
    _onEditImage: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.productID = menuItem.attr('data-itemID');

        $.post(this.settings.getImagesSectionUrl, { productID: this.productID }, $.proxy(this._onEditImageCompleted, this));
    },
    _onEditImageCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this._insertPartialView(data);
        }
    },
    _onEditEventType: function (data, event) {
        ko.mapping.fromJS(this.emptyEventType, {}, this.viewModel.currentEventType);
        $("#EventTypeDialog").dialog("open");
    },
    _onViewEventType: function () {
        $.post(this.settings.getEventTypeUrl, { eventTypeID: this.viewModel.eventTypeID() }, $.proxy(this._onViewEventTypeCompleted, this));
    },
    _onViewEventTypeCompleted: function(data) {
        if(data){
            if(data.success) {
                ko.mapping.fromJS(data.eventType, {}, this.viewModel.currentEventType);
                $("#EventTypeDialog").dialog("open");
            }
        }
    },
    _onCloseEventTypeDialog: function() {
        $("#EventTypeDialog").dialog("close");
    },
    _onFullList: function (data, event) {
        this.viewModel.isSearch(false);
        LeftMenuManager.loadMenu(this.viewModel, this.settings.searchProductUrl, { type: this.productType, search: "" });
    },
    _onLoadContent: function (data, event) {
        $.post(this.settings.getProductUrl, { type: this.productType, productID: this.productID, contentID: $('#sVersions').val() }, $.proxy(this._onEditCompleted, this));
    },
    _onManageSaleTime: function (data, event) {
        var priceID = $(event.currentTarget).closest('tr').attr('data-priceID');
        this._getSaleDates(priceID);
    },
    _onNewProduct: function (data, event) {
        $.post(this.settings.getProductUrl, { type: this.productType, productID: 0 }, $.proxy(this._onNewProductCompleted, this));
    },
    _onNewProductCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(false);
            this.productID = 0;
            this.viewModel.sectionHeader("Add New " + this.productString());
            this.viewModel.categoryID(null);
            this._insertPartialView(data);
        }
    },
    _onNewStockUnit: function (data, event) {
        $.post(this.settings.addStockUnitUrl, { productID: this.productID }, $.proxy(this._onNewStockUnitCompleted, this));
    },
    _onNewStockUnitCompleted: function (data) {
        if (data && data.success) {
            this._getStockInfo();
        }
    },
    _onPreviewContent: function (data, event) {
        var contentID = $('#sVersions').val();
        var url = this.settings.previewContentUrl + '?sectionID=' + this.productID + '&contentID=' + contentID;
        window.open(url, "_blank");
    },
    _onProgress: function (event, position, total, percentComplete) {
        $("#Progressbar").progressbar("value", percentComplete);
    },
    _onRemoveAttribute: function (data, event) {
        var attrID = $(event.currentTarget).attr('data-attributeID');
        var attrName = $(event.currentTarget).siblings("span").text();

        if (confirm("Are you sure you want to delete '" + attrName + "' attribute?")) {
            $.post(this.settings.removeAttributeUrl, { productID: this.productID, attributeID: attrID }, $.proxy(this._onRemoveAttributeCompleted, this));
        }
    },
    _onRemoveAttributeCompleted: function (data, event) {
        if (data && data.success) {
            this._getStockInfo();
        }
    },
    _onSaveOrder: function (data, event) {
        var orderedItems = $('.leftMenu ul ul .menuItem').map(function () {
            return $(this).attr('data-itemID')
        }).get();

        $.ajax(this.settings.saveProductOrderUrl, {
            type: "POST",
            data: { orderedProductIDs: orderedItems },
            traditional: true,
            success: $.proxy(this._onSaveOrderCompleted, this)
        });
    },
    _onSaveOrderCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Sections order was successfuly saved.");
                $('#orderIndicator').removeClass('icon-exclamation').addClass('icon-ok');
            }
        }
    },
    _onSaveSelected: function (data, event) {
        var parent = this;
        var callbacks = [];
        $("tbody input:checkbox[name=selected]:checked").each(function () {
            callbacks.push(parent._saveStockUnit(this));
        });
        $.when.apply($, callbacks).done(function () {
            alert("Stock units successfully saved.");
        });
    },
    _onSaveStockUnit: function (data, event) {
        this._saveStockUnit(event.currentTarget).done($.proxy(this._onSaveStockUnitCompleted, this));
    },
    _onSaveStockUnitCompleted: function (data) {
        if (data && data.success) {
            alert("Stock unit successfully saved.");
        }
    },
    _onSearch: function (data, event) {
        this.viewModel.isSearch(true);
        LeftMenuManager.loadMenu(this.viewModel, this.settings.searchProductUrl, { type: this.productType, search: $('#tbSearch').val() });
        $('#tbSearch').val('');
    },
    _onSearchKeyUp: function (data, event) {
        if (event.keyCode == 13)
            this._onSearch();
    },
    _onStockMartix: function (data, event) {
        $('#CreateMatrixWindow').dialog({ title: "Create Stock Units", modal: true });
    },
    _onSubmit: function (form) {
        var seoForm = $('#SEOForm');
        var detailsForm = $('#DetailsForm');
        var content = $('#ProductContent');

        $.validator.unobtrusive.parse(this.element);
        seoForm.validate();
        detailsForm.validate();

        var valid = seoForm.valid();
        valid = detailsForm.valid() && valid;

        if (valid) {

            var dataArray = seoForm.serializeArray();
            dataArray = dataArray.concat(detailsForm.serializeArray());
            dataArray.push({ name: "content", value: content.val() });

            var url = this.viewModel.isEdit() ? this.settings.updateProductUrl : this.settings.addProductUrl;

            $.post(url, dataArray, $.proxy(this._onSubmitCompleted, this));
        }
        else
            this.element.find('.mainContainer').scrollTop(0);
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert(this.productString() + " saved successfully.");
                this._onFullList();
                $.post(this.settings.getProductUrl, { type: this.productType, productID: data.productID, contentID: data.contentID }, $.proxy(this._onEditCompleted, this));
                this.isChange = false;
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onSubmitImagesCompleted: function (data) {
        if (data) {
            if (data.success)
                $.post(this.settings.getImagesSectionUrl, { productID: this.productID }, $.proxy(this._onEditImageCompleted, this));
        }
        $('#UploadWindow').dialog("close");
        $("#Progressbar").progressbar("destroy");
        $('#UploadWindow').dialog("destroy");
    },
    _onStock: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.productID = menuItem.attr('data-itemID');
        this._getStockInfo();
    },
    _onUpdateTimeWindow: function (data, event) {
        var form = $(event.currentTarget).closest('form');
        this._saveTimeWindow(this.settings.updateTimeWindowUrl, form);
    },
    _saveStockUnit: function (object) {
        var priceValues = $(object).closest('tr');
        var priceID = priceValues.attr('data-priceID');
        var sku = priceValues.find('[name="SKU"]').val();
        var rrp = parseFloat(priceValues.find('[name="RRP"]').val().replace(',', '.'));
        var price = parseFloat(priceValues.find('[name="Price"]').val().replace(',', '.'));
        var salePrice = parseFloat(priceValues.find('[name="SalePrice"]').val().replace(',', '.'));
        var onSale = priceValues.find('[name="OnSale"]').prop('checked');
        var stock = parseInt(priceValues.find('[name="Stock"]').val());
        var barcode = priceValues.find('[name="Barcode"]').val();
        var weight = parseFloat(priceValues.find('[name="Weight"]').val().replace(',', '.'));
        var startDate = priceValues.find('[name="StartDate"]').val();
        var endDate = priceValues.find('[name="EndDate"]').val();
        var priceForRegular = priceValues.find('[name="PriceForRegularPlan"]').val();

        var attrValuesIDs = priceValues.find('select').map(function () {
            return $(this).val();
        }).get();

        return $.ajax(this.settings.updateProductPriceUrl, {
            type: "POST",
            data: {
                productID: this.productID,
                priceID: priceID,
                barcode: barcode,
                onSale: onSale,
                price: isNaN(price) ? 0 : price,
                RRP: isNaN(rrp) ? 0 : rrp,
                satePrice: isNaN(salePrice) ? 0 : salePrice,
                SKU: sku,
                stock: isNaN(stock) ? 0 : stock,
                weight: isNaN(weight) ? 0 : weight,
                endDate: endDate,
                startDate: startDate,
                attrValues: attrValuesIDs,
                priceForRegular: priceForRegular
            },
            traditional: true
        });
    },
    _saveTimeWindow: function (url, form) {
        form.validate();
        if (form.valid()) {
            $.post(url, form.serializeArray(), $.proxy(this._saveTimeWindowCompleted, this));
        }
    },
    _saveTimeWindowCompleted: function (data) {
        if (data && data.success) {
            this._getSaleDates(data.priceID);
        }
    },
    _errorMessage: function() {
        alert("Can't process this request right now, please try again later.")
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();

        this._initializeChangeEvent();
        this._getAttributes();
        $("#EventTypeDialog").dialog({ title: "Event Type", autoOpen: false, width: 'auto' });

        MessageManager.subscribe(UIMessage.TinyMCEContentChanged, $.proxy(this._onChange, this));
        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#ProductContent",
            url: this.settings.getCustomEditorObjects,
            domainID: $('#DomainID').val(),
            cssFile: this.cssFile
        });
    }
}

$(function () {
    var ProductsManager = new HostPipe.UI.ProductsManager();
    ProductsManager.initialize();
})
