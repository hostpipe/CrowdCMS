/// <reference path="../../../_references.js" />

function CategoryManager() {
    this.categoryID = $('#Events').attr('data-categoryID');
}

CategoryManager.prototype = {
    _initilizeButtons: function () {
        $('[data-name="btnView"]').click($.proxy(this._onChangeView, this));
    },
    _initilizeCalendar: function () {
        $("#Events").find('#EventsCalendar').fullCalendar({
            header: {
                left: 'prevYear,prev,next,nextYear today',
                center: 'title',
                right: 'month,basicWeek'
            },
            weekMode: 'liquid',
            events: {
                url: '/Website/GetEventsData',
                type: 'POST',
                data: {
                    categoryID: this.categoryID
                },
                editable: false
            }
        });
    },
    _onChangeView: function (event) {
        event.preventDefault();
        var button = $(event.currentTarget);
        var url = button.attr("href");
        var viewType = button.attr("data-viewType");

        $.post(url, { type: viewType, categoryID: this.categoryID }, $.proxy(this._onChangeViewCompleted, this));
    },
    _onChangeViewCompleted: function (data) {
        if (data) {
            $("#Events").html(data);
            this._initilizeCalendar();
        }
    },
    initialize: function () {
        $('.categoryTile').mouseenter(function () {
            $(this).find('.catDesc').stop().animate({ height: '178px' }, { duration: 600, queue: false });
        });
        $('.categoryTile').mouseleave(function () {
            $(this).find('.catDesc').stop().animate({ height: '0px' }, { duration: 600, queue: false });
        });

        this._initilizeButtons();
        this._initilizeCalendar();
    }
}

$(function () {
    var categoryManager = new CategoryManager();
    categoryManager.initialize();
})