/// <reference path="../../../_references.js" />

HostPipe.UI.LeftMenuManager = function () {
    this.usedCookieName = "ExpandedMenuItems";
}

HostPipe.UI.LeftMenuManager.prototype = {
    _addToExpandedList: function (id) {
        var list = new Array();
        var expandedItems = $.cookie(this.usedCookieName);
        if (expandedItems)
            list = expandedItems.split(',');

        if (list.indexOf(id) == -1) {
            list.push(id);
            $.cookie(this.usedCookieName, list.join());
        }
    },
    _expand: function (itemID, leftMenu) {
        var item = leftMenu.find('.menuItem[data-itemID="' + itemID + '"]');
        var subItemsList = item.next('ul');
        subItemsList.removeClass("hidden");

        this._refreshColor();
    },
    _expandList: function (itemIDs) {
        var leftMenu = $('.leftMenu');
        $(itemIDs).each($.proxy(function (id) {
            this._expand(itemIDs[id], leftMenu);
        }, this));
    },
    _loadMenuCompleted: function (data) {
        if (data) {
            $('.leftMenu').remove();
            $("#loader").hide();
            $('.leftMenuContainer').append(data).hide().show();
            this.initialize();
            if (this.viewModel)
                ko.applyBindings(this.viewModel, $('.leftMenu').get(0));
        }
    },
    _onExpand: function (event) {
        event.preventDefault();

        var menuItem = $(event.currentTarget).closest('.menuItem');
        var subMenu = menuItem.next('ul');
        subMenu.toggleClass('hidden');

        this._refreshColor();

        if (subMenu.hasClass('hidden'))
            this._removeFromExpandedList(menuItem.attr('data-itemID'));
        else
            this._addToExpandedList(menuItem.attr('data-itemID'));
    },
    _onOrderChanged: function () {
        $('#orderIndicator').removeClass('icon-ok').addClass('icon-exclamation');
        this._refreshColor();
    },
    _refreshColor: function () {
        $('.leftMenu .menuItem').removeClass('even');
        $('.leftMenu ul:not(:hidden) > li > .menuItem:even').addClass('even');
    },
    _removeFromExpandedList: function (id) {
        var expandedItems = $.cookie(this.usedCookieName);
        if (expandedItems) {
            var list = expandedItems.split(',');
            if (list.indexOf(id) != -1) {
                list.splice(list.indexOf(id), 1);
                $.cookie(this.usedCookieName, list.join());
            }
        }
    },
    initialize: function () {
        $('.leftMenu [data-name="btnExpand"]').click($.proxy(this._onExpand, this));
        $(".leftMenu ul").sortable({
            axis: "y",
            handle: '[data-name="btnMove"]',
            update: $.proxy(this._onOrderChanged, this)
        });

        this._refreshColor();

        var expandedItems = $.cookie(this.usedCookieName);
        if (expandedItems)
            this._expandList(expandedItems.split(','));
    },
    loadMenu: function (viewModel, customUrl, customParams) {
        this.viewModel = viewModel;
        var url = customUrl ? customUrl : "/Admn/LeftMenu";

        var params = {};
        params.url = window.location.pathname;

        $.extend(params, customParams);
        $('.leftMenu').remove();
        $("#loader").show();
        $.post(url, params, $.proxy(this._loadMenuCompleted, this));
    }
}

var LeftMenuManager = new HostPipe.UI.LeftMenuManager();

$(function () {
    LeftMenuManager.initialize();
})