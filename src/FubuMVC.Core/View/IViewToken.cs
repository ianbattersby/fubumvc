using System;
using FubuCore;
using FubuMVC.Core.Registration.Nodes;

namespace FubuMVC.Core.View
{
    /// <summary>
    ///   A handle to a view (e.g. a Webforms or Spark View)
    /// </summary>
    public interface IViewToken
    {
        // Eliminate?
        Type ViewType { get; }
        Type ViewModel { get; }

        // Convert to method
        string Name();

        string Namespace { get; }

        [MarkedForTermination("Die.")]
        BehaviorNode ToBehavioralNode();
    }
}