/*
 * Translated default messages for the jQuery validation plugin.
 * Locale: ZH (Chinese; 中文 (Zhōngwén), 汉语, 漢語)
 * Region: TW (Taiwan)
 */
$.extend($.validator.messages, {
    required: "此欄位為必填欄位",
    remote: "請檢查此欄位",
    email: "email格式不正確",
    url: "請輸入有效的網址",
    date: "日期格式不正確",
    dateISO: "日期格式不正確",
    number: "請輸入一組數字",
    digits: "請輸入一組數字",
    creditcard: "請輸入有效的信用卡號碼",
    equalTo: "請重複輸入一次",
    extension: "請輸入有效的後綴",
    maxlength: $.validator.format("最大長度為 {0} 個字元"),
    minlength: $.validator.format("請至少輸入 {0} 個字元"),
    rangelength: $.validator.format("請輸入長度為 {0} 至 {1} 之間的字串"),
    range: $.validator.format("請輸入 {0} 至 {1} 之間的數值"),
    max: $.validator.format("輸入值必須小於等於 {0}"),
    min: $.validator.format("輸入值必須大於等於 {0}")
});
$.validator.setDefaults({
    highlight: function (element) {
        $(element).closest('.form-group').addClass('has-error');
    },
    unhighlight: function (element) {
        $(element).closest('.form-group').removeClass('has-error');
    },
    errorElement: 'span',
    errorClass: 'help-block',
    errorPlacement: function (error, element) {
        if (element.parent('.input-group').length) {
            error.insertAfter(element.parent());
        } else {
            error.insertAfter(element);
        }
    }
});