HostPipe.UI.DonationInfoManager = function () {
    this.element = $("#DonationInfoContent");
}

HostPipe.UI.DonationInfoManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createSettings: function () {
        this.settings = {
            deleteDonationInfoUrl: this.element.attr('data-deleteDonationInfo-url'),
            deleteDonationInfoImageUrl: this.element.attr('data-deleteDonationInfoImage-url'),
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            getDonationInfoUrl: this.element.attr('data-getDonationInfo-url'),
            getDonationInfoImageUrl: this.element.attr('data-getDonationInfoImage-url'),
            getDonationInfoDataUrl: this.element.attr('data-getDonationInfoData-url'),
            saveDonationInfoUrl: this.element.attr('data-saveDonationInfo-url')
        };
    },
    _createViewModel: function () {
        this.viewModel = {
            domainID: ko.observable(0),
            domains: ko.observableArray([]),
            lDomainID: ko.observable(),
            isNewDonationInfo: ko.observable(true),
            isDonationInfoEdit: ko.observable(true),
            donationInfoID: ko.observable(0),
            title: ko.observable(),
            amount: ko.observable(),
            description: ko.observable(),
            donationTypeID: ko.observable(),
            live: ko.observable(true),
            sectionHeader: ko.observable('Add Donation Info'),


            onDelete: $.proxy(this._onDeleteDonationInfo, this),
            onDeleteImage: $.proxy(this._onDeleteImage, this),
            onEdit: $.proxy(this._onEditDonationInfo, this),
            onEditImage: $.proxy(this._onEditImage, this),
            onLDomainChange: $.proxy(this._reloadLeftMenu, this),
            onDomainChange: $.proxy(this._onDomainChange, this),
            onSaveDonationInfo: $.proxy(this._onSaveDonationInfo, this),
            onNewDonationInfo: $.proxy(this._onNewDonationInfo, this),
        }

         this.viewModel.donationInfoID.subscribe(function (value) {
            if (value > 0) {
                this.sectionHeader('Update Donation Info');
            }
            else {
                this.sectionHeader('Add Donation Info');
            }
        }, this.viewModel);
    },
    _onDeleteDonationInfo: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = _.str.trim("Are you sure that you wish to delete " + menuItem.find('.title').text()) + "? This action cannot be undone.";
        var donationInfoID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (donationInfoID == this.viewModel.donationInfoID())
                this.reload = true;
            $.post(this.settings.deleteDonationInfoUrl, { donationInfoID: donationInfoID }, $.proxy(this._onDeleteDonationInfoCompleted, this));
        }
    },
    _onDeleteDonationInfoCompleted: function (data, event) {
        if (data && data.success == true) {
            alert("Donation Info successfully deleted");
            this._reloadLeftMenu();
            if (this.reload)
                this._onNewDonationInfo();
            this.reload = false;
        }
    },
    _onDeleteImage: function (data, event) {
        var donationInfoID = $(event.currentTarget).attr('data-donationInfoID');
        if (confirm("Are you sure you wish to delete this image? This action cannot be undone.")) {
            $.post(this.settings.deleteDonationInfoImageUrl, { donationInfoID: donationInfoID }, $.proxy(this._onDeleteImageCompleted, this));
        }
    },
    _onDeleteImageCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Image deleted successfully.");
                $.post(this.settings.getDonationInfoImageUrl, { donationInfoID: this.viewModel.donationInfoID }, $.proxy(this._onEditImageCompleted, this));
            }
        }
    },
    _onEditImage: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.donationInfoID(menuItem.attr('data-itemID'));
        $.post(this.settings.getDonationInfoImageUrl, { donationInfoID: this.viewModel.donationInfoID }, $.proxy(this._onEditImageCompleted, this));
    },
    _onEditImageCompleted: function (data) {
        if (data) {
            this.viewModel.isDonationInfoEdit(false);
            this.viewModel.sectionHeader("Manage Donation Info Image");
            this._insertPartialView(data);
            this._initializeImageForm();
        }
    },
    _onEditDonationInfo: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.donationInfoID(menuItem.attr('data-itemID'));
        if (!this.viewModel.isDonationInfoEdit())
            $.post(this.settings.getDonationInfoUrl, $.proxy(this._onLoadFormCompleted, this));
        else
            $.post(this.settings.getDonationInfoDataUrl, { donationInfoID: this.viewModel.donationInfoID }, $.proxy(this._onEditDonationInfoCompleted, this));
    },
    _onEditDonationInfoCompleted: function (data) {
        if (data && data.success) {
            this.viewModel.isNewDonationInfo(false);
            this.viewModel.donationInfoID(data.donation.DonationInfoID);
            this.viewModel.domainID(data.donation.DI_DomainID);
            this.viewModel.donationTypeID(data.donation.DI_DonationTypeID);
            this.viewModel.title(data.donation.DI_Title);
            this.viewModel.description(data.donation.DI_Description);
            this.viewModel.amount(data.donation.DI_Amount);
            this.viewModel.live(data.donation.DI_Live);
        }
    },
    _onLoadFormCompleted: function (data) {
        if (data) {
            this._insertPartialView(data);
            this.viewModel.isDonationInfoEdit(true);
            $.post(this.settings.getDonationInfoDataUrl, { donationInfoID: this.viewModel.donationInfoID }, $.proxy(this._onEditDonationInfoCompleted, this));
        }
    },
    _onNewDonationInfo: function () {
        this.viewModel.donationInfoID(0);
        this.viewModel.domainID(0);
        this.viewModel.donationTypeID(0);
        this.viewModel.title(null);
        this.viewModel.description(null);
        this.viewModel.amount(null);
        this.viewModel.live(true);
        this.viewModel.isNewDonationInfo(true);
        if(!this.viewModel.isDonationInfoEdit())
            $.post(this.settings.getDonationInfoUrl, $.proxy(this._onLoadFormCompleted, this));
    },
    _onSaveDonationInfo: function () {
        form = $('#DonationInfoForm');
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.settings.saveDonationInfoUrl;
            $.post(url, data, $.proxy(this._onSaveDonationInfoCompleted, this));
        }
        return false;
    },
    _onSaveDonationInfoCompleted: function (data) {
        if (data && data.success) {
            alert("Donation Info successfully saved.");
            this._reloadLeftMenu();
            $.post(this.settings.getDonationInfoDataUrl, { donationInfoID: data.DonationInfoID }, $.proxy(this._onEditDonationInfoCompleted));
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
    _onBeforeSubmitImages: function (arr, $form, options) {
        $('#UploadWindow').dialog({ title: "Image Upload", modal: true, width: 250 });
        $('#Progressbar').progressbar();
    },
    _onProgress: function (event, position, total, percentComplete) {
        $("#Progressbar").progressbar("value", percentComplete);
    },
    _onSubmitImagesCompleted: function (data) {
        if (data) {
            if (data.success)
                $.post(this.settings.getDonationInfoImageUrl, { donationInfoID: this.viewModel.donationInfoID }, $.proxy(this._onEditImageCompleted, this));
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
    _insertPartialView: function (data) {
        this.element.find('.mainContainer').html(data);
        this._applyViewModel(this.element.find('.mainContainer'));
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        $.validator.unobtrusive.parse(this.element);
    },
    _reloadLeftMenu: function () {
        LeftMenuManager.loadMenu(this.viewModel, "", { domainID: this.viewModel.lDomainID });
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
        this._getDomainsList();
    }
}

$(function () {
    var DonationInfoManager = new HostPipe.UI.DonationInfoManager();
    DonationInfoManager.initialize();
})