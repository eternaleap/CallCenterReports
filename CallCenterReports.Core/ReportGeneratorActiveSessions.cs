using System.Collections;

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

                var start = (int)(record.StartDate - record.StartDate.Date).TotalSeconds;
                recordsByDay[date].Add((start, start + record.Duration));
            }
        }
        
        var result = new List<ActiveSessionsByDay>();

        foreach (var dayRecords in recordsByDay)
        {
            var maxOverlaps = GetMaxOverlaps(dayRecords.Value.OrderBy(x => x.End).ThenBy(x => x.Start).ToArray());
            result.Add(new ActiveSessionsByDay { Date = dayRecords.Key, ActiveSessionsCount = maxOverlaps });
        }

        return result;
    }
    
    static int GetMaxOverlaps(IReadOnlyList<(int Start, int End)> lines)
    {
        if (!lines.Any()) 
            return 0;

        var maxOverlaps = 0;
        var activeIntervals = new List<(int Start, int End)> { lines[0] };

        for (var i = 1; i < lines.Count; i++)
        {
            var currentInterval = lines[i];
            var overlapsWithAll = activeIntervals.All(interval => IsOverlaps(currentInterval, interval));

            if (overlapsWithAll)
            {
                activeIntervals.Add(lines[i]);
                maxOverlaps = Math.Max(maxOverlaps, activeIntervals.Count);
            }
            else
            {
                activeIntervals.RemoveAll(interval => !IsOverlaps(currentInterval, interval));
                activeIntervals.Add(currentInterval);
                maxOverlaps = Math.Max(maxOverlaps, activeIntervals.Count);
            }
        }

        return maxOverlaps;
    }

    static bool IsOverlaps((int Start, int End) first, (int Start, int End) second) =>
        first.Start <= second.End && second.Start <= first.End;
}