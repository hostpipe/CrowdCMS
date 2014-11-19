/// <reference path="../../../_references.js" />

HostPipe.UI.TestimonialsManager = function () {
    this.element = $('#TestimonialsContent');
    this.urls = [];
    this.customforms = [];
}

HostPipe.UI.TestimonialsManager.prototype = {
    _applyViewModel: function () {
        ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            date: ko.observable(''),
            client: ko.observable(''),
            company: ko.observable(''),
            content: ko.observable(''),
            isEdit: ko.observable(false),
            sectionHeader: ko.observable("Add New Testimonial"),
            testimonialID: ko.observable(0),

            onApprove: $.proxy(this._onApprove, this),
            onEdit: $.proxy(this._onEdit, this),
            onDelete: $.proxy(this._onDelete, this),
            onNewTestimonial: $.proxy(this._onNewTestimonial, this),
            onSaveOrder: $.proxy(this._onSaveOrder, this),
            onSubmit: $.proxy(this._onSubmit, this),
            onToggleVisibility: $.proxy(this._onToggleVisibility, this)
        }

        this.viewModel.testimonialID.subscribe($.proxy(function (value) {
            if (value == 0) {
                this.viewModel.isEdit(false);
                this.viewModel.sectionHeader("Add New Testimonial");
            }
            else {
                this.viewModel.isEdit(true);
                this.viewModel.sectionHeader("Update Testimonial");
            }

        }, this));
    },
    _createSettings: function () {
        this.settings = {
            addTestimonialUrl: this.element.attr("data-addTestimonial-url"),
            approveTestimonialUrl: this.element.attr("data-approveTestimonial-url"),
            deleteTestimonialUrl: this.element.attr('data-deleteTestimonial-url'),
            getTestimonialUrl: this.element.attr('data-getTestimonial-url'),
            updateTestimonialUrl: this.element.attr('data-updateTestimonial-url'),
            saveTestimonialOrderUrl: this.element.attr('data-saveTestimonialOrder-url'),
            toggleVisibilityUrl: this.element.attr('data-saveTestimonialVisibility-url'),
            getCustomEditorObjects: this.element.attr('data-getCustomEditorObjects-url')
        };

        this.cssFile = this.element.attr('data-domainCSSFile');
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var confirmationText = "Are you sure you wish to delete " + _.str.trim(menuItem.find('.title').text()) + "? This action cannot be undone.";
        var testimonialID = (menuItem.attr('data-itemID'));

        if (confirm(confirmationText)) {
            if (testimonialID == this.viewModel.testimonialID())
                this.reload = true;

            $.post(this.settings.deleteTestimonialUrl, { testimonialID: testimonialID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Testimonial deleted successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                if (this.reload)
                    this._onNewTestimonial();
                this.reload = false;
            }
        }
    },
    _onEdit: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var testimonialID = menuItem.attr('data-itemID');

        $.post(this.settings.getTestimonialUrl, { testimonialID: testimonialID }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this.viewModel.date(data.TestimonialDate);
            this.viewModel.client(data.Client);
            this.viewModel.company(data.Company);
            this.viewModel.testimonialID(data.TestimonialID);
            tinyMCE.activeEditor.setContent(data.TestimonialContent);
            MessageManager.dispatchMessage(UIMessage.TinyMCEContentChanged, this, { element: $('#TestimonialContent') });
        }
    },
    _onNewTestimonial: function () {
        $.post(this.settings.getTestimonialUrl, { testimonialID: 0 }, $.proxy(this._onNewTestimonialCompleted, this));
    },
    _onNewTestimonialCompleted: function (data) {
        if (data) {
            this.viewModel.date(data.TestimonialDate);
            this.viewModel.client('');
            this.viewModel.company('');
            tinyMCE.activeEditor.setContent(data.TestimonialContent);
            MessageManager.dispatchMessage(UIMessage.TinyMCEContentChanged, this, { element: $('#TestimonialContent') });
            this.viewModel.testimonialID(0);
        }
    },
    _onSaveOrder: function () {
        var orderedItems = $('.leftMenu .menuItem').map(function () {
            return $(this).attr('data-itemID')
        }).get();

        $.ajax(this.settings.saveTestimonialOrderUrl, {
            type: "POST",
            data: { orderedMenuItemIDs: orderedItems },
            traditional: true,
            success: $.proxy(this._onSaveOrderCompleted, this)
        });
    },
    _onSaveOrderCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Testimonials order was successfuly saved.");
                $('#orderIndicator').removeClass('icon-exclamation').addClass('icon-ok');
            }
        }
    },
    _onSubmit: function (data, event) {
        tinyMCE.triggerSave();
        var form = $('#TestimonialForm');
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            data.push({ name: "TestimonialContent", value: $('#TestimonialContent').val() });

            var url = this.viewModel.isEdit() ? this.settings.updateTestimonialUrl : this.settings.addTestimonialUrl;
            $.post(url, data, $.proxy(this._onSubmitCompleted, this));
        }
        return false;
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Testimonial saved successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                $.post(this.settings.getTestimonialUrl, { testimonialID: data.testimonialID }, $.proxy(this._onEditCompleted, this));
            }
        }
    },
    _onToggleVisibility: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var testimonialID = menuItem.attr('data-itemID');

        $.post(this.settings.toggleVisibilityUrl, { testimonialID: testimonialID }, $.proxy(this._onToggleVisibilityCompleted, this));
    },
    _onToggleVisibilityCompleted: function (data) {
        if (data && data.success) {
            LeftMenuManager.loadMenu(this.viewModel);
        }
    },
    initialize: function () {
        this._createViewModel();
        this._applyViewModel();
        this._createSettings();

        MessageManager.subscribe(UIMessage.TinyMCEInitialized, $.proxy(this._onNewTestimonial, this));
        MessageManager.dispatchMessage(UIMessage.TinyMCEInitialize, this, {
            elementSelector: "#TestimonialContent",
            url: this.settings.getCustomEditorObjects,
            domainID: 0,
            cssFile: this.cssFile
        });
    }
};

$(function () {
    var TestimonialsManager = new HostPipe.UI.TestimonialsManager();
    TestimonialsManager.initialize();
});