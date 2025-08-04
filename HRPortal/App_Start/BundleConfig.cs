using System.Web.Optimization;

namespace HRPortal
{
    public class BundleConfig
    {
        // 如需「搭配」的詳細資訊，請瀏覽 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/ie8").Include(
                "~/Scripts/CP0L0/ie8.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jquery1").Include(
                "~/Scripts/plugins/jquery/jquery-{version}.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/json2.js",
                "~/Scripts/plugins/html5shiv.js",
                "~/Scripts/respond.js",
                "~/Scripts/bootstrap-datetimepicker.js",
                "~/Scripts/bootstrap-datetimepicker.zh-TW.js",
                "~/Scripts/SimpleDatePicker/jquery.simple-dtpicker_1.3.js",
                "~/Scripts/jquery.url.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/jquery2").Include(
                //"~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.unobtrusive-ajax*"));

            bundles.Add(new ScriptBundle("~/bundles/ajaxjquery").Include(
        "~/Scripts/jquery.unobtrusive-ajax*"));

            bundles.Add(new ScriptBundle("~/bundles/validate").Include(
                "~/Scripts/jquery.validate.unobtrusive.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/bootbox.min.js",
                "~/Scripts/plugins/metisMenu/metisMenu.js",
                "~/Scripts/plugins/sb-admin-2/sb-admin-2.js",
                "~/Scripts/jquery.validate.js",
                "~/Scripts/CP0L0/localization/messages_zh_TW.js",
                "~/Scripts/moment.js",
                "~/Scripts/toastr.js",
                "~/Scripts/plugins/locale/zh-tw.js",
                "~/Scripts/plugins/bootstrap-dialog/bootstrap-dialog.js",
                "~/Scripts/plugins/bootstrap-treeview/bootstrap-treeview.js",
                "~/Scripts/jquery.placeholder.js",
                "~/Scripts/plugins/formatter/jquery.formatter.js",
                "~/Scripts/security.js",
                "~/Scripts/CP0L0/CP0L0.js"));

            bundles.Add(new ScriptBundle("~/bundles/form").Include(
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/locales/bootstrap-datepicker.zh-TW.js",
                "~/Scripts/plugins/bootstrap-timepicker/bootstrap-timepicker.js",
                "~/Scripts/CP0L0/form/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/grid").Include(
                "~/Scripts/jquery.jqGrid.src.js",
                "~/Scripts/i18n/grid.locale-tw.js",
                "~/Scripts/CP0L0/jqGrid.js"));

            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                "~/Scripts/knockout-{version}.js",
                "~/Scripts/knockout.validation.js",
                "~/Scripts/plugins/knockout-mapping/knockout.mapping-latest.js",
                "~/Scripts/CP0L0/knockout/validation/extension.js",
                "~/Scripts/CP0L0/knockout/validation/Localization/zh-tw.js",
                "~/Scripts/CP0L0/knockout/knockout.bindings.date.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-center").Include(
                "~/Scripts/plugins/jquery-center/jquery.center.js"));

            bundles.Add(new ScriptBundle("~/bundles/multipleSelectJS").Include(
                "~/Scripts/multiple-select-master/multiple-select.js"));

            bundles.Add(new ScriptBundle("~/bundles/CalendarJs").Include(
                "~/Scripts/fullcalendar/moment.min.js",
                "~/Scripts/fullcalendar/fullcalendar.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/linkify").Include(
               "~/Scripts/jquery.linkify-1.0.js",
               "~/Scripts/add-content-link.js"

               ));
            bundles.Add(new ScriptBundle("~/bundles/displayPDFContent").Include(
                "~/Scripts/pdfobject.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/imageZoom").Include(
                "~/Scripts/lightbox.js"
                ));

            bundles.Add(new StyleBundle("~/content/multipleSelectCSS").Include(
                "~/Content/multiple-select-master/multiple-select.css"));

            bundles.Add(new StyleBundle("~/content/ie8").Include(
                "~/Content/CP0L0/bootstrap-ie7.css"));

            bundles.Add(new StyleBundle("~/content/form").Include(
                //"~/Content/bootstrap-datepicker3.css",
                //"~/Content/plugins/bootstrap-timepicker/bootstrap-timepicker.css",
                "~/Content/jquery.simple-dtpicker_1.3.css"
                ));

            bundles.Add(new StyleBundle("~/content/grid").Include(
                "~/Content/jquery.jqGrid/ui.jqgrid.css",
                "~/Content/plugins/jqGrid/jqGrid.bootstrap.css"));

            bundles.Add(new StyleBundle("~/content/site").Include(
                "~/Content/bootstrap.css",
                "~/Content/bootstrap-datetimepicker.css",
                "~/Content/font-awesome.css",
                "~/Content/toastr.css",
                "~/Content/plugins/sb-admin-2/sb-admin-2.css",
                "~/Content/plugins/bootstrap-dialog/bootstrap-dialog.css",
                "~/Content/plugins/bootstrap-treeview/bootstrap-treeview.css",
                "~/Content/plugins/metisMenu/metisMenu.css",
                "~/Content/CP0L0/CP0L0.css",
                "~/Content/Site.css",
                "~/Content/CP0L0/NewPanel.css",
                "~/Content/PagedListPager"));

            bundles.Add(new StyleBundle("~/content/CP0L0/loginSite").Include("~/Content/CP0L0/index-2016.css"));
            bundles.Add(new StyleBundle("~/content/CP0L0/BG_2022").Include("~/Content/CP0L0/index-2022-BG.css"));
            bundles.Add(new StyleBundle("~/content/CP0L0/HomeSite").Include("~/Content/CP0L0/page-2016.css"));

            //遠百班表月曆
            bundles.Add(new StyleBundle("~/content/CalendarCSS").Include("~/Content/fullCalendar/fullcalendar.css"
                ));

            bundles.Add(new StyleBundle("~/content/LightBox").Include("~/Content/lightbox.css"));

            //BambooHR新報表加上DataTable Library
            bundles.Add(new StyleBundle("~/content/DataTable").Include("~/Content/DataTable/datatables.min.css"));
            bundles.Add(new ScriptBundle("~/bundles/DataTable").Include("~/Scripts/DataTable/datatables.min.js"));

            //BambooHR新報表查詢條件使用JQueryUI多選套件
            bundles.Add(new StyleBundle("~/content/jqueryuiMultiSelect").Include(
                "~/Content/jquery-ui.min.css", 
                "~/Content/theme.css", 
                "~/Content/jquery-ui.multiselect.css"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryuiMultiSelect").Include(
                "~/Scripts/jquery-ui.multiselect.js", 
                "~/Scripts/jquery-ui.multiselect.zh-tw.js"));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好實際執行時，請使用 http://modernizr.com 上的建置工具，只選擇您需要的測試。
        }
    }
}
