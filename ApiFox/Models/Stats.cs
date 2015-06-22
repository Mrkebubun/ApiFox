using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiFox.Models
{
    public class Stats
    {
        private IQueryable<ApiRequests> apiRequests;
        private IQueryable<ApiRequests> todayApiRequests;
        private IQueryable<ApiRequests> yesterdayApiRequests;

        public Stats()
        {
            this.GenereatedOn = DateTime.UtcNow;
        }

        public Stats(IQueryable<ApiRequests> apiRequests, string name)
        {
            this.Name = name;
            this.GenereatedOn = DateTime.UtcNow;
            this.apiRequests = apiRequests;
            var tomorrow = DateTime.Today.AddDays(1);
            var yesterday = DateTime.Today.AddDays(-1);

            this.todayApiRequests = apiRequests.Where(a => a.DateCreated >= DateTime.Today && a.DateCreated < tomorrow);
            this.yesterdayApiRequests = apiRequests.Where(a => a.DateCreated < DateTime.Today && a.DateCreated >= yesterday);

            // Todo load by apiUrl
            this.LoadStats();
        }

        private void LoadStats()
        {
        }

        public string Name { get; set; }
        public DateTime GenereatedOn { get; set; }
        public Dictionary<string, DayStats> AggregatedStats
        {
            get
            {
                var aggregatedStats = new Dictionary<string, DayStats>();

                var groups = apiRequests.GroupBy(g => g.Api.Id);
                foreach (var item in groups.ToList())
                {
                    var val = item.ToList().FirstOrDefault();
                    if (val != null)
                        aggregatedStats.Add(item.Key.ToString(), new DayStats(val.ApiUrl, item.Count(), DateTime.UtcNow));
                }
                return aggregatedStats;
            }
        }

        public object ToDto()
        {
            return new
            {
                today = todayApiRequests.GroupBy(d => d.ApiUrl, d => d, (key, g) => new { ApiName = key, Count = g.Count(), Date = DateTime.UtcNow, LatestRequest = g.Max(k => k.DateCreated)}),
                yesterday = yesterdayApiRequests.GroupBy(d => d.ApiUrl, d => d, (key, g) => new { ApiName = key, Count = g.Count(), Date = DateTime.UtcNow, LatestRequest = g.Max(k => k.DateCreated)}),
                name = this.Name,
                utcDate = this.GenereatedOn.ToString("yyy/MM/dd hh:mm:ss"),
                apis = AggregatedStats.Select(a => a.Value).ToArray()
            };
        }
    }

    public class TodayStats
    {
        public string ApiName { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public DateTime? LatestRequest { get; set; }
    }

    public class DayStats
    {
        public DayStats(DateTime date)
        {
            this.Date = date;
        }
        public DayStats(DateTime date, string apiName)
        {
            this.Date = date;
            this.ApiName = apiName;
        }
        public DayStats(string apiName, int count, DateTime date)
        {
            this.Date = date;
            this.ApiName = apiName;
            this.Count = count;
        }
        public string ApiName { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public DateTime? LatestRequest { get; set; }


        internal object ToDto()
        {
            return new { count = this.Count, latestRequestAt = this.LatestRequest.HasValue ? this.LatestRequest.Value.ToString("yyyy/MM/dd hh:mm:ss") : string.Empty, name = this.ApiName };
        }
    }
}