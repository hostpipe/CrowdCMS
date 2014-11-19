/// <reference path="../../../_references.js" />

HostPipe.UI.DomainsManager = function () {
    this.element = $('#DomainContent');
    this.domainID = 0;
    this.isDomain = false;
    this.isDomainLinks = false;
    this.isPaymentLogosConf = false;
}

HostPipe.UI.DomainsManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            isNewDomain: ko.observable(true),
            sectionHeader: ko.observable('Add Domain'),
            isPaypalPayment: ko.observable(),
            togglePaypalPayment: function () {
                this.isPaypalPayment(!this.isPaypalPayment());
            },

            isSagePayPayment: ko.observable(),
            toggleSagePayPayment: function () {
                this.isSagePayPayment(!this.isSagePayPayment());
            },

            isSecureTradingPayment: ko.observable(),
            toggleSecureTrading: function () {
                this.isSecureTradingPayment(!this.isSecureTradingPayment());
            },

            isStripePayment: ko.observable(),
            toggleStripe: function () {
                this.isStripePayment(!this.isStripePayment());
            },

            isMailChimp: ko.observable(),
            toggleMailChimp: function(){
                this.isMailChimp(!this.isMailChimp());
            },
            isTwitter: ko.observable(),
            toggleTwitter: function () {
                this.isTwitter(!this.isTwitter());
            },
            isGoogleAnalytics: ko.observable(),
            toggleGoogleAnalytics: function () {
                this.isGoogleAnalytics(!this.isGoogleAnalytics());
            },
            isCommuniGator: ko.observable(),
            toggleCommuniGator: function () {
                this.isCommuniGator(!this.isCommuniGator());
            },

            isCookieConsentEnabled: ko.observable(),
            toggleCookieConsent: function () {
                this.isCookieConsentEnabled(!this.isCookieConsentEnabled());
            },

            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onDeleteDomainLinks: $.proxy(this._onDeleteDomainLinks, this),
            onManage: $.proxy(this._onManage, this),
            onNewDomain: $.proxy(this._onNewDomain, this),
            onPaymentLogosConf: $.proxy(this._onPaymentLogosConf, this),
            onSubmit: $.proxy(this._onSubmit, this),
            onUpdateIcons: $.proxy(this._updateIcons, this),
            resetDefault: $.proxy(this._resetToDefault, this)
        }
    },

    _updateIcon: function (event) {
        var ele = $(event.currentTarget);
        this._iconUpdater(ele);
    },

    _updateIcons: function () {
        var main = this;
        this.element.find('.updateicon').each(function (i) {
            main._iconUpdater($(this));
        });
    },

    _iconUpdater: function (ele) {
        var id = ele.attr('id').replace(/^\D+/g, '');
        $('#demo_' + id).css('background-color', $('#BackColour' + id).val());
        $('#demo_' + id).css('color', $('#ForeColour' + id).val());
        $('#demo_' + id).css('border-radius', $('#BorderRadius' + id).val() + '%');
    },
    _resetToDefault: function(event) {
        var ele = $(event.currentTarget);
        var id = ele.attr('id').replace(/^\D+/g, '');
        $('#BackColour' + id).val($('#DefaultBackColour' + id).val());
        $('#ForeColour' + id).val($('#DefaultForeColour' + id).val());
        $('#BorderRadius' + id).val($('#DefaultBorderRadius' + id).val());
        this._updateIcons();
    },

    _createSettings: function () {
        this.settings = {
            addDomainUrl: this.element.attr('data-addDomain-url'),
            addDomainLinkUrl: this.element.attr('data-addDomainLink-url'),
            deleteDomainUrl: this.element.attr('data-deleteDomain-url'),
            deleteDomainLinksUrl: this.element.attr('data-deleteDomainLinks-url'),
            getDomainLinksUrl: this.element.attr('data-getDomainLinks-url'),
            getDomainUrl: this.element.attr('data-getDomain-url'),
            getPaymentLogosUrl: this.element.attr('data-getPaymentLogos-url'),
            getSelectSettingsOptions: this.element.attr('data-getSelectSettingsOptions-url'),
            updateDomainUrl: this.element.attr('data-updateDomain-url')
        }
    },
    _initializeImageForm: function () {
        $(".ImageUpload").ajaxForm({
            dataType: 'json',
            beforeSubmit: $.proxy(this._onBeforeSubmitImages, this),
            success: $.proxy(this._onSubmitImagesCompleted, this),
            uploadProgress: $.proxy(this._onProgress, this)
        });
        $(".ImageDelete").ajaxForm({
            dataType: 'json',
            beforeSubmit: $.proxy(this._confirmDelete, this),
            success: $.proxy(this._onSubmitImagesCompleted, this),
        });
    },
    _confirmDelete: function () {
        return confirm("Are you sure you wish to delete this logo?");
    },
    _onBeforeSubmitImages: function (arr, $form, options) {
        if (arr[2].value == "")
            return false;
        $('#UploadWindow').dialog({ title: "Image Upload", modal: true, width: 250 });
        $('#Progressbar').progressbar();
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var domainID = menuItem.attr('data-itemID');

        if (confirm(confirmationText)) {
            if (domainID == this.domainID)
                this.reload = true;

            $.post(this.settings.deleteDomainUrl, { domainID: domainID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Domain deleted successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                if (this.reload)
                    this._onNewDomain();
                this.reload = false;
            }
            else if (data.warning) {
                if (confirm(data.error + " Are you sure you wish to delete this domain? This action cannot be undone.")) {
                    $.post(this.settings.deleteDomainUrl, { domainID: data.domainID, forced: true }, $.proxy(this._onDeleteCompleted, this));
                }
            }
        }
    },
    _onDeleteDomainLinks: function (data, event) {
        event.preventDefault();
        var domainLinksToDelete = [];
        this.element.find('input:checked').each(function (i) {
            domainLinksToDelete.push($(this).val());
        });

        if (confirm("Are you sure you wish to delete selected domain links? This action cannot be undone.")) {
            $.ajax(this.settings.deleteDomainLinksUrl, {
                type: "POST",
                data: { domainLinksIDs: domainLinksToDelete },
                traditional: true,
                success: $.proxy(this._onDeleteDomainLinksCompleted, this)
            });
        }
    },
    _onDeleteDomainLinksCompleted: function (data) {
        if (data) {
            alert("Domain links successfully deleted.");
            this._updatePage(data);
        }
    },
    _onEdit: function (data, event) {
        this._setContent(true, false, false);
        event.preventDefault();
        this.viewModel.isNewDomain(false);
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.domainID = menuItem.attr('data-itemID');

        $.post(this.settings.getDomainUrl, { domainID: this.domainID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.sectionHeader("Edit Domain");
            this._updatePage(data);
            this.viewModel.isPaypalPayment($("#IsPaypalPayment").attr('checked') == "checked");
            this.viewModel.isSagePayPayment($("#IsSagePayPayment").attr('checked') == "checked");
            this.viewModel.isSecureTradingPayment($("#IsSecureTradingPayment").attr('checked') == "checked");
            this.viewModel.isStripePayment($("#IsStripePayment").attr('checked') == "checked");
            this.viewModel.isMailChimp($('#DO_EnableMailChimp').prop('checked'));
            this.viewModel.isTwitter($('#DO_UpdateTwitter').prop('checked'));
            this.viewModel.isGoogleAnalytics($('#DO_GoogleAnalyticsVisible').prop('checked'));
            this.viewModel.isCommuniGator($('#DO_EnableCommuniGator').prop('checked'));
            this.viewModel.isCookieConsentEnabled($("#IsCookieConsentEnabled").attr('checked') == "checked");
            this._getSelectSettings();
            var main = this;
            $('.updateicon').change(function(event) {
                main._updateIcon(event);
            });
            $('.reset').click(function (event) {
                main._resetToDefault(event);
            });
        }
    },
    _getSelectSettings: function () {
        this._styleTiles();
        var context = this;
        $(".dynamic select").each(function () {
            var selectID = $(this).attr("data-selectID");
            if (selectID) {
                $.post(context.settings.getSelectSettingsOptions, { settingID: selectID, domainID: context.domainID }, $.proxy(context._onSelectOptionsCompleted, this));
            }
        });
    },
    _onSelectOptionsCompleted: function (data) {
        if (data && data.success) {
            var context = this;
            var selected = $(this).attr("data-savedValue");
            $.each(data.options, function (key, value) {
                var option = $("<option></option>").attr("value", value.Value).html(value.Text);
                if (selected == value.Value) {
                    option.attr('selected', 'selected');
                }
                $(context).append(option);
            });
        }
    },
    _onManage: function (data, event) {
        this._setContent(false, true, false);
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.domainID = menuItem.attr('data-itemID');

        $.post(this.settings.getDomainLinksUrl, { domainID: this.domainID }, $.proxy(this._onManageCompleted, this));
    },
    _onManageCompleted: function (data) {
        if (data) {
            this._updatePage(data);
        }
    },
    _onNewDomain: function () {
        this._setContent(true, false, false);
        this.viewModel.isNewDomain(true);
        this.domainID = 0;
        $.post(this.settings.getDomainUrl, $.proxy(this._onNewDomainCompleted, this));
    },
    _onNewDomainCompleted: function (data) {
        if (data) {
            this.viewModel.sectionHeader("Add Domain");
            this._updatePage(data);
            this._getSelectSettings();
        }
    },
    _onPaymentLogosConf: function (data, event) {
        this._setContent(false, false, true);
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.domainID = menuItem.attr('data-itemID');
        $.post(this.settings.getPaymentLogosUrl, { domainID: this.domainID }, $.proxy(this._onPaymentLogosConfCompleted, this));
    },
    _onPaymentLogosConfCompleted: function (data) {
        if (data) {
            this.viewModel.sectionHeader("Manage Payment Logos");
            this._updatePage(data);
        }
    },
    _onProgress: function (event, position, total, percentComplete) {
        $("#Progressbar").progressbar("value", percentComplete);
    },
    _onSubmit: function (form) {
        form = $(form);
        $.validator.unobtrusive.parse(form);
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = '';
            if (this.isDomain)
                url = this.viewModel.isNewDomain() ? this.settings.addDomainUrl : this.settings.updateDomainUrl;
            else if (this.isDomainLinks)
                url = this.settings.addDomainLinkUrl;

            $.post(url, data, $.proxy(this._onSubmitCompleted, this));
        }
        return false;
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (this.isDomain) {
                if (data.success) {
                    alert("Domain successfully saved.");
                    LeftMenuManager.loadMenu(this.viewModel);
                    $.post(this.settings.getDomainUrl, { domainID: data.domainID }, $.proxy(this._onEditCompleted, this));
                }
            }
            else if (this.isDomainLinks) {
                alert("Domain link successfully created.");
                this._updatePage(data);
            }
        }
    },
    _onSubmitImagesCompleted: function (data) {
        if (data) {
            if (data.success) {
                $("#" + data.id).attr("src", data.src);
            }
        }
        $('#UploadWindow').dialog("close");
        $('#UploadWindow').dialog("destroy");
        $("#Progressbar").progressbar("destroy");
    },
    _setContent: function (domain, domainLinks, paymentLogosConf) {
        this.isDomain = domain;
        this.isDomainLinks = domainLinks;
        this.isPaymentLogosConf = paymentLogosConf;
    },
    _updatePage: function (data) {
        this.element.find('.mainContainer').html(data);
        this._applyViewModel(this.element.find('.mainContainer'));
        MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        this._initializeImageForm();
    },
    _styleTiles: function(){
        $(".configureAnchor").click(function () {
            if (!$(this).hasClass('greyedOut')) {
                $(this).closest('.serviceTile').next().css('display', 'block');
            }
            return false;
        });

        $(".closingX, .closingBttn").click(function () {
            $(this).closest('.dialogTile').css('display', 'none');
        });
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();

        ko.bindingHandlers.fadeVisible = {
            init: function (element, valueAccessor) {
                var value = valueAccessor();
                $(element).toggle(ko.utils.unwrapObservable(value));
            },
            update: function (element, valueAccessor) {
                var value = valueAccessor();
                ko.utils.unwrapObservable(value) ? $(element).fadeIn() : $(element).fadeOut();
            }
        };
    }
}

$(function () {
    var DomainsManager = new HostPipe.UI.DomainsManager();
    DomainsManager.initialize();
})