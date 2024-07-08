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
            var maxOverlaps = GetMaxOverlapsCount(dayRecords.Value
                .OrderBy(x => x.End)
                .ThenBy(x => x.Start)
                .ToList());
            
            result.Add(new ActiveSessionsByDay { Date = dayRecords.Key, ActiveSessionsCount = maxOverlaps });
        }

        return result;
    }
    
    static int GetMaxOverlapsCount(List<(int Start, int End)> lines)
    {
        if (!lines.Any()) return 0;

        var markers = new List<(int Time, bool IsStart)>();
        foreach (var line in lines)
        {
            markers.Add((line.Start, true));
            markers.Add((line.End, false));
        }

        markers.Sort((first, second) =>
        {
            var timeComparison = first.Time.CompareTo(second.Time);
            return timeComparison != 0 ? timeComparison : first.IsStart.CompareTo(second.IsStart);
        });

        int maxOverlaps = 0, currentOverlaps = 0;

        foreach (var marker in markers)
        {
            if (marker.IsStart)
                currentOverlaps++;
            else
                currentOverlaps--;

            maxOverlaps = Math.Max(maxOverlaps, currentOverlaps);
        }

        return maxOverlaps > 0 ? maxOverlaps - 1 : 0;
    }
}