/// <reference path="../../../_references.js" />

function BlogManager() {
}

BlogManager.prototype = {
    _initializeCommentForm: function () {
        $("#CommentForm").ajaxForm({
            dataType: 'json',
            success: $.proxy(this._onSubmitCommentCompleted, this)
        });
    },
    _onSubmitCommentCompleted: function (data) {
        if (data) {
            if (data.success) {
                $('#CommentForm').hide();
                $('#CommentSaved').show();
            }

            if (data.error) {
                $('#validationSummary').text(data.error);
                window.Recaptcha.reload();
            }
        }
    },
    initialize: function () {
        this._initializeCommentForm();
    },
    scrollToComment: function () {
        $.scrollTo($('#CommentForm'), 600);
    }
}

var manager = new BlogManager();
$(function () {
    manager.initialize();
})