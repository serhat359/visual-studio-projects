using System;
using System.IO;
using System.Reflection;

namespace GameProject
{
    static class Extensions
    {
        private static string projectPath = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.FullName;

        public static string GetPath(string filename)
        {
            string path = Path.Combine(projectPath, filename);
            return path;
        }

        public static long GetMicroSeconds()
        {
            return DateTime.Now.Ticks / 10;
        }
    }
}
