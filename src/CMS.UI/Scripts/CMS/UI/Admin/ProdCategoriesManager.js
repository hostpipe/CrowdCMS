/// <reference path="../../../_references.js" />

HostPipe.UI.ProdCategoriesManager = function () {
    this.element = $('#ProdCategoriesContent');
    this.categoryID = 0;
}

HostPipe.UI.ProdCategoriesManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            categories: ko.observableArray([]),
            pCategoryID: ko.observable(0),
            isEdit: ko.observable(false),
            sectionHeader: ko.observable("Add Category"),
            lDomainID: ko.observable(),
            domains: ko.observableArray([]),

            onLDomainChange: $.proxy(this._reloadLeftMenu, this),
            onApprove: $.proxy(this._onApprove, this),
            onDelete: $.proxy(this._onDelete, this),
            onDeleteImage: $.proxy(this._onDeleteImage, this),
            onDeleteContent: $.proxy(this._onDeleteContent, this),
            onEdit: $.proxy(this._onEdit, this),
            onEditImage: $.proxy(this._onEditImage, this),
            onGetCategories: $.proxy(this._getCategories, this),
            onLoadContent: $.proxy(this._onLoadContent, this),
            onNewCategory: $.proxy(this._onNewCategory, this),
            onPreviewContent: $.proxy(this._onPreviewContent, this),
            onSaveOrder: $.proxy(this._onSaveOrder, this),
            onSubmit: $.proxy(this._onSubmit, this),
            onSubmitImages: $.proxy(this._onSubmitImages, this)
        }
    },
    _createSettings: function () {
        this.settings = {
            approveCategoryUrl: this.element.attr("data-approveCategory-url"),
            deleteCategoryUrl: this.element.attr("data-deleteCategory-url"),
            deleteCategoryImage: this.element.attr("data-deleteCategoryImage-url"),
            deleteCategoryVersionUrl: this.element.attr("data-deleteCategoryVersion-url"),
            getCategoryUrl: this.element.attr("data-getCategory-url"),
            getCategoryImageUrl: this.element.attr("data-getCategoryImage-url"),
            getCategoriesUrl: this.element.attr("data-getCategories-url"),
            getCustomEditorObjects: this.element.attr('data-getCustomEditorObjects-url'),
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            newCategoryUrl: this.element.attr("data-newCategory-url"),
            previewCategoryVersionUrl: this.element.attr("data-previewCategoryVersion-url"),
            saveCategoriesOrderUrl: this.element.attr("data-saveCategoriesOrder-url"),
            updateCategoryUrl: this.element.attr("data-updateCategory-url"),
            productType: this.element.attr("data-productType")
        };

        this.cssFile = this.element.attr('data-categoryCSSFiles');
    },
    _getCategories: function (data, event) {
        $.post(this.settings.getCategoriesUrl, { domainID: $('#ddlDomain').val(), categoryID: this.categoryID, type: this.settings.productType }, $.proxy(this._getCategoriesCompleted, this));
    },
    _getCategoriesCompleted: function (data) {
        if (data && data.categories) {
            this.viewModel.categories(data.categories);
            this.viewModel.pCategoryID(data.pCategoryID);
        }
    },
    _initializeChangeEvent: function () {
        $('input:not([type="button"])').change($.proxy(this._onChange, this));
        $('textarea').change($.proxy(this._onChange, this));
        $('select:not(#sVersions):not(#ddlParent)').change($.proxy(this._onChange, this));
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
        this._initializeChangeEvent();
        this._getCategories();
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#CategoriesContent",
            url: this.settings.getCustomEditorObjects,
            domainID: $('#ddlDomain').val(),
            cssFile: this.cssFile
        });
    },
    _onApprove: function (data, event) {
        var contentID = 0;
        var categoryID = 0;
        if ($(event.currentTarget).get(0).tagName === "A") {
            var menuItem = $(event.currentTarget).closest('.menuItem');
            if (menuItem)
                categoryID = menuItem.attr('data-itemID');

            this.reload = categoryID == this.categoryID;
        }
        else if ($(event.currentTarget).get(0).tagName === "INPUT") {
            this.reload = true;
            contentID = $('#ContentID').val();
            categoryID = this.categoryID;
        }

        $.post(this.settings.approveCategoryUrl, { categoryID: categoryID, contentID: contentID }, $.proxy(this._onApproveCompleted, this));
    },
    _onApproveCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Content approved successfully.");
                $.post(this.settings.getCategoryUrl, { categoryID: data.categoryID }, $.proxy(this._onEditCompleted, this));
                this._reloadLeftMenu();
                this.reload = false;
            }
        }
    },
    _onBeforeSubmitImages: function (arr, $form, options) {
        $('#UploadWindow').dialog({ title: "Image Upload", modal: true, width: 250 });
        $('#Progressbar').progressbar();
    },
    _onChange: function () {
        $('#btnApproveCategory').prop("disabled", true).attr("title", "Category settings changed, please save new changes first.");
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var categoryID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (categoryID == this.categoryID)
                this.reload = true;

            $.post(this.settings.deleteCategoryUrl, { categoryID: categoryID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Page deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewCategory();
                else
                    this._getCategories();
                this.reload = false;
            }
            else {
                alert("Cannot delete category, possible it contains products or subcategories.");
            }
        }
    },
    _onDeleteImage: function (data, event) {
        var imageID = $(event.currentTarget).attr('data-imageID');
        var categoryID = $('#CategoryID').val();

        if (confirm("Are you sure you wish to delete this image? This action cannot be undone.")) {
            $.post(this.settings.deleteCategoryImage, { categoryID: categoryID, imageID: imageID }, $.proxy(this._onDeleteImageCompleted, this));
        }
    },
    _onDeleteImageCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Image deleted successfully.");
                $.post(this.settings.getCategoryImageUrl, { categoryID: this.categoryID }, $.proxy(this._onEditImageCompleted, this));
            }
        }
    },
    _onDeleteContent: function (data, event) {
        var contentDate = _.str.trim($('#sVersions').find('option:selected').text());
        if (confirm("Are you sure you want to delete content created at " + contentDate)) {
            if ($('#sVersions').val() == $('#ContentID').val())
                this.reload = true;

            $.post(this.settings.deleteCategoryVersionUrl, { contentID: $('#sVersions').val() }, $.proxy(this._onDeleteContentCompleted, this));
        }
    },
    _onDeleteContentCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Category content version deleted successfully.");
                if (this.reload)
                    $.post(this.settings.getCategoryUrl, { categoryID: this.categoryID }, $.proxy(this._onEditCompleted, this));
                else
                    $('#sVersions').find('option:selected').remove();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.categoryID = menuItem.attr('data-itemID');

        $.post(this.settings.getCategoryUrl, { categoryID: this.categoryID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.viewModel.sectionHeader("Update Category");
            this._insertPartialView(data);
        }
    },
    _onEditImage: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.categoryID = menuItem.attr('data-itemID');

        $.post(this.settings.getCategoryImageUrl, { categoryID: this.categoryID }, $.proxy(this._onEditImageCompleted, this));
    },
    _onEditImageCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.element.find('.mainContainer').html(data);
            this._applyViewModel(this.element.find('.mainContainer'));
            this._initializeImageForm();
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
            $.validator.unobtrusive.parse(this.element);
        }
    },
    _onLoadContent: function (data, event) {
        $.post(this.settings.getCategoryUrl, { categoryID: this.categoryID, contentID: $('#sVersions').val() }, $.proxy(this._onEditCompleted, this));
    },
    _onNewCategory: function () {
        $.post(this.settings.getCategoryUrl, { categoryID: 0 }, $.proxy(this._onNewCategoryCompleted, this));
    },
    _onNewCategoryCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(false);
            this.categoryID = 0;
            this.viewModel.sectionHeader("Add Category");
            this._insertPartialView(data);
        }
    },
    _onPreviewContent: function () {
        var contentID = $('#sVersions').val();
        var url = this.settings.previewCategoryVersionUrl + '?sectionID=' + this.categoryID + '&contentID=' + contentID;
        window.open(url, "_blank");
    },
    _onProgress: function (event, position, total, percentComplete) {
        $("#Progressbar").progressbar("value", percentComplete);
    },
    _onSaveOrder: function () {
        var orderedItems = $('.leftMenu .menuItem').map(function () {
            return $(this).attr('data-itemID')
        }).get();

        $.ajax(this.settings.saveCategoriesOrderUrl, {
            type: "POST",
            data: { orderedCategoriesIDs: orderedItems },
            traditional: true,
            success: $.proxy(this._onSaveOrderCompleted, this)
        });
    },
    _onSaveOrderCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Categories order was successfuly saved.");
                $('#orderIndicator').removeClass('icon-exclamation').addClass('icon-ok');
            }
        }
    },
    _onSubmit: function (form) {
        var seoForm = $('#SEOForm');
        var detailsForm = $('#DetailsForm');
        var content = $('#CategoriesContent');

        $.validator.unobtrusive.parse(this.element);
        seoForm.validate();
        detailsForm.validate();

        var valid = seoForm.valid();
        valid = detailsForm.valid() && valid;

        if (valid) {

            var dataArray = seoForm.serializeArray();
            dataArray = dataArray.concat(detailsForm.serializeArray());
            dataArray.push({ name: "content", value: content.val() });
            dataArray.push({ name: "domainID", value: $('#ddlDomain').val() });
            dataArray.push({ name: "parentID", value: $('#ddlParent').val() });
            dataArray.push({ name: "taxID", value: $('#ddlTaxes').val() });
            dataArray.push({ name: "categoryTitle", value: $('#tbTitle').val() });
            dataArray.push({ name: "live", value: $('[name="PC_Live"]:checked').val() });
            dataArray.push({ name: "categoryID", value: this.categoryID });
            dataArray.push({ name: "isMenu", value: $('[name="rbMenu"]:checked').val() });
            dataArray.push({ name: "displayType", value: $('[name="rbDisplayType"]:checked').val() });
            dataArray.push({ name: "featured", value: $('[name="PC_Featured"]:checked').val() });
            dataArray.push({ name: "type", value: this.settings.productType });

            var url = this.viewModel.isEdit() ? this.settings.updateCategoryUrl : this.settings.newCategoryUrl;

            $.post(url, dataArray, $.proxy(this._onSubmitCompleted, this));
        }
        else
            this.element.find('.mainContainer').scrollTop(0);
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Category saved successfully.");
                this._reloadLeftMenu();
                $.post(this.settings.getCategoryUrl, { categoryID: data.categoryID, contentID: data.contentID }, $.proxy(this._onEditCompleted, this));
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onSubmitImagesCompleted: function (data) {
        if (data) {
            if (data.success)
                $.post(this.settings.getCategoryImageUrl, { categoryID: this.categoryID }, $.proxy(this._onEditImageCompleted, this));
        }
        $('#UploadWindow').dialog("close");
        $("#Progressbar").progressbar("destroy");
        $('#UploadWindow').dialog("destroy");
    },
    _getDomainsList: function () {
        $.post(this.settings.getDomainsListUrl, { currentDomain: true }, $.proxy(this._getDomainsListCompleted, this));
    },
    _getDomainsListCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.domains(data.domains);
            this.viewModel.lDomainID(data.selected);
            this._reloadLeftMenu();
        }
    },
    _reloadLeftMenu: function () {
        LeftMenuManager.loadMenu(this.viewModel, "", { domainID: this.viewModel.lDomainID() });
    },
    initialize: function () {
        this._createViewModel();
        this._applyViewModel();
        this._createSettings();
        this._getDomainsList();
        this._initializeChangeEvent();
        this._getCategories();

        MessageManager.subscribe(UIMessage.TinyMCEContentChanged, $.proxy(this._onChange, this));
        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#CategoriesContent",
            url: this.settings.getCustomEditorObjects,
            domainID: $('#ddlDomain').val(),
            cssFile: this.cssFile
        });
    }
}

$(function () {
    var ProdCategoriesManager = new HostPipe.UI.ProdCategoriesManager();
    ProdCategoriesManager.initialize();
})