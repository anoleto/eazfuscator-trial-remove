using System;
using System.Linq;

namespace eaztrialremove
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ConfigHandler : Attribute
    {
        public string[] Arg { get; }
        public string Desc { get; }

        public ConfigHandler(string desc, params string[] args) { Arg = args.Select(arg => $"--{arg}").ToArray(); Desc = desc; }
    }

}
