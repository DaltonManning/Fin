using System;
using System.IO;

namespace Fin
{
    internal static class Paths
    {
        public static string Root { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");

        public static string ProjectTree  => Ensure(Path.Combine(Root, "project"));
        public static string Diagrams     => Ensure(Path.Combine(Root, "diagrams"));
        public static string Hardware     => Ensure(Path.Combine(Root, "hardware"));
        public static string Applications => Ensure(Path.Combine(Root, "applications"));

        private static string Ensure(string path)
        {
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
