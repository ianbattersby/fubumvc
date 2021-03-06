using System;
using System.Linq.Expressions;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Resources.Conneg;
using NUnit.Framework;
using FubuTestingSupport;
using System.Linq;

namespace FubuMVC.Tests.Registration.Conventions
{
    [TestFixture]
    public class MediaExpressionAndConnegAttachmentPolicyTester
    {
        private FubuRegistry theFubuRegistry;
        private Lazy<BehaviorGraph> theGraph;


        [SetUp]
        public void SetUp()
        {
            theFubuRegistry = new FubuRegistry();
            theFubuRegistry.Actions.IncludeType<Controller1>();

            theGraph = new Lazy<BehaviorGraph>(() => BehaviorGraph.BuildFrom(theFubuRegistry));
        } 

        private BehaviorChain chainFor(Expression<Func<Controller1, object>> expression)
        {
            return theGraph.Value.BehaviorFor(expression);
        }

        [Test]
        public void apply_content_by_action()
        {
            theFubuRegistry.Policies.Add(policy => {
                policy.Where.ResourceTypeIs<ViewModel3>();

                policy.Conneg.ApplyConneg();
            });

            chainFor(x => x.C()).Output.Writers.Any().ShouldBeTrue();
            chainFor(x => x.D()).Output.Writers.Any().ShouldBeTrue();
            chainFor(x => x.E()).Output.Writers.Any().ShouldBeTrue();

        }

        [Test]
        public void apply_content_by_looking_at_a_chain()
        {
            theFubuRegistry.Policies.Add(policy => {
                policy.Where.AnyActionMatches(call => call.Method.Name == "A");
                policy.Conneg.ApplyConneg();
            });

            chainFor(x => x.A()).Output.Writers.Any().ShouldBeTrue();

            // Pretty close to killing this altogether
            //chainFor(x => x.B()).HasConnegOutput().ShouldBeFalse();
            //chainFor(x => x.C()).HasConnegOutput().ShouldBeFalse();
            //chainFor(x => x.D()).HasConnegOutput().ShouldBeFalse();
            //chainFor(x => x.E()).HasConnegOutput().ShouldBeFalse();
        }
    }

    public class ViewModel1
    {
    }

    public class ViewModel2
    {
    }

    public class ViewModel3
    {
    }

    public class ViewModel4
    {
    }

    public class ViewModel5
    {
    }


    public class Controller1
    {
        public ViewModel1 A()
        {
            return null;
        }

        public ViewModel2 B()
        {
            return null;
        }

        public ViewModel3 C()
        {
            return null;
        }

        public ViewModel3 D()
        {
            return null;
        }

        public ViewModel3 E()
        {
            return null;
        }
    }
}