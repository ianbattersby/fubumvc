using System;
using System.Net;
using FubuMVC.Core.Http;
using FubuMVC.Core.Http.Headers;
using FubuMVC.Core.Resources.Conneg.New;
using FubuMVC.Core.Runtime;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Linq;

namespace FubuMVC.Tests.NewConneg
{
    [TestFixture]
    public class when_there_are_outputs_that_would_write_headers : InteractionContext<OutputBehavior<Address>>
    {
        protected override void beforeEach()
        {
            var headers1 = new HttpHeaderValues();
            headers1["a"] = "1";
            headers1["b"] = "2";

            var headers2 = new HttpHeaderValues();
            headers2["c"] = "3";
            headers2["d"] = "4";

            MockFor<IFubuRequest>().Stub(x => x.Find<IHaveHeaders>()).Return(new IHaveHeaders[] { headers1, headers2 });

            ClassUnderTest.WriteHeaders();
        }


        [Test]
        public void should_write_all_possible_headers()
        {
            MockFor<IOutputWriter>().AssertWasCalled(x => x.AppendHeader("a", "1"));
            MockFor<IOutputWriter>().AssertWasCalled(x => x.AppendHeader("b", "2"));
            MockFor<IOutputWriter>().AssertWasCalled(x => x.AppendHeader("c", "3"));
            MockFor<IOutputWriter>().AssertWasCalled(x => x.AppendHeader("d", "4"));
        }
    }

    [TestFixture]
    public class when_selecting_a_media_when_only_some_runtime_matches : OutputBehaviorContext
    {
        protected override void theContextIs()
        {
            mediaMimetypesAre(0, MimeType.Css);
            mediaMimetypesAre(1, MimeType.Css);
            mediaMimetypesAre(2, MimeType.Css);
            mediaMimetypesAre(3, MimeType.Xml);
            mediaMimetypesAre(4, MimeType.Xml);
        }

        [Test]
        public void select_by_matches_and_mimetype()
        {
            mediaDoesNotMatch(0);
            mediaDoesNotMatch(1);
            mediaMatches(2);  

            theCurrentMimeType.AcceptTypes = new MimeTypeList(MimeType.Css);

            theSelectedMediaShouldBe(2);
        }

        [Test]
        public void select_by_matches_and_mimetype_2()
        {
            mediaMatches(0);
            mediaDoesNotMatch(1);
            mediaMatches(2);

            theCurrentMimeType.AcceptTypes = new MimeTypeList(MimeType.Css);

            theSelectedMediaShouldBe(0);
        }

        [Test]
        public void select_by_matches_and_mimetype_3()
        {
            mediaDoesNotMatch(3);
            mediaMatches(4);

            theCurrentMimeType.AcceptTypes = new MimeTypeList(MimeType.Xml);

            theSelectedMediaShouldBe(4);
        }

        [Test]
        public void select_any()
        {
            theCurrentMimeType.AcceptTypes = new MimeTypeList(MimeType.Text, MimeType.Any);

            mediaMatches(3);

            theSelectedMediaShouldBe(3);
        }
    }



    [TestFixture]
    public class when_selecting_a_media_when_all_applies : OutputBehaviorContext
    {
        protected override void theContextIs()
        {
            theMedia.Each(m => m.Stub(x => x.MatchesRequest()).Return(true));
        }

        [Test]
        public void select_the_first_media_if_the_accepted_types_takes_a_wild_card_and_no_mime_types_match()
        {
            mediaMimetypesAre(0, MimeType.Text);
            mediaMimetypesAre(1, MimeType.Json);
            mediaMimetypesAre(2, MimeType.Css);
            mediaMimetypesAre(3, MimeType.Bmp);
            mediaMimetypesAre(4, MimeType.Gif);

            theCurrentMimeType.AcceptTypes = new MimeTypeList(MimeType.Xml, MimeType.Any);

            theSelectedMediaShouldBe(0);
        }

        [Test]
        public void select_the_first_media_that_matches_the_accepted_mimetype()
        {
            mediaMimetypesAre(0, MimeType.Text);
            mediaMimetypesAre(1, MimeType.Json);
            mediaMimetypesAre(2, MimeType.Css);
            mediaMimetypesAre(3, MimeType.Bmp);
            mediaMimetypesAre(4, MimeType.Xml);

            theCurrentMimeType.AcceptTypes = new MimeTypeList(MimeType.Xml, MimeType.Any);

            theSelectedMediaShouldBe(4);

            theCurrentMimeType.AcceptTypes = new MimeTypeList(MimeType.Text);
            theSelectedMediaShouldBe(0);

            theCurrentMimeType.AcceptTypes = new MimeTypeList(MimeType.Css, MimeType.Bmp);
            theSelectedMediaShouldBe(2);

        }
    }



    [TestFixture]
    public class when_writing_and_a_media_can_be_found : OutputBehaviorContext
    {
        private IMedia<OutputTarget> theSelectedMedia;
        private string theAcceptedMimetype;

        protected override void theContextIs()
        {
            theSelectedMedia = MockFor<IMedia<OutputTarget>>();
            Services.PartialMockTheClassUnderTest();

            ClassUnderTest.Stub(x => x.SelectMedia(theCurrentMimeType)).Return(theSelectedMedia);

            theAcceptedMimetype = "text/json";
            theSelectedMedia.Stub(x => x.Mimetypes).Return(new[]{theAcceptedMimetype});
            theCurrentMimeType.AcceptTypes = new MimeTypeList(theAcceptedMimetype);

            // Pre-condition
            theCurrentMimeType.SelectFirstMatching(theSelectedMedia.Mimetypes)
                .ShouldEqual(theAcceptedMimetype);
        
            ClassUnderTest.Write();
        }

        [Test]
        public void should_use_the_selected_media()
        {
            theSelectedMedia.AssertWasCalled(x => x.Write(theAcceptedMimetype, theTarget));
        }
    }



    [TestFixture]
    public class when_writing_and_no_matching_writer_can_be_found : OutputBehaviorContext
    {
        protected override void theContextIs()
        {
            Services.PartialMockTheClassUnderTest();

            ClassUnderTest.Stub(x => x.SelectMedia(theCurrentMimeType)).Return(null);

            ClassUnderTest.Write();
        }

        [Test]
        public void should_write_a_406_not_acceptable()
        {
            MockFor<IOutputWriter>().AssertWasCalled(x => x.WriteResponseCode(HttpStatusCode.NotAcceptable));
        }

        [Test]
        public void nothing_is_written_anywhere()
        {
            theMedia.Each(media => media.AssertWasNotCalled(x => x.Write(null, null), x => x.IgnoreArguments()));
        }
    }




    public abstract class OutputBehaviorContext : InteractionContext<OutputBehavior<OutputTarget>>
    {
        protected IMedia<OutputTarget>[] theMedia;
        protected CurrentMimeType theCurrentMimeType;
        protected OutputTarget theTarget;


        protected override sealed void beforeEach()
        {
            theMedia = Services.CreateMockArrayFor<IMedia<OutputTarget>>(5);
            theCurrentMimeType = new CurrentMimeType();
            theTarget = new OutputTarget();

            MockFor<IFubuRequest>().Stub(x => x.Get<OutputTarget>()).Return(theTarget);
            MockFor<IFubuRequest>().Stub(x => x.Get<CurrentMimeType>()).Return(theCurrentMimeType);

            MockFor<IFubuRequest>().Stub(x => x.Find<IHaveHeaders>()).Return(new IHaveHeaders[0]);

            theContextIs();
        
        }

        protected abstract void theContextIs();

        protected void mediaMimetypesAre(int index, params string[] mimeTypes)
        {
            theMedia[index].Stub(x => x.Mimetypes).Return(mimeTypes);
        }

        protected void mediaMimetypesAre(int index, params MimeType[] mimeTypes)
        {
            mediaMimetypesAre(index, mimeTypes.Select(x => x.Value).ToArray());
        }

        protected void theSelectedMediaShouldBe(int index)
        {
            ClassUnderTest.SelectMedia(theCurrentMimeType)
                .ShouldBeTheSameAs(theMedia[index]);
        }

        protected void mediaMatches(int index)
        {
            theMedia[index].Stub(x => x.MatchesRequest()).Return(true);
        }

        protected void mediaDoesNotMatch(int index)
        {
            theMedia[index].Stub(x => x.MatchesRequest()).Return(false);
        }
    }

    public class OutputTarget
    {
        
    }
}