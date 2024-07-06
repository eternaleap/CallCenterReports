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
                    (int)(record.EndDate - record.StartDate.Date).TotalSeconds
                ));
            }
        }
        
        var result = new List<ActiveSessionsByDay>();

        foreach (var dayRecords in recordsByDay)
        {
            var maxOverlaps = GetMaxOverlaps(dayRecords.Value.ToList());
            result.Add(new ActiveSessionsByDay { Date = dayRecords.Key, ActiveSessionsCount = maxOverlaps });
        }

        return result;
    }
    
    static int GetMaxOverlaps(List<(int Start, int End)> ranges)
    {
        var hash = new Dictionary<int, int>();
        var lastRangeEndValue = ranges[0].End;
        
        foreach (var range in ranges)
        {
            if (!hash.TryAdd(range.Start, 1))
            {
                hash[range.Start]++;
            }

            if (!hash.TryAdd(range.End, -1))
            {
                hash[range.End]--;
            }

            lastRangeEndValue = Math.Max(lastRangeEndValue, range.End);
        }

        var totalOverlaps = hash.First().Value;
        var maxTotalOverlaps = totalOverlaps;
        
        foreach (var pair in hash)
        {
            totalOverlaps += hash[pair.Key];
            
            if (totalOverlaps > maxTotalOverlaps)
            {
                maxTotalOverlaps = totalOverlaps;
            }
        }
        
        return maxTotalOverlaps;
    }
}