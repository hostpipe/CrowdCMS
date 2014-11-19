/// <reference path="../_references.js" />
var HostPipe = {};
HostPipe.UI = {};

HostPipe.Core = function () {
}

HostPipe.Core.prototype = {
    _bindEventCallBacks: function (obj, params) {
        this.addEventToggleSectionBtn(params ? params.element : null);
        this.initializeCounter();
        this.initializeDatePicker();
        this.initializeBorderSlider();
        if (typeof $.validator != 'undefined' && typeof $.validator.unobtrusive != 'undefined')
            $.validator.unobtrusive.parse(document);
    },
    _countTinyMCEWordsAndChars: function (obj, params) {
        this.countWordsAndChars(params.element);
    },
    addEventToggleSectionBtn: function (element) {
        if (element) {
            $(element).find(".btnToggleSection").click(function () {
                    $(this).next().slideToggle();
                return false;
            });
        }
        else
            $(".btnToggleSection").click(function () {
                $(this).next().slideToggle();
                return false;
            });
    },
    countWordsAndChars: function (element) {
        if (element && element.length > 0) {
            var name = element.attr('name');
            var minWords = parseInt(element.attr('data-minWords'));
            var maxWords = parseInt(element.attr('data-maxWords'));
            var minChars = parseInt(element.attr('data-minChars'));
            var maxChars = parseInt(element.attr('data-maxChars'));

            if (isNaN(minWords)) minWords = 0;
            if (isNaN(maxWords)) maxWords = 99999;
            if (isNaN(minChars)) minChars = 0;
            if (isNaN(maxChars)) maxChars = 999999;

            var text = '';
            try{
                text =  element.hasClass('tinyMCEEditor') ? element.text() : element.val();
            } catch (er) {
                text = element[0].textContent;
            }
            
            var value = text.replace(/\n/gi, " ");
            value = value.replace(/(&nbsp;)/gi, " ");
            value = value.replace(/[ ]{2,}/gi, " ");
            value = value.replace(/(^[\s,\n]*)|([\s,\n]*$)/gi, "");
            var words = value.length > 0 ? value.split(/[ ]/).length : 0;
            var chars = text.length;

            var isWarning = (minChars > chars || maxChars < chars) || (minWords > words || maxWords < words);

            var info = $('span[data-for="' + name + '"]');
            info.find('[data-name="icon"]').removeClass().addClass(isWarning ? "icon-warning" : "icon-info");
            info.find('[data-name="text"]').text(words + " words " + chars + " characters");
        }
    },
    initializeCounter: function () {
        var self = this;
        $('[data-count="true"]').keyup(function (event) {
            self.countWordsAndChars($(event.currentTarget));
        }).each(function (i) {
            self.countWordsAndChars($(this));
        });
    },
    initializeDatePicker: function () {
        if (typeof $().datepicker == 'function')
            $('[data-input-type="date"]').datepicker({
               dateFormat: "dd/mm/yy",
            });

        if (typeof $().datetimepicker == 'function')
            $('[data-input-type="dateTime"]').datetimepicker({
                dateFormat: "dd/mm/yy",
                timeFormat: "HH:mm"
           });
    },
    initializeBorderSlider: function () {
        if (typeof $().slider == 'function')
            $('[data-input-type="borderSlider"]').slider({
                min: 0,
                max: 50
            });
    },
    initialize: function () {
        this._bindEventCallBacks();

        if (typeof MessageManager != 'undefined') {
            MessageManager.subscribe(UIMessage.DocumentChanged, $.proxy(this._bindEventCallBacks, this));
            MessageManager.subscribe(UIMessage.TinyMCEContentChanged, $.proxy(this._countTinyMCEWordsAndChars, this));
            MessageManager.subscribe(UIMessage.TinyMCEInitialized, $.proxy(this._countTinyMCEWordsAndChars, this));
        }

        if (typeof _ != 'undefined' && typeof _.mixin == 'function')
            _.mixin(_.str.exports());
    },
    isBusy: function (value, container) {
        if (!container)
            container = 'body';

        if (value) {
            var busyIndicator = $('<div class="loading"></div>');
            $(container).append(busyIndicator);
        }
        else {
            $(container).find('.loading').remove();
        }
    }
}

var Core = new HostPipe.Core();

$(function () {
    Core.initialize();
})