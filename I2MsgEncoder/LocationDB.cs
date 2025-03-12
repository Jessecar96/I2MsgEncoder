using System;
using System.Data.SQLite;
using System.Data;
using System.IO;
using I2MsgEncoder.Records;
using System.Xml.Serialization;

namespace I2MsgEncoder
{
    class LocationDB
    {
        static private SQLiteConnection connection;

        public static void Connect()
        {
            connection = new SQLiteConnection("Data Source=" + Path.Combine(Util.GetDir(), "Data", "LFRecord"));
            connection.Open();
        }

        public static LFRecord FindLocation(string location)
        {
            if (connection == null)
                Connect();

            var command = new SQLiteCommand("SELECT * FROM datarecords WHERE key = @key", connection);
            command.Parameters.Add("@key", DbType.String);
            command.Parameters["@key"].Value = location;
            var reader = command.ExecuteReader();
            reader.Read();

            XmlSerializer serializer = new XmlSerializer(typeof(LFRecord));
            LFRecord result = null;

            using (TextReader textReader = new StringReader(reader["xml"].ToString()))
            {
                try
                {
                    result = (LFRecord)serializer.Deserialize(textReader);

                }
                catch (Exception e)
                {
                    Log.Warning("Error parsing location XML: {0}", e.Message);
                    Log.Warning(reader["xml"].ToString());
                }
            }

            return result;
        }

    }
}
