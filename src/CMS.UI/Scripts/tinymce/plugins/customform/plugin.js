/**
* Plugin that adds a toolbar button with form loader (popup)
*/
tinymce.PluginManager.add('customform', function (editor, url) {

    function createFormList(callback) {
        return function () {
            var formList = editor.settings.customform_list;
            if (typeof (linkList) == "string") {
                tinymce.util.XHR.send({
                    url: formList,
                    success: function (text) {
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
            height: 120 + editor.getLang('example.delta_height', 0),
            body:
                [
					{ type: 'textbox', name: 'recipientEmail', label: 'Recipient email',
					    onchange: function () {
					        data.text = this.value();
					    }
					},
                    formListCtrl
				],
            onsubmit: function (e) {
                // Insert content when the window form is submitted
                var customFormTag = '[%customform data-formID="' + data.customformID + '" data-recipient="' + data.text + '" /%]';
                editor.insertContent(customFormTag);

            }
        });
    }

    // Add a button that opens a window
    editor.addButton('insertform', {
        title: 'Insert form',
        image: '/Images/insert_form.png',
        onclick: createFormList(showDialog)
    });

    this.showDialog = showDialog;

    // Adds a menu item to the tools menu
    editor.addMenuItem('insertform', {
        text: 'Insert form',
        context: 'insert',
        onclick: createFormList(showDialog)
    });
});