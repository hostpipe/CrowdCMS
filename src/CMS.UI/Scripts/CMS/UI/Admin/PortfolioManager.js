/// <reference path="../../../_references.js" />

HostPipe.UI.PortfolioManager = function () {
    this.element = $('#PortfolioContent');
    this.portfolioID = 0;
    this.emptyPortfolioCategory = {
        Title: "",
        PortfolioCategoryID: 0
    };
    this.suppressPortfolioCategoryID = 0;
}

HostPipe.UI.PortfolioManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            portfolioID: ko.observable(0),
            isEdit: ko.observable(false),
            sectionHeader: ko.observable("Add Portfolio Item"),
            lDomainID: ko.observable(),
            domains: ko.observableArray([]),

            portfolioCategoryID: ko.observable(),
            portfolioCategories: ko.observableArray([]),

            currentPortfolioCategory: {
                Title: ko.observable(),
                PortfolioCategoryID: ko.observable(0)
            },

            onEditPortfolioCategory: $.proxy(this._onEditPortfolioCategory, this),
            onViewPortfolioCategory: $.proxy(this._onViewPortfolioCategory, this),
            onSubmitPortfolioCategory: $.proxy(this._onSubmitPortfolioCategory, this),
            onClosePortfolioCategoryDialog: $.proxy(this._onClosePortfolioCategoryDialog, this),

            onLDomainChange: $.proxy(this._reloadLeftMenu, this),
            onApprove: $.proxy(this._onApprove, this),
            onDelete: $.proxy(this._onDelete, this),
            onDeleteImage: $.proxy(this._onDeleteImage, this),
            onDeleteContent: $.proxy(this._onDeleteContent, this),
            onEdit: $.proxy(this._onEdit, this),
            onGetPortfolio: $.proxy(this._getPortfolioItems, this),
            onLoadContent: $.proxy(this._onLoadContent, this),
            onNewPortfolio: $.proxy(this._onNewPortfolio, this),
            onPreviewContent: $.proxy(this._onPreviewContent, this),
            onSaveOrder: $.proxy(this._onSaveOrder, this),
            onSubmit: $.proxy(this._onSubmit, this),
            onToggleVisibility: $.proxy(this._onToggleVisibility, this),
            onScreenGrab: $.proxy(this._onScreenGrab, this),
            onDeletePortfolioImage: $.proxy(this._onDeletePortfolioImage, this),
            onEditImage: $.proxy(this._onEditImage, this),
            onSaveImageOrder: $.proxy(this._onSaveImageOrder, this),
            onDeletePortfolioCategory: $.proxy(this._onDeletePortfolioCategory, this)
        }
    },
    _createSettings: function () {
        this.settings = {
            approvePortfolioUrl: this.element.attr("data-approvePortfolio-url"),
            deletePortfolioUrl: this.element.attr("data-deletePortfolio-url"),
            deletePortfolioVersionUrl: this.element.attr("data-deletePortfolioVersion-url"),
            deletePortfolioImageUrl: this.element.attr("data-deletePortfolioImage-url"),
            getPortfolioUrl: this.element.attr("data-getPortfolio-url"),
            getCustomEditorObjects: this.element.attr('data-getCustomEditorObjects-url'),
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            newPortfolioUrl: this.element.attr("data-newPortfolio-url"),
            previewPortfolioVersionUrl: this.element.attr("data-previewPortfolioVersion-url"),
            savePortfolioOrderUrl: this.element.attr("data-savePortfolioOrder-url"),
            updatePortfolioUrl: this.element.attr("data-updatePortfolio-url"),
            addPortfolioUrl: this.element.attr("data-addPortfolio-url"),
            toggleVisibilityUrl: this.element.attr("data-toggleVisibility-url"),
            screenGrabUrl: this.element.attr("data-screenGrab-url"),
            getPortfolioImageUrl: this.element.attr("data-getPortfolioImage-url"),
            saveImageOrderUrl: this.element.attr("data-saveImageOrder-url"),
            getPortfolioCategoriesUrl: this.element.attr('data-getcategories-url'),
            getPortfolioCategoryUrl: this.element.attr('data-getcategory-url'),
            savePortfolioCategoryUrl: this.element.attr('data-savecategory-url'),
            deletePortfolioCategoryUrl: this.element.attr('data-deletecategory-url')
        };

        this.cssFile = this.element.attr('data-portfolioCSSFiles');
    },
    //TODO: Implement following fucntions
    _onEditPortfolioCategory: function (data, event) {
        ko.mapping.fromJS(this.emptyPortfolioCategory, {}, this.viewModel.currentPortfolioCategory);
        $("#PortfolioCategoryDialog").dialog("open");
    },
    _onViewPortfolioCategory: function () {
        $.post(this.settings.getPortfolioCategoryUrl, { portfolioCategoryID: this.viewModel.portfolioCategoryID() }, $.proxy(this._onViewPortfolioCategoryCompleted, this));
    },
    _onViewPortfolioCategoryCompleted: function (data) {
        if (data) {
            if (data.success) {
                ko.mapping.fromJS(data.portfolioCategories, {}, this.viewModel.currentPortfolioCategory);
                $("#PortfolioCategoryDialog").dialog("open");
            }
        }
    },
    _onClosePortfolioCategoryDialog: function () {
        $("#PortfolioCategoryDialog").dialog("close");
    },
    _getPortfolioCategories: function () {
        $.post(this.settings.getPortfolioCategoriesUrl, { portfolioID: $('#PortfolioID').val() }, $.proxy(this._getPortfolioCategoriesCompleted, this));
    },
    _getPortfolioCategoriesCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.portfolioCategories(data.portfolioCategories);
            if (this.suppressPortfolioCategoryID != 0) {
                this.viewModel.portfolioCategoryID(this.suppressPortfolioCategoryID);
                this.suppressPortfolioCategoryID = 0;
            }
            else {
                this.viewModel.portfolioCategoryID(data.selected);
            }
        }
    },
    _onSubmitPortfolioCategory: function (data, event) {
        $.ajax({
            url: this.settings.savePortfolioCategoryUrl,
            type: 'POST',
            data: ko.toJSON(data),
            contentType: 'application/json; charset=utf-8',
            success: $.proxy(this._onSubmitPortfolioCategoryCompleted, this),
            error: this._errorMessage
        });
    },
    _onSubmitPortfolioCategoryCompleted: function (data) {
        if (data) {
            if (data.success) {
                $("#PortfolioCategoryDialog").dialog("close");
                var model = ko.mapping.toJS(this.viewModel.currentPortfolioCategory);
                this._getPortfolioCategories();
                this.suppressPortfolioCategoryID = data.PortfolioCategoryID;
            }
        }
    },
    _onDeletePortfolioCategory: function () {
        if (confirm("Are you sure you wish to delete this category? This action cannot be undone.")) {
            $.post(this.settings.deletePortfolioCategoryUrl, { portfolioCategoryID: $('#PortfolioCategoryID').val() }, $.proxy(this._onDeletePortfolioCategoryCompleted, this));
        }
        
    },
    _onDeletePortfolioCategoryCompleted: function(data) {
        if(data) {
            if (data.success) {
                $("#PortfolioCategoryDialog").dialog("close");
                this._getPortfolioCategories();
            }
            else if (data.error) {
                alert(data.error);
            }
        }
    },

    _onScreenGrab: function () {
        $.post(this.settings.screenGrabUrl, { url: $('#tbLink').val() }, $.proxy(this._onScreenGrabCompleted, this));
    },
    _onScreenGrabCompleted: function (data) {
        this.element.find('#ScreenGrab').html(data);
    },
    _getPortfolio: function (data, event) {
        $.post(this.settings.getPortfolioUrl, { domainID: $('#ddlDomain').val(), portfolioID: this.portfolioID }, $.proxy(this._getPortfolioCompleted, this));
    },
    _getPortfolioCompleted: function (data) {
        if (data) {
            this.viewModel.portfolioID(data.PortfolioID);
        }
    },

    _onDeletePortfolioImage: function (data, event) {
        var portfolioImageID = $(event.currentTarget).attr('data-portfolioimageid');
        var portfolioID = $('#PortfolioID').val();

        if (confirm("Are you sure you wish to delete this image? This action cannot be undone.")) {
            $.post(this.settings.deletePortfolioImageUrl, { portfolioID: portfolioID, portfolioImageID: portfolioImageID }, $.proxy(this._onDeletePortfolioImageCompleted, this));
        }
    },
    _onDeletePortfolioImageCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Image deleted successfully.");
                $.post(this.settings.getPortfolioImageUrl, { portfolioID: this.portfolioID }, $.proxy(this._onEditImageCompleted, this));
            }
        }
    },

    _initializeChangeEvent: function () {
        $('input:not([type="button"])').change($.proxy(this._onChange, this));
        $('textarea').change($.proxy(this._onChange, this));
        $('select:not(#sVersions):not(#ddlParent)').change($.proxy(this._onChange, this));
    },
    _onEditImage: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.portfolioID = menuItem.attr('data-itemID');

        $.post(this.settings.getPortfolioImageUrl, { portfolioID: this.portfolioID }, $.proxy(this._onEditImageCompleted, this));
    },
    _onEditImageCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.element.find('.mainContainer').html(data);
            this._applyViewModel(this.element.find('.mainContainer'));
            this._initializeImageForm();
            this._initilaiseSortableImages();
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
            $.validator.unobtrusive.parse(this.element);
        }
    },
    _initializeImageForm: function () {
        $("#ImagesUpload").ajaxForm({
            dataType: 'json',
            beforeSubmit: $.proxy(this._onBeforeSubmitImages, this),
            success: $.proxy(this._onSubmitImagesCompleted, this),
            uploadProgress: $.proxy(this._onProgress, this)
        });
    },
    _initilaiseSortableImages: function () {
        $("ul.sortable-images").sortable({
            update: $.proxy(this._onImageOrderChanged, this)
        });
    },
    _onSaveImageOrder: function () {
        var orderedItems = $('ul.sortable-images li').map(function () {
            return $(this).attr('data-sortportfolioimageid')
        }).get();

        $.ajax(this.settings.saveImageOrderUrl, {
            type: "POST",
            data: { orderedPortfolioImageIDs: orderedItems },
            traditional: true,
            success: $.proxy(this._onSaveImageOrderCompleted, this)
        });
    },
    _onSaveImageOrderCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Image order saved successfuly.");
                $('#ImageOrderIndicator span').removeClass('glyphicon-warning-sign warning-orange').addClass('glyphicon-ok ok-green');
            }
        }
    },
    _onImageOrderChanged: function () {
        $('#ImageOrderIndicator span').removeClass('glyphicon-ok ok-green').addClass('glyphicon-warning-sign warning-orange');
    },
    _onBeforeSubmitImages: function (arr, $form, options) {
        $('#UploadWindow').dialog({ title: "Image Upload", modal: true, width: 250 });
        $('#Progressbar').progressbar();
    },
    _onSubmitImagesCompleted: function (data) {
        if (data) {
            if (data.success)
                $.post(this.settings.getPortfolioImageUrl, { portfolioID: this.portfolioID }, $.proxy(this._onEditImageCompleted, this));
        }
        $('#UploadWindow').dialog("close");
        $("#Progressbar").progressbar("destroy");
        $('#UploadWindow').dialog("destroy");
    },

    _insertPartialView: function (data) {
        this.element.find('.mainContainer').html(data);
        this._applyViewModel(this.element.find('.mainContainer'));
        this._initializeChangeEvent();
        this._getPortfolioCategories();
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#SEOContent",
            url: this.settings.getCustomEditorObjects,
            domainID: $('#ddlDomain').val(),
            cssFile: this.cssFile
        });
    },
    _onApprove: function (data, event) {
        var contentID = 0;
        var portfolioID = 0;
        if ($(event.currentTarget).get(0).tagName === "A") {
            var menuItem = $(event.currentTarget).closest('.menuItem');
            if (menuItem)
                portfolioID = menuItem.attr('data-itemID');

            this.reload = portfolioID == this.portfolioID;
        }
        else if ($(event.currentTarget).get(0).tagName === "INPUT") {
            this.reload = true;
            contentID = $('#ContentID').val();
            portfolioID = this.portfolioID;
        }

        $.post(this.settings.approvePortfolioUrl, { portfolioID: portfolioID, contentID: contentID }, $.proxy(this._onApproveCompleted, this));
    },
    _onApproveCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Content approved successfully.");
                $.post(this.settings.getPortfolioUrl, { portfolioID: data.portfolioID }, $.proxy(this._onEditCompleted, this));
                this._reloadLeftMenu();
                this.reload = false;
            }
        }
    },
    _onChange: function () {
        $('#btnApprovePortfolio').prop("disabled", true).attr("title", "Portfolio settings changed, please save new changes first.");
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + menuItem.find('.title').text().trim() + "? This action cannot be undone.";
        var portfolioID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (portfolioID == this.portfolioID)
                this.reload = true;

            $.post(this.settings.deletePortfolioUrl, { portfolioID: portfolioID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Portfolio item deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewPortfolio();
                this.reload = false;
            }
            else {
                alert("The portfolio item could not be deleted");
            }
        }
    },
    _onDeleteContent: function (data, event) {
        var contentDate = _.str.trim($('#sVersions').find('option:selected').text());
        if (confirm("Are you sure you want to delete content created at " + contentDate)) {
            if ($('#sVersions').val() == $('#ContentID').val())
                this.reload = true;

            $.post(this.settings.deletePortfolioVersionUrl, { contentID: $('#sVersions').val() }, $.proxy(this._onDeleteContentCompleted, this));
        }
    },
    _onDeleteContentCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Portfolio content version deleted successfully.");
                if (this.reload)
                    $.post(this.settings.getPortfolioUrl, { portfolioID: this.portfolioID }, $.proxy(this._onEditCompleted, this));
                else
                    $('#sVersions').find('option:selected').remove();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.portfolioID = menuItem.attr('data-itemID');

        $.post(this.settings.getPortfolioUrl, { portfolioID: this.portfolioID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.viewModel.sectionHeader("Update Portfolio Item");
            this._insertPartialView(data);
        }
    },
    _onLoadContent: function (data, event) {
        $.post(this.settings.getPortfolioUrl, { portfolioID: this.portfolioID, contentID: $('#sVersions').val() }, $.proxy(this._onEditCompleted, this));
    },
    _onNewPortfolio: function () {
        $.post(this.settings.newPortfolioUrl, { portfolioID: 0 }, $.proxy(this._onNewPortfolioCompleted, this));
    },
    _onNewPortfolioCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(false);
            this.portfolioID = 0;
            this.viewModel.sectionHeader("Add Portfolio Item");
            this._insertPartialView(data);
        }
    },
    _onPreviewContent: function () {
        var contentID = $('#sVersions').val();
        var url = this.settings.previewPortfolioVersionUrl + '?sectionID=' + this.portfolioID + '&contentID=' + contentID;
        window.open(url, "_blank");
    },
    _onSaveOrder: function () {
        var orderedItems = $('.leftMenu .menuItem').map(function () {
            return $(this).attr('data-itemID')
        }).get();

        $.ajax(this.settings.savePortfolioOrderUrl, {
            type: "POST",
            data: { orderedPortfolioIDs: orderedItems },
            traditional: true,
            success: $.proxy(this._onSaveOrderCompleted, this)
        });
    },
    _onSaveOrderCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Portfolio order was successfuly saved.");
                $('#orderIndicator').removeClass('icon-exclamation').addClass('icon-ok');
            }
        }
    },
    _onSubmit: function (form) {
        var seoForm = $('#SEOForm');
        var detailsForm = $('#DetailsForm');
        var content = $('#SEOContent');

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
            dataArray.push({ name: "portfolioTitle", value: $('#tbTitle').val() });
            dataArray.push({ name: "portfolioCategoryID", value: $('#ddCategory').val() });
            dataArray.push({ name: "live", value: $('[name="PO_Live"]:checked').val() });
            dataArray.push({ name: "portfolioID", value: this.portfolioID });
            dataArray.push({ name: "showlink", value: $('[name="PO_ShowLink"]:checked').val() });
            dataArray.push({ name: "link", value: $('#tbLink').val() });

            var url = this.viewModel.isEdit() ? this.settings.updatePortfolioUrl : this.settings.addPortfolioUrl;

            $.post(url, dataArray, $.proxy(this._onSubmitCompleted, this));
        }
        else
            this.element.find('.mainContainer').scrollTop(0);
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Portfolio saved successfully.");
                this._reloadLeftMenu();
                $.post(this.settings.getPortfolioUrl, { portfolioID: data.portfolioID, contentID: data.contentID }, $.proxy(this._onEditCompleted, this));
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onToggleVisibility: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var portfolioID = menuItem.attr('data-itemID');

        if (portfolioID == this.portfolioID)
            this.reload = true;

        $.post(this.settings.toggleVisibilityUrl, { portfolioID: portfolioID }, $.proxy(this._onToggleVisibilityCompleted, this));
    },
    _onToggleVisibilityCompleted: function (data) {
        if (data) {
            this._reloadLeftMenu();
            if (data.success) {
                alert("Portfolio visibility updated");
                if (this.reload)
                    $.post(this.settings.getPortfolioUrl, { portfolioID: this.portfolioID }, $.proxy(this._onEditCompleted, this));
                this.reload = false;
            }
        }
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
        this._getPortfolioCategories();
        $("#PortfolioCategoryDialog").dialog({ title: "Portfolio Category", autoOpen: false, width: 'auto' });

        MessageManager.subscribe(UIMessage.TinyMCEContentChanged, $.proxy(this._onChange, this));
        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#SEOContent",
            url: this.settings.getCustomEditorObjects,
            domainID: $('#ddlDomain').val(),
            cssFile: this.cssFile
        });
    }
}

$(function () {
    var PortfolioManager = new HostPipe.UI.PortfolioManager();
    PortfolioManager.initialize();
})