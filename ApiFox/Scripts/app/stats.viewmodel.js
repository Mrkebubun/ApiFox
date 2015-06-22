function StatsViewModel(app, dataModel) {
    var self = this;
    self.apis = ko.observableArray([]);
    self.myHometown = ko.observable("");
    self.username = ko.observable();
    self.today = ko.observableArray();
    self.yesterday = ko.observableArray();

    self.resetStats = function () {
        self.apis.removeAll();
        self.today.removeAll();
        self.yesterday.removeAll();
    };
    var testApi = '';
    Sammy(function () {
        this.get('#stats', function () {
            app.view(self);
            // Make a call to the protected Web API by passing in a Bearer Authorization Header
            self.resetStats();

            $.ajax({
                method: 'get',
                url: app.dataModel.apiStatsUrl(testApi),
                contentType: "application/json; charset=utf-8",
                headers: {
                    'Authorization': 'Bearer ' + app.dataModel.getAccessToken()
                },
                success: function (data) {
                    var apis = data.apis || [];
                    var today = data.today || [];
                    var yesterday = data.yesterday || [];

                    for (var i = 0; i < today.length; i++)
                        self.today.push(new DayStats(today[i]));
                    for (var i = 0; i < yesterday.length; i++)
                        self.yesterday.push(new DayStats(yesterday[i]));
                    for (var i = 0; i < apis.length; i++)
                        self.apis.push(new DayStats(apis[i]));
                }
            });
        });
    });

    return self;
}

app.addViewModel({
    name: "Stats",
    bindingMemberName: "stats",
    factory: StatsViewModel
});
