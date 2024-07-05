namespace CallCenterReports.Core;

public interface IWorkDataRecordProvider
{
    public IReadOnlyCollection<OperatorWorkRecord> GetData();
}