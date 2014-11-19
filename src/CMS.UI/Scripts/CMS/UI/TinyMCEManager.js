/// <reference path="../../_references.js" />

HostPipe.TinyMCEManager = function () {
}

HostPipe.TinyMCEManager.prototype = {
    _initializeTinyMCE: function (obj, params) {
        this.cssFile = params.cssFile;
        $.post(params.url, { currentDomainID: params.domainID, selector: params.elementSelector }, $.proxy(this._initializeTinyMCEReady, this));
    },
    _initializeTinyMCEReady: function (data) {
        this.urls = data.link_list;
        this.customforms = data.customForms;
        $(data.selector).tinymce({
            script_url: '/Scripts/tinymce/tiny_mce.js',
            customform_list: this.customforms,
            auto_focus: false,
            theme: "advanced",
            height: "500px",
            width: "98%",
            plugins:
                "advlist,autolink,lists,print,preview,advhr,pagebreak,searchreplace,wordcount,visualblocks,visualchars,fullscreen,insertdatetime,media,nonbreaking,save,table,contextmenu,directionality,template,paste,imagemanager,filemanager,customform"
            ,
            theme_advanced_buttons2_add_before: "blockquote",
            theme_advanced_buttons3_add: "fullscreen,pastetext,pasteword,selectall,tablecontrols,media,insertfile,insertimage,insertform",
            theme_advanced_blockformats: "p,div,h1,h2,h3,h4,h5,h6",
            theme_advanced_toolbar_location: "top",
            theme_advanced_toolbar_align: "left",
            extended_valid_elements: "span[*],div[*],section[*]",
            image_advtab: true,
            remove_script_host: true,
            paste_auto_cleanup_on_paste: true,
            paste_convert_middot_lists: false,
            paste_unindented_list_class: "unindentedList",
            relative_urls: false,
            content_css: this.cssFile,
            external_link_list_url: "/Website/WebsiteUrls/" + new Date().getTime(),
            setup: $.proxy(function (editor) {
                editor.onChange.add($.proxy(function (e) {
                    MessageManager.dispatchMessage(UIMessage.TinyMCEContentChanged, this, { element: $(this.elementSelector) });
                }, this)),
                editor.onInit.add($.proxy(function (e) {
                    MessageManager.dispatchMessage(UIMessage.TinyMCEInitialized, this, { element: $(this.elementSelector) });
                }, this))
            }, this)
        });
    },
    initialize: function () {
        MessageManager.subscribe(UIMessage.TinyMCEInitialize, $.proxy(this._initializeTinyMCE, this));
    }
}

var TinyMCEManager = new HostPipe.TinyMCEManager();
TinyMCEManager.initialize();