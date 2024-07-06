using CallCenterReports.Core;
using CallCenterReports.Service;

namespace CallCenterReportsViewer;

class Program
{
    static void Main(string[] args)
    {
        var recordsDataProvider = new WorkDataRecordProvider(args[0]);
        var reportGeneratorActiveSessions =  new ReportGeneratorActiveSessions(recordsDataProvider);

        Console.WriteLine("Active sessions number report start");
        
        foreach (var record in reportGeneratorActiveSessions.GetReport())
        {
            Console.WriteLine($"{record.Date}  {record.ActiveSessionsCount}");
        }
        
        Console.WriteLine("Report end");
        Console.WriteLine("==============================");
        
        var reportGeneratorOperatorInStatusDurations =  new ReportGeneratorOperatorInStatusDurations(recordsDataProvider);
        
        Console.WriteLine("Total minutes in status report start");

        var records = reportGeneratorOperatorInStatusDurations.GetReport();
        
        foreach (var record in records.GroupBy(x => x.Operator))
        {
            var durationInStatus = record.Select(x => $"{x.Status}: {x.Duration}");
            
            Console.WriteLine($"{record.Key} {string.Join(" ", durationInStatus)}");
        }
        
        Console.WriteLine("Report end");
    }
}