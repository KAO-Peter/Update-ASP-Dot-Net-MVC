ko.bindingHandlers.date = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = valueAccessor();
        var allBindings = allBindingsAccessor();
        var valueUnwrapped = ko.utils.unwrapObservable(value);

        // Date formats: http://momentjs.com/docs/#/displaying/format/
        var pattern = allBindings.format || 'YYYY/MM/DD';

        var output = '';
        if (valueUnwrapped !== null && valueUnwrapped !== undefined && valueUnwrapped.length > 0) {
            output = moment(valueUnwrapped).format(pattern);
        }

        if ($(element).is("input") === true) {
            var parent = $(element).parent().closest(".input-group.date");
            if (parent.length > 0) {
                if (output == '')
                    $(parent).datepicker('update');
                else
                    $(parent).datepicker('update', output);
            }
            else {
                $(element).val(output).trigger('change');
            }
        } else {
            $(element).text(output);
        }
    }
};
/*custom bindingHandler for error message*/
ko.bindingHandlers.validationCore = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var o = valueAccessor();
        // if requested, add binding to decorate element
        if (!!(o && o.rules && o.isValid && o.isModified)) {
            //ko.applyBindingsToNode(element, { validationElement: observable });
            // insert the message
            var span = document.createElement('SPAN');                      //element to hold error message
            span.className = 'help-block';                           //error message style
            var parent = $(element).parent().closest(".input-group");       //find the holder div of the input
            if (parent.length > 0) {
                $(parent).after(span);                                      //has holder: add message holder just after the input holder       
            } else {
                $(element).after(span);                                     //no holderL add message holder just after the input itself
            }
            ko.applyBindingsToNode(span, { validationMessage: valueAccessor() });
        }

    }
};
ko.validation.init({
    errorElementClass: 'has-error',
    errorMessageClass: 'help-block'
});
ko.unapplyBindings = function ($node, remove) {
    // unbind events
    //$node.find("*").each(function () {
    //    $(this).unbind();
    //});

    // Remove KO subscriptions and references
    if (remove) {
        ko.removeNode($node[0]);
    } else {
        ko.cleanNode($node[0]);
    }
};