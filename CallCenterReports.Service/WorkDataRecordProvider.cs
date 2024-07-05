using System.Globalization;
using CallCenterReports.Core;

namespace CallCenterReports.Service;

public class WorkDataRecordProvider : IWorkDataRecordProvider
{
    private List<OperatorWorkRecord> _records;
    private string _filePath;
    
    public WorkDataRecordProvider(string filePath)
    {
        _filePath = filePath;
    }

    public IReadOnlyCollection<OperatorWorkRecord> GetData()
    {
        if (_records != null)
            return _records;
        
        _records = new List<OperatorWorkRecord>();

        const string format = "dd.MM.yyyy HH:mm:ss";
        const char separator = ';'; 
        CultureInfo culture = new CultureInfo("ru-RU");
        
        using (StreamReader reader = new StreamReader(_filePath))
        {
            string line;
            bool isHeaderLine = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (isHeaderLine)
                {
                    isHeaderLine = false;
                    continue;
                }

                string[] fields = line.Split(separator);
                if (fields.Length == 6)
                {
                    OperatorWorkRecord record = new OperatorWorkRecord
                    {
                        StartDate = DateTime.ParseExact(fields[0].Trim(), format, culture),
                        EndDate = DateTime.ParseExact(fields[1].Trim(), format, culture),
                        Project = fields[2],
                        Operator = fields[3],
                        Status = fields[4],
                        Duration = int.Parse(fields[5])
                    };
                    _records.Add(record);
                }
            }
        }
        
        return _records;
    }
}