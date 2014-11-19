/// <reference path="../../../_references.js" />

HostPipe.UI.NewsManager = function () {
    this.element = $('#NewsContent');
    this.isChange = false;
    this.sectionID = 0;
    this.urls = [];
    this.customforms = [];
}

HostPipe.UI.NewsManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _checkChanges: function () {
        if (this.isChange && !confirm("There are unsaved changes on this page, are you sure you want to change the page?"))
            return true;

        this.isChange = false;
        return false;
    },
    _createViewModel: function () {
        this.viewModel = {
            categories: ko.observableArray([]),
            isEdit: ko.observable(this.sectionID == 0 ? false : true),
            sectionHeader: ko.observable("Add News Article"),
            tags: ko.observableArray([]),
            lDomainID: ko.observable(),
            domains: ko.observableArray([]),

            onAddTag: $.proxy(this._onAddTag, this),
            onAddCategory: $.proxy(this._onAddCategory, this),
            onApprove: $.proxy(this._onApprove, this),
            onAuthoriseComment: $.proxy(this._onAuthoriseComment, this),
            onDelete: $.proxy(this._onDelete, this),
            onDeleteCategory: $.proxy(this._onDeleteCategory, this),
            onDeleteComment: $.proxy(this._onDeleteComment, this),
            onDeleteContent: $.proxy(this._onDeleteContent, this),
            onDeleteImage: $.proxy(this._onDeleteImage, this),
            onDeleteTag: $.proxy(this._onDeleteTag, this),
            onEdit: $.proxy(this._onEdit, this),
            onEditImage: $.proxy(this._onEditImage, this),
            onGetComments: $.proxy(this._onGetComments, this),
            onLoadContent: $.proxy(this._onLoadContent, this),
            onNewArticle: $.proxy(this._onNewArticle, this),
            onPreviewContent: $.proxy(this._onPreviewContent, this),
            onSaveNewsCategory: $.proxy(this._onSaveNewsCategory, this),
            onSubmit: $.proxy(this._onSubmit, this),
            onToggleVisibility: $.proxy(this._onToggleVisibility, this),
            onLDomainChange: $.proxy(this._reloadLeftMenu, this),
            onUpdateCaption: $.proxy(this._onUpdateCaption, this)
        }
    },
    _createSettings: function () {
        this.settings = {
            addNewsUrl: this.element.attr('data-addNews-url'),
            addTagUrl: this.element.attr('data-addTag-url'),
            addCategoryUrl: this.element.attr('data-addCategory-url'),
            approveContentUrl: this.element.attr('data-approveContent-url'),
            authoriseCommentUrl: this.element.attr('data-authoriseComment-url'),
            deleteBlogUrl: this.element.attr('data-deleteBlog-url'),
            deleteCategoryUrl: this.element.attr('data-deleteCategory-url'),
            deleteCommentUrl: this.element.attr('data-deleteComment-url'),
            deleteImageUrl: this.element.attr('data-deleteImage-url'),
            deleteVersion: this.element.attr('data-deleteVersion-url'),
            deleteTagUrl: this.element.attr('data-deleteTag-url'),
            getBlogUrl: this.element.attr('data-getBlog-url'),
            getCategoriesUrl: this.element.attr('data-getCategories-url'),
            getCommentsUrl: this.element.attr('data-getComments-url'),
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            getImagesUrl: this.element.attr('data-getImages-url'),
            getTagsUrl: this.element.attr('data-getTags-url'),
            previewContentUrl: this.element.attr('data-previewContent-url'),
            updateNewsUrl: this.element.attr('data-updateNews-url'),
            saveNewsCategoriesUrl: this.element.attr('data-saveNewsCategories-url'),
            toggleVisibilityUrl: this.element.attr('data-saveNewsVisibility-url'),
            getCustomEditorObjects: this.element.attr('data-getCustomEditorObjects-url'),
            updateCaptionUrl: this.element.attr('data-updateCaption-url')
        }

        this.sectionID = this.element.attr('data-sectionID');
        this.cssFile = this.element.attr('data-domainCSSFile');
    },
    _onUpdateCaption: function (data, event) {
        var imageID = $(event.currentTarget).attr('data-imageID');
        var desc = $('#imageCaption-' + imageID).val();
        var linkID = $('#linkID-' + imageID).val();
        $.post(this.settings.updateCaptionUrl, { imageID: imageID, description: desc, slinkID: linkID }, $.proxy(this._onUpdateCaptionCompleted, this));
    },
    _onUpdateCaptionCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Image details updated successfully.");
            } else {
                alert("Image could not be found.");
            }
        }
    },
    _getCategories: function () {
        $.post(this.settings.getCategoriesUrl, { sectionID: this.sectionID }, $.proxy(this._getCategoriesCompleted, this));
    },
    _getCategoriesCompleted: function (data) {
        if (data) {
            this.viewModel.categories(data.categories);
        }
    },
    _getTags: function () {
        $.post(this.settings.getTagsUrl, { sectionID: this.sectionID }, $.proxy(this._getTagsCompleted, this));
    },
    _getTagsCompleted: function (data) {
        if (data) {
            this.viewModel.tags(data.tags);
        }
    },
    _initializeChangeEvent: function () {
        $('input:not([type="button"]):not([type="checkbox"]):not(#tbTag):not(#tbCategory)').change($.proxy(this._onChange, this));
        $('textarea').change($.proxy(this._onChange, this));
        $('select:not(#sVersions)').change($.proxy(this._onChange, this));
    },
    _initializeImageForm: function () {
        $("#ImagesUpload").ajaxForm({
            dataType: 'json',
            success: $.proxy(this._onSubmitImagesCompleted, this),
            beforeSubmit: $.proxy(this._onBeforeSubmitImages, this),
            uploadProgress: $.proxy(this._onProgress, this)
        });
    },
    _insertPartialView: function (data) {
        this.element.find('.mainContainer').html(data);
        this._applyViewModel(this.element.find('.mainContainer'));
        this._initializeChangeEvent();
        this._getCategories();
        this._getTags();
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#BlogContent",
            url: this.settings.getCustomEditorObjects,
            domainID: $('#ddlDomain').val(),
            cssFile: this.cssFile
        });
    },
    _onAddCategory: function (data, event) {
        $.post(this.settings.addCategoryUrl, { name: $('#tbCategory').val(), sitemapID: this.sectionID }, $.proxy(this._onAddCategoryCompleted, this));
    },
    _onAddCategoryCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.categories.push(data.item);
                $('#tbCategory').val('');
            }
        }
    },
    _onAddTag: function (data, event) {
        $.post(this.settings.addTagUrl, { tagName: $('#tbTag').val(), sectionID: this.sectionID }, $.proxy(this._onAddTagCompleted, this));
    },
    _onAddTagCompleted: function (data) {
        if (data) {
            if (data.success) {
                this._getTags();
                $('#tbTag').val('');
            }
        }
    },
    _onApprove: function (data, event) {
        event.preventDefault();
        $.post(this.settings.approveContentUrl, { sectionID: this.sectionID, contentID: $('#ContentID').val() }, $.proxy(this._onApproveCompleted, this));
    },
    _onApproveCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Content approved successfully.");
                $.post(this.settings.getBlogUrl, { sectionID: this.sectionID, contentID: $('#ContentID').val() }, $.proxy(this._onEditCompleted, this));
                this._reloadLeftMenu();
            }
        }
    },
    _onAuthoriseComment: function (data, event) {
        var comment = $(event.currentTarget).closest('.comments');
        var commentID = comment.attr('data-itemID');
        var sitemapID = comment.attr('data-sitemapID');

        $.post(this.settings.authoriseCommentUrl, { commentID: commentID, sitemapID: sitemapID }, $.proxy(this._onAuthoriseCommentCompleted, this));
    },
    _onAuthoriseCommentCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Comment authorized successfully.");
                $.post(this.settings.getCommentsUrl, { sitemapID: this.sitemapID }, $.proxy(this._onGetCommentsCompleted, this));
                this._reloadLeftMenu();
            }
        }
    },
    _onBeforeSubmitImages: function (arr, $form, options) {
        $('#UploadWindow').dialog({ title: "Image Upload", modal: true, width: 250 });
        $('#Progressbar').progressbar();
    },
    _onChange: function () {
        $('#btnApprovePage').prop("disabled", true).attr("title", "Page settings changed, please save new changes first.");
        this.isChange = true;
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var sectionID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (sectionID == this.sectionID)
                this.reload = true;

            $.post(this.settings.deleteBlogUrl, { sectionID: sectionID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Blog entry deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewArticle();
                this.reload = false;
            }
        }
    },
    _onDeleteCategory: function (data, event) {
        var categoryID = $(event.currentTarget).attr('data-categoryID');
        $.post(this.settings.deleteCategoryUrl, { categoryID: categoryID }, $.proxy(this._onDeleteCategoryCompleted, this));
    },
    _onDeleteCategoryCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Category deleted successfully.");
                this._getCategories();
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onDeleteComment: function (data, event) {
        var comment = $(event.currentTarget).closest('.comments');
        var commentID = comment.attr('data-itemID');

        var confirmationText = "Are you sure you wish to delete comment written by " + _.str.trim(comment.find('.comment-author').text()) + " (" + _.str.trim(comment.find('.comment-date').text()) + ")" + "? This action cannot be undone.";

        if (confirm(confirmationText)) {
            $.post(this.settings.deleteCommentUrl, { commentID: commentID, sitemapID: this.sitemapID }, $.proxy(this._onDeleteCommentCompleted, this));
        }
    },
    _onDeleteCommentCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Comment deleted successfully.");
                this._reloadLeftMenu();
                $.post(this.settings.getCommentsUrl, { sitemapID: this.sitemapID }, $.proxy(this._onGetCommentsCompleted, this));
            }
        }
    },
    _onDeleteContent: function (data, event) {
        var contentDate = _.str.trim($('#sVersions').find('option:selected').text());
        if (confirm("Are you sure you want to delete content created at " + contentDate)) {
            if ($('#sVersions').val() == $('#ContentID').val())
                this.reload = true;

            $.post(this.settings.deleteVersion, { contentID: $('#sVersions').val() }, $.proxy(this._onDeleteContentCompleted, this));
        }
    },
    _onDeleteContentCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Selected content version deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    $.post(this.settings.getBlogUrl, { sectionID: this.sectionID }, $.proxy(this._onEditCompleted, this));
                else
                    $('#sVersions').find('option:selected').remove();
                this.reload = false;
            }
        }
    },
    _onDeleteImage: function (data, event) {
        var imageID = $(event.currentTarget).attr('data-imageID');

        if (confirm("Are you sure you wish to delete this image? This action cannot be undone.")) {
            $.post(this.settings.deleteImageUrl, { imageID: imageID }, $.proxy(this._onDeleteImageCompleted, this));
        }
    },
    _onDeleteImageCompleted: function (data) {
        $.post(this.settings.getImagesUrl, { sectionID: this.sectionID }, $.proxy(this._onEditImageCompleted, this));
    },
    _onDeleteTag: function (data, event) {
        var tagID = $(event.currentTarget).attr('data-tagID');
        $.post(this.settings.deleteTagUrl, { tagID: tagID }, $.proxy(this._onDeleteTagCompleted, this));
    },
    _onDeleteTagCompleted: function (data) {
        if (data) {
            if (data.success)
                this._getTags();
        }
    },
    _onEdit: function (data, event) {
        if (!this._checkChanges()) {
            event.preventDefault();
            var menuItem = $(event.currentTarget).closest('.menuItem');
            this.sectionID = menuItem.attr('data-itemID');

            $.post(this.settings.getBlogUrl, { sectionID: this.sectionID }, $.proxy(this._onEditCompleted, this));
        }
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.viewModel.sectionHeader("Update News Article");
            this._insertPartialView(data);
        }
    },
    _onEditImage: function (data, event) {
        if (!this._checkChanges()) {
            event.preventDefault();
            var menuItem = $(event.currentTarget).closest('.menuItem');
            this.sectionID = menuItem.attr('data-itemID');

            $.post(this.settings.getImagesUrl, { sectionID: this.sectionID }, $.proxy(this._onEditImageCompleted, this));
        }
    },
    _onEditImageCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.element.find('.mainContainer').html(data);
            this._applyViewModel(this.element.find('.mainContainer'));
            this._initializeImageForm();
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        }
    },
    _onGetComments: function (data, event) {
        if (!this._checkChanges()) {
            var menuItem = $(event.currentTarget).closest('.menuItem');
            this.sitemapID = menuItem.attr('data-itemID');

            $.post(this.settings.getCommentsUrl, { sitemapID: this.sitemapID }, $.proxy(this._onGetCommentsCompleted, this));
        }
    },
    _onGetCommentsCompleted: function (data) {
        if (data) {
            this.element.find('.mainContainer').html(data);
            this.viewModel.sectionHeader("View comments");
            this._applyViewModel(this.element.find('.mainContainer'));
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        }
    },
    _onLoadContent: function (data, event) {
        if (!this._checkChanges()) {
            $.post(this.settings.getBlogUrl, { sectionID: this.sectionID, contentID: $('#sVersions').val() }, $.proxy(this._onEditCompleted, this));
        }
    },
    _onNewArticle: function () {
        if (!this._checkChanges()) {
            $.post(this.settings.getBlogUrl, { sectionID: 0 }, $.proxy(this._onNewArticleCompleted, this));
        }
    },
    _onNewArticleCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(false);
            this.sectionID = 0;
            this.viewModel.sectionHeader("Add News Article");
            this._insertPartialView(data);
        }
    },
    _onPreviewContent: function () {
        var contentID = $('#sVersions').val();
        var url = this.settings.previewContentUrl + '?sectionID=' + this.sectionID + '&contentID=' + contentID;
        window.open(url, "_blank");
    },
    _onProgress: function (event, position, total, percentComplete) {
        $("#Progressbar").progressbar("value", percentComplete);
    },
    _onSaveNewsCategory: function (data, event) {
        if (this.sectionID > 0) {
            var categories = [];
            $('[name="category"]:checked').each(function (i) {
                categories.push($(this).val());
            });

            $.ajax(this.settings.saveNewsCategoriesUrl, {
                data: { sitemapID: this.sectionID, categoryIDs: categories },
                type: "POST",
                traditional: true,
                success: $.proxy(this._onSaveNewsCategoryCompleted, this)
            });
        }
        return true;
    },
    _onSaveNewsCategoryCompleted: function (data) {
        if (data) {
            if (!data.success)
                alert("There was a problem saving the category for this article.");
        }
    },
    _onSubmit: function (form) {
        var seoForm = $('#SEOForm');
        seoForm.validate();
        if (seoForm.valid()) {

            var dataArray = seoForm.serializeArray();
            dataArray.push({ name: "domainID", value: $('#ddlDomain').val() });
            dataArray.push({ name: "contentID", value: $('#ContentID').val() });
            dataArray.push({ name: "sitemapID", value: this.sectionID });
            dataArray.push({ name: "blogDate", value: $('#BlogDate').val() });
            dataArray.push({ name: "content", value: $('#BlogContent').val() });
            dataArray.push({ name: "tweet", value: $('#tbTweet').val() });
            dataArray.push({ name: "blogPublishDate", value: $('#PublishDate').val() });

            var url = this.viewModel.isEdit() ? this.settings.updateNewsUrl : this.settings.addNewsUrl;

            $.post(url, dataArray, $.proxy(this._onSubmitStage1Completed, this));
        }
        else
            this.element.find('.mainContainer').scrollTop(0);
    },
    _onSubmitStage1Completed: function (data) {
        if (data) {
            if (data.success) {
                this.sectionID = data.sectionID;
                this.contentID = data.contentID;
                var categories = [];
                $('[name="category"]:checked').each(function (i) {
                    categories.push($(this).val());
                });

                $.ajax(this.settings.saveNewsCategoriesUrl, {
                    data: { sitemapID: this.sectionID, categoryIDs: categories },
                    type: "POST",
                    traditional: true,
                    success: $.proxy(this._onSubmitStage2Completed, this)
                });
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onSubmitStage2Completed: function (data) {
        if (data) {
            alert("Blog entry saved successfully.");
            this._reloadLeftMenu();
            $.post(this.settings.getBlogUrl, { sectionID: this.sectionID, contentID: this.contentID }, $.proxy(this._onEditCompleted, this));
            this.isChange = false;
        }
    },
    _onSubmitImagesCompleted: function (data) {
        if (data) {
            if (data.success)
                $.post(this.settings.getImagesUrl, { sectionID: this.sectionID }, $.proxy(this._onEditImageCompleted, this));
        }
        $('#UploadWindow').dialog("close");
        $("#Progressbar").progressbar("destroy");
        $('#UploadWindow').dialog("destroy");
    },
    _onToggleVisibility: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var sectionID = menuItem.attr('data-itemID');

        if (sectionID == this.sectionID)
            this.reload = true;

        $.post(this.settings.toggleVisibilityUrl, { sitemapID: sectionID, contentID: $('#ContentID').val() }, $.proxy(this._onToggleVisibilityCompleted, this));
    },
    _onToggleVisibilityCompleted: function (data) {
        if (data) {
            this._reloadLeftMenu();
            if (data.success) {
                alert("Content approved successfully.");
                if (this.reload)
                    $.post(this.settings.getBlogUrl, { sectionID: this.sectionID, contentID: $('#sVersions').val() }, $.proxy(this._onEditCompleted, this));
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
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._getDomainsList();
        this._getCategories();
        this._getTags();

        MessageManager.subscribe(UIMessage.TinyMCEContentChanged, $.proxy(this._onChange, this));
        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#BlogContent",
            url: this.settings.getCustomEditorObjects,
            domainID: $('#ddlDomain').val(),
            cssFile: this.cssFile
        });
    }
}

$(function () {
    var NewsManager = new HostPipe.UI.NewsManager();
    NewsManager.initialize();
})