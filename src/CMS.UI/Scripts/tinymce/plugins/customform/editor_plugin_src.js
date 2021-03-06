﻿tinymce.PluginManager.add('customform', function (editor, url) {

    function createFormList(callback) {
        return function () {
            var formList = editor.settings.customform_list;
            if (typeof (linkList) == "string") {
                tinymce.util.XHR.send({
                    url: formList,
                    success :function (text) {
                        callback(tinymce.util.JSON.parse(text));
                    }
                });
            } else {
                callback(formList);
            }
        };
    }

    function showDialog(formList) {
        var data = {}, selection = editor.selection, dom = editor.dom, initialText;
        var win, formListCtrl;

        function formListChangeHandler(e) {
            data.customformID = e.control.value();
        }

        function buildFormList() {
            var formListItems = [{ text: 'None', value: ''}];

            tinymce.each(formList, function (item) {
                formListItems.push({
                    text: item.text || item.title,
                    value: item.value || item.url,
                    menu: item.menu
                });
            });

            return formListItems;
        }

        if (formList) {
            formListCtrl = {
                type: 'listbox',
                label: 'Form list',
                values: buildFormList(),
                onselect: formListChangeHandler
            };
        }

        // Open window
        editor.windowManager.open({
            title: 'Insert form',
            width: 420 + editor.getLang('example.delta_width', 0),
            height: 150 + editor.getLang('example.delta_height', 0),
            file: url +'/customForm.html'
        }, {
        val: formList
        });
    }

    editor.addButton('insertform', {
        title: 'Insert form',
        image: '/Images/insert_form.png',
        onclick: createFormList(showDialog)
    });

});

