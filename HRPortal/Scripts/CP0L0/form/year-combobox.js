!function ($) {
    "use strict";
    var YearCombobox = function (element, options) {
        this.options = $.extend({}, $.fn.year_combobox.defaults, options);
        this.$source = $(element);
        this.$container = this.setup();
        this.$menu = this.$container.find('ul.dropdown-menu');
        this.listen();
    };
    YearCombobox.prototype = {
        constructor: YearCombobox,
        setup: function () {
            var year_combobox = $('<div class="input-group"></div>');
            this.$source.before(year_combobox);
            this.$source.val(this.options.value);
            year_combobox.append(this.$source);
            year_combobox.append('<div class="input-group-btn"><button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown"><span class="caret"></span></button><ul class="dropdown-menu pull-right"></ul></div>');
            for (var i in this.options.items) {
                $('ul.dropdown-menu', year_combobox).append('<li><a href="#">' + this.options.items[i] + '</a></li>');
            }
            this.$source.formatter({
                'pattern': '{{999}}'
            }).focusout(function () {
                $(this).trigger("change");
            });
            return year_combobox;
        },
        listen: function () {
            var self = this;
            $('a', this.$menu).click(function () {
                self.$source.val($(this).text()).trigger('change');
            });
            this.$source.keypress(function (event) {
                if (event.which == 13) {
                    self.$source.blur().trigger('change');
                }
            });
        }
    };
    $.fn.year_combobox = function (option) {
        return this.each(function () {
            var $this = $(this)
              , data = $this.data('year_combobox')
              , options = typeof option == 'object' && option;
            if (!data) { $this.data('year_combobox', (data = new YearCombobox(this, options))); }
            if (typeof option == 'string') { data[option](); }
        });
    };
    $.fn.year_combobox.defaults = {
        value: new Date().getFullYear() - 1911,
        items: [new Date().getFullYear() - 1911, new Date().getFullYear() - 1911 - 1, new Date().getFullYear() - 1911 - 2]
    };
}(window.jQuery);