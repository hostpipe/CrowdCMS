/// <reference path="../_references.js" />

// ------------------------------------------------------------------------------------
// HostPipe.MessageManager
// ------------------------------------------------------------------------------------

HostPipe.MessageManager = function () { throw "Cannot instantiate an object of a static class"; };

HostPipe.MessageManager.messageQueue = [];

HostPipe.MessageManager.messageTypes = {};

HostPipe.MessageManager.registerMessageType = function () {
    if (arguments.length < 1)
        throw "Message type name is required.";

    var result = [];

    for (var n = 0; n < arguments.length; n++) {
        var messageTypeName = arguments[n];

        var messageType = this.getMessageType(messageTypeName);

        if (typeof (messageType) != 'undefined') {
            throw String.format("Message type '{0}' is already defined.", messageTypeName);
        }

        messageType = { name: messageTypeName, subscribers: [] };

        result.push(messageType);

        this.messageTypes[messageTypeName.toLowerCase()] = messageType;
    }

    return result.length > 1 ? result : result[0];
};

HostPipe.MessageManager.getMessageType = function (messageTypeName) {
    return this.messageTypes[messageTypeName.toLowerCase()];
};

HostPipe.MessageManager.subscribe = function (messageType, subscriberCallback) {
//    var e = Function.validateParameters(arguments, [
//            { name: "messageType" },
//            { name: "subscriberCallback", type: Function }
//            ]);

//    if (e) throw e;

    if (typeof (messageType.name) == 'undefined')
        messageType = this.getMessageType(messageType.toString());

    if (typeof (messageType) == 'undefined')
        throw String.format("Unknown message type '{0}'.", arguments[0]);

    if (_.indexOf(messageType.subscribers, subscriberCallback) < 0)
        messageType.subscribers.push(subscriberCallback);
};

HostPipe.MessageManager.unsubscribe = function (messageType, subscriberCallback) {
//    var e = Function.validateParameters(arguments, [
//            { name: "messageType" },
//            { name: "subscriberCallback", type: Function }
//            ]);

//    if (e) throw e;

    if (typeof (messageType.name) == 'undefined')
        messageType = this.getMessageType(messageType.toString());

    if (typeof (messageType) == 'undefined')
        throw String.format("Unknown message type '{0}'.", arguments[0]);

    messageType.subscribers = _.without(messageType.subscribers, subscriberCallback);
};

HostPipe.MessageManager.clearSubscribers = function (messageTypeList) {
    if (!messageTypeList || messageTypeList.length == 0)
        throw "You have to provide a list of message types, which subscribers you would like to remove";

    for (var i = 0; i < messageTypeList.length; i++) {
        this.messageTypes[messageTypeList[i].toLowerCase()].subscribers.length = 0;
    }
};

HostPipe.MessageManager.dispatchMessage = function (messageType, sender, messageData) {
    if (typeof (messageType.name) == 'undefined')
        messageType = this.getMessageType(messageType.toString());

    if (typeof (messageType) == 'undefined')
        throw String.format("Unknown message type '{0}'.", arguments[0]);

    var queueItem = { date: new Date(), type: messageType, sender: sender, data: messageData };

    this.messageQueue.push(queueItem);

    for (var n = 0; n < messageType.subscribers.length; n++) {
        messageType.subscribers[n](sender, messageData);
    }
}

var MessageManager = HostPipe.MessageManager;