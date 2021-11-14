//define namespace for bootstrap components
var bootstrapNS = {};
//register js namespace for bootstrap components
(function () {
    this.version = "component 1.0";//just a reminder on how to use scoped ns variables
    //this.JSDataTables = {};

    this.initComponent = function (name) {
        //SayHello(name);//call to outside function SayHello
        this.SayHello();
    }
    this.SayHello = function (name) {
        alert("Hello, from bootstrap-datatables component lib " + this.version + " call");
    }
    this.ToggleModal = function (modal, mode) {
        $(modal).modal(mode);
    }
    this.ToggleToast = function (toast, options) {
        $(toast).toast(options);
    }
    this.JSDataTable = function (table, options) {
        if (!$.fn.dataTable.isDataTable(table)) {
               $(table).DataTable(options);
            //this.JSDataTables[table] = table;
        }
    }
    this.RefreshJSDataTable = function (table, options) {
        $(table).dataTable().fnDestroy();
    }
}).apply(bootstrapNS);

//remember it's a spa, so it is only called once when the DOM of initial screen is ready
//and therefore is for the most useless
$(document).ready(function () {
   
});
//called when the entire window is ready, than means also:
//dom is ready, frames/iframes are ready
$(window).on("load", function () {
    /*bootstrapNS.MakeTabs("adminTabs a");*/
    //SayHello();
    //bootstrapNS.MakeRichTables(); does not do a shit unless a table is on the initial screen
});
