jQuery.validator.addMethod("requireif", function (value, element, param) {
    var $element = $(element);
    var dependentprop = $element.attr('data-val-requireif-dependatnproperty');
    var $depenedentprop = $('#' + dependentprop);
    var targetval = $element.attr('data-val-requireif-targetvalue');
    var dependantpropValue;
    if ($depenedentprop.attr('type') in { checkbox: "checkbox", radio: "radio" }) {
        dependantpropValue = $depenedentprop.prop('checked').toString();
        targetval = targetval.toLowerCase();
    }
    else
        dependantpropValue = $depenedentprop.val();

    if (dependantpropValue == targetval) {
        if (!value || value == "" || (/^\s$/g).test(value))
            return false;
        return true;
    }
    return true;
});
jQuery.validator.unobtrusive.adapters.addBool("requireif");

jQuery.validator.addMethod("percentageof", function (value, element, params) {
    var propertyname = params['propertyname'];
    var minimumpercentage = params['minimumpercentage'];
    var propValue = $('#' + propertyname).val();
    var minValue = (propValue / 100) * minimumpercentage;
    return value >= minValue;
});
jQuery.validator.unobtrusive.adapters.add("percentageof", ["propertyname", "minimumpercentage"], function (options) {
    options.rules["percentageof"] = options.params;
    if (options.message) {
        options.messages['percentageof'] = options.message;
    }
});