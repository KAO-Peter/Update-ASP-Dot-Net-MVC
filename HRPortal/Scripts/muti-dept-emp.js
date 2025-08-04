var mutiSelect = {
    url: "",
    deptSelect: $('#DepartmentList'),
    deptID: $('#DeptID'),
    empSelect: $('#EmployeeList'),
    empID: $('#EmpID'),
    divEmp: $('.divEmp'),
    statusSelect: $('#status'),
    empvalmode: "",
    selAll: true,

    setDepartmentList: function () {
        if (this.deptSelect.find('option').length == 1) {
            this.deptSelect.removeAttr('multiple');
        }
        else {
            if (mutiSelect.deptSelect.find('option:selected').length == 0) {
                mutiSelect.deptSelect.find('option').prop('selected', true);
                this.getdeptID();
                this.getEmployee();
            }

            this.deptSelect.multipleSelect({
                selectAllText: '全部',
                allSelected: '全部'
            });

            var $multiSelectArea = this.deptSelect.parent();
            $multiSelectArea.children('.ms-parent').css({ 'padding': '0 0 0 0', 'max-width': '100%' });
            $multiSelectArea.find('.ms-choice').css({ 'height': '100%' });
        }

        this.deptSelect.change(function () {
            if (mutiSelect.departmentSelected()) {
                mutiSelect.getEmployee();
            }
        });

        this.empSelect.change(function () {
            if (mutiSelect.empSelect.children().length > 0) {
                var curSelectEmp = [];
                $.each(mutiSelect.empSelect.find('option:selected'), function () {
                    curSelectEmp.push($(this).val());
                })
                mutiSelect.empID.val(curSelectEmp.join());
            }
        });

        this.statusSelect.change(function () {
            mutiSelect.empID.val('');
            mutiSelect.getdeptID();
            mutiSelect.getEmployee();
        });

        this.setEmployeeSelect();
    },
    departmentSelected: function () {
        this.empID.val('');
        var selectAll = this.deptSelect.parent().find('.ms-select-all')[0];

        if (this.deptSelect.find('option').length > 1 && $(selectAll).hasClass('selected')) {
            this.divEmp.hide();
            this.empSelect.empty();
            this.deptID.val('All');
            this.empID.val('All');
            return false;
        }
        else {
            mutiSelect.getdeptID();
            return true;
        }
    },
    setEmployeeSelect: function () {
        if (this.deptSelect.find('option').length == 1 && this.empSelect.find('option').length == 1) {
            this.empSelect.removeAttr('multiple');
            this.empID.val(this.empSelect.val());
        }
        else {
            this.empSelect.multipleSelect({
                filter: true,
                noMatchesFound: '無符合資料',
                selectAllText: '全部',
                allSelected: '全部'
            });

            var $employeeSelectArea = this.empSelect.parent();
            $employeeSelectArea.children('.ms-parent').css({ 'max-width': '100%', 'padding': '0 0 0 0' });
            $employeeSelectArea.find('.ms-choice').css({ 'height': '100%' });
        }
    },
    getdeptID: function () {
        var curSelectDept = [];
        $.each(mutiSelect.deptSelect.find('option:selected'), function () {
            curSelectDept.push($(this).val());
        });
        mutiSelect.deptID.val(curSelectDept.join());
    },
    getEmployee: function () {
        $.post(mutiSelect.url + '/GetEmployee', { departmentId: mutiSelect.deptID.val(), status: mutiSelect.statusSelect.val(), empvalmode: mutiSelect.empvalmode },
            function (employees) {
                mutiSelect.empSelect.empty();
                if (employees.length == 0) {
                    mutiSelect.divEmp.hide();
                }
                else {
                    mutiSelect.divEmp.show();
                    $.each(employees, function (index, employee) {
                        mutiSelect.empSelect.append($('<option></option>').val(employee.Value).attr('title', employee.Title).html(employee.Text));
                    });
                    mutiSelect.setEmployeeSelect();
                }
            }, 'json');
    },
    setDepartmentList2: function () {
        if (this.deptSelect.find('option').length == 1) {
            this.deptSelect.removeAttr('multiple');
        }
        else {
            if (mutiSelect.deptSelect.find('option:selected').length == 0) {
                mutiSelect.deptSelect.find('option').prop('selected', mutiSelect.selAll);
                this.getdeptID();
                this.getEmployee2();
            }

            this.deptSelect.multipleSelect({
                selectAllText: '全部',
                allSelected: '全部'
            });

            var $multiSelectArea = this.deptSelect.parent();
            $multiSelectArea.children('.ms-parent').css({ 'padding': '0 0 0 0', 'max-width': '100%' });
            $multiSelectArea.find('.ms-choice').css({ 'height': '100%' });
        }

        this.deptSelect.change(function () {
            if (mutiSelect.departmentSelected()) {
                mutiSelect.getEmployee2();
            }
        });

        this.empSelect.change(function () {
            if (mutiSelect.empSelect.children().length > 0) {
                var curSelectEmp = [];
                $.each(mutiSelect.empSelect.find('option:selected'), function () {
                    curSelectEmp.push($(this).val());
                })
                mutiSelect.empID.val(curSelectEmp.join());
            }
        });

        this.statusSelect.change(function () {
            mutiSelect.empID.val('');
            mutiSelect.getdeptID();
            mutiSelect.getEmployee2();
        });

        this.setEmployeeSelect();
    },
    getEmployee2: function () {
        $.post(mutiSelect.url + '/GetEmployee2', { departmentId: mutiSelect.deptID.val(), status: mutiSelect.statusSelect.val(), empvalmode: mutiSelect.empvalmode },
            function (employees) {
                mutiSelect.empSelect.empty();
                if (employees.length == 0) {
                    mutiSelect.divEmp.hide();
                }
                else {
                    mutiSelect.divEmp.show();
                    $.each(employees, function (index, employee) {
                        mutiSelect.empSelect.append($("<option></option>").val(employee.Value).attr('title', employee.Title).html(employee.Text));
                    });
                    mutiSelect.empSelect.find('option').prop('selected', true);
                    mutiSelect.setEmployeeSelect();
                }
            }, 'json');
    }
}