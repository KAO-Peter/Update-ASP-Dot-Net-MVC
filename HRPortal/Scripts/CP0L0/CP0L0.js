window['HRPortal'] = {};
function showToastr(data) {
    if (data.result == true) {
        toastr.success(data.message, data.title);
    }
    else if (data.result == false) {
        BootstrapDialog.errorAlert(data.message, data.title);
    }
}
$(function () {
    BootstrapDialog.errorAlert = function (message, title) {
        return new BootstrapDialog({
            type: BootstrapDialog.TYPE_DANGER,
            title: title,
            message: message,
            buttons: [{
                label: '確定',
                action: function (dialog) {
                    dialog.close();
                }
            }]
        }).open();
    };
});
var newGuid = function () {
    var guid = "";
    for (var i = 1; i <= 32; i++) {
        var n = Math.floor(Math.random() * 16.0).toString(16);
        guid += n;
        if ((i == 8) || (i == 12) || (i == 16) || (i == 20))
            guid += "-";
    }
    return guid;
}
function isNull(obj) {
    return (obj == null);
}
function isNullOrEmpty(obj) {
    return (obj == null) || (obj == '');
}
function isNullOrUndefined(obj) {
    return (obj == null) || (obj == undefined);
}
function isNullOrEmptyOrUndefined(obj) {
    return (obj == null) || (obj == '') || (obj == undefined);
}
var childModel = function (data, model) {
    if (model != null)
        data = $.extend({}, model, data);
    ko.mapping.fromJS(data, {}, this);
}
function showCourseValue(value) {
    if (value == null || value == -1)
        return '未定';
    return value;
}
function showCourseTimeLocations(times, link_func) {
    if (times == null || times.length == 0)
        return '未定';
    var arr = [];
    times.sort(function (obj_A, obj_B) {
        if (obj_A.start_time >= obj_B.start_time)
            return 1;
        else
            return -1;
    });
    $.each(times, function (index, value) {
        var course_location_name = value.course_location_name, course_location_room_name = value.course_location_room_name;
        if (course_location_name == null)
            course_location_name = HRPortal.services.getCourseLocation(value.course_location_id).name;
        if (course_location_room_name == null)
            course_location_room_name = HRPortal.services.getCourseLocationRoom(value.course_location_room_id).name;
        arr.push(showCourseTimeLocation(value.start_time, value.end_time, course_location_name, course_location_room_name, value.course_location_room_id, link_func));
    });
    return arr.join('\n\r');
}
function showCourseTimeLocation(start_time, end_time, course_location_name, course_location_room_name, course_location_room_id, link_func) {
    var text = showCourseTime(start_time, end_time);
    if (link_func == null || course_location_room_id == null)
        text += ' (' + course_location_name + ' ' + course_location_room_name + ')';
    else
        text += ' (<a href="javascript: void(0);" onclick="' + link_func + '(\'' + course_location_room_id + '\');">' + course_location_name + ' ' + course_location_room_name + '</a>)'
    return text;
}
function showCourseTime(start_time, end_time) {
    if (start_time == null) return '未定';
    var text = moment(start_time).format('YYYY/MM/DD (dd) HH:mm');
    text += '~' + moment(end_time).format('HH:mm');
    return text;
}
function showCourseDate(value) {
    if (value == null) return '未定';
    return moment(value).format('YYYY/MM/DD');
}
function bytesToSize(value) {
    if (value == 0) return '0 Byte';
    var k = 1024;
    var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
    var i = Math.floor(Math.log(value) / Math.log(k));
    return (value / Math.pow(k, i)).toPrecision(3) + ' ' + sizes[i];
}
function openReportView(title, url, print_url) {
    $("#loadingWidget").show();
    var dialog = new BootstrapDialog({
        type: BootstrapDialog.TYPE_DEFAULT,
        size: BootstrapDialog.SIZE_WIDE,
        title: title,
        message: function (dialogRef) {
            $content = $('<iframe src="' + url + '" frameborder="0" scrolling="auto" style="padding: 0px; min-width:100%;width: auto !important;_width:100%;height:600px;overflow-y:hidden;"></iframe>');
            $content.load(function () {
                if ($('#ReportViewer', $(this).contents()).length == 0) {
                    setTimeout(function () {
                        $('#loadingWidget').hide();
                        dialogRef.close();
                    }, 500);
                } else {
                    setTimeout(function () {
                        dialogRef.getModalDialog().show();
                        $('#loadingWidget').hide();
                    }, 500);
                }
            });
            return $content;
        },
        buttons: [{
            label: '列印',
            action: function (dialogRef) {
                if ($('input[name="ReportViewer$ctl05$ctl06$ctl00$ctl00$ctl00"]', $('iframe', dialogRef.$modalBody).contents()).length == 1)
                    $('input[name="ReportViewer$ctl05$ctl06$ctl00$ctl00$ctl00"]', $('iframe', dialogRef.$modalBody).contents())[0].click();
                else
                    window.open(print_url, '_blank');
            }
        }, {
            label: '離開',
            action: function (dialogRef) {
                dialogRef.close();
            }
        }]
    });
    dialog.realize();
    dialog.getModalDialog().hide();
    dialog.open();
}
function findById(list, id) {
    for (var i = 0, len = list.length; i < len; i++) {
        if (list[i].id === id) {
            return list[i];
        }
    }
    return null;
}
$(function () {
    var $body = $(document.body)
      .on('hidden.bs.modal', '.modal', function () {
          if ($('.modal-backdrop.fade.in').length > 0) {
              $body.addClass('modal-open');
          }
      })
    $('input, textarea').placeholder();
})