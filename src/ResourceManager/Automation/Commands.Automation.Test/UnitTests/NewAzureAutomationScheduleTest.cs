// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.Automation.Cmdlet;
using Microsoft.Azure.Commands.Automation.Common;
using Microsoft.Azure.Commands.Automation.Model;
using Microsoft.WindowsAzure.Commands.Common.Test.Mocks;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Moq;
using System;
using System.Linq;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Microsoft.Azure.Commands.ResourceManager.Automation.Test.UnitTests
{
    public class NewAzureAutomationScheduleTest : RMTestBase
    {
        private Mock<IAutomationClient> mockAutomationClient;

        private MockCommandRuntime mockCommandRuntime;

        private NewAzureAutomationSchedule cmdlet;

        public NewAzureAutomationScheduleTest()
        {
            mockAutomationClient = new Mock<IAutomationClient>();
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new NewAzureAutomationSchedule
            {
                AutomationClient = mockAutomationClient.Object,
                CommandRuntime = mockCommandRuntime
            };
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureAutomationScheduleByOneTimeSuccessfull()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string scheduleName = "schedule";

            mockAutomationClient.Setup(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()));

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = scheduleName;
            cmdlet.StartTime = DateTimeOffset.Now;
            cmdlet.OneTime = true;
            cmdlet.SetParameterSet(AutomationCmdletParameterSets.ByOneTime);
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient
                .Verify(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureAutomationScheduleByDailySuccessfull()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string scheduleName = "schedule";
            byte dayInterval = 1;

            mockAutomationClient.Setup(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()));

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = scheduleName;
            cmdlet.StartTime = DateTimeOffset.Now;
            cmdlet.DayInterval = dayInterval;
            cmdlet.SetParameterSet(AutomationCmdletParameterSets.ByDaily);
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient
                .Verify(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureAutomationScheduleByHourlySuccessfull()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string scheduleName = "schedule";
            byte hourInterval = 1;

            mockAutomationClient.Setup(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()));

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = scheduleName;
            cmdlet.StartTime = DateTimeOffset.Now;
            cmdlet.HourInterval = hourInterval;
            cmdlet.SetParameterSet(AutomationCmdletParameterSets.ByHourly);
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient
                .Verify(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureAutomationScheduleByDailyWithDefaultExpiryTimeDayIntervalSuccessfull()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string scheduleName = "schedule";
            byte dayInterval = 1;

            mockAutomationClient
                .Setup(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()))
                .Returns((string a, string b, Schedule s) => s);

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = scheduleName;
            cmdlet.StartTime = DateTimeOffset.Now;
            cmdlet.DayInterval = dayInterval;
            cmdlet.SetParameterSet(AutomationCmdletParameterSets.ByDaily);
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient
                .Verify(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()), Times.Once());

            Assert.AreEqual(1, ((MockCommandRuntime)cmdlet.CommandRuntime).OutputPipeline.Count);
            var schedule = (Schedule)((MockCommandRuntime)cmdlet.CommandRuntime)
                .OutputPipeline
                .FirstOrDefault();
            Assert.IsNotNull(schedule);
            Assert.AreEqual(scheduleName, schedule.Name, "Schedule name is unexpectedly {0}", schedule.Name);

            // Test for default values
            Assert.AreEqual(
                Constants.DefaultScheduleExpiryTime,
                schedule.ExpiryTime,
                "Expiry time is unexpectedly {0}",
                schedule.ExpiryTime);
            Assert.AreEqual(
                dayInterval,
                schedule.Interval,
                "Day Interval is unexpectedly {0}",
                schedule.Interval);
            Assert.AreEqual(
                ScheduleFrequency.Day,
                schedule.Frequency,
                "Day Frequency is unexpectedly {0}",
                schedule.Frequency);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureAutomationScheduleByHourlyWithDefaultExpiryTimeDayIntervalSuccessfull()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string scheduleName = "schedule";
            byte hourInterval = 1;

            mockAutomationClient
                .Setup(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()))
                .Returns((string a, string b, Schedule s) => s);

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = scheduleName;
            cmdlet.StartTime = DateTimeOffset.Now;
            cmdlet.HourInterval = hourInterval;
            cmdlet.SetParameterSet(AutomationCmdletParameterSets.ByHourly);
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient
                .Verify(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()), Times.Once());

            Assert.AreEqual(1, ((MockCommandRuntime)cmdlet.CommandRuntime).OutputPipeline.Count);
            var schedule = (Schedule)((MockCommandRuntime)cmdlet.CommandRuntime)
                .OutputPipeline
                .FirstOrDefault();
            Assert.IsNotNull(schedule);
            Assert.AreEqual(scheduleName, schedule.Name, "Schedule name is unexpectedly {0}", schedule.Name);

            // Test for default values
            Assert.AreEqual(
                Constants.DefaultScheduleExpiryTime,
                schedule.ExpiryTime,
                "Expiry time is unexpectedly {0}",
                schedule.ExpiryTime);
            Assert.AreEqual(
                hourInterval,
                schedule.Interval,
                "Hour Interval is unexpectedly {0}",
                schedule.Interval);
            Assert.AreEqual(
                ScheduleFrequency.Hour,
                schedule.Frequency,
                "Hour Frequency is unexpectedly {0}",
                schedule.Frequency);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureAutomationScheduleByDailyWithExpiryTimeSuccessfull()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string scheduleName = "schedule";
            byte dayInterval = 1;
            var startTime = DateTimeOffset.Now;
            var expiryTime = startTime.AddDays(10);

            mockAutomationClient
                .Setup(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()))
                .Returns((string a, string b, Schedule s) => s);

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = scheduleName;
            cmdlet.StartTime = startTime;
            cmdlet.ExpiryTime = expiryTime;
            cmdlet.DayInterval = dayInterval;
            cmdlet.SetParameterSet(AutomationCmdletParameterSets.ByDaily);
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient
                .Verify(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()), Times.Once());

            Assert.AreEqual(1, ((MockCommandRuntime)cmdlet.CommandRuntime).OutputPipeline.Count);
            var schedule = (Schedule)((MockCommandRuntime)cmdlet.CommandRuntime)
                .OutputPipeline
                .FirstOrDefault();
            Assert.IsNotNull(schedule);
            Assert.AreEqual(scheduleName, schedule.Name, "Schedule name is unexpectedly {0}", schedule.Name);

            Assert.AreEqual(
                expiryTime,
                schedule.ExpiryTime,
                "Expiry time is unexpectedly {0}",
                schedule.ExpiryTime);
            Assert.AreEqual(
                dayInterval,
                schedule.Interval,
                "Day Interval is unexpectedly {0}",
                schedule.Interval);
            Assert.AreEqual(
                ScheduleFrequency.Day,
                schedule.Frequency,
                "Day Frequency is unexpectedly {0}",
                schedule.Frequency);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void NewAzureAutomationScheduleByHourlyWithExpiryTimeSuccessfull()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string scheduleName = "schedule";
            byte hourInterval = 2;
            var startTime = DateTimeOffset.Now;
            var expiryTime = startTime.AddDays(10);

            mockAutomationClient
                .Setup(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()))
                .Returns((string a, string b, Schedule s) => s);

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = scheduleName;
            cmdlet.StartTime = startTime;
            cmdlet.ExpiryTime = expiryTime;
            cmdlet.HourInterval = hourInterval;
            cmdlet.SetParameterSet(AutomationCmdletParameterSets.ByHourly);
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient
                .Verify(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()), Times.Once());

            Assert.AreEqual(1, ((MockCommandRuntime)cmdlet.CommandRuntime).OutputPipeline.Count);
            var schedule = (Schedule)((MockCommandRuntime)cmdlet.CommandRuntime)
                .OutputPipeline
                .FirstOrDefault();
            Assert.IsNotNull(schedule);
            Assert.AreEqual(scheduleName, schedule.Name, "Schedule name is unexpectedly {0}", schedule.Name);

            // Test for default values
            Assert.AreEqual(
                expiryTime,
                schedule.ExpiryTime,
                "Expiry time is unexpectedly {0}",
                schedule.ExpiryTime);
            Assert.AreEqual(
                hourInterval,
                schedule.Interval,
                "Hour Interval is unexpectedly {0}",
                schedule.Interval);
            Assert.AreEqual(
                ScheduleFrequency.Hour,
                schedule.Frequency,
                "Hour Frequency is unexpectedly {0}",
                schedule.Frequency);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void WeeklyWithTimeZoneSetsTheTimeZoneProperty()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string scheduleName = "schedule";
            byte weekInterval = 2;
            var startTime = DateTimeOffset.Now;
            var expiryTime = startTime.AddDays(10);
            var timeZone = "America/Los_Angeles";

            mockAutomationClient
                .Setup(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()))
                .Returns((string a, string b, Schedule s) => s);

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = scheduleName;
            cmdlet.StartTime = startTime;
            cmdlet.ExpiryTime = expiryTime;
            cmdlet.WeekInterval = weekInterval;
            cmdlet.TimeZone = timeZone;
            cmdlet.SetParameterSet(AutomationCmdletParameterSets.ByWeekly);
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient
                .Verify(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()), Times.Once());

            Assert.AreEqual(1, ((MockCommandRuntime)cmdlet.CommandRuntime).OutputPipeline.Count);
            var schedule = (Schedule)((MockCommandRuntime)cmdlet.CommandRuntime)
                .OutputPipeline
                .FirstOrDefault();
            Assert.IsNotNull(schedule);

            // Test for correct time zone value
            Assert.AreEqual(timeZone, schedule.TimeZone);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void MonthlyDaysOfMonthWithTimeZoneSetsTheTimeZoneProperty()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string scheduleName = "schedule";
            byte monthInterval = 1;
            var startTime = DateTimeOffset.Now;
            var expiryTime = startTime.AddDays(10);
            var timeZone = "America/Los_Angeles";

            mockAutomationClient
                .Setup(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()))
                .Returns((string a, string b, Schedule s) => s);

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = scheduleName;
            cmdlet.StartTime = startTime;
            cmdlet.ExpiryTime = expiryTime;
            cmdlet.MonthInterval = monthInterval;
            cmdlet.TimeZone = timeZone;
            cmdlet.SetParameterSet(AutomationCmdletParameterSets.ByMonthlyDaysOfMonth);
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient
                .Verify(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()), Times.Once());

            Assert.AreEqual(1, ((MockCommandRuntime)cmdlet.CommandRuntime).OutputPipeline.Count);
            var schedule = (Schedule)((MockCommandRuntime)cmdlet.CommandRuntime)
                .OutputPipeline
                .FirstOrDefault();
            Assert.IsNotNull(schedule);

            // Test for correct time zone value
            Assert.AreEqual(timeZone, schedule.TimeZone);
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void MonthlyDayOfWeekWithTimeZoneSetsTheTimeZoneProperty()
        {
            // Setup
            string resourceGroupName = "resourceGroup";
            string accountName = "automation";
            string scheduleName = "schedule";
            byte monthInterval = 1;
            var startTime = DateTimeOffset.Now;
            var expiryTime = startTime.AddDays(10);
            var timeZone = "America/Los_Angeles";

            mockAutomationClient
                .Setup(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()))
                .Returns((string a, string b, Schedule s) => s);

            cmdlet.ResourceGroupName = resourceGroupName;
            cmdlet.AutomationAccountName = accountName;
            cmdlet.Name = scheduleName;
            cmdlet.StartTime = startTime;
            cmdlet.ExpiryTime = expiryTime;
            cmdlet.MonthInterval = monthInterval;
            cmdlet.TimeZone = timeZone;
            cmdlet.SetParameterSet(AutomationCmdletParameterSets.ByMonthlyDayOfWeek);
            cmdlet.ExecuteCmdlet();

            // Assert
            mockAutomationClient
                .Verify(f => f.CreateSchedule(resourceGroupName, accountName, It.IsAny<Schedule>()), Times.Once());

            Assert.AreEqual(1, ((MockCommandRuntime)cmdlet.CommandRuntime).OutputPipeline.Count);
            var schedule = (Schedule)((MockCommandRuntime)cmdlet.CommandRuntime)
                .OutputPipeline
                .FirstOrDefault();
            Assert.IsNotNull(schedule);

            // Test for correct time zone value
            Assert.AreEqual(timeZone, schedule.TimeZone);
        }
    }
}
