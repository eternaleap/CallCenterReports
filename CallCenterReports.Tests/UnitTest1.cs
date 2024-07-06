using CallCenterReports.Service;

namespace CallCenterReports.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var provider = new WorkDataRecordProvider("C:\\repos\\CallCenterReports\\Data\\test_data_onemonth.csv");
        var data = provider.GetData();

        var operator1 = data.Where(x => x.Operator == "Копытько Екатерина Игоревна" && x.Status == "Разговор");
        
    }
}