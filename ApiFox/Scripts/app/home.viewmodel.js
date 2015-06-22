function HomeViewModel(app, dataModel) {
    var self = this;
    self.apis = ko.observableArray();

    self.availApis = ko.computed(function () {
        return ko.utils.arrayFilter(self.apis(), function (a) {
            return !a.isDeleted();
        });
    });
    self.loading = ko.observable();
    self.popoverBindingHeader = ko.observable("this is title");
    
    self.gSheetUrl = ko.observable();

    self.convertGSheet = function () {

        dataModel.saveGSheet(self.gSheetUrl()).done(function (data) {
            var info = "G-sheet--" + moment().format("DD-MMM-hh.mm");
            anonUser = data.newusername;
            app.home().apis.push(new Api(info, data.apiUrl, data.id, dataModel));


            // TODO DRY for the section below:
            // if no guid then register and add cookie/token
            if (typeof $.cookie('guid') === 'undefined' && anonUser != null) {
                //no cookie
                self.register(anonUser).done(function () {
                    $.cookie('guid', anonUser);
                    //get access token
                    self.ensureAccessTokenByCookie();
                });
            } else {
                //has cookie, check if it has token, if no token then get one
                self.ensureAccessTokenByCookie();
            }
        });
    }

    self.uploadSuccees = function (e) {
        if (e.operation == "remove")
            return;
        var anonUser = null;
        $.map(e.files, function (file) {
            //TODO get the unique apiname from server, todo e.id=39
            var info = file.name.substring(0, file.name.length - 4);
            anonUser = e.response.newusername;
            app.home().apis.push(new Api(info, e.response.apiUrl, e.response.id, dataModel));
        });

        // if no guid then register and add cookie/token
        if (typeof $.cookie('guid') === 'undefined' && anonUser!= null) {
            //no cookie
            self.register(anonUser).done(function () {
                $.cookie('guid', anonUser);
                //get access token
                self.ensureAccessTokenByCookie();
            });
        } else {
            //has cookie, check if it has token, if no token then get one
            self.ensureAccessTokenByCookie();
        }
    }

    self.ensureAccessTokenByCookie = function () {
        var user = $.cookie('guid');

        if (app.dataModel.getAccessToken() == null && user)
            return app.dataModel.getNewAccessToken(user, "Anonymous").done(function (data) {
                app.dataModel.setAccessToken(data.access_token);
            });
        return null;
    };
    self.myHometown = ko.observable("");
    self.username = ko.observable();
 
    self.register = function (username) {
        $('form #anonemail').val(username);

        return $.ajax({
            method: 'post',
            url: '/Account/RegisterAnonymous',
            data: $('form#hdnregister').serialize(),
            success: function (data) {
                console.log('created user: ' + data.username);
            }
        });
    };
    self.resetGrids = function () {
        self.apis.removeAll();
    };

    // Make a call to the protected Web API by passing in a Bearer Authorization Header
    self.loadHomepage = function () {
        return $.ajax({
            method: 'get',
            url: app.dataModel.userInfoUrl,
            contentType: "application/json; charset=utf-8",
            headers: {
                'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
            },
            success: function (data) {
                self.myHometown(data.Hometown);
                self.username(data.Username);

                self.loading(true);
                dataModel.getApilist().done(function (data) {
                    self.loading(false);
                    $.map(data || [], function (item) {
                        self.apis.push(new Api(item.apiName, item.apiUrl, item.apiId, dataModel));
                    });
                });

                self.myHometown('Your Hometown is : ' + data.hometown);
            }
        });
    };

    Sammy(function () {
        this.get('#home', function () {
            app.view(self);
            self.resetGrids(); 
            var promise = self.ensureAccessTokenByCookie();
            if (promise != null)
                promise.done(function () {
                    self.loadHomepage();
                });
            else if (app.dataModel.getAccessToken() == null) {
               // window.location = "/Account/Authorize?client_id=web&response_type=token&state=" + encodeURIComponent(window.location.hash);
            }
            else
                self.loadHomepage();
        });
        this.get('/#_ = _', function () { this.app.runRoute('get', '#home') });
        this.get('/', function () { this.app.runRoute('get', '#home') });
    });

    return self;
}

app.addViewModel({
    name: "Home",
    bindingMemberName: "home",
    factory: HomeViewModel
});
