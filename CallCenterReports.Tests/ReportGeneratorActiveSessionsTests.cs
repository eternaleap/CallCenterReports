using CallCenterReports.Core;
using CallCenterReports.Service;
using Moq;

namespace CallCenterReports.Tests
{
    public class ReportGeneratorActiveSessionsTests
    {
        [Fact]
        public void GetReport_WithNoRecords_ReturnsEmptyCollection()
        {
            // Arrange
            var mockProvider = new Mock<IWorkDataRecordProvider>();
            mockProvider.Setup(p => p.GetData()).Returns(new List<OperatorWorkRecord>());

            var generator = new ReportGeneratorActiveSessions(mockProvider.Object);

            // Act
            var result = generator.GetReport();

            // Assert
            Assert.Empty(result);
        }
        
        [Fact]
        public void GetReport_ForSingleDay_Returns10()
        {
            // Arrange
            var generator = new ReportGeneratorActiveSessions(new WorkDataRecordProvider("C:\\repos\\CallCenterReports\\Data\\test_data_oneday.csv"));

            // Act
            var result = generator.GetReport().ToArray();

            // Assert
            Assert.Equal(12, result[0].ActiveSessionsCount);
        }

        [Fact]
        public void GetReport_MultipleDays_CalculatesCorrectly()
        {
            // Arrange
            var mockProvider = new Mock<IWorkDataRecordProvider>();
            var startDate = new DateTime(2023, 4, 1);
            var endDate = new DateTime(2023, 4, 1).AddHours(1);
            var records = new List<OperatorWorkRecord>
            {
                // doesn't overlap
                new() { StartDate = startDate, EndDate = endDate.AddHours(-1), Operator = "0" },
                
                // overlaps
                new() { StartDate = startDate, EndDate = endDate, Operator = "1" },
                new() { StartDate = startDate, EndDate = endDate, Operator = "1" },
                new() { StartDate = startDate.AddMinutes(10), EndDate = endDate.AddMinutes(10), Operator = "2" },
                
                // +2 days - doesn't overlaps with previous
                new() { StartDate = startDate.AddDays(2), EndDate = endDate.AddDays(2), Operator = "3" }
            };
            mockProvider.Setup(p => p.GetData()).Returns(records);

            var generator = new ReportGeneratorActiveSessions(mockProvider.Object);

            // Act
            var result = generator.GetReport().ToArray();

            // Assert
            Assert.Equal(2, result.Length);
            Assert.Equal(startDate.Date, result[0].Date);
            Assert.Equal(3, result[0].ActiveSessionsCount);
        }

        [Fact]
        public void GetReport_SingleDay_CalculatesCorrectly()
        {
            // Arrange
            var mockProvider = new Mock<IWorkDataRecordProvider>();
            var startDate = new DateTime(2023, 4, 1).AddHours(8);
            var endDate = new DateTime(2023, 4, 1).AddHours(16);
            var records = new List<OperatorWorkRecord>
            {
                // doesn't overlap with later dataset
                
                new() { StartDate = startDate.AddHours(-8), EndDate = endDate.AddHours(-16), Operator = "0" },
                new() { StartDate = startDate.AddHours(-8), EndDate = endDate.AddHours(-8), Operator = "0" },
                new() { StartDate = startDate.AddHours(-8), EndDate = endDate.AddHours(-8), Operator = "0" },
                
                // overlaps and gets into result
                new() { StartDate = startDate, EndDate = endDate.AddMinutes(-7), Operator = "0" },
                new() { StartDate = startDate, EndDate = endDate, Operator = "1" },
                new() { StartDate = startDate, EndDate = endDate, Operator = "1" },
                new() { StartDate = startDate.AddMinutes(10), EndDate = endDate.AddMinutes(10), Operator = "2" },
                new() { StartDate = startDate.AddMinutes(10), EndDate = endDate.AddMinutes(10), Operator = "2" },
                new() { StartDate = endDate.AddHours(-1), EndDate = endDate, Operator = "2" },
                
                // doesn't overlap
                new() { StartDate = endDate, EndDate = endDate, Operator = "2" },
            };
            
            records.ForEach(x => x.Duration = (int)(x.EndDate - x.StartDate).TotalSeconds);
            
            mockProvider.Setup(p => p.GetData()).Returns(records);

            var generator = new ReportGeneratorActiveSessions(mockProvider.Object);

            // Act
            var result = generator.GetReport().ToArray();

            // Assert
            Assert.Single(result);
            Assert.Equal(startDate.Date, result[0].Date);
            Assert.Equal(6, result[0].ActiveSessionsCount);
        }
    }
}