/// <reference path="../../../_references.js" />

HostPipe.UI.GalleryManager = function () {
    this.element = $('#GalleryContent');
    this.galleryID = 0;
    this.emptyGalleryCategory = {
        Title: "",
        GalleryCategoryID: 0
    };
    this.suppressGalleryCategoryID = 0;
}

HostPipe.UI.GalleryManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            galleryID: ko.observable(0),
            isEdit: ko.observable(false),
            sectionHeader: ko.observable("Add Gallery Item"),
            lDomainID: ko.observable(),
            domains: ko.observableArray([]),

            tags: ko.observableArray([]),

            customers: ko.observableArray([]),
            customerID: ko.observable(0),
            email: ko.observable(),
            dtitle: ko.observable(),
            dfirstname: ko.observable(),
            dsurname: ko.observable(),
            isCustomerGallery: ko.observable(false),

            galleryCategoryID: ko.observable(),
            galleryCategories: ko.observableArray([]),

            currentGalleryCategory: {
                Title: ko.observable(),
                GalleryCategoryID: ko.observable(0)
            },

            isImageEdit: ko.observable(false),
            imageID: ko.observable(0),
            imageName: ko.observable(''),
            imagesTags: ko.observableArray([]),
            imageDescription: ko.observable(''),

            onAddImageTag: $.proxy(this._onAddImageTag, this),
            onAddTag: $.proxy(this._onAddTag, this),
            onApprove: $.proxy(this._onApprove, this),
            onCancelImageEdition: $.proxy(this._onCancelImageEdition, this),
            onCloseGalleryCategoryDialog: $.proxy(this._onCloseGalleryCategoryDialog, this),
            onDelete: $.proxy(this._onDelete, this),
            onDeleteContent: $.proxy(this._onDeleteContent, this),
            onDeleteGalleryCategory: $.proxy(this._onDeleteGalleryCategory, this),
            onDeleteGalleryImage: $.proxy(this._onDeleteGalleryImage, this),
            onDeleteImage: $.proxy(this._onDeleteImage, this),
            onDeleteImageTag: $.proxy(this._onDeleteImageTag, this),
            onDeleteTag: $.proxy(this._onDeleteTag, this),
            onEdit: $.proxy(this._onEdit, this),
            onEditGalleryCategory: $.proxy(this._onEditGalleryCategory, this),
            onEditImage: $.proxy(this._onEditImage, this),
            onEditImageItem: $.proxy(this._onEditImageItem, this),
            onGetCustomers: $.proxy(this._onGetCustomers, this),
            onGetGallery: $.proxy(this._getGalleryItems, this),
            onLDomainChange: $.proxy(this._reloadLeftMenu, this),
            onLoadContent: $.proxy(this._onLoadContent, this),
            onNewGallery: $.proxy(this._onNewGallery, this),
            onPreviewContent: $.proxy(this._onPreviewContent, this),
            onPreviewImage: $.proxy(this._onPreviewImage, this),
            onSaveImage: $.proxy(this._onSaveImage, this),
            onSaveImageOrder: $.proxy(this._onSaveImageOrder, this),
            onSaveOrder: $.proxy(this._onSaveOrder, this),
            onScreenGrab: $.proxy(this._onScreenGrab, this),
            onSubmit: $.proxy(this._onSubmit, this),
            onSubmitGalleryCategory: $.proxy(this._onSubmitGalleryCategory, this),
            onToggleVisibility: $.proxy(this._onToggleVisibility, this),
            onUnlinkFromCustomer: $.proxy(this._onUnlinkFromCustomer, this),
            onViewGalleryCategory: $.proxy(this._onViewGalleryCategory, this),
        };
        this.viewModel.customerID.subscribe(function (value) {
            if (value)
                this._getCustomer();
        }, this);
    },
    _createSettings: function () {
        this.settings = {
            addGalleryUrl: this.element.attr("data-addGallery-url"),
            addImageTagUrl: this.element.attr('data-addimagetag-url'),
            addTagUrl: this.element.attr('data-addtag-url'),
            approveGalleryUrl: this.element.attr("data-approveGallery-url"),
            deleteGalleryCategoryUrl: this.element.attr('data-deletecategory-url'),
            deleteGalleryUrl: this.element.attr("data-deleteGallery-url"),
            deleteGalleryVersionUrl: this.element.attr("data-deleteGalleryVersion-url"),
            deleteGalleryImageUrl: this.element.attr("data-deleteGalleryImage-url"),
            deleteTagUrl: this.element.attr('data-deletetag-url'),
            getAllCustomersUrl: this.element.attr('data-getallcustomers-url'),
            getCustomerUrl: this.element.attr('data-getcustomer-url'),
            getCustomEditorObjects: this.element.attr('data-getCustomEditorObjects-url'),
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            getGalleryCategoriesUrl: this.element.attr('data-getcategories-url'),
            getGalleryCategoryUrl: this.element.attr('data-getcategory-url'),
            getGalleryImageUrl: this.element.attr("data-getGalleryImage-url"),
            getGalleryUrl: this.element.attr("data-getGallery-url"),
            getImageTagsUrl: this.element.attr('data-getimagetags-url'),
            getTagsUrl: this.element.attr('data-gettags-url'),
            newGalleryUrl: this.element.attr("data-newGallery-url"),
            previewGalleryVersionUrl: this.element.attr("data-previewGalleryVersion-url"),
            saveGalleryOrderUrl: this.element.attr("data-saveGalleryOrder-url"),
            saveImageOrderUrl: this.element.attr("data-saveImageOrder-url"),
            saveGalleryCategoryUrl: this.element.attr('data-savecategory-url'),
            saveGalleryImageUrl: this.element.attr('data-savegalleryimage-url'),
            saveTagsUrl: this.element.attr('data-savetags-url'),
            screenGrabUrl: this.element.attr("data-screenGrab-url"),
            toggleVisibilityUrl: this.element.attr("data-toggleVisibility-url"),
            updateGalleryUrl: this.element.attr("data-updateGallery-url")
        };

        this.cssFile = this.element.attr('data-galleryCSSFiles');
    },

    _getCustomer: function () {
        if (this.viewModel.customerID() > 0)
            $.post(this.settings.getCustomerUrl, { customerID: this.viewModel.customerID() }, $.proxy(this._getCustomerCompleted, this));
        else
            this._onUnlinkFromCustomer();
    },
    _getCustomerCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.email(data.Email);
            this.viewModel.dtitle(data.Title);
            this.viewModel.dfirstname(data.FirstName);
            this.viewModel.dsurname(data.Surname);
            this.viewModel.customerID(data.CustomerID)
            this.viewModel.isCustomerGallery(true);
        } else {
            this._onUnlinkFromCustomer();
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
    _getImageTags: function () {
        $.post(this.settings.getImageTagsUrl, { imageID: this.viewModel.imageID() }, $.proxy(this._getImageTagsCompleted, this));
    },
    _getImageTagsCompleted: function (data) {
        if (data) {
            this.viewModel.imagesTags(data.tags);
        }
    },
    _getGallery: function (data, event) {
        $.post(this.settings.getGalleryUrl, { domainID: $('#ddlDomain').val(), galleryID: this.galleryID }, $.proxy(this._getGalleryCompleted, this));
    },
    _getGalleryCompleted: function (data) {
        if (data) {
            this.viewModel.galleryID(data.GalleryID);
        }
    },
    _getGalleryCategories: function () {
        $.post(this.settings.getGalleryCategoriesUrl, { galleryID: $('#GalleryID').val() }, $.proxy(this._getGalleryCategoriesCompleted, this));
    },
    _getGalleryCategoriesCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.galleryCategories(data.galleryCategories);
            if (this.suppressGalleryCategoryID != 0) {
                this.viewModel.galleryCategoryID(this.suppressGalleryCategoryID);
                this.suppressGalleryCategoryID = 0;
            }
            else {
                this.viewModel.galleryCategoryID(data.selected);
            }
        }
    },
    _getTags: function () {
        $.post(this.settings.getTagsUrl, { galleryID: this.galleryID }, $.proxy(this._getTagsCompleted, this));
    },
    _getTagsCompleted: function (data) {
        if (data) {
            this.viewModel.tags(data.tags);
        }
    },

    _onAddImageTag: function (data, event) {
        $.post(this.settings.addImageTagUrl, { name: $('#tbNewTag').val(), imageID: this.viewModel.imageID() }, $.proxy(this._onAddImageTagCompleted, this));
    },
    _onAddImageTagCompleted: function (data, event) {
        if (data) {
            if (data.success) {
                this.viewModel.imagesTags.push(data.item);
                $('#tbNewTag').val('');
            }
        }
    },
    _onAddTag: function (data, event) {
        $.post(this.settings.addTagUrl, { name: $('#tbNewTag').val(), galleryID: this.galleryID }, $.proxy(this._onAddTagCompleted, this));
    },
    _onAddTagCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.tags.push(data.item);
                $('#tbNewTag').val('');
            }
        }
    },
    _onDeleteImageTag: function (data, event) {
        var tagID = $(event.currentTarget).attr('data-tagID');
        $.post(this.settings.deleteTagUrl, { tagID: tagID }, $.proxy(this._onDeleteImageTagCompleted, this));
    },
    _onDeleteImageTagCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Tag deleted successfully.");
                this._getImageTags();
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onDeleteTag: function (data, event) {
        var tagID = $(event.currentTarget).attr('data-tagID');
        $.post(this.settings.deleteTagUrl, { tagID: tagID }, $.proxy(this._onDeleteTagCompleted, this));
    },
    _onDeleteTagCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Tag deleted successfully.");
                this._getTags();
            }
            else if (data.error)
                alert(data.error);
        }
    },

    _onGetCustomers: function () {
        $.post(this.settings.getAllCustomersUrl, { domainID: $('#ddlDomain').val() }, $.proxy(this._onGetCustomersCompleted, this));
    },
    _onGetCustomersCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.customers(data.customers);
        }
    },
    _onUnlinkFromCustomer: function () {
        this.viewModel.customerID(0);
        this.viewModel.email('');
        this.viewModel.dtitle('');
        this.viewModel.dfirstname('');
        this.viewModel.dsurname('');
        this.viewModel.isCustomerGallery(false);
    },
    _onPreviewImage: function (data, event) {
        event.preventDefault();
        var imageItem = $(event.currentTarget).siblings('.image-preview').clone().removeClass('hidden');
        $("#ImagePreviewDialog").html(imageItem.get(0).outerHTML);
        $("#ImagePreviewDialog").dialog("open");
    },

    _onEditGalleryCategory: function (data, event) {
        ko.mapping.fromJS(this.emptyGalleryCategory, {}, this.viewModel.currentGalleryCategory);
        $("#GalleryCategoryDialog").dialog("open");
    },
    _onViewGalleryCategory: function () {
        $.post(this.settings.getGalleryCategoryUrl, { galleryCategoryID: this.viewModel.galleryCategoryID() }, $.proxy(this._onViewGalleryCategoryCompleted, this));
    },
    _onViewGalleryCategoryCompleted: function (data) {
        if (data) {
            if (data.success) {
                ko.mapping.fromJS(data.galleryCategories, {}, this.viewModel.currentGalleryCategory);
                $("#GalleryCategoryDialog").dialog("open");
            }
        }
    },
    _onCloseGalleryCategoryDialog: function () {
        $("#GalleryCategoryDialog").dialog("close");
    },
    _onSubmitGalleryCategory: function (data, event) {
        $.ajax({
            url: this.settings.saveGalleryCategoryUrl,
            type: 'POST',
            data: ko.toJSON(data),
            contentType: 'application/json; charset=utf-8',
            success: $.proxy(this._onSubmitGalleryCategoryCompleted, this),
            error: this._errorMessage
        });
    },
    _onSubmitGalleryCategoryCompleted: function (data) {
        if (data) {
            if (data.success) {
                $("#GalleryCategoryDialog").dialog("close");
                var model = ko.mapping.toJS(this.viewModel.currentGalleryCategory);
                this._getGalleryCategories();
                this.suppressGalleryCategoryID = data.GalleryCategoryID;
            }
        }
    },
    _onDeleteGalleryCategory: function () {
        if (confirm("Are you sure you wish to delete this category? This action cannot be undone.")) {
            $.post(this.settings.deleteGalleryCategoryUrl, { galleryCategoryID: $('#GalleryCategoryID').val() }, $.proxy(this._onDeleteGalleryCategoryCompleted, this));
        }
        
    },
    _onDeleteGalleryCategoryCompleted: function(data) {
        if(data) {
            if (data.success) {
                $("#GalleryCategoryDialog").dialog("close");
                this._getGalleryCategories();
            }
            else if (data.error) {
                alert(data.error);
            }
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
    _onBeforeSubmitImages: function (arr, $form, options) {
        $('#UploadWindow').dialog({ title: "Image Upload", modal: true, width: 250 });
        $('#Progressbar').progressbar();
    },
    _onCancelImageEdition: function (data, event) {
        this.viewModel.isImageEdit(false);
        this.viewModel.imageID(0);
        this.viewModel.imageDescription('');

        this._getImageTags();
    },
    _onDeleteGalleryImage: function (data, event) {
        var galleryImageID = $(event.currentTarget).attr('data-galleryimageid');
        var galleryID = $('#GalleryID').val();

        if (confirm("Are you sure you wish to delete this image? This action cannot be undone.")) {
            $.post(this.settings.deleteGalleryImageUrl, { galleryID: galleryID, galleryImageID: galleryImageID }, $.proxy(this._onDeleteGalleryImageCompleted, this));
        }
    },
    _onDeleteGalleryImageCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Image deleted successfully.");
                $.post(this.settings.getGalleryImageUrl, { galleryID: this.galleryID }, $.proxy(this._onEditImageCompleted, this));
            }
        }
    },
    _onEditImage: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.galleryID = menuItem.attr('data-itemID');

        $.post(this.settings.getGalleryImageUrl, { galleryID: this.galleryID }, $.proxy(this._onEditImageCompleted, this));
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
            this._onCancelImageEdition();
        }
    },
    _onEditImageItem: function (data, event) {
        var imageItem = $(event.currentTarget).closest('li');

        this.viewModel.isImageEdit(true);
        this.viewModel.imageID(parseInt(imageItem.attr('data-imageID')));
        this.viewModel.imageDescription(imageItem.attr('data-image-description'));
        this.viewModel.imageName(imageItem.attr('data-preview'));

        this._getImageTags();
    },
    _onImageOrderChanged: function () {
        $('#ImageOrderIndicator span').removeClass('glyphicon-ok ok-green').addClass('glyphicon-warning-sign warning-orange');
    },
    _onSaveImage: function (data, event) {
        var data = $('#ImagesUpload').serializeArray();

        $.post(this.settings.saveGalleryImageUrl, data, $.proxy(this._onSaveImageCompleted, this));
    },
    _onSaveImageCompleted: function (data) {
        if (data && data.success) {
            alert("Image was saved succassfully.");
            $.post(this.settings.getGalleryImageUrl, { galleryID: this.galleryID }, $.proxy(this._onEditImageCompleted, this));
        }
    },
    _onSaveImageOrder: function () {
        var orderedItems = $('ul.sortable-images li').map(function () {
            return $(this).attr('data-sortgalleryimageid')
        }).get();

        $.ajax(this.settings.saveImageOrderUrl, {
            type: "POST",
            data: { orderedGalleryImageIDs: orderedItems },
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
    _onSubmitImagesCompleted: function (data) {
        if (data) {
            if (data.success)
                $.post(this.settings.getGalleryImageUrl, { galleryID: this.galleryID }, $.proxy(this._onEditImageCompleted, this));
        }
        $('#UploadWindow').dialog("close");
        $("#Progressbar").progressbar("destroy");
        $('#UploadWindow').dialog("destroy");
    },

    _initializeChangeEvent: function () {
        $('input:not([type="button"])').change($.proxy(this._onChange, this));
        $('textarea').change($.proxy(this._onChange, this));
        $('select:not(#sVersions):not(#ddlParent)').change($.proxy(this._onChange, this));
    },
    _insertPartialView: function (data) {
        this.element.find('.mainContainer').html(data);
        this.viewModel.customerID(this.element.find('#tbCustomerID').val());
        this._applyViewModel(this.element.find('.mainContainer'));
        this._initializeChangeEvent();
        this._getGalleryCategories();
        this._onGetCustomers();
        this._getCustomer();
        this._getTags();
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
        var galleryID = 0;
        if ($(event.currentTarget).get(0).tagName === "A") {
            var menuItem = $(event.currentTarget).closest('.menuItem');
            if (menuItem)
                galleryID = menuItem.attr('data-itemID');

            this.reload = galleryID == this.galleryID;
        }
        else if ($(event.currentTarget).get(0).tagName === "INPUT") {
            this.reload = true;
            contentID = $('#ContentID').val();
            galleryID = this.galleryID;
        }

        $.post(this.settings.approveGalleryUrl, { galleryID: galleryID, contentID: contentID }, $.proxy(this._onApproveCompleted, this));
    },
    _onApproveCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Content approved successfully.");
                $.post(this.settings.getGalleryUrl, { galleryID: data.galleryID }, $.proxy(this._onEditCompleted, this));
                this._reloadLeftMenu();
                this.reload = false;
            }
        }
    },
    _onChange: function () {
        $('#btnApproveGallery').prop("disabled", true).attr("title", "Gallery settings changed, please save new changes first.");
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + menuItem.find('.title').text().trim() + "? This action cannot be undone.";
        var galleryID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (galleryID == this.galleryID)
                this.reload = true;

            $.post(this.settings.deleteGalleryUrl, { galleryID: galleryID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Gallery item deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewGallery();
                this.reload = false;
            }
            else {
                alert("The gallery item could not be deleted");
            }
        }
    },
    _onDeleteContent: function (data, event) {
        var contentDate = _.str.trim($('#sVersions').find('option:selected').text());
        if (confirm("Are you sure you want to delete content created at " + contentDate)) {
            if ($('#sVersions').val() == $('#ContentID').val())
                this.reload = true;

            $.post(this.settings.deleteGalleryVersionUrl, { contentID: $('#sVersions').val() }, $.proxy(this._onDeleteContentCompleted, this));
        }
    },
    _onDeleteContentCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Gallery content version deleted successfully.");
                if (this.reload)
                    $.post(this.settings.getGalleryUrl, { galleryID: this.galleryID }, $.proxy(this._onEditCompleted, this));
                else
                    $('#sVersions').find('option:selected').remove();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.galleryID = menuItem.attr('data-itemID');

        $.post(this.settings.getGalleryUrl, { galleryID: this.galleryID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.viewModel.sectionHeader("Update Gallery Item");
            this._insertPartialView(data);
        }
    },
    _onLoadContent: function (data, event) {
        $.post(this.settings.getGalleryUrl, { galleryID: this.galleryID, contentID: $('#sVersions').val() }, $.proxy(this._onEditCompleted, this));
    },
    _onNewGallery: function () {
        $.post(this.settings.newGalleryUrl, { galleryID: 0 }, $.proxy(this._onNewGalleryCompleted, this));
    },
    _onNewGalleryCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(false);
            this.galleryID = 0;
            this.viewModel.sectionHeader("Add Gallery Item");
            this._insertPartialView(data);
        }
    },
    _onPreviewContent: function () {
        var contentID = $('#sVersions').val();
        var url = this.settings.previewGalleryVersionUrl + '?sectionID=' + this.galleryID + '&contentID=' + contentID;
        window.open(url, "_blank");
    },
    _onProgress: function (event, position, total, percentComplete) {
        $("#Progressbar").progressbar("value", percentComplete);
    },
    _onSaveOrder: function () {
        var orderedItems = $('.leftMenu .menuItem').map(function () {
            return $(this).attr('data-itemID')
        }).get();

        $.ajax(this.settings.saveGalleryOrderUrl, {
            type: "POST",
            data: { orderedGalleryIDs: orderedItems },
            traditional: true,
            success: $.proxy(this._onSaveOrderCompleted, this)
        });
    },
    _onSaveOrderCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Gallery order was successfuly saved.");
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
            dataArray.push({ name: "galleryTitle", value: $('#tbTitle').val() });
            dataArray.push({ name: "galleryCategoryID", value: $('#ddCategory').val() });
            dataArray.push({ name: "customerID", value: $('#tbCustomerID').val() });
            dataArray.push({ name: "live", value: $('[name="G_Live"]:checked').val() });
            dataArray.push({ name: "displayType", value: $('[name="rbDisplayType"]:checked').val() });
            dataArray.push({ name: "galleryID", value: this.galleryID });

            var url = this.viewModel.isEdit() ? this.settings.updateGalleryUrl : this.settings.addGalleryUrl;

            $.post(url, dataArray, $.proxy(this._onSubmitStage1Completed, this));
        }
        else
            this.element.find('.mainContainer').scrollTop(0);
    },
    _onSubmitStage1Completed: function (data) {
        if (data) {
            if (data.success) {
                this.galleryID = data.galleryID;
                this.contentID = data.contentID;
                var tags = [];
                $('[name="tag"]:checked').each(function (i) {
                    tags.push($(this).val());
                });

                $.ajax(this.settings.saveTagsUrl, {
                    data: { galleryID: this.galleryID, tagIDs: tags, contentID: this.contentID },
                    type: "POST",
                    traditional: true,
                    success: $.proxy(this._onSubmitCompleted, this)
                });
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Gallery saved successfully.");
                this._reloadLeftMenu();
                $.post(this.settings.getGalleryUrl, { galleryID: data.galleryID, contentID: data.contentID }, $.proxy(this._onEditCompleted, this));
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onToggleVisibility: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var galleryID = menuItem.attr('data-itemID');

        if (galleryID == this.galleryID)
            this.reload = true;

        $.post(this.settings.toggleVisibilityUrl, { galleryID: galleryID }, $.proxy(this._onToggleVisibilityCompleted, this));
    },
    _onToggleVisibilityCompleted: function (data) {
        if (data) {
            this._reloadLeftMenu();
            if (data.success) {
                alert("Gallery visibility updated");
                if (this.reload)
                    $.post(this.settings.getGalleryUrl, { galleryID: this.galleryID }, $.proxy(this._onEditCompleted, this));
                this.reload = false;
            }
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
        this._getGalleryCategories();
        this._onGetCustomers();
        this._getImageTags();
        this._getTags();
        $("#GalleryCategoryDialog").dialog({ title: "Gallery Category", autoOpen: false, width: 'auto' });
        $("#ImagePreviewDialog").dialog({ title: "Image Preview", autoOpen: false, width: 'auto' });

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
    var GalleryManager = new HostPipe.UI.GalleryManager();
    GalleryManager.initialize();
})