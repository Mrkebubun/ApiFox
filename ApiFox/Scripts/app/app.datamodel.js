function AppDataModel() {
    var self = this;
    // Routes
    self.userInfoUrl = "/apis/Me";
    self.userApilistUrl = "/apis/apilist";
    self.siteUrl = "/";
    self.apiStatsUrl = function (apiUrl) { return "/api/stats/" + (apiUrl || '') };
    self.authTokenUrl = "/token";
    self.authTokenData = function (username, password) { return "grant_type=password&username="+ username+"&password="+password; };
    self.gSheetStore = "/Home/SaveGSheet";

    // Route operations
    function manageImportedFileUrl(id) { return "apis/Apis/" + id; }

    // Other private operations

    // Operations
    self.setImportedFileInfo = function (id, data) {
        return $.ajax(manageImportedFileUrl(id), {
            type: "PUT",
            data: JSON.stringify(data),
            contentType: "application/json;charset=utf-8",
            headers: {
                'Authorization': 'Bearer ' + self.getAccessToken()
            }
        });
        
    };
    self.deleteApi = function (id) {
        var deleteUrl = "/apis/apis/delete";
        return $.ajax(deleteUrl +"/"+id, {
            type: "PUT",
            data: JSON.stringify({ "apiId": id }),
            contentType: "application/json;charset=utf-8",
            headers: {
                'Authorization': 'Bearer ' + self.getAccessToken()
            }
        });

    };

    self.saveGSheet = function (gSheet) {
        console.log('saving Google Sheet %s', gSheet);

        return $.ajax(self.gSheetStore, {
            type: "POST",
            data: JSON.stringify({ "gSheetUrl": gSheet }),
            contentType: "application/json;charset=utf-8",
            headers: {
                'Authorization': 'Bearer ' + self.getAccessToken()
            }
        });
    };
    // Data
    self.returnUrl = self.siteUrl;

    // Data access operations
    self.getApilist = function () {
       return $.ajax(self.userApilistUrl, {
            type: "get",
            contentType: "application/json;charset=utf-8",
            headers: {
                'Authorization': 'Bearer ' + self.getAccessToken()
            }
        });
    };
    self.setAccessToken = function (accessToken) {
        sessionStorage.setItem("accessToken", accessToken);
    };
   self.getNewAccessToken = function (username, password) {
      return $.ajax(self.authTokenUrl, {
           type: "post",
           data: self.authTokenData(username, password)
       });
    };
    self.getAccessToken = function () {
        return sessionStorage.getItem("accessToken");
    };
}
