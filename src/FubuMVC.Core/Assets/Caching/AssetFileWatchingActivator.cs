using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;

namespace FubuMVC.Core.Assets.Caching
{
    public class AssetFileWatchingActivator : IActivator
    {
        private readonly IAssetFileWatcher _watcher;

        public AssetFileWatchingActivator(IAssetFileWatcher watcher)
        {
            _watcher = watcher;
        }

        public void Activate(IEnumerable<IBottleInfo> packages, IBottleLog log)
        {
            _watcher.StartWatchingAll();
        }
    }
}