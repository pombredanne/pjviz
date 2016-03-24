using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pjviz
{
    public class ProjectLockJson
    {
        /// <summary>
        /// target name to nuget package name to nuget package description
        /// </summary>
        public Dictionary<string, Dictionary<string, NugetPackageDescription>> targets;
    }

    public class NugetPackageDescription
    {
        string type;

        /// <summary>
        /// name of nuget package to its version
        /// </summary>
        public Dictionary<string, string> dependencies;

        /// <summary>
        /// assembly type to dll path
        /// </summary>
        public Dictionary<string, ReferenceDescription> compile, runtime;
    }

    // empty?
    public class ReferenceDescription
    {
    }
}
