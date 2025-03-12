using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace I2MsgEncoder.Records
{
    class HolidayMapping
    {
        private static bool UseSD;

        public static XElement Generate(bool UseSD)
        {
            HolidayMapping.UseSD = UseSD;
            XElement mapping = new XElement("Mapping");

            mapping.Add(new XAttribute("id", 735700726089676854 + Util.GetCurrentUnixTimestampMillis()));
            mapping.Add(new XAttribute("name", "HolidayMapping"));

            /**
             * These holidays are copied from i2\Managed\Mapping\HolidayMapping.xml
             * Use wikipedia to lookup what days holidays are on
             */

            for (int year = DateTime.Now.Year; year <= DateTime.Now.Year + 1; year++)
            {
                var firstDayOfYear = new DateTime(year, 1, 1);

                // New Years Day (Jan 1st)
                var newYearsDay = new DateTime(year, 1, 1);
                mapping.Add(Holiday("New Year's Day", newYearsDay, "hol_newyears_color", "212 170 69", "237 67 99"));

                // MLK Day (Third Monday of January)
                var MLKDay = FindDay(year, 1, DayOfWeek.Monday, 3);
                mapping.Add(Holiday("MLK Day", MLKDay, "hol_mlkday_color", "0 0 0", "79 52 137"));

                // Groundhog Day (Feb 2nd)
                var groundhogDay = new DateTime(year, 2, 2);
                mapping.Add(Holiday("Groundhog Day", groundhogDay, "hol_groundhogday_color", "0 0 0", "174 133 187"));

                // National Weatherman's Day (Feb 5th)
                var feb5th = new DateTime(year, 2, 5);
                mapping.Add(Holiday("National Weatherman's Day", feb5th, "hol_nationalweathermansday_color", "0 0 0", "0 51 153"));

                // Valentine's Day (Feb 14th)
                var feb14 = new DateTime(year, 2, 14);
                mapping.Add(Holiday("Valentine's Day", feb14, "hol_valentinesday_color", "0 0 0", "221 27 137"));

                // Spring Clock Change (Second Sunday in March)
                var springClock = FindDay(year, 3, DayOfWeek.Sunday, 2);
                mapping.Add(Holiday("Spring Clock Change", springClock, "hol_dls_spring_color", "107 188 121", "164 162 208"));

                // St. Patrick's Day (March 17th)
                var mar17 = new DateTime(year, 3, 17);
                mapping.Add(Holiday("St. Patrick's Day", mar17, "hol_stpatricksday_color", "5 134 11", "146 171 67"));

                // First Day of Spring
                var spring = firstDayOfYear.AddDays(80 + (DateTime.IsLeapYear(year) ? 1 : 0));
                mapping.Add(Holiday("First Day of Spring", spring, "hol_spring_color", "200 129 209", "230 196 35"));

                // April Fool's Day
                var apr1 = new DateTime(year, 4, 1);
                mapping.Add(Holiday("April Fool's Day", apr1, "hol_aprilfoolsday_color", "0 0 0", "252 213 0"));

                // Tax Day
                var apr15 = new DateTime(year, 4, 15);
                mapping.Add(Holiday("Tax Day", apr15, "hol_taxday_color", "0 0 0", "33 56 107"));

                // Easter
                var easter = EasterSunday(DateTime.Now.Year);
                mapping.Add(Holiday("Easter", easter, "hol_easter_color", "232 117 195", "86 199 232"));

                // Earth Day
                var apr22 = new DateTime(year, 4, 22);
                mapping.Add(Holiday("Earth Day", apr22, "hol_earthday_color", "0 100 0", "66 135 68"));

                // Arbor Day (last Friday in April)
                var arborDay = FindDay(year, 4, DayOfWeek.Friday, 4);
                mapping.Add(Holiday("Arbor Day", arborDay, "hol_arborday_color", "64 103 50", "141 211 114"));

                // Cinco de Mayo
                var may5 = new DateTime(year, 5, 5);
                mapping.Add(Holiday("Cinco de Mayo", may5, "hol_cincodemayo_color", "0 0 0", "121 186 116"));

                // Mother's Day (Second Sunday of May)
                var mothersDay = FindDay(year, 5, DayOfWeek.Sunday, 2);
                mapping.Add(Holiday("Mother's Day", mothersDay, "hol_mothersday_color", "248 143 163", "167 145 230"));

                // Memorial Day (last Monday of May)
                var memorialDay = FindDay(year, 5, DayOfWeek.Monday, 4);
                mapping.Add(Holiday("Memorial Day", memorialDay, "hol_memorialday_color", "23 78 213", "214 47 47"));

                // First Day of Tropical Season
                var june1 = new DateTime(year, 6, 1);
                mapping.Add(Holiday("First Day of Tropical Season", june1, "hol_tropicalseason_color", "0 0 0", "136 144 109"));

                // Friday, the 13th
                //var fri13 = ???
                //mapping.Add(Holiday("Friday, the 13th", fri13, "hol_fridaythe13th_color", "53 53 53", "88 89 91"));

                // Father's Day (third Sunday in June)
                var fathersDay = FindDay(year, 6, DayOfWeek.Sunday, 3);
                mapping.Add(Holiday("Father's Day", fathersDay, "hol_fathersday_color", "92 130 11", "3 107 193"));

                // First Day of Summer
                var summer = firstDayOfYear.AddDays(172 + (DateTime.IsLeapYear(year) ? 1 : 0));
                mapping.Add(Holiday("First Day of Summer", summer, "hol_summer_color", "232 113 6", "18 180 169"));

                // Fourth of July
                var jul4 = new DateTime(year, 7, 4);
                mapping.Add(Holiday("Fourth of July", jul4, "hol_fourthofjuly_color", "222 42 22", "3 85 157"));

                // Labor Day (first Monday in September)
                var laborDay = FindDay(year, 9, DayOfWeek.Monday, 1);
                mapping.Add(Holiday("Labor Day", laborDay, "hol_laborday_color", "28 50 88", "24 84 114")); // RGBLogo = 165 45 48

                // Intl Talk Like a Pirate Day
                var sept19 = new DateTime(year, 9, 19);
                mapping.Add(Holiday("Intl Talk Like a Pirate Day", sept19, "hol_inttalklikeapirateday_color", "111 22 35", "57 74 100"));

                // Oktoberfest Begins (Varies...)
                var octFest = new DateTime(2020, 9, 19);
                mapping.Add(Holiday("Oktoberfest Begins", octFest, "hol_oktoberfest_color", "12 137 210", "239 155 57"));

                // First Day of Fall
                var fall = firstDayOfYear.AddDays(266 + (DateTime.IsLeapYear(year) ? 1 : 0));
                mapping.Add(Holiday("First Day of Fall", fall, "hol_fall_color", "167 65 1", "221 132 71"));

                // Columbus Day (2nd Monday in October)
                var columbusDay = FindDay(year, 10, DayOfWeek.Monday, 2);
                mapping.Add(Holiday("Columbus Day", columbusDay, "hol_columbusday_color", "33 168 166", "165 33 33"));

                // Boss's Day (Oct 16th but varies)
                var bossDay = new DateTime(year, 9, 16);
                mapping.Add(Holiday("Boss's Day", bossDay, "hol_bossday_color", "59 136 156", "219 193 83"));

                // Halloween
                var oct31 = new DateTime(year, 10, 31);
                mapping.Add(Holiday("Halloween", oct31, "hol_halloween_color", "215 159 0", "239 105 38"));

                // Fall Clock Change (1st Sunday in November)
                var fallClock = FindDay(year, 11, DayOfWeek.Sunday, 1);
                mapping.Add(Holiday("Fall Clock Change", fallClock, "hol_dlsfall_color", "179 173 0", "181 121 24"));

                // Election Day (Varies...)
                var electionDay = new DateTime(2019, 11, 5);
                mapping.Add(Holiday("Election Day", electionDay, "hol_electionday_color", "0 28 100", "234 61 61"));

                // Veteran’s Day
                var nov11 = new DateTime(year, 11, 11);
                mapping.Add(Holiday("Veteran’s Day", nov11, "hol_veteransday_color", "138 48 48", "49 158 219"));

                // Thanksgiving (fourth Thursday of November)
                var thanksgiving = FindDay(year, 11, DayOfWeek.Thursday, 4);
                mapping.Add(Holiday("Thanksgiving", thanksgiving, "hol_thanksgiving_color", "184 84 35", "96 45 32"));

                // Black Friday
                var blackFriday = thanksgiving.AddDays(1);
                mapping.Add(Holiday("Black Friday", blackFriday, "hol_blackfriday_color", "153 57 61", "48 48 48"));

                // Cyber Monday
                var cyberMonday = thanksgiving.AddDays(4);
                mapping.Add(Holiday("Cyber Monday", cyberMonday, "hol_cybermonday_color", "124 157 104", "66 103 127"));

                // First Day of Hanukkah (Varies...)
                var hanukkah = new DateTime(2019, 12, 23);
                mapping.Add(Holiday("First Day of Hanukkah", hanukkah, "hol_hanukkah_color", "222 173 57", "36 66 109"));

                // First Day of Winter 
                var winter = firstDayOfYear.AddDays(355 + (DateTime.IsLeapYear(year) ? 1 : 0));
                mapping.Add(Holiday("First Day of Winter", winter, "hol_winter_color", "14 16 93", "63 167 178"));

                // Christmas Eve
                var dec24 = new DateTime(year, 12, 24);
                mapping.Add(Holiday("Christmas Eve", dec24, "hol_christmas_color", "190 49 42", "35 102 53"));

                // Christmas
                var dec25 = new DateTime(year, 12, 25);
                mapping.Add(Holiday("Christmas", dec25, "hol_christmas_color", "190 49 42", "35 102 53"));

                // New Year's Eve
                var dec31 = new DateTime(year, 12, 31);
                mapping.Add(Holiday("New Year's Eve", dec31, "hol_newyears_color", "212 170 69", "237 67 99"));
            }

            return mapping;
        }

        private static XElement Holiday(string name, DateTime date, string icon, string heroColor, string iconColor)
        {
            string IconPrefix = HolidayMapping.UseSD ? @"domesticSD\AdSys\Holidays\Icons\" : @"domestic\AdSys\Holidays\Icons\";
            return new XElement("Holiday",
                new XAttribute("date", date.ToString("yyyyMMdd")),
                new XElement("Name", name),
                new XElement("Date", date.ToString(@"MM\/dd\/yyyy")),
                new XElement("Icon", IconPrefix + icon),
                new XElement("RGBHero", heroColor),
                new XElement("RGBLogo", heroColor),
                new XElement("RGBIcon", iconColor)
            );
        }

        // https://stackoverflow.com/a/2510411/1385710
        public static DateTime EasterSunday(int year)
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }

        /**
         * Stolen from https://github.com/tinohager/Nager.Date/blob/master/Src/Nager.Date/DateSystem.cs
         */

        /// <summary>
        /// Find the latest weekday for example monday in a month
        /// </summary>
        /// <param name="year">The year</param>
        /// <param name="month">The month</param>
        /// <param name="day">The name of the day</param>
        /// <returns></returns>
        public static DateTime FindLastDay(int year, int month, DayOfWeek day)
        {
            var resultedDay = FindDay(year, month, day, 5);
            if (resultedDay == DateTime.MinValue)
            {
                resultedDay = FindDay(year, month, day, 4);
            }

            return resultedDay;
        }

        /// <summary>
        /// Find the next weekday for example monday from a specific date
        /// </summary>
        /// <param name="year">The year</param>
        /// <param name="month">The month</param>
        /// <param name="day">The day</param>
        /// <param name="dayOfWeek">The name of the day</param>
        /// <returns></returns>
        public static DateTime FindDay(int year, int month, int day, DayOfWeek dayOfWeek)
        {
            var calculationDay = new DateTime(year, month, day);

            if ((int)dayOfWeek >= (int)calculationDay.DayOfWeek)
            {
                var daysNeeded = (int)dayOfWeek - (int)calculationDay.DayOfWeek;
                return calculationDay.AddDays(daysNeeded);
            }
            else
            {
                var daysNeeded = (int)dayOfWeek - (int)calculationDay.DayOfWeek;
                return calculationDay.AddDays(daysNeeded + 7);
            }
        }

        /// <summary>
        /// Find the next weekday for example monday from a specific date
        /// </summary>
        /// <param name="date">The search date</param>
        /// <param name="dayOfWeek">The name of the day</param>
        /// <returns></returns>
        public static DateTime FindDay(DateTime date, DayOfWeek dayOfWeek)
        {
            return FindDay(date.Year, date.Month, date.Day, dayOfWeek);
        }

        /// <summary>
        /// Find a day between two dates
        /// </summary>
        /// <param name="yearStart">The start year</param>
        /// <param name="monthStart">The start month</param>
        /// <param name="dayStart">The start day</param>
        /// <param name="yearEnd">The end year</param>
        /// <param name="monthEnd">The end month</param>
        /// <param name="dayEnd">The end day</param>
        /// <param name="dayOfWeek">The name of the day</param>
        /// <returns></returns>
        public static DateTime FindDayBetween(int yearStart, int monthStart, int dayStart, int yearEnd, int monthEnd, int dayEnd, DayOfWeek dayOfWeek)
        {
            DateTime startDay = new DateTime(yearStart, monthStart, dayStart);
            DateTime endDay = new DateTime(yearEnd, monthEnd, dayEnd);
            TimeSpan diff = endDay - startDay;
            int days = diff.Days;
            for (var i = 0; i <= days; i++)
            {
                DateTime specificDayDate = startDay.AddDays(i);
                if (specificDayDate.DayOfWeek == dayOfWeek)
                {
                    return specificDayDate;
                }

            }
            return startDay;
        }

        /// <summary>
        /// Find a day between two dates
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="dayOfWeek">The name of the day</param>
        /// <returns></returns>
        public static DateTime FindDayBetween(DateTime startDate, DateTime endDate, DayOfWeek dayOfWeek)
        {
            return FindDayBetween(startDate.Year, startDate.Month, startDate.Day, endDate.Year, endDate.Month, endDate.Day, dayOfWeek);
        }

        /// <summary>
        /// Find the next weekday for example monday before a specific date
        /// </summary>
        /// <param name="year">The year</param>
        /// <param name="month">The month</param>
        /// <param name="day">The day</param>
        /// <param name="dayOfWeek">The name of the day</param>
        /// <returns></returns>
        public static DateTime FindDayBefore(int year, int month, int day, DayOfWeek dayOfWeek)
        {
            var calculationDay = new DateTime(year, month, day);

            if ((int)dayOfWeek < (int)calculationDay.DayOfWeek)
            {
                var daysSubtract = (int)calculationDay.DayOfWeek - (int)dayOfWeek;
                return calculationDay.AddDays(-daysSubtract);
            }
            else
            {
                var daysSubtract = (int)dayOfWeek - (int)calculationDay.DayOfWeek;
                return calculationDay.AddDays(daysSubtract - 7);
            }
        }

        /// <summary>
        /// Find the next weekday for example monday before a specific date
        /// </summary>
        /// <param name="date">The date where the search starts</param>
        /// <param name="dayOfWeek">The name of the day</param>
        /// <returns></returns>
        public static DateTime FindDayBefore(DateTime date, DayOfWeek dayOfWeek)
        {
            return FindDayBefore(date.Year, date.Month, date.Day, dayOfWeek);
        }

        /// <summary>
        /// Find for example the 3th monday in a month
        /// </summary>
        /// <param name="year">The year</param>
        /// <param name="month">The month</param>
        /// <param name="day">The day</param>
        /// <param name="occurrence"></param>
        /// <returns></returns>
        public static DateTime FindDay(int year, int month, DayOfWeek day, int occurrence)
        {
            if (occurrence == 0 || occurrence > 5)
            {
                throw new Exception("Occurance is invalid");
            }

            var firstDayOfMonth = new DateTime(year, month, 1);

            //Substract first day of the month with the required day of the week
            var daysNeeded = (int)day - (int)firstDayOfMonth.DayOfWeek;

            //if it is less than zero we need to get the next week day (add 7 days)
            if (daysNeeded < 0)
            {
                daysNeeded = daysNeeded + 7;
            }

            //DayOfWeek is zero index based; multiply by the Occurance to get the day
            var resultedDay = (daysNeeded + 1) + (7 * (occurrence - 1));

            if (resultedDay > DateTime.DaysInMonth(year, month))
            {
                return DateTime.MinValue;
            }

            return new DateTime(year, month, resultedDay);
        }


    }
}
