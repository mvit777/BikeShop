//a test function from global space
var SayHello = function Hello(name) {
    alert('Hello  from ' + name + ' call!!!');
}

//define namespace for bootstrap components
var bootstrapNS = {};
//register js namespace for bootstrap components
(function () {
    this.version = "component 1.0";//just a reminder on how to use scoped ns variables
    this.JSDataTables = {};

    this.initComponent = function (name) {
        //SayHello(name);//call to outside function SayHello
        this.SayHello();
    }
    this.SayHello = function (name) {
        alert("Hello, from comp" + this.version + " call");
    }
    this.JSDataTable = function (table, options) {
        //console.log(table);
        //if (this.hasOwnProperty(table) == false) {
        //    console.log("table " + table + " is not present");
        //    var oTable = $(table).DataTable(options);
        //    this.JSDataTables[table] = oTable;
        //} else {
        //    console.log("table " + table + " is present");
        //}
        if ($.fn.dataTable.isDataTable(table)) {
            //table.destroy();
        }
        else {
            table = $(table).DataTable(options);
            this.JSDataTables[table] = table;
        }
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
    //bootstrapNS.MakeRichTables(); does do a shit unless a table is on the initial screen
});
