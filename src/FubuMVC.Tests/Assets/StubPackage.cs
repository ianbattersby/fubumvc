using System;
using System.Collections.Generic;
using System.IO;
using Bottles;
using Bottles.PackageLoaders.Assemblies;
using FubuCore.Util;

namespace FubuMVC.Tests.Assets
{
    public class StubPackage : IPackageInfo
    {
        private readonly Cache<string, string> _folderNames = new Cache<string, string>();
        private readonly string _name;

        public StubPackage(string name)
        {
            _name = name;
        }

        public Action<IAssemblyRegistration> LoadingAssemblies { get; set; }

        public IEnumerable<Dependency> GetDependencies()
        {
            yield break;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Role { get; set; }

        public void LoadAssemblies(IAssemblyRegistration loader)
        {
            LoadingAssemblies(loader);
        }

        public void ForFolder(string folderName, Action<string> onFound)
        {
            _folderNames.WithValue(folderName, onFound);
        }

        public void ForData(string searchPattern, Action<string, Stream> dataCallback)
        {
            throw new NotImplementedException();
        }

        public void RegisterFolder(string folderAlias, string folderName)
        {
            _folderNames[folderAlias] = folderName;
        }
    }
}