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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Azure.Commands.Automation.Cmdlet;
using Microsoft.Azure.Commands.Automation.Common;
using System;
using Microsoft.Azure.Management.Automation.Models;

namespace Microsoft.Azure.Commands.Automation.Model
{
    /// <summary>
    /// The Schedule.
    /// </summary>
    public class Schedule : BaseProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Schedule"/> class.
        /// </summary>
        /// <param name="resourceGroupName">
        /// The resource group name.
        /// </param>
        /// <param name="automationAccountName">
        /// The automation account name.
        /// </param> 
        /// <param name="schedule">
        /// The schedule.
        /// </param>
        public Schedule(string resourceGroupName, string automationAccountName, Management.Automation.Models.Schedule schedule)
        {
            Requires.Argument("schedule", schedule).NotNull();

            ResourceGroupName = resourceGroupName;
            AutomationAccountName = automationAccountName;
            Name = schedule.Name;
            Description = schedule.Properties.Description;
            StartTime = AdjustOffset(schedule.Properties.StartTime, schedule.Properties.StartTimeOffsetMinutes);
            ExpiryTime = AdjustOffset(schedule.Properties.ExpiryTime, schedule.Properties.ExpiryTimeOffsetMinutes);
            CreationTime = schedule.Properties.CreationTime.ToLocalTime();
            LastModifiedTime = schedule.Properties.LastModifiedTime.ToLocalTime();
            IsEnabled = schedule.Properties.IsEnabled;
            NextRun = AdjustOffset(schedule.Properties.NextRun, schedule.Properties.NextRunOffsetMinutes);
            Interval = schedule.Properties.Interval ?? Interval;
            Frequency = (ScheduleFrequency)Enum.Parse(typeof(ScheduleFrequency), schedule.Properties.Frequency, true);
            WeeklyScheduleOptions = CreateWeeklyScheduleOptions(schedule);
            MonthlyScheduleOptions = CreateMonthlyScheduleOptions(schedule);
            TimeZone = schedule.Properties.TimeZone;
        }

        #region Public Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="Schedule"/> class.
        /// </summary>
        public Schedule()
        {
        }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public DateTimeOffset StartTime { get; set; }

        /// <summary>
        /// Gets or sets the expiry time.
        /// </summary>
        public DateTimeOffset ExpiryTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the next run.
        /// </summary>
        public DateTimeOffset? NextRun { get; set; }

        /// <summary>
        /// Gets or sets the schedule interval.
        /// </summary>
        public byte? Interval { get; set; }

        /// <summary>
        /// Gets or sets the schedule frequency.
        /// </summary>
        public ScheduleFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the monthly schedule options.
        /// </summary>
        public MonthlyScheduleOptions MonthlyScheduleOptions { get; set; }

        /// <summary>
        /// Gets or sets the weekly schedule options.
        /// </summary>
        public WeeklyScheduleOptions WeeklyScheduleOptions { get; set; }

        /// <summary>
        /// The create advanced schedule.
        /// </summary>
        /// <returns>
        /// The <see cref="AdvancedSchedule"/>.
        /// </returns>
        public AdvancedSchedule GetAdvancedSchedule()
        {
            if (AdvancedScheduleIsNull(this))
            {
                return null;
            }

            var advancedSchedule = new AdvancedSchedule
            {
                WeekDays = WeeklyScheduleOptions == null ? null : WeeklyScheduleOptions.DaysOfWeek,
                MonthDays = MonthlyScheduleOptions == null || MonthlyScheduleOptions.DaysOfMonth == null ? null : MonthlyScheduleOptions.DaysOfMonth.Select(v => Convert.ToInt32(v)).ToList(),
                MonthlyOccurrences = MonthlyScheduleOptions == null || MonthlyScheduleOptions.DayOfWeek == null
                    ? null
                    : new[]
                    {
                        new AdvancedScheduleMonthlyOccurrence
                        {
                            Day = MonthlyScheduleOptions.DayOfWeek.Day,
                            Occurrence = GetDayOfWeekOccurrence(MonthlyScheduleOptions.DayOfWeek.Occurrence)
                        }
                    }
            };

            return advancedSchedule;
        }

        /// <summary>
        /// Gets or sets the schedule time zone.
        /// </summary>
        public string TimeZone { get; set; }

        #endregion Public Properties

        #region Private Methods

        /// <summary>
        /// The is null or empty list.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsNullOrEmptyList<T>(IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// The is monthly occurrence null.
        /// </summary>
        /// <param name="advancedSchedule">
        /// The advanced schedule.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsMonthlyOccurrenceNull(AdvancedSchedule advancedSchedule)
        {
            return advancedSchedule == null || IsNullOrEmptyList(advancedSchedule.MonthlyOccurrences);
        }

        /// <summary>
        /// The advanced schedule is null.
        /// </summary>
        /// <param name="schedule">
        /// The schedule.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool AdvancedScheduleIsNull(Schedule schedule)
        {
            return schedule.WeeklyScheduleOptions == null
                && schedule.MonthlyScheduleOptions == null;
        }

        /// <summary>
        /// The get day of week occurrence.
        /// </summary>
        /// <param name="dayOfWeekOccurrence">
        /// The day of week occurrence.
        /// </param>
        /// <returns>
        /// The <see cref="int?"/>.
        /// </returns>
        private int? GetDayOfWeekOccurrence(string dayOfWeekOccurrence)
        {
            if (string.IsNullOrWhiteSpace(dayOfWeekOccurrence))
            {
                return null;
            }

            return Convert.ToInt32(Enum.Parse(typeof(DayOfWeekOccurrence), dayOfWeekOccurrence));
        }

        /// <summary>
        /// The create weekly schedule options.
        /// </summary>
        /// <param name="schedule">
        /// The schedule.
        /// </param>
        /// <returns>
        /// The <see cref="WeeklyScheduleOptions"/>.
        /// </returns>
        private WeeklyScheduleOptions CreateWeeklyScheduleOptions(Management.Automation.Models.Schedule schedule)
        {
            return schedule.Properties.AdvancedSchedule == null
                ? null
                : new WeeklyScheduleOptions
                {
                    DaysOfWeek = schedule.Properties.AdvancedSchedule.WeekDays
                };
        }

        /// <summary>
        /// The create monthly schedule options.
        /// </summary>
        /// <param name="schedule">
        /// The schedule.
        /// </param>
        /// <returns>
        /// The <see cref="MonthlyScheduleOptions"/>.
        /// </returns>
        private MonthlyScheduleOptions CreateMonthlyScheduleOptions(
            Management.Automation.Models.Schedule schedule)
        {
            return schedule.Properties.AdvancedSchedule == null
                || schedule.Properties.AdvancedSchedule.MonthDays == null && schedule.Properties.AdvancedSchedule.MonthlyOccurrences == null
                ? null
                : new MonthlyScheduleOptions
                {
                    DaysOfMonth = GetDaysOfMonth(schedule.Properties.AdvancedSchedule.MonthDays),
                    DayOfWeek = IsMonthlyOccurrenceNull(schedule.Properties.AdvancedSchedule)
                        ? null
                        : new DayOfWeek
                        {
                            Day = schedule.Properties.AdvancedSchedule.MonthlyOccurrences.First().Day,
                            Occurrence = GetDayOfWeekOccurrence(schedule.Properties.AdvancedSchedule.MonthlyOccurrences.First().Occurrence)
                        }
                };
        }

        /// <summary>
        /// The get day of week occurrence.
        /// </summary>
        /// <param name="dayOfWeekOccurrence">
        /// The day of week occurrence.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string GetDayOfWeekOccurrence(int? dayOfWeekOccurrence)
        {
            return dayOfWeekOccurrence.HasValue
                ? Enum.GetName(typeof(DayOfWeekOccurrence), dayOfWeekOccurrence)
                : null;
        }

        /// <summary>
        /// The get days of month.
        /// </summary>
        /// <param name="daysOfMonth">
        /// The days of month.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        private IList<DaysOfMonth> GetDaysOfMonth(IList<int> daysOfMonth)
        {
            return daysOfMonth.Select(value => (DaysOfMonth)value).ToList();
        }

        private static DateTimeOffset? AdjustOffset(DateTimeOffset? dateTimeOffset, double offsetMinutes)
        {
            if (dateTimeOffset.HasValue)
            {
                return AdjustOffset(dateTimeOffset.Value, offsetMinutes);
            }

            return null;
        }

        private static DateTimeOffset AdjustOffset(DateTimeOffset dateTimeOffset, double offsetMinutes)
        {
            var timeSpan = TimeSpan.FromMinutes(offsetMinutes);
            return dateTimeOffset.ToOffset(timeSpan);
        }

        #endregion Private Methods
    }
}
