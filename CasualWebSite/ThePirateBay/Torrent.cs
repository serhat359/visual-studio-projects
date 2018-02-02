using System;

namespace ThePirateBay
{
    public class Torrent
    {
        public string Name { get; set; }
        public string Magnet { get; set; }
        public string File { get; set; }
        public string Uploaded { get; set; }
        public DateTime UploadDate
        {
            get
            {
                try
                {
                    if (Uploaded.Contains("Today")) // Today 05:11
                    {
                        string timeString = Uploaded.Replace("Today ", "");

                        string[] parts = timeString.Split(':');

                        return DateTime.Today
                            .AddHours(int.Parse(parts[0]))
                            .AddMinutes(int.Parse(parts[1]));
                    }
                    else if (Uploaded.Contains("Y-day")) // Y-day 05:53
                    {
                        string timeString = Uploaded.Replace("Y-day ", "");

                        string[] parts = timeString.Split(':');

                        return DateTime.Today
                            .AddDays(-1)
                            .AddHours(int.Parse(parts[0]))
                            .AddMinutes(int.Parse(parts[1]));
                    }
                    else if (Uploaded.Contains("-")) // 10-03 09:33 or 04-13 2016
                    {
                        string[] parts = Uploaded.Split(' ');

                        string[] monthAndYear = parts[0].Split('-');

                        int month = int.Parse(monthAndYear[0]);
                        int day = int.Parse(monthAndYear[1]);

                        string right = parts[1];

                        if (right.Contains(":")) // 10-03 09:33
                        {
                            int year = DateTime.Today.Year;

                            string[] time = right.Split(':');

                            return new DateTime(year, month, day)
                                .AddHours(int.Parse(time[0]))
                                .AddMinutes(int.Parse(time[1]));
                        }
                        else // 04-13 2016
                        {
                            int year = int.Parse(right);

                            return new DateTime(year, month, day);
                        }
                    }
                    else
                    {
                        throw new FormatException();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public string Size { get; set; }
        public decimal SizeBytes { get; set; }
        public string Uled { get; set; }
        public int Seeds { get; set; }
        public int Leechers { get; set; }
        public int CategoryParent { get; set; }
        public int Category { get; set; }
        public int Comments { get; set; }
        public bool HasCoverImage { get; set; }
        public bool IsTrusted { get; set; }
        public bool IsVip { get; set; }
    }
}
