//here can go javascript initialisers
//import "./multiselect.min.js"
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
        this.MultiSelect = function (multiselect, options) {
            var opt = {
                search: {
                    left: '<input type="text" name="q" class="form-control" placeholder="Search..." />',
                    right: '<input type="text" name="q" class="form-control" placeholder="Search..." />',
                },
                fireSearch: function (value) {
                    return value.length > 3;
                }
            }
            $(multiselect).multiselect(opt);
        }

        this.GetSelectedOptions = function (multiselect) {
            var selectedOptions = [];
            $(multiselect + ">option").map(function () {
                selectedOptions.push($(this).val());
            });
            //console.log("selected options: ");
            //console.log(JSON.stringify(selectedOptions));
            return selectedOptions;
        }
        this.JSDataTable = function (table, options) {
            if (!$.fn.dataTable.isDataTable(table)) {
                //var opt = {dom: 'Bfrtip',buttons: ['copy', 'excel', 'pdf']};//fix it, only copy button appears, plugin missing?
                var opt = {};
                if (options.length > 0) {
                    opt = $.parseJSON(options[0]);
                }          
                $(table).DataTable(opt);
                //this.JSDataTables[table] = table;
            }
        }
        this.RefreshJSDataTable = function (table, options) {
            $(table).dataTable().fnDestroy();
        }
    }).apply(bootstrapNS);

