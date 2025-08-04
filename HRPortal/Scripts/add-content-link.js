function AddContentLink() {
    $('.modal-body .form-group label').filter(function () {return $(this).text()=='內容'}).next('div').linkify({
        handleLinks: function (links) {
            links.attr('target', '_blank');
        }
    });
}