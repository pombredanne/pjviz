using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace pjviz
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: TODO");
                return;
            }

            string lockJsonFile = args[0];
            string pkgName = args[1]; // System.Runtime
            string tfm = args[2];
            string dgmlPath = args[3];

            ProjectLockJson pj = JsonConvert.DeserializeObject<ProjectLockJson>(File.ReadAllText(lockJsonFile));

            using (FileStream xmlFile = new FileStream(dgmlPath, FileMode.Create, FileAccess.Write))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                StreamWriter sw = new StreamWriter(xmlFile);
                using (XmlWriter xml = XmlWriter.Create(sw, settings))
                {
                    xml.WriteStartDocument();
                    xml.WriteStartElement("DirectedGraph", "http://schemas.microsoft.com/vs/2009/dgml");

                    xml.WriteStartElement("Nodes");
                    HashSet<string> nuPkgNames = new HashSet<string>();
                    foreach (var targetPair in pj.targets)
                    {
                        string targetName = targetPair.Key;
                        if (targetName != tfm)
                        {
                            continue;
                        }

                        foreach (var nugetPair in targetPair.Value)
                        {
                            string nugetPackageNameAndVersion = nugetPair.Key;
                            NugetPackageDescription ndescription = nugetPair.Value;

                            string[] nuPkgNameParts = nugetPackageNameAndVersion.Split(new char[] { '/' });
                            if (nuPkgNameParts.Length != 2)
                            {
                                Console.WriteLine($"Warning: Improper package name format: {nugetPackageNameAndVersion}");
                                continue;
                            }

                            string nuPkgName = nuPkgNameParts[0];
                            string nuPkgVersion = nuPkgNameParts[1];
                            xml.WriteStartElement("Node");
                            xml.WriteAttributeString("Id", nuPkgName);
                            xml.WriteAttributeString("Label", nuPkgName + "\n" + nuPkgVersion);
                            if (nuPkgName == pkgName)
                            {
                                xml.WriteAttributeString("Background", "#FF008080");
                            }
                            xml.WriteEndElement();
                            nuPkgNames.Add(nuPkgName);

                            if (ndescription.dependencies != null)
                            {
                                foreach (var dep in ndescription.dependencies)
                                {
                                    string dependencyName = dep.Key;
                                    string dependencyVersion = dep.Value;
                                }
                            }
                        }
                    }
                    xml.WriteEndElement();

                    xml.WriteStartElement("Links");
                    foreach (var targetPair in pj.targets)
                    {
                        string targetName = targetPair.Key;
                        if (targetName != tfm)
                        {
                            continue;
                        }

                        foreach (var nugetPair in targetPair.Value)
                        {
                            string nugetPackageNameAndVersion = nugetPair.Key;
                            NugetPackageDescription ndescription = nugetPair.Value;

                            string[] nuPkgNameParts = nugetPackageNameAndVersion.Split(new char[] { '/' });
                            if (nuPkgNameParts.Length != 2)
                            {
                                Console.WriteLine($"Warning: Improper package name format: {nugetPackageNameAndVersion}");
                                continue;
                            }

                            string nuPkgName = nuPkgNameParts[0];
                            string nuPkgVersion = nuPkgNameParts[1];

                            if (ndescription.dependencies != null)
                            {
                                foreach (var dep in ndescription.dependencies)
                                {
                                    string dependencyName = dep.Key;
                                    string dependencyVersion = dep.Value;

                                    if (!nuPkgNames.Contains(dependencyName))
                                    {
                                        Console.WriteLine($"Warning: {nuPkgName} ({nuPkgVersion}) -> [MISSING] {dependencyName} (dependencyVersion)");
                                        return;
                                    }

                                    xml.WriteStartElement("Link");
                                    xml.WriteAttributeString("Source", nuPkgName);
                                    xml.WriteAttributeString("Target", dependencyName);
                                    xml.WriteAttributeString("Label", $"Target version: {dependencyVersion}");
                                    xml.WriteEndElement();
                                }
                            }
                        }
                    }
                    xml.WriteEndElement();
                }
            }
        }

        public static void PrintVisitPJ(ProjectLockJson pj)
        {
            foreach (var targetPair in pj.targets)
            {
                string targetName = targetPair.Key;
                Console.WriteLine($"target: {targetName}:");

                foreach (var nugetPair in targetPair.Value)
                {
                    string nugetPackageNameAndVersion = nugetPair.Key;
                    NugetPackageDescription ndescription = nugetPair.Value;
                    Console.WriteLine($"\tpkg name: {nugetPackageNameAndVersion}");

                    string[] nuPkgNameParts = nugetPackageNameAndVersion.Split(new char[] { '/' });
                    if (nuPkgNameParts.Length != 2)
                    {
                        Console.WriteLine($"Warning: Improper package name format: {nugetPackageNameAndVersion}");
                        continue;
                    }

                    string nuPkgName = nuPkgNameParts[0];
                    string nuPkgVersion = nuPkgNameParts[1];

                    Console.WriteLine($"\t\tname={nuPkgName}");
                    Console.WriteLine($"\t\tver.={nuPkgVersion}");

                    Console.WriteLine("\t\tdependencies:");
                    if (ndescription.dependencies != null)
                    {
                        foreach (var dep in ndescription.dependencies)
                        {
                            string dependencyName = dep.Key;
                            string dependencyVersion = dep.Value;
                            Console.WriteLine($"\t\t\t{dependencyName} : {dependencyVersion}");
                        }
                    }

                    Console.WriteLine("\t\tcompile refs:");
                    if (ndescription.compile != null)
                    {
                        foreach (var compileAssemblies in ndescription.compile)
                        {
                            string refAssemblyName = compileAssemblies.Key;
                            Console.WriteLine($"\t\t\tref name: {refAssemblyName}");
                        }
                    }

                    Console.WriteLine("\t\truntime refs:");
                    if (ndescription.runtime != null)
                    {
                        foreach (var runtimeAssemblies in ndescription.runtime)
                        {
                            string refAssemblyName = runtimeAssemblies.Key;
                            Console.WriteLine($"\t\t\tref name: {refAssemblyName}");
                        }
                    }
                }
            }
        }
    }
}
