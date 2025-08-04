(function (factory) {
    window['HRPortal']['formService'] = factory(jQuery);
})(function ($) {
    var methods = {
        showEmployee: showEmployee,
        doAction: doAction,
        getActions: getActions,
        getActionName: getActionName,
        getActionColor: getActionColor,
        getActionIcon: getActionIcon,
        getActionEmployee: getActionEmployee,
        getRegisterRecord: getRegisterRecord,
        applyChangeSession: applyChangeSession,
        applyLeave: applyLeave,
        getFormName: getFormName,
        getFlowStateName: getFlowStateName,
        options: {
            return_reason: ''
        }
    };
    return methods;
    function getActionEmployee(action, name, job_level_name) {
        var actionName = getActionName(action);
        if (actionName == '')
            return showEmployee(name, job_level_name);
        var actionColor = getActionColor(action);
        var actionIcon = getActionIcon(action);
        if (actionIcon == '') {
            return '<span class="label label-' + actionColor + '">' + actionName + '</span>\n' + methods.showEmployee(cellvalue, rowObject.action_job_level_name);
        }
        return '<span class="label label-' + actionColor + '"><i class="' + actionIcon + '"></i> ' + actionName + '</span>\n' + methods.showEmployee(name, job_level_name);
    }
    function getActions(selRows, selRowsDate) {
        if (selRows.length > 0) {
            var actions;
            $.each(selRowsDate, function (key, value) {
                if (actions == null) {
                    actions = value.actions.split(',');
                }
                else {
                    var new_actions = [];
                    var value_actions = value.actions.split(',');
                    for (var i = 0; i < actions.length; i++) {
                        if (value_actions.indexOf(actions[i]) > -1) {
                            new_actions.push(actions[i]);
                        }
                    }
                    actions = new_actions;
                }
            });
            return actions;
        }
        else {
            return [];
        }
    }
    function getFlowStateName(action) {
        switch (action) {
            case -1:
                return '已取消';
            case 0:
                return '申請中';
            case 1:
                return '主管審核中';
            case 2:
                return '人力發展中心確認中';
            case 3:
                return '已完成';
        }
        return '';
    }
    function getFormName(action) {
        switch (action) {
            case 0:
            case 1:
                return '報名申請';
            case 2:
                return '轉梯次申請';
            case 3:
            case 4:
                return '請假申請';
        }
        return '';
    }
    function getActionName(action) {
        switch (action) {
            case 0:
                return '建立';
            case 1:
                return '修改';
            case 2:
                return '刪除';
            case 3:
            case 4:
                return '送簽';
            case 11:
                return '送件';
            case 5:
            case 7:
                return '同意';
            case 6:
            case 8:
                return '不同意';
            case 9:
                return '退回';
            case 10:
                return '退回申請人';
        }
        return '';
    }
    function getActionColor(action) {
        switch (action) {
            case 0:
                return 'info';
            case 2:
            case 6:
            case 8:
                return 'warning';
            case 9:
            case 10:
                return 'danger';
            case 3:
            case 4:
            case 5:
            case 7:
            case 11:
                return 'success';
        }
        return '';
    }
    function getActionIcon(action) {
        switch (action) {
            case 2:
                return 'fa fa-trash-o';
            case 3:
            case 4:
            case 11:
                return 'fa fa-share';
            case 5:
            case 7:
                return 'fa fa-check';
            case 6:
            case 8:
                return 'fa fa-times';
            case 9:
                return 'fa fa-reply';
            case 10:
                return 'fa fa-reply-all';
        }
        return '';
    }
    function showEmployee(name, job_level_name) {
        if (job_level_name != '' && job_level_name != null)
            return job_level_name + ' ' + name;
        else
            return name;
    }
    function doAction(ids, role, action, callback) {
        var _openDialog = false;
        var _title = '';
        var _type = BootstrapDialog.TYPE_DANGER;
        var _button_cssClass = '';
        var _text = '';
        var _html = '';
        var _button = {
            id: 'but_ok',
            label: '確定',
            cssClass: 'btn-danger',
            action: function (dialogRef) {
                $.ajax({
                    url: methods.options.url + '/DoAction',
                    type: 'post',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        ids: ids,
                        role: role,
                        form_action: action,
                        description: $('textarea', dialogRef.getModalBody()).val()
                    }),
                    success: function (response) {
                        callback(response);
                        dialogRef.close();
                    }
                });
            }
        };
        switch (action) {
            //case '2':
            //    _openDialog = true;
            //    _title = '刪除';
            //    _html = '<form><div class="form-group"><textarea name="description" class="form-control" placeholder="刪除原因" required></textarea></div></form>';
            //    break;
            case '6':
            case '8':
                _openDialog = true;
                _title = '不同意';
                _html = '<form><div class="form-group"><textarea name="description" class="form-control" placeholder="不同意原因" required></textarea></div></form>';
                _type = BootstrapDialog.TYPE_WARNING;
                _button['cssClass'] = 'btn-warning';
                break;
            case '9':
                _openDialog = true;
                _title = '退回';
                _html = '<form><div class="form-group"><textarea name="description" class="form-control" placeholder="退回原因" required>' + methods.options.return_reason + '</textarea></div></form>';
                break;
            case '10':
                _openDialog = true;
                _title = '退回申請人';
                _html = '<form><div class="form-group"><textarea name="description" class="form-control" placeholder="退回申請人原因" required></textarea></div></form>';
                break;
        }
        if (_openDialog) {
            var bootstrapDialog = new BootstrapDialog({
                type: _type,
                title: _title,
                message: $(_html),
                buttons: [{
                    label: '取消',
                    action: function (dialogRef) {
                        dialogRef.close();
                    }
                }, _button],
                onshown: function (dialogRef) {
                    var $footerButton = dialogRef.getButton('but_ok');
                    var form = $("form", dialogRef.getModalBody());
                    var checkForm = function () {
                        if (form.valid())
                            $footerButton.enable();
                        else
                            $footerButton.disable();
                    };
                    $(':text, [type="password"], [type="file"], select, textarea, ' +
                        '[type="number"], [type="search"] ,[type="tel"], [type="url"], ' +
                        '[type="email"], [type="datetime"], [type="date"], [type="month"], ' +
                        '[type="week"], [type="time"], [type="datetime-local"], ' +
                        '[type="range"], [type="color"], [type="radio"], [type="checkbox"]', form).bind('focusin focusout keyup', function () {
                            checkForm();
                        });
                    $('select, option, [type="radio"], [type="checkbox"]', form).bind('click', function () {
                        checkForm();
                    });
                    checkForm();
                },
            });
            bootstrapDialog.open();
        }
        else {
            $.ajax({
                url: methods.options.url + '/DoAction',
                type: 'post',
                contentType: 'application/json',
                data: JSON.stringify({
                    ids: ids,
                    role: role,
                    form_action: action,
                    description: ''
                }),
                success: function (response) {
                    callback(response);
                }
            });
        }
    }
    function getRegisterRecord(id, callback) {
        $.ajax({
            url: methods.options.url + '/RegisterRecord/' + id,
            contentType: 'application/json',
            success: function (response) {
                callback(response);
            }
        });
    }
    function applyLeave(id, description, callback) {
        var result;
        $.ajax({
            url: methods.options.url + '/ApplyLeave',
            type: 'post',
            async: callback != null,
            data: JSON.stringify({ id: id, description: description }),
            contentType: 'application/json',
            success: function (response) {
                if (response != null) {
                    result = response;
                    if (callback)
                        callback(result);
                }
            }
        });
        if (callback == null)
            return result;
    }
    function applyChangeSession(id, session_id, description, callback) {
        var result;
        $.ajax({
            url: methods.options.url + '/ApplyChangeSession',
            type: 'post',
            async: callback != null,
            data: JSON.stringify({ id: id, session_id: session_id, description: description }),
            contentType: 'application/json',
            success: function (response) {
                if (response != null) {
                    result = response;
                    if (callback)
                        callback(result);
                }
            }
        });
        if (callback == null)
            return result;
    }
});