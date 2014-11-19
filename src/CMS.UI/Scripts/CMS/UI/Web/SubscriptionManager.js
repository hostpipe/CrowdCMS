function SubscriptionManager() {
    this.element = $("#SubscribeForm");
}

SubscriptionManager.prototype = {
    _applyViewModel: function (element) {
        if (this.element && this.element.length > 0)
            ko.applyBindings(this.viewModel, this.element.get(0));
    },
    _createSettings: function () {
        this.settings = {
            saveSubscriptionUrl: this.element.attr('data-saveSubscription-url')
        };
    },
    _createViewModel: function () {
        this.viewModel = {
            emailSubscription: ko.observable(''),
            errorSubscription: ko.observable(false),
            successSubscription: ko.observable(false),

            saveSubscription: $.proxy(this._onSaveSubscription, this)
        };
    },
    _onSaveSubscription: function (data) {
        var form = this.element.find('form');
        form.validate();
        if (form.valid()) {
            $.post(this.settings.saveSubscriptionUrl, { email: this.viewModel.emailSubscription() }, $.proxy(this._onSaveSubscriptionCompleted, this));
        }
    },
    _onSaveSubscriptionCompleted: function (data) {
        if (data) {
            if (data.success) {
                this.viewModel.successSubscription(true);
            }
            else {
                this.viewModel.errorSubscription(true);
            }
        } else {
            this.viewModel.errorSubscription(true);
        }
    },
    initialize: function () {
        if (this.element && this.element.length > 0) {
            this._createViewModel();
            this._applyViewModel();
            this._createSettings();
        }
    }
}

$(function () {
    var manager = new SubscriptionManager();
    manager.initialize();
})