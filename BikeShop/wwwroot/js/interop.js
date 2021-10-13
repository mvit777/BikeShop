//a test function from global space
var SayHello = function Hello(name) {
    alert('Hello  from ' + name + ' call!!!');
}

//define namespace for bootstrap components
var bootstrapNS = {};
//register js namespace for bootstrap components
(function () {
    this.version = "component 1.0";//just a reminder on how to use scoped ns variables

    this.initComponent = function (name) {
        //SayHello(name);//call to outside function SayHello
        this.SayHello();
    }
    this.SayHello = function (name) {
        alert("Hello, from comp" + this.version + " call");
    }
    this.JSDataTable = function (table, options) {
        $(table).DataTable(options);
    }
}).apply(bootstrapNS);

//remember it's a spa, so it is only called once when the DOM of initial screen is ready
$(document).ready(function () {
   
});
//called when the entire window is ready, than means also:
//dom is ready, frames/iframes are ready
$(window).on("load", function () {
    /*bootstrapNS.MakeTabs("adminTabs a");*/
    //SayHello();
    //bootstrapNS.MakeRichTables(); does do a shit unless a table is on the initial screen
});
