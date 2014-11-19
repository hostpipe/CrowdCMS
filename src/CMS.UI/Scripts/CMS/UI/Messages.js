/// <reference path="../../_references.js" />

// Message types ----------------------------------------------------------------------------------

var UIMessage = {
    HashStringChanged: "UIMessage.HashStringChanged",
    DocumentChanged: "UIMessage.DocumentChanged",
    DomainChanged: "UIMessage.DomainChanged",
    TinyMCEInitialize: "UIMessage.TinyMCEInitialize",
    TinyMCEInitialized: "UIMessage.TinyMCEInitialized",
    TinyMCEContentChanged: "UIMessage.TinyMCEContentChanged"
};

MessageManager.registerMessageType(
    UIMessage.HashStringChanged,
    UIMessage.DocumentChanged,
    UIMessage.DomainChanged,
    UIMessage.TinyMCEInitialize,
    UIMessage.TinyMCEInitialized,
    UIMessage.TinyMCEContentChanged
);

// NOTE: Assigning as above to enable 'strongly typed' message types understood by IntelliSense.

// HashChange -------------------------------------------------------------------------------------

$(window).bind('hashchange', function () {
    MessageManager.dispatchMessage(UIMessage.HashStringChanged, this, window.location.hash);
});