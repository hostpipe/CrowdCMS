/// <reference path="../../../_references.js" />

HostPipe.UI.EventTypesManager = function () {
    this.element = $('#EventTypesContent');
}

HostPipe.UI.EventTypesManager.prototype = {
    _applyViewModel: function (element) {
        if (element)
            ko.applyBindings(this.viewModel, element.get(0));
        else
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createViewModel: function () {
        this.viewModel = {
            eventTypeID: ko.observable(0),
            isEdit: ko.observable(false),
            sectionHeader: ko.observable('Add Event Type'),

            onDelete: $.proxy(this._onDelete, this),
            onDeleteImage: $.proxy(this._onDeleteImage, this),
            onEdit: $.proxy(this._onEdit, this),
            onEditImage: $.proxy(this._onEditImage, this),
            onNewEventType: $.proxy(this._onNewEventType, this),
            onSubmit: $.proxy(this._onSubmit, this)
        }

        this.viewModel.eventTypeID.subscribe(function (value) {
            if (value > 0) {
                this.isEdit(true);
                this.sectionHeader('Update Event Type');
            }
            else {
                this.isEdit(false);
                this.sectionHeader('Add Event Type');
            }
        }, this.viewModel);
    },
    _createSettings: function () {
        this.settings = {
            addEventTypeUrl: this.element.attr('data-addEventType-url'),
            deleteEventTypeUrl: this.element.attr('data-deleteEventType-url'),
            deleteEventTypeImageUrl: this.element.attr('data-deleteEventTypeImage-url'),
            getEventTypeUrl: this.element.attr('data-getEventType-url'),
            getEventTypeImageUrl: this.element.attr('data-getEventTypeImage-url'),
            saveEventTypeImageUrl: this.element.attr('data-saveEventTypeImage-url'),
            updateEventTypeUrl: this.element.attr('data-updateEventType-url')
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
    _insertPartial: function (data) {
        if (data) {
            this.element.find('.mainContainer').html(data);
            this._applyViewModel(this.element.find('.mainContainer'));
            MessageManager.dispatchMessage(UIMessage.DocumentChanged, this);
            $.validator.unobtrusive.parse(this.element);
        }
    },
    _onBeforeSubmitImages: function (arr, $form, options) {
        $('#UploadWindow').dialog({ title: "Image Upload", modal: true, width: 250 });
        $('#Progressbar').progressbar();
    },
    _onDelete: function (data, event) {
        event.preventDefault();
        var menuItem = $(event.currentTarget).closest('.menuItem');
        var eventTypeID = menuItem.attr('data-itemID');

        if (confirm("Are you sure you wish to delete this event type? This action cannot be undone.")) {
            if (eventTypeID == this.viewModel.eventTypeID())
                this.reload = true;

            $.post(this.settings.deleteEventTypeUrl, { eventTypeID: eventTypeID }, $.proxy(this._onDeleteCompleted, this));
        }
    },
    _onDeleteCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Event type deleted successfully.");
                LeftMenuManager.loadMenu(this.viewModel);
                if (this.reload)
                    this._onNewEventType();
                this.reload = false;
            }
            else {
                alert("Error: ensure firstly that this event type is not connected to any other event");
            }
        }
    },
    _onDeleteImage: function (data, event) {
        var eventTypeID = $(event.currentTarget).attr('data-eventTypeID');

        if (confirm("Are you sure you wish to delete this image? This action cannot be undone.")) {
            $.post(this.settings.deleteEventTypeImageUrl, { eventTypeID: eventTypeID }, $.proxy(this._onDeleteImageCompleted, this));
        }
    },
    _onDeleteImageCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Image deleted successfully.");
                this._getEventTypeImage();
            }
        }
    },
    _getEventTypeImage: function () {
        $.post(this.settings.getEventTypeImageUrl, { eventTypeID: this.viewModel.eventTypeID() }, $.proxy(this._onEditImageCompleted, this));
    },
    _onEdit: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.eventTypeID(menuItem.attr('data-itemID'));
        $.post(this.settings.getEventTypeUrl, { eventTypeID: this.viewModel.eventTypeID() }, $.proxy(this._onEditCompleted, this));
    },
    _onEditCompleted: function (data) {
        if (data) {
            this._insertPartial(data);
        }
    },
    _onEditImage: function (data, event) {
        var menuItem = $(event.currentTarget).closest('.menuItem');
        this.viewModel.eventTypeID(menuItem.attr('data-itemID'));

        this._getEventTypeImage();
    },
    _onEditImageCompleted: function (data) {
        if (data) {
            this._insertPartial(data);
            this._initializeImageForm();
        }
    },
    _onNewEventType: function () {
        this.viewModel.eventTypeID(0);
        $.post(this.settings.getEventTypeUrl, { eventTypeID: 0 }, $.proxy(this._onEditCompleted, this));
    },
    _onProgress: function (event, position, total, percentComplete) {
        $("#Progressbar").progressbar("value", percentComplete);
    },
    _onSubmit: function () {
        form = $('#EventTypeForm');
        form.validate();
        if (form.valid()) {
            var data = form.serializeArray();
            var url = this.viewModel.isEdit() ? this.settings.updateEventTypeUrl : this.settings.addEventTypeUrl;

            $.post(url, data, $.proxy(this._onSubmitCompleted, this));
        }
    },
    _onSubmitCompleted: function (data) {
        if (data) {
            if (data.success) {
                alert("Event type successfully saved.");
                LeftMenuManager.loadMenu(this.viewModel);
                this.viewModel.eventTypeID(data.eventTypeID);
                $.post(this.settings.getEventTypeUrl, { eventTypeID: this.viewModel.eventTypeID() }, $.proxy(this._onEditCompleted, this));
            }
            else if (data.error)
                alert(data.error);
        }
    },
    _getEventType: function() {
         $.post(this.settings.getEventTypeUrl, { eventTypeID: this.viewModel.eventTypeID() }, $.proxy(this._onEditCompleted, this));
    },
    _onSubmitImagesCompleted: function (data) {
        if (data) {
            if (data.success)
                this._getEventTypeImage();
        }
        $('#UploadWindow').dialog("close");
        $("#Progressbar").progressbar("destroy");
        $('#UploadWindow').dialog("destroy");
    },
    initialize: function () {
        this._createSettings();
        this._createViewModel();
        this._applyViewModel();
    }
}

$(function () {
    var EventTypesManager = new HostPipe.UI.EventTypesManager();
    EventTypesManager.initialize();
})