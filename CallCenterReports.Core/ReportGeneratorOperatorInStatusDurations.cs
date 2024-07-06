namespace CallCenterReports.Core;

public class OperatorInStatusDurations
{
    public string Operator { get; set; }
    public string Status { get; set; }
    public int Duration { get; set; }
}

public class ReportGeneratorOperatorInStatusDurations
{
    private readonly IWorkDataRecordProvider _dataRecordProvider;

    public ReportGeneratorOperatorInStatusDurations(IWorkDataRecordProvider dataRecordProvider)
    {
        _dataRecordProvider = dataRecordProvider;
    }

    public IReadOnlyCollection<OperatorInStatusDurations> GetReport()
    {
        var operatorStatusDurations = new Dictionary<(string Operator, string Status), int>();
        var records = _dataRecordProvider.GetData();

        foreach (var record in records)
        {
            var key = (record.Operator, record.Status);
            var totalMinutes = (int)(record.EndDate - record.StartDate).TotalMinutes;

            if (!operatorStatusDurations.TryAdd(key, totalMinutes))
            {
                operatorStatusDurations[key] += totalMinutes;
            }
        }

        return operatorStatusDurations.Select(x => new OperatorInStatusDurations
            { Operator = x.Key.Operator, Status = x.Key.Status, Duration = x.Value }).ToArray();
    }
}
