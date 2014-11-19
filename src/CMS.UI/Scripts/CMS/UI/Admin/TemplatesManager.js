/// <reference path="../../../_references.js" />

HostPipe.UI.TemplatesManager = function () {
    this.element = $('#TemplatesContent');
}

HostPipe.UI.TemplatesManager.prototype = {
    _applyViewModel: function () {
        ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            templateID: ko.observable(0),
            useFooter: ko.observable(false),
            footer: ko.observable(''),
            useHeader: ko.observable(false),
            header: ko.observable(''),
            name: ko.observable(''),
            live: ko.observable(false),
            isEdit: ko.observable(''),
            sectionHeader: ko.observable("Add New template"),

            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onNewTemplate: $.proxy(this._onNewTemplate, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.templateID.subscribe($.proxy(function (value) {
            if (value == 0) {
                this.viewModel.isEdit(false);
                this.viewModel.sectionHeader("Add New template");
            }
            else {
                this.viewModel.isEdit(true);
                this.viewModel.sectionHeader("Update template");
            }
        }, this));

        this.viewModel.header.subscribe($.proxy(function (value) {
            tinyMCE.get("Header").setContent(value);
        }, this));

        this.viewModel.footer.subscribe($.proxy(function (value) {
            tinyMCE.get("Footer").setContent(value);
        }, this));
    },
    _createSettings: function () {
        this.settings = {
            addTemplateUrl: this.element.attr("data-addTemplate-url"),
            deleteTemplateUrl: this.element.attr('data-deleteTemplate-url'),
            getTemplateUrl: this.element.attr('data-getTemplate-url'),
            updateTemplateUrl: this.element.attr('data-updateTemplate-url'),

            getCustomEditorObjectsUrl: this.element.attr('data-getCustomEditorObjects-url')
        };

        this.cssFile = this.element.attr('data-domainCSSFile');
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete '" + menuItem.find('.title').text().trim() + "'? This action cannot be undone.";
        var templateID = (menuItem.attr('data-itemID'));

        if (confirm(confirmationText)) {
            if (templateID == this.viewModel.templateID())
                this.reload = true;

            $.post(this.settings.deleteTemplateUrl, { templateID: templateID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("template deleted successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                if (this.reload)
                    this._onNewTemplate();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var templateID = menuItem.attr('data-itemID');

        $.post(this.settings.getTemplateUrl, { templateID: templateID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.useFooter(data.UseFooter);
            this.viewModel.footer(data.Footer);
            this.viewModel.useHeader(data.UseHeader);
            this.viewModel.header(data.Header);
            this.viewModel.templateID(data.TemplateID);
            this.viewModel.name(data.Name);
            this.viewModel.live(data.Live);
            //tinyMCE.activeEditor.setContent(data.templateContent);
            //MessageManager.dispatchMessage(UIMessage.TinyMCEContentChanged, this, { element: $('#templateContent') });
        }
    },
    _onNewTemplate: function () {
        $.post(this.settings.getTemplateUrl, { templateID: 0 }, $.proxy(this._onEditCompleted, this));
    },
    _onSubmit: function (data, event) {
        tinyMCE.triggerSave();
        var form = $('#TemplateForm');
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            //data.push({ name: "templateContent", value: $('#templateContent').val() });

            var url = this.viewModel.isEdit() ? this.settings.updateTemplateUrl : this.settings.addTemplateUrl;
            $.post(url, data, $.proxy(this._onSubmitCompleted, this));
        }
        return false;
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Template saved successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                $.post(this.settings.getTemplateUrl, { templateID: data.templateID }, $.proxy(this._onEditCompleted, this));
            }
        }
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();

        LeftMenuManager.loadMenu(this.viewModel);

        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#Footer",
            url: this.settings.getCustomEditorObjectsUrl,
            domainID: 0,
            cssFile: this.cssFile
        });

        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#Header",
            url: this.settings.getCustomEditorObjectsUrl,
            domainID: 0,
            cssFile: this.cssFile
        });
    }
};

$(function () {
    var TemplatesManager = new HostPipe.UI.TemplatesManager();
    TemplatesManager.initialize();
});