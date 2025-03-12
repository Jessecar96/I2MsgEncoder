using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace I2MsgEncoder.Records
{
    class HolidayBackgrounds
    {
        private static bool UseSD;
        private static XElement mapping;

        public static XElement Generate(bool UseSD)
        {
            HolidayBackgrounds.UseSD = UseSD;
            mapping = new XElement("Events");

            mapping.Add(new XAttribute("Type", "LocalImage"));
            mapping.Add(new XAttribute("FilePath", "{VizDataRoot}/image"));
            mapping.Add(new XAttribute("DefaultFile", (UseSD ? "domesticSD" : "domestic") + "/backgrounds/Enhanced/generic_generic-blur_001"));
            mapping.Add(new XAttribute("StartRandom", "True"));

            int year = DateTime.Now.Year;
            var firstDayOfYear = new DateTime(year, 1, 1);

            // ArborDay
            var arborDay = HolidayMapping.FindDay(year, 4, DayOfWeek.Friday, 4);
            Add(arborDay, "ArborDay", 1, 7);

            // Black_Friday
            var thanksgiving = HolidayMapping.FindDay(year, 11, DayOfWeek.Thursday, 4);
            var blackFriday = thanksgiving.AddDays(1);
            var cyberMonday = thanksgiving.AddDays(4);
            Add(blackFriday, "Black_Friday", 1, 7);

            // Boss Day
            var bossDay = new DateTime(year, 9, 16);
            Add(bossDay, "Boss_Day", 1, 7);

            // Christmas
            var dec25 = new DateTime(year, 12, 25);
            Add(dec25, "Christmas", 1, 7);

            // Columbus_Day
            var columbusDay = HolidayMapping.FindDay(year, 10, DayOfWeek.Monday, 2);
            Add(columbusDay, "Columbus_Day", 1, 7);

            // Cyber_Monday
            Add(cyberMonday, "Cyber_Monday", 1, 7);

            // earth_day
            var apr22 = new DateTime(year, 4, 22);
            Add(apr22, "earth_day", 1, 7);

            // Easter
            var easter = HolidayMapping.EasterSunday(DateTime.Now.Year);
            Add(easter, "Easter", 1, 7);

            // Election_Day (Varies...)
            var electionDay = new DateTime(2019, 11, 5);
            Add(electionDay, "Election_Day", 1, 7);

            // Fall_Time_Change
            var fallClock = HolidayMapping.FindDay(year, 11, DayOfWeek.Sunday, 1);
            Add(fallClock, "Fall_Time_Change", 1, 7);

            // fathers_day
            var fathersDay = HolidayMapping.FindDay(year, 6, DayOfWeek.Sunday, 3);
            Add(fathersDay, "fathers_day", 1, 8);

            var fall = firstDayOfYear.AddDays(266 + (DateTime.IsLeapYear(year) ? 1 : 0));
            Add(fall, "First_Day_of_Fall", 1, 7);

            var winter = firstDayOfYear.AddDays(355 + (DateTime.IsLeapYear(year) ? 1 : 0));
            Add(winter, "First_Day_Of_Winter", 1, 7);

            var spring = firstDayOfYear.AddDays(80 + (DateTime.IsLeapYear(year) ? 1 : 0));
            Add(spring, "first_day_spring", 1, 7);

            var summer = firstDayOfYear.AddDays(172 + (DateTime.IsLeapYear(year) ? 1 : 0));
            Add(summer, "First_dayof_summer", 1, 7);

            var jul4 = new DateTime(year, 7, 4);
            Add(jul4, "fourth_of_july", 1, 10);

            // Friday the 13th??
            //Add(fri13, "friday_the_13th", 1, 7);

            var groundhogDay = new DateTime(year, 2, 2);
            Add(groundhogDay, "GroundhogDay", 1, 11);

            var oct31 = new DateTime(year, 10, 31);
            Add(oct31, "Halloween", 1, 7);

            // Varies
            var hanukkah = new DateTime(2019, 12, 23);
            Add(hanukkah, "Hanukkah", 1, 13);

            var sept19 = new DateTime(year, 9, 19);
            Add(sept19, "Int_Talk_Like_a_Pirate", 1, 7);

            var laborDay = HolidayMapping.FindDay(year, 9, DayOfWeek.Monday, 1);
            Add(laborDay, "Labor_Day", 1, 8);

            var memorialDay = HolidayMapping.FindDay(year, 5, DayOfWeek.Monday, 4);
            Add(memorialDay, "memorial_day", 1, 8);

            var MLKDay = HolidayMapping.FindDay(year, 1, DayOfWeek.Monday, 3);
            Add(MLKDay, "mlk", 1, 9);

            var mothersDay = HolidayMapping.FindDay(year, 5, DayOfWeek.Sunday, 2);
            Add(mothersDay, "mothers_day", 1, 11);

            var newYearsDay = new DateTime(year, 1, 1);
            Add(newYearsDay, "New_Years", 1, 7);

            var newYearsDay2 = new DateTime(year+1, 1, 1);
            Add(newYearsDay2, "New_Years", 1, 7);

            // Varies
            var octFest = new DateTime(2020, 9, 19);
            Add(octFest, "Oktoberfest", 1, 7);

            var springClock = HolidayMapping.FindDay(year, 3, DayOfWeek.Sunday, 2);
            Add(springClock, "spring_clock_change", 1, 7);

            var mar17 = new DateTime(year, 3, 17);
            Add(mar17, "st_patricks_day", 1, 7);

            Add(thanksgiving, "Thanksgiving", 1, 8);

            var feb14 = new DateTime(year, 2, 14);
            Add(feb14, "ValentinesDay", 1, 9);

            var nov11 = new DateTime(year, 11, 11);
            Add(nov11, "Veterans_Day", 1, 9);

            return mapping;
        }

        private static void Add(DateTime date, string image, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                Add(date, image + "_00" + i);
            }
        }

        private static void Add(DateTime date, string image)
        {
            var e = new XElement("Event");
            e.Add(new XAttribute("StartDate", date.ToString(@"MM\/dd\/yyyy")));
            e.Add(new XAttribute("EndDate", date.ToString(@"MM\/dd\/yyyy")));
            e.Add(new XElement("Text", (UseSD ? "domesticSD" : "domestic") + @"\AdSys\Holidays\Backgrounds\" + image));
            mapping.Add(e);
        }
    }
}
