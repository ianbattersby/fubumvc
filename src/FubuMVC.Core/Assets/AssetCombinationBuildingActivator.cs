using System;
using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuMVC.Core.Assets.Combination;
using FubuMVC.Core.Assets.Files;
using FubuMVC.Core.Bootstrapping;
using FubuCore;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.Core.Runtime;
using System.Linq;

namespace FubuMVC.Core.Assets
{
    public class AssetCombinationBuildingActivator : IActivator
    {
        private readonly IContainerFacility _container;
        private readonly AssetGraph _graph;
        private readonly IAssetCombinationCache _cache;
        private readonly IAssetPipeline _pipeline;

        public AssetCombinationBuildingActivator(IContainerFacility container, AssetGraph graph, IAssetCombinationCache cache, IAssetPipeline pipeline)
        {
            _container = container;
            _graph = graph;
            _cache = cache;
            _pipeline = pipeline;
        }

        public void Activate(IEnumerable<IBottleInfo> packages, IBottleLog log)
        {
            _graph.PolicyTypes.Each(type =>
            {
                if (type.CanBeCastTo<IAssetPolicy>())
                {
                    log.Trace("Registering {0} as an IAssetPolicy", type.FullName);
                    _container.Inject(typeof(IAssetPolicy), type);
                }

                if (type.CanBeCastTo<ICombinationPolicy>())
                {
                    log.Trace("Registering {0} as an ICombinationPolicy", type.FullName);
                    _container.Inject(typeof(ICombinationPolicy), type);
                }
            });

            _graph.ForCombinations((name, assetNames) =>
            {
                var mimeType = MimeType.MimeTypeByFileName(assetNames.First());
                _cache.AddFilesToCandidate(mimeType, name, assetNames.Select(x => _pipeline.Find(x)));
            });

        }
    }
}