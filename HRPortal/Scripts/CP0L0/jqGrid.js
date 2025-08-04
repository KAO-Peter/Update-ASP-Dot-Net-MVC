var jqGrid = function (grid_selector, options) {
    var self = this;
    var defaultsJqgridOptions = {
        datatype: 'json',
        jsonReader: {
            repeatitems: false
        },
        sortorder: 'asc',
        height: 'auto',
        gridview: true,
        altRows: true,
        viewrecords: true,
        multiselect: true,
        gridComplete: gridCompleteHandler,
        loadComplete: loadCompleteHandler,
        selectRow: function () { },
        onSelectRow: onSelectRowHandler,
        onSelectAll: onSelectAllHandler,
        selRows: [],
        selRowsDate: {},
        mtype: 'post',
        rowNum: 10,
        rowList: [10, 20, 50, 100],
        selectionChange: null
    };
    self.reloadGrid = function (data) {
        if (data != null && data.length > 0) {
            if (data[0].current == true)
                self.resetSelection();
        }
        if (self.observableArray() != null) {
            var list = self.observableArray();
            self.setData(ko.mapping.toJS(list()));
        }
        self.jqGrid.trigger('reloadGrid', data);
        return self;
    };
    self.resetSelection = function () {
        $(grid_selector).jqGrid('resetSelection');
        $(grid_selector).jqGrid('getGridParam', 'selRows').length = 0;
        self.setGridParam({ 'selRowsDate': {} })
        var selectionChange = $(grid_selector).jqGrid('getGridParam', 'selectionChange');
        if (selectionChange != null)
            selectionChange($(grid_selector).jqGrid('getGridParam', 'selRows'), $(grid_selector).jqGrid('getGridParam', 'selRowsDate'));
        return self;
    };
    self.selRows = function () {
        return $(grid_selector).jqGrid('getGridParam', 'selRows');
    };
    self.selRowsDate = function () {
        var data = [];
        $.each($(grid_selector).jqGrid('getGridParam', 'selRowsDate'), function (ind, val) {
            data.push(val);
        });
        return data;
    };
    self.resizeJqGridWidth = function (selector) {
        resizeJqGridWidth(grid_selector, selector);
        return self;
    };
    self.setGridWidth = function (new_width, shrink) {
        $(grid_selector).jqGrid('setGridWidth', new_width, shrink);
        return self;
    };
    self.clearGridData = function () {
        self.resetSelection();
        $(grid_selector).jqGrid('clearGridData', true);
        return self;
    };
    self.getGridParam = function (param) {
        return $(grid_selector).jqGrid('getGridParam', param);
    };
    self.observableArray = function () {
        return $(grid_selector).jqGrid('getGridParam', 'observableArray');
    };
    self.getRowData = function (value) {
        var isArray = $.isArray(value);
        if (self.observableArray() == null) {
            if (isArray) {
                var data = [];
                $.each(value, function (ind, val) {
                    data.push(self.getRowData(val));
                });
                return data;
            } else {
                return $(grid_selector).jqGrid('getRowData', value);
            }
        }
        var data = ko.utils.arrayFilter(self.observableArray()(), function (item) {
            if (isArray)
                return value.indexOf(item.id()) >= 0;
            else
                return value === item.id();
        });
        if (isArray)
            return ko.mapping.toJS(data);
        else
            return ko.mapping.toJS(data)[0];
    };
    self.setRowData = function (id, data) {
        if (self.getRowData(id) == null) {
            self.addRowData(data);
        }
        else {
            if (self.observableArray() == null) {
                $(grid_selector).jqGrid('setRowData', id, data);
            }
            else {
                var list = self.observableArray();
                list.remove(function (item) { return id === item.id() });
                list.push(new childModel(data));
            }
        }
        return self;
    };
    self.addRowData = function (data) {
        if (data.id.length != 36 || data.id == '00000000-0000-0000-0000-000000000000')
            data.id = newGuid();
        if (self.observableArray() == null) {
            $(grid_selector).jqGrid('addRowData', data.id, data);
        }
        else {
            self.observableArray().push(new childModel(data));
        }
        return self;
    };
    self.delRowData = function (value) {
        if (self.observableArray() == null) {
            if (value instanceof Array) {
                $.each(value, function (ind, val) {
                    $(grid_selector).jqGrid('delRowData', val);
                });
            } else {
                $(grid_selector).jqGrid('delRowData', value);
            }
        }
        else {
            if (value instanceof Array) {
                self.observableArray().remove(function (item) { return value.indexOf(item.id()) >= 0 });
            } else {
                self.observableArray().remove(function (item) { return value == item.id() });
            }
        }
        return self;
    };
    self.setData = function (value, sel_ids) {
        self.clearGridData();
        if (self.observableArray() == null) {
            $.each(value, function (ind, val) {
                self.addRowData(val);
            });
        } else {
            self.setGridParam({ 'data': value });
        }
        if (sel_ids != null) {
            self.setGridParam({ 'selRows': sel_ids });
            self.reloadGrid();
        }
        return self;
    };
    self.getData = function () {
        var data = self.getGridParam('getRowData');
        return data;
    };
    self.setGridParam = function (value) {
        $(grid_selector).jqGrid('setGridParam', value);
    };
    self.jqGrid = $(grid_selector).jqGrid($.extend({}, defaultsJqgridOptions, options));
};

var gridCompleteHandler = function () {
    var table = this;
    setTimeout(function () {
        updatePagerIcons(table);
    }, 0);
};
var loadCompleteHandler = function (data) {
    var table = this;
    var selRows = $(table).jqGrid('getGridParam', 'selRows');
    if (selRows.length > 0) {
        var tmp = $(table).jqGrid('getDataIDs');
        $.each(selRows, function (index, value) {
            var pos = $.inArray(value, tmp);
            if (pos > -1) {
                $(table).jqGrid('setSelection', value);
            }
        });
    }
};
var onSelectRowHandler = function (id, status) {
    selectionManager(this, id, status);
};
var onSelectAllHandler = function (idArr, status) {
    selectionManager(this, idArr, status);
};
var selectionManager = function (table, id, status) {
    var selRows = $(table).jqGrid('getGridParam', 'selRows');
    if (status) {
        if (!$.isArray(id)) {
            if ($.inArray(id, selRows) < 0) { selRows.push(id) }
        } else {
            var tmp = $.grep(id, function (item, ind) {
                return $.inArray(item, selRows) < 0;
            });
            $.merge(selRows, tmp);
        }
    } else {
        if (!$.isArray(id)) {
            selRows.splice($.inArray(id, selRows), 1);
        } else {
            selRows = $.grep(selRows, function (item, ind) {
                return $.inArray(item, id) > -1;
            }, true);
            $(table).jqGrid('getGridParam', 'selRows').length = 0;
            $.each(selRows, function (index, value) {
                $(table).jqGrid('getGridParam', 'selRows').push(value);
            });
        }
    }
    var selRows_date = $(table).jqGrid('getGridParam', 'selRowsDate');
    $.each(selRows_date, function (key, value) {
        if (selRows.indexOf(key) == -1)
            delete selRows_date[key];
    });
    $.each(selRows, function (index, value) {
        if (selRows_date[value] == null)
            selRows_date[value] = $(table).jqGrid('getRowData', value);
    });
    var selectionChange = $(table).jqGrid('getGridParam', 'selectionChange');
    if (selectionChange != null)
        selectionChange(selRows, selRows_date);
};
var updatePagerIcons = function (table) {
    var replacement = {
        'ui-icon-seek-first': 'text-center fa fa-angle-double-left bigger-140',
        'ui-icon-seek-prev': 'text-center fa fa-angle-left bigger-140',
        'ui-icon-seek-next': 'text-center fa fa-angle-right bigger-140',
        'ui-icon-seek-end': 'text-center fa fa-angle-double-right bigger-140'
    };
    $('.ui-pg-table:not(.navtable) > tbody > tr > .ui-pg-button > .ui-icon').each(function () {
        var icon = $(this);
        var $class = $.trim(icon.attr('class').replace('ui-icon', ''));

        if ($class in replacement) icon.attr('class', 'ui-icon ' + replacement[$class]);
    })
};
function resizeJqGridWidth(table, div_id) {
    $(window).on('resize.jqGrid', function () {
        $(table).jqGrid('setGridWidth', Math.round($(div_id).width(), true), true);
    });
    $(window).triggerHandler('resize.jqGrid');
}
function serializeGridData(postData, rules) {
    if (rules.length > 0) {
        postData.filters = {
            'groupOp': 'AND',
            'rules': rules
        };
    }
    else {
        postData.filters = null;
    }
    return postData;
}
$.extend($.fn.fmatter, {
    textFmatter: function (cellvalue, opts, rowdata) {
        var op = { func: opts.func};
        if (opts.colModel !== undefined && opts.colModel.formatoptions !== undefined) {
            op = $.extend({}, op, opts.colModel.formatoptions);
        }
        return op.func(cellvalue, opts, rowdata);
    },
    labelFmatter: function (cellvalue, opts, rowdata) {
        var op = { true_label: opts.true_label || '停用', true_class: opts.true_class || 'danger', false_label: opts.false_label || '正常', false_class: opts.false_class || 'success' };
        if (opts.colModel !== undefined && opts.colModel.formatoptions !== undefined) {
            op = $.extend({}, op, opts.colModel.formatoptions);
        }
        if (cellvalue === undefined || cellvalue === null || cellvalue === 0 || cellvalue == false) {
            return '<span class="label label-' + op.false_class + '">' + op.false_label + '</span>';
        }
        return '<span class="label label-' + op.true_class + '">' + op.true_label + '</span>';
    },
    buttonFmatter: function (cellvalue, opts, rowdata) {
        var op = { url: opts.url || '', func: opts.func, css: opts.css || 'btn btn-default', label: opts.label || cellvalue, target: opts.target }, href = '', target = '';
        if (opts.colModel !== undefined && opts.colModel.formatoptions !== undefined) {
            op = $.extend({}, op, opts.colModel.formatoptions);
        }
        var value = cellvalue;
        if (op.id_name)
            value = rowdata[op.id_name];
        if (value == null)
            return '未定';
        if (op.target) { target = 'target=' + op.target; };
        if (op.func) {
            href = 'href="javascript: void(0);" onclick="' + op.func + '(\'' + value + '\');"';
        }
        else {
            href = 'href="' + op.url.replace('{0}', value) + '"';
        }
        return '<a ' + target + ' ' + href + ' class="' + op.css + '">' + op.label + '</a>';
    },
    telFmatter: function (cellvalue, options, rowdata) {
        if (isNullOrEmptyOrUndefined(rowdata.tel))
            return '';
        else
            return ((isNullOrEmptyOrUndefined(rowdata.tel_area_code)) ? '' : '(' + rowdata.tel_area_code + ')') + rowdata.tel + ((isNullOrEmptyOrUndefined(rowdata.tel_ext) ? '' : '#' + rowdata.tel_ext));
    },
    course_session_times: function (cellvalue, options, rowdata) {
        return showCourseTimeLocations(cellvalue);
    },
    course_session_time: function (cellvalue, options, rowdata) {
        return showCourseTime(rowdata.start_time, rowdata.end_time);
    },
    bytesToSize: function (cellvalue, options, rowdata) {
        return bytesToSize(cellvalue);
    },
    showCourseDate: function (cellvalue, options, rowdata) {
        return showCourseDate(cellvalue);
    },
    showCourseValue: function (cellvalue, options, rowdata) {
        return showCourseValue(cellvalue);
    }
});