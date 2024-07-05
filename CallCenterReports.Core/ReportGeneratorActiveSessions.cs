namespace CallCenterReports.Core;

public class ActiveSessionsByDay
{
    public DateTime Date { get; set; }
    public int ActiveSessionsCount { get; set; }
}

public class ReportGeneratorActiveSessions
{
    private readonly IWorkDataRecordProvider _dataRecordProvider;

    public ReportGeneratorActiveSessions(IWorkDataRecordProvider dataRecordProvider)
    {
        _dataRecordProvider = dataRecordProvider;
    }

    public IReadOnlyCollection<ActiveSessionsByDay> GetReport()
    {
        var records = _dataRecordProvider.GetData();
        var recordsByDay = new Dictionary<DateTime, int>();

        foreach (var record in records)
        {
            var startDate = record.StartDate.Date;
            var endDate = record.EndDate.Date;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (recordsByDay.ContainsKey(date))
                {
                    recordsByDay[date]++;
                }
                else
                {
                    recordsByDay[date] = 1;
                }
            }
        }

        return recordsByDay.Select(x => new ActiveSessionsByDay { Date = x.Key, ActiveSessionsCount = x.Value })
            .ToArray();
    }
}