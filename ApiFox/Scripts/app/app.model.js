function Api(apiname, apiurl, id, dataModel) {
    var self = this;
    this.apiname = ko.observable(apiname);
    this.apiurl = ko.observable(apiurl);
    self.isFocus1 = ko.observable(false);
    self.isFocus2 = ko.observable(false);

    self.isDeleted = ko.observable(false);

    self.isEditMode = ko.observable(false);
    self.editMode = ko.computed({
        read: function () {
            return self.isEditMode();//self.isFocus1() || self.isFocus2();
        },
        write: function (newVal) {
            if (newVal) {
                self.isEditMode(true);
                self.isFocus1(newVal);
                self.isFocus2(!newVal)
            }
            else {
                self.isFocus1(false);
                self.isFocus2(false);
            }
        }
    });
    self.id = ko.observable(id);

    self.saveApi = function () {
        var apiViewModel = new Object();
        apiViewModel.ApiName = self.apiname();
        apiViewModel.ApiUrl = self.apiurl();
        dataModel.setImportedFileInfo(self.id(), apiViewModel);
        //todo if Save not valid then show error
        self.editMode(false);
    };

    self.editMode.subscribe(function (newVal) {
        if (!newVal) self.saveApi();
    });

    self.deleteMe = function()
    {
        dataModel.deleteApi(self.id())
            .done(function (resp, data, xhr) {
                //remove api from client side now

                self.isDeleted(true);

                console.dir(xhr);

                console.dir(data);

            })
            .fail(function (resp, data,xhr) {

                //display the error
                console.dir(resp);

                console.dir(data);
            });
    }
}