namespace CallCenterReports.Core;

public class OperatorWorkRecord
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Project { get; set; }
    public string Operator { get; set; }
    public string Status { get; set; }
    public int Duration { get; set; }
}
