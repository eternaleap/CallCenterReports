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
        var recordsByDay = new Dictionary<DateTime, List<(int Start, int End)>>();

        foreach (var record in records)
        {
            var startDate = record.StartDate.Date;
            var endDate = record.EndDate.Date;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (!recordsByDay.ContainsKey(date))
                {
                    recordsByDay[date] = new List<(int Start, int End)>();
                }
                recordsByDay[date].Add((
                    (int)(record.StartDate - record.StartDate.Date).TotalSeconds,
                    // TODO: speed up solution by using precalculated Duration field
                    (int)(record.EndDate - record.StartDate.Date).TotalSeconds
                ));
            }
        }
        
        var result = new List<ActiveSessionsByDay>();

        foreach (var dayRecords in recordsByDay)
        {
            var maxOverlaps = GetMaxOverlaps(dayRecords.Value.OrderBy(x => x.Start).ThenBy(x => x.End).ToList());
            result.Add(new ActiveSessionsByDay { Date = dayRecords.Key, ActiveSessionsCount = maxOverlaps });
        }

        return result;
    }
    
    static int GetMaxOverlaps(IReadOnlyList<(int Start, int End)> lines)
    {
        if (lines == null || !lines.Any()) return 0;

        var maxOverlaps = 0;
        var activeIntervals = new List<(int Start, int End)> { lines[0] };

        for (var i = 1; i < lines.Count; i++)
        {
            var overlapsWithAll = activeIntervals.All(interval => lines[i].Start <= interval.End && interval.Start <= lines[i].End);

            if (overlapsWithAll)
            {
                activeIntervals.Add(lines[i]);
                maxOverlaps = Math.Max(maxOverlaps, activeIntervals.Count);
            }
            else
            {
                activeIntervals.Clear();
                activeIntervals.Add(lines[i]);
            }
        }

        return maxOverlaps;
    }
}