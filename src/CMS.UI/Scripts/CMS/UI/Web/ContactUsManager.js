/// <reference path="../../../_references.js" />

function ContactUsManager() {
}

ContactUsManager.prototype = {
    _initializeCustomForms: function () {
        $(".customForm").ajaxForm({
            dataType: 'json',
            beforeSubmit: $.proxy(this._onBeforeSubmitForm, this),
            success: $.proxy(this._onSubmitFormCompleted, this)
        });
    },
    createViewModel: function () {
    },
    _onBeforeSubmitForm: function (arr, $form, options) {
    },
    _onSubmitFormCompleted: function (data) {
        if (data) {
            if (data.success) {
                if (data.redirect) {
                    var form = $('<form action="' + data.url + '" method="post"><input type="hidden" name="formid" value="' + data.formID + '" /></form>');
                    $('body').append(form);
                    form.submit();
                } else {
                    $('#form' + data.formID).hide();
                    $('#emailSent' + data.formID).show();
                }
            }
            if (data.error) {
                $('#validationSummary' + data.formID).text(data.error);
                window.Recaptcha.reload();
            }
        }
    },
    initialize: function () {
        this._initializeCustomForms();
    }
}

var manager = new ContactUsManager();
$(function () {
    manager.initialize();
})