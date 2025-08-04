!function ($) {
    "use strict";
    var OptionCombobox = function (element, options) {
        this.options = $.extend({}, $.fn.option_combobox.defaults, options);
        this.$source = $(element);
        this.$container = this.setup();
        this.$menu = this.$container.find('ul.dropdown-menu');
        this.listen();
    };
    OptionCombobox.prototype = {
        constructor: OptionCombobox,
        setup: function () {
            var self = this;
            if (this.options.value != null)
                self.$source.append($('<option></option>').attr('value', '').text(this.options.value));
            $.each(this.options.items, function (ind, val) {
                self.$source.append($('<option></option>').attr('value', val.v).text(val.t));
            });
            return this.$source;
        },
        listen: function () {
        }
    };
    $.fn.option_combobox = function (option) {
        return this.each(function () {
            var $this = $(this)
              , data = $this.data('option_combobox')
              , options = typeof option == 'object' && option;
            if (!data) { $this.data('option_combobox', (data = new OptionCombobox(this, options))); }
            if (typeof option == 'string') { data[option](); }
        });
    };
    $.fn.option_combobox.defaults = {
        value: null,
        items: null
    };
}(window.jQuery);