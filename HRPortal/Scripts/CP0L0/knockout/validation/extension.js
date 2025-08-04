ko.validation.rules['extension'] = {
    validator: function (val, extensions) {
        extensions = typeof extensions === "string" ? extensions.replace(/,/g, "|") : "png|jpe?g|gif";
        return isNullOrEmptyOrUndefined(val) || val.toString().match(new RegExp(".(" + extensions + ")$", "i")) !== null;
    },
    message: 'Please enter a value with a valid extension.'
};
ko.validation.registerExtenders();