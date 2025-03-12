using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I2MsgEncoder
{
    static class Log
    {

        private static string PREFIX_DEBUG = " - DEBUG - ";
        private static string PREFIX_INFO = " - INFO - ";
        private static string PREFIX_WARNING = " - WARNING - ";

        private static Color COLOR_DEBUG = Color.LightSeaGreen;
        private static Color COLOR_INFO = Color.LightSkyBlue;
        private static Color COLOR_WARNING = Color.OrangeRed;

        private static LogLevel level = LogLevel.Info;

        private static string GetDate()
        {
            return DateTime.Now.ToString(@"hh:mm:ss");
        }

        public static void SetLogLevel(string newLevel)
        {
            newLevel = newLevel.ToLower();
            switch (newLevel)
            {
                case "debug":
                    level = LogLevel.Debug;
                    break;

                case "info":
                    level = LogLevel.Info;
                    break;

                case "warning":
                    level = LogLevel.Warning;
                    break;
            }
        }

        // DEBUG

        public static void Debug(string str)
        {
            if (level > LogLevel.Debug) return;
            str = GetDate() + PREFIX_DEBUG + str;
            Console.WriteLine(str.Pastel(COLOR_DEBUG));
        }

        public static void Debug(string str, object arg0)
        {
            if (level > LogLevel.Debug) return;
            str = GetDate() + PREFIX_DEBUG + str;
            Console.WriteLine(str.Pastel(COLOR_DEBUG), arg0);
        }

        public static void Debug(string str, object arg0, object arg1)
        {
            if (level > LogLevel.Debug) return;
            str = GetDate() + PREFIX_DEBUG + str;
            Console.WriteLine(str.Pastel(COLOR_DEBUG), arg0, arg1);
        }

        public static void Debug(string str, object arg0, object arg1, object arg2)
        {
            if (level > LogLevel.Debug) return;
            str = GetDate() + PREFIX_DEBUG + str;
            Console.WriteLine(str.Pastel(COLOR_DEBUG), arg0, arg1, arg2);
        }

        // INFO

        public static void Info(string str)
        {
            if (level > LogLevel.Info) return;
            str = GetDate() + PREFIX_INFO + str;
            Console.WriteLine(str.Pastel(COLOR_INFO));
        }

        public static void Info(string str, object arg0)
        {
            if (level > LogLevel.Info) return;
            str = GetDate() + PREFIX_INFO + str;
            Console.WriteLine(str.Pastel(COLOR_INFO), arg0);
        }

        public static void Info(string str, object arg0, object arg1)
        {
            if (level > LogLevel.Info) return;
            str = GetDate() + PREFIX_INFO + str;
            Console.WriteLine(str.Pastel(COLOR_INFO), arg0, arg1);
        }

        public static void Info(string str, object arg0, object arg1, object arg2)
        {
            if (level > LogLevel.Info) return;
            str = GetDate() + PREFIX_INFO + str;
            Console.WriteLine(str.Pastel(COLOR_INFO), arg0, arg1, arg2);
        }

        // WARNING

        public static void Warning(string str)
        {
            if (level > LogLevel.Warning) return;
            str = GetDate() + PREFIX_WARNING + str;
            Console.WriteLine(str.Pastel(COLOR_WARNING));
        }

        public static void Warning(string str, object arg0)
        {
            if (level > LogLevel.Warning) return;
            str = GetDate() + PREFIX_WARNING + str;
            Console.WriteLine(str.Pastel(COLOR_WARNING), arg0);
        }

        public static void Warning(string str, object arg0, object arg1)
        {
            if (level > LogLevel.Warning) return;
            str = GetDate() + PREFIX_WARNING + str;
            Console.WriteLine(str.Pastel(COLOR_WARNING), arg0, arg1);
        }

    }

    enum LogLevel
    {
        Debug,
        Info,
        Warning
    }
}
