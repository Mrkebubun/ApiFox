function DayStats(data){
	this.ApiName = data.ApiName;
	this.Date = moment(data.Date);
	this.Count = ko.observable(data.Count);
	this.LatestRequest = ko.observable(moment(data.LatestRequest));
}