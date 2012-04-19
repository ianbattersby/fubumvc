using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bottles;
using Bottles.Diagnostics;
using Bottles.BottleLoaders.Assemblies;

namespace FubuMVC.Core
{
    public class FubuModuleAttributePackageLoader : IBottleLoader
    {
        public IEnumerable<IBottleInfo> Load(IBottleLog log)
        {
            var list = new List<string>{AppDomain.CurrentDomain.BaseDirectory};

            string binPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            if (Directory.Exists(binPath))
            {
                list.Add(binPath);
            }

            list.Each(x =>
            {
                log.Trace("Looking for assemblies marked with the [FubuModule] attribute in " + x);
            });

            return list.SelectMany(
                x =>
                AssembliesFromPath(x, assem => assem.GetCustomAttributes(typeof (FubuModuleAttribute), false).Any()))
                .Select(AssemblyBottleInfo.CreateFor);
        }

        // TODO -- this is so common here and in FubuMVC, just get something into FubuCore
        public static IEnumerable<Assembly> AssembliesFromPath(string path, Predicate<Assembly> assemblyFilter)
        {


            var assemblyPaths = Directory.GetFiles(path)
                .Where(file =>
                       Path.GetExtension(file).Equals(
                           ".exe",
                           StringComparison.OrdinalIgnoreCase)
                       ||
                       Path.GetExtension(file).Equals(
                           ".dll",
                           StringComparison.OrdinalIgnoreCase));

            foreach (string assemblyPath in assemblyPaths)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.LoadFrom(assemblyPath);
                }
                catch
                {
                }

                if (assembly != null && assemblyFilter(assembly))
                {
                    yield return assembly;
                }
            }
        }
    }
}