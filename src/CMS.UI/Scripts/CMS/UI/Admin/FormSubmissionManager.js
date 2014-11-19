/// <reference path="../../../_references.js" />

HostPipe.UI.FormSubmissionManager = function () {
    this.element = $('#FormSubmissions');
}

HostPipe.UI.FormSubmissionManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            formTitle: ko.observable(0),
            sectionHeader: ko.observable("Viewing Form Submissions"),
            domains: ko.observableArray([]),
            lDomainID: ko.observable(),

            onLDomainChange: $.proxy(this._reloadLeftMenu, this),
            onGetComments: $.proxy(this._onGetComments, this),
            onMarkAsRead: $.proxy(this._onMarkAsRead, this),
            onDeleteSubmission: $.proxy(this._onDeleteSubmission, this),
            onDownloadAsCSV: $.proxy(this._onDownloadAsCSV, this)
        }

        this.viewModel.formTitle.subscribe($.proxy(function (value) {
            this.viewModel.sectionHeader("Viewing Form Submissions for " + value);
        }, this));
    },
    _createSettings: function () {
        this.settings = {
            getDomainsListUrl: this.element.attr('data-getDomainsList-url'),
            getFormSubmissionsUrl: this.element.attr('data-getFormSubmissions-url'),
            markAsReadFormSubmissionUrl: this.element.attr('data-markAsReadFormSubmission-url'),
            deleteFormSubmissionUrl: this.element.attr('data-deleteFormSubmission-url'),
            downloadCSVUrl: this.element.attr('data-downloadCSVUrl-url')
        }
    },
    _onDownloadAsCSV: function (data, event) {
        var formID = $('.submissions').first().attr('data-formID');
        var url = this.settings.downloadCSVUrl + "?" + this._serialize(
            {
                formID: formID
            });

        $(event.currentTarget).attr('href', url);
        return true;
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
    _getColumnsCount: function () {
        return Math.round(($(".sectionHeader").width() - 30) / 352);
    },
    _onGetComments: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var formID = menuItem.attr('data-itemID');
        var formTitle = $(menuItem).find(".title").html();

        this.viewModel.formTitle(formTitle);
        $.post(this.settings.getFormSubmissionsUrl, { formID: formID, columns: this._getColumnsCount() }, $.proxy(this._onGetCommentsCompleted, this));
    },
    _onGetCommentsCompleted: function (data) {
        if (data) {
            this.element.find('.mainContainer').html(data);
            this._applyViewModel(this.element.find('.mainContainer'));
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
        }
    },
    _onMarkAsRead: function (data, event) {
        var submission = $(event.currentTarget).closest('.submissions');
        var itemID = submission.attr('data-itemID');
        var formID = submission.attr('data-formID');

        $.post(this.settings.markAsReadFormSubmissionUrl, { formSubmissionID: itemID, formID: formID }, $.proxy(this._onMarkAsReadCompleted, this));
    },
    _onMarkAsReadCompleted: function (data, event) {
        if (data) {
            if (data.success) {
                $.post(this.settings.getFormSubmissionsUrl, { formID: data.formID, columns: this._getColumnsCount() }, $.proxy(this._onGetCommentsCompleted, this));
                this._reloadLeftMenu();
            }
        }
    },
    _onDeleteSubmission: function (data, event) {
        var submission = $(event.currentTarget).closest('.submissions');
        var itemID = submission.attr('data-itemID');
        var formID = submission.attr('data-formID');

        var confirmationText = "Are you sure you wish to delete this form submission? This action cannot be undone.";
        if (confirm(confirmationText)) {
            $.post(this.settings.deleteFormSubmissionUrl, { formSubmissionID: itemID, formID: formID }, $.proxy(this._onDeleteSubmissionCompleted, this));
        }
    },
    _onDeleteSubmissionCompleted: function (data, event) {
        if (data) {
            if (data.success) {
                alert("Form submission deleted successfully.");
                this._reloadLeftMenu();

                $.post(this.settings.getFormSubmissionsUrl, { formID: data.formID, columns: this._getColumnsCount() }, $.proxy(this._onGetCommentsCompleted, this));
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
    }
}

$(function () {
    var FormSubmissionManager = new HostPipe.UI.FormSubmissionManager();
    FormSubmissionManager.initialize();
})