/// <reference path="../../../_references.js" />
HostPipe.UI.SectionManager = function () {
    this.element = $('#SectionContent');
    this.isChange = false;
    this.sectionID = 0;
    this.urls = [];
    this.customforms = [];
}

HostPipe.UI.SectionManager.prototype = {
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
            isChildLevel: ko.observable(false),
            isEdit: ko.observable(this.sectionID == 0 ? false : true),
            parentURL: ko.observable(''),
            sectionHeader: ko.observable(this.sectionID == 0 ? "Add New Page" : "Update Page"),
            sections: ko.observable([]),
            domains: ko.observableArray([]),
            lDomainID: ko.observable(),
            parentID: ko.observable(0),

            onLDomainChange: $.proxy(this._reloadLeftMenu, this),
            onApprove: $.proxy(this._onApprove, this),
            onDelete: $.proxy(this._onDelete, this),
            onDeleteImage: $.proxy(this._onDeleteImage, this),
            onDeleteContent: $.proxy(this._onDeleteContent, this),
            onEdit: $.proxy(this._onEdit, this),
            onEditImage: $.proxy(this._onEditImage, this),
            onEditUrl: $.proxy(this._onEditUrl, this),
            onGetParentUrl: $.proxy(this._getParentUrl, this),
            onGetSections: $.proxy(this._getSections, this),
            onLoadContent: $.proxy(this._onLoadContent, this),
            onLevelRadioClick: $.proxy(this._setChildLevelVisibility, this),
            onNewSection: $.proxy(this._onNewSection, this),
            onPreviewContent: $.proxy(this._onPreviewContent, this),
            onSaveOrder: $.proxy(this._onSaveOrder, this),
            onSubmit: $.proxy(this._onSubmit, this),
            onSubmitImages: $.proxy(this._onSubmitImages, this),
            onUpdateCaption: $.proxy(this._onUpdateCaption, this)
        }

        this.viewModel.isChildLevel.subscribe(function (value) {
            if (value) {
                this._getParentUrl();
            }
            else {
                this.viewModel.parentURL('/');
            }
        }, this);
    },
    _createSettings: function () {
        this.settings = {
            addSectionUrl: this.element.attr('data-addSection-url'),
            approveContentUrl: this.element.attr("data-approveContent-url"),
            deleteSectionUrl: this.element.attr('data-deleteSection-url'),
            deleteImageUrl: this.element.attr('data-deleteImage-url'),
            deleteVersion: this.element.attr('data-deleteVersion-url'),
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            getParentUrl: this.element.attr('data-getParentUrl-url'),
            getSectionUrl: this.element.attr('data-getSection-url'),
            getSectionsListUrl: this.element.attr('data-getSections-url'),
            getImagesSectionUrl: this.element.attr('data-getImagesSection-url'),
            previewContentUrl: this.element.attr('data-previewContent-url'),
            updateSectionUrl: this.element.attr('data-updateSection-url'),
            saveSectionOrderUrl: this.element.attr('data-saveSectionOrder-url'),
            getCustomEditorObjects: this.element.attr('data-getCustomEditorObjects-url'),
            updateCaptionUrl: this.element.attr('data-updateCaption-url')
        };

        this.sectionID = this.element.attr('data-sectionID');
        this.cssFile = this.element.attr('data-domainCSSFile');
    },
    _onUpdateCaption: function (data, event) {
        var imageID = $(event.currentTarget).attr('data-imageID');
        var desc = $('#imageCaption-' + imageID).val();
        var heading = $('#imageHeading-' + imageID).val();;
        var linkID = $('#linkID-' + imageID).val();
        $.post(this.settings.updateCaptionUrl, { imageID: imageID, description: desc, slinkID: linkID, heading: heading }, $.proxy(this._onUpdateCaptionCompleted, this));
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
    _getParentUrl: function () {
        if (this.viewModel.parentID() > 0)
            $.post(this.settings.getParentUrl, { parentID: $('#ParentID').val() }, $.proxy(this._getParentUrlCompleted, this));
        else
            this._getParentUrlCompleted('/');
    },
    _getParentUrlCompleted: function (data) {
        if (data) {
            this.viewModel.parentURL(data);
        }
    },
    _getSections: function () {
        $.post(this.settings.getSectionsListUrl, { domainID: $('#DomainID').val(), sectionID: this.sectionID }, $.proxy(this._getSectionsCompleted, this));
    },
    _getSectionsCompleted: function (data) {
        if (data && data.sections) {
            this.viewModel.sections(data.sections);
            this.viewModel.parentID(data.parentID);
            this._suppressChange();
        }
    },
    _initializeChangeEvent: function () {
        $('input:not([type="button"])').change($.proxy(this._onChange, this));
        $('textarea').change($.proxy(this._onChange, this));
        $('select:not(#sVersions)').change($.proxy(this._onChange, this));
    },
    _initializeImageForm: function () {
        $("#ImagesUpload").ajaxForm({
            dataType: 'json',
            beforeSubmit: $.proxy(this._onBeforeSubmitImages, this),
            success: $.proxy(this._onSubmitImagesCompleted, this),
            uploadProgress: $.proxy(this._onProgress, this)
        });
    },
    _initializeFriendUrl: function () {
        $('#Name').friendurl({ id: 'Path' });
    },
    _insertPartialView: function (data) {
        this.element.find('.mainContainer').html(data);
        this._applyViewModel(this.element.find('.mainContainer'));
        this._initializeFriendUrl();
        this._initializeChangeEvent();
        this._setChildLevelVisibility();
        this._getSections();
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#PageContent",
            url: this.settings.getCustomEditorObjects,
            domainID: $('#DomainID').val(),
            cssFile: this.cssFile
        });
    },
    _onApprove: function (data, event) {
        event.preventDefault();
        var contentID = 0;
        var sectionID = 0;
        if ($(event.currentTarget).get(0).tagName === "A") {
            var menuItem = $(event.currentTarget).closest('.menuItem');
            if (menuItem)
                sectionID = menuItem.attr('data-itemID');

            this.reload = sectionID == this.sectionID;
        }
        else if ($(event.currentTarget).get(0).tagName === "INPUT") {
            this.reload = true;
            contentID = $('#ContentID').val();
            sectionID = $('#SiteMapID').val();
        }

        $.post(this.settings.approveContentUrl, { sectionID: sectionID, contentID: contentID }, $.proxy(this._onApproveCompleted, this));
    },
    _onApproveCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Content approved successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    $.post(this.settings.getSectionUrl, { sectionID: data.sectionID }, $.proxy(this._onEditCompleted, this));

                this.reload = false;
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
    _suppressChange: function () {
        $('#btnApprovePage').prop("disabled", false).attr("title", "");
        this.isChange = false;
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var sectionID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (sectionID == this.sectionID)
                this.reload = true;

            $.post(this.settings.deleteSectionUrl, { sectionID: sectionID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Page deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    this._onNewSection();
                else
                    this._getSections();
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
        if (data) {
            if (data.success) {
                alert("Image deleted successfully.");
                $.post(this.settings.getImagesSectionUrl, { sectionID: this.sectionID }, $.proxy(this._onEditImageCompleted, this));
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
                alert("Page content version deleted successfully.");
                this._reloadLeftMenu();
                if (this.reload)
                    $.post(this.settings.getSectionUrl, { sectionID: this.sectionID }, $.proxy(this._onEditCompleted, this));
                else
                    $('#sVersions').find('option:selected').remove();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        if (!this._checkChanges()) {
            event.preventDefault();
            var menuItem = $(event.currentTarget).closest('.menuItem');
            this.sectionID = menuItem.attr('data-itemID');

            $.post(this.settings.getSectionUrl, { sectionID: this.sectionID }, $.proxy(this._onEditCompleted, this));
        }
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.viewModel.sectionHeader("Update Page");
            this._insertPartialView(data);
        }
    },
    _onEditImage: function (data, event) {
        if (!this._checkChanges()) {
            event.preventDefault();
            var menuItem = $(event.currentTarget).closest('.menuItem');
            this.sectionID = menuItem.attr('data-itemID');

            $.post(this.settings.getImagesSectionUrl, { sectionID: this.sectionID }, $.proxy(this._onEditImageCompleted, this));
        }
    },
    _onEditImageCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(true);
            this.element.find('.mainContainer').html(data);
            this._applyViewModel(this.element.find('.mainContainer'));
            this._initializeImageForm();
            Core.addEventToggleSectionBtn();
            $.validator.unobtrusive.parse(this.element);
        }
    },
    _onEditUrl: function () {
        $('#Path').prop("disabled", false);
    },
    _onLoadContent: function (data, event) {
        if (!this._checkChanges()) {
            $.post(this.settings.getSectionUrl, { sectionID: this.sectionID, contentID: $('#sVersions').val() }, $.proxy(this._onEditCompleted, this));
        }
    },
    _onNewSection: function () {
        if (!this._checkChanges()) {
            $.post(this.settings.getSectionUrl, { sectionID: 0 }, $.proxy(this._onNewSectionCompleted, this));
        }
    },
    _onNewSectionCompleted: function (data) {
        if (data) {
            this.viewModel.isEdit(false);
            this.sectionID = 0;
            this.viewModel.sectionHeader("Add New Page");
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
    _onSaveOrder: function () {
        var orderedItems = $('.leftMenu .menuItem').map(function () {
            return $(this).attr('data-itemID')
        }).get();

        $.ajax(this.settings.saveSectionOrderUrl, {
            type: "POST",
            data: { orderedMenuItemIDs: orderedItems },
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
    _onSubmit: function (form) {
        var seoForm = $('#SEOForm');
        var detailsForm = $('#DetailsForm');
        var content = $('#PageContent');

        $.validator.unobtrusive.parse(this.element);
        seoForm.validate();
        detailsForm.validate();

        var valid = seoForm.valid();
        valid = detailsForm.valid() && valid;

        if (valid) {

            var dataArray = seoForm.serializeArray();
            dataArray = dataArray.concat(detailsForm.serializeArray());
            dataArray.push({ name: "content", value: content.val() });
            dataArray.push({ name: "path", value: $('#Path').val() });

            var url = this.viewModel.isEdit() ? this.settings.updateSectionUrl : this.settings.addSectionUrl;

            $.post(url, dataArray, $.proxy(this._onSubmitCompleted, this));
        }
        else
            this.element.find('.mainContainer').scrollTop(0);
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Page saved successfully.");
                this._reloadLeftMenu();
                this.sectionID = data.sectionID;
                $.post(this.settings.getSectionUrl, { sectionID: data.sectionID, contentID: data.contentID }, $.proxy(this._onEditCompleted, this));
                this.isChange = false;
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _onSubmitImagesCompleted: function (data) {
        if (data) {
            if (data.success)
                $.post(this.settings.getImagesSectionUrl, { sectionID: this.sectionID }, $.proxy(this._onEditImageCompleted, this));
        }
        $('#UploadWindow').dialog("close");
        $("#Progressbar").progressbar("destroy");
        $('#UploadWindow').dialog("destroy");
    },
    _setChildLevelVisibility: function () {
        this.viewModel.isChildLevel($('[name="TopLevel"]:checked').val() == "False");
        return true;
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

        this._initializeFriendUrl();
        this._setChildLevelVisibility();
        this._getSections();

        MessageManager.subscribe(UIMessage.TinyMCEContentChanged, $.proxy(this._onChange, this));
        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#PageContent",
            url: this.settings.getCustomEditorObjects,
            domainID: $('#DomainID').val(),
            cssFile: this.cssFile
        });
    }
};

$(function () {
    var SectionManager = new HostPipe.UI.SectionManager();
    SectionManager.initialize();
});