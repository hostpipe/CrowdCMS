/// <reference path="../../../_references.js" />

function EventManager() {
    this.element = $('#EventsSummary');
}

EventManager.prototype = {
    _initializeSettings: function () {
        this.settings = {
            getEventsUrl: this.element.attr('data-event-url')
        }
    },

    _getDates: function (dataDate, context) {
        var d = new Date(dataDate);
        var firstDay = new Date(d.getFullYear(), d.getMonth(), 1, 0,0,0,0);
        var lastDay = new Date(d.getFullYear(), d.getMonth() + 1, 0, 23, 59, 59, 0);
        $.ajax(context.settings.getEventsUrl, {
            cache: false,
            type: "POST",
            data: { date: new Date(), start: firstDay.getTime() / 1000, end: lastDay.getTime() / 1000 },
            success: function (data) {
                $('#EventsSummary').replaceWith(data);

            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(errorThrown);
            }
        });
    },
    _getDatesCompleted: function (data) {
        if (data) {
            this.element.replaceWith(data);
        }
    },

    _bindDateTags: function () {
        var main = this;
        $('.monthSelector').click(function () {

            main._getDates($(this).attr('data-event-month'), main)
            $('.monthSelector').removeClass('active');
            $(this).addClass('active');

        });
    },

    initialize: function () {
        this._initializeSettings();
        this._bindDateTags();
    }
}

$(function () {
    var manager = new EventManager();
    manager.initialize();
})