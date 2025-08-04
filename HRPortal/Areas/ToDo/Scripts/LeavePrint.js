/*列印請假單用*/
$(function () {
    $("#PrintBtn").click(function () {
        fncPrint();
    });
})


function fncPrint() {
    var myPrintContent = document.getElementById('printdiv');
    var style = "<style>.hid-border-bottom {border-bottom-width:0px;}.m-border{border-left-width:0px;border-right-width:1px;border-top-width:0px;}#hidden_div{display: none;}.m-border-L{border-left-width:1px;border-right-width:0px;border-top-width:0px;}th, td {border:1px solid #aaa}table, div{margin-left: auto;margin-right: auto;border-collapse:collapse;font-family: Microsoft JhengHei,DFKai-sb;}";
    style += ".text{text-align:justify;text-justify: inter-ideograph;-ms-text-justify: inter-ideograph;-webkit-text-align-last: justify;-moz-text-align-last: justify;} .text:after{content: '';display: inline-block;width: 100%;}";
    style += " .tdpad {padding-left:20px;padding-right:20px;}";
    style += " .tdpadII {padding-left:4px;padding-right:4px;}";
    style += "</style>";

    console.log("aaaaa");

    try {
        var myPrintWindow = window.open('', 'PRINT', 'left=300,top=100,width=600,height=400');
        myPrintWindow.document.write(style + myPrintContent.innerHTML);
        myPrintWindow.document.getElementById('hidden_div').style.display = 'block'
        myPrintWindow.document.close();
        myPrintWindow.focus();
        myPrintWindow.print();

        console.log(navigator.userAgent.match("MSIE"));
        console.log(navigator.userAgent);

        if (navigator.userAgent.match("Chrome")) {
            myPrintWindow.close();
        } else {
            myPrintWindow.onfocus = function () { setTimeout(function () { myPrintWindow.close(); }, 500); }
        }

    } catch (err) {

    }

    return false;
}