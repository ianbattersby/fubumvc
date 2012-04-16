using System;
using System.Net;
using System.Xml.Serialization;
using FubuMVC.Core;
using FubuMVC.Core.Resources.Conneg.New;
using HtmlTags;
using NUnit.Framework;
using IntegrationTesting.Conneg;
using StringWriter = System.IO.StringWriter;

namespace IntegrationTesting.Conneg
{
    [TestFixture]
    public class conneg_with_endpoint_that_accepts_all_formatters_and_form_posts : FubuRegistryHarness
    {
        private XmlJsonHtmlMessage input;
        private string expectedJson;
        private string expectedXml;

        public conneg_with_endpoint_that_accepts_all_formatters_and_form_posts()
        {
            input = new XmlJsonHtmlMessage{
                Id = Guid.NewGuid()
            };

            expectedJson = JsonUtil.ToJson(input);

            var writer = new StringWriter();
            new XmlSerializer(typeof (XmlJsonHtmlMessage)).Serialize(writer, input);
            expectedXml = writer.ToString();
        }

        protected override void configure(FubuRegistry registry)
        {
            registry.Actions.IncludeType<ConnegController>();
            registry.Media // TODO -- I really don't like that you have to do this.
                .ApplyContentNegotiationToActions(call => true);
        }

        [Test]
        public void send_json_expecting_json()
        {
            endpoints.PostJson(input, contentType: "text/json", accept: "text/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);

            endpoints.PostJson(input, contentType: "application/json", accept: "application/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("application/json", expectedJson);

            endpoints.PostJson(input, contentType: "application/json", accept: "text/xml,application/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("application/json", expectedJson);

            endpoints.PostJson(input, contentType: "text/json", accept: "text/xml,text/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);
        }

        [Test]
        public void uses_json_for_global_accept()
        {
            endpoints.PostJson(input, contentType: "text/json", accept: "*/*")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);

            endpoints.PostJson(input, contentType: "text/json", accept: "text/xml,*/*")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);
        }

        [Test]
        public void will_accept_xml_as_an_input()
        {
            endpoints.PostXml(input, accept: "text/xml")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/xml", expectedXml);
        }

        [Test]
        public void requesting_an_unsupported_media_type_returns_406()
        {
            endpoints.PostJson(input, accept: "random/format").StatusCodeShouldBe(HttpStatusCode.NotAcceptable);
        }

        [Test]
        public void send_the_request_as_http_form_expect_json_back()
        {
            endpoints.PostAsForm(input, accept: "text/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);

            endpoints.PostAsForm(input, accept: "application/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("application/json", expectedJson);
        }

        [Test]
        public void send_the_request_as_http_form_expect_xml_back()
        {
            endpoints.PostAsForm(input, accept: "text/xml")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/xml", expectedXml);

            endpoints.PostAsForm(input, accept: "application/xml")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("application/xml", expectedXml);
        }
    }

    [TestFixture]
    public class symmetric_json_endpoints_with_conneg : FubuRegistryHarness
    {
        private readonly SymmetricJson input;
        private string expectedJson;

        public symmetric_json_endpoints_with_conneg()
        {
            input = new SymmetricJson
            {
                Id = Guid.NewGuid(),
                Name = "Somebody"
            };

            expectedJson = JsonUtil.ToJson(input);
        }

        protected override void configure(FubuRegistry registry)
        {
            registry.Actions.IncludeType<ConnegController>();
            registry.Media // TODO -- I really don't like that you have to do this.
                .ApplyContentNegotiationToActions(call => true);
        }

        [Test]
        public void send_json_expecting_json()
        {
            endpoints.PostJson(input, contentType: "text/json", accept: "text/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);

            endpoints.PostJson(input, contentType: "application/json", accept: "application/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("application/json", expectedJson);

            endpoints.PostJson(input, contentType: "application/json", accept: "text/xml,application/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("application/json", expectedJson);

            endpoints.PostJson(input, contentType: "text/json", accept: "text/xml,text/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);
        }

        [Test]
        public void uses_json_for_global_accept()
        {
            endpoints.PostJson(input, contentType: "text/json", accept: "*/*")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);

            endpoints.PostJson(input, contentType: "text/json", accept: "text/xml,*/*")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);
        }

        [Test]
        public void will_not_accept_xml_as_an_input()
        {
            endpoints.PostXml(input, accept: "*/*").StatusCodeShouldBe(HttpStatusCode.UnsupportedMediaType);
        }

        [Test]
        public void requesting_an_unsupported_media_type_returns_406()
        {
            endpoints.PostJson(input, accept: "text/xml").StatusCodeShouldBe(HttpStatusCode.NotAcceptable);
        }

        [Test]
        public void will_not_accept_a_form_post()
        {
            endpoints.PostAsForm(input, accept: "text/json")
                .StatusCodeShouldBe(HttpStatusCode.UnsupportedMediaType);
        }
    }



    [TestFixture]
    public class asymmetric_json_endpoints_with_conneg : FubuRegistryHarness
    {
        private readonly AsymmetricJson input;
        private string expectedJson;

        public asymmetric_json_endpoints_with_conneg()
        {
            input = new AsymmetricJson{
                Id = Guid.NewGuid()
            };

            expectedJson = JsonUtil.ToJson(input);
        }

        protected override void configure(FubuRegistry registry)
        {
            registry.Actions.IncludeType<ConnegController>();
            registry.Media // TODO -- I really don't like that you have to do this.
                .ApplyContentNegotiationToActions(call => true);
        }

        [Test]
        public void send_json_expecting_json()
        {
            endpoints.PostJson(input, contentType: "text/json", accept: "text/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);

            endpoints.PostJson(input, contentType: "application/json", accept: "application/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("application/json", expectedJson);

            endpoints.PostJson(input, contentType: "application/json", accept: "text/xml,application/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("application/json", expectedJson);

            endpoints.PostJson(input, contentType: "text/json", accept: "text/xml,text/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);
        }

        [Test]
        public void uses_json_for_global_accept()
        {
            endpoints.PostJson(input, contentType: "text/json", accept: "*/*")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);

            endpoints.PostJson(input, contentType: "text/json", accept: "text/xml,*/*")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);
        }

        [Test]
        public void will_not_accept_xml_as_an_input()
        {
            endpoints.PostXml(input, accept: "*/*").StatusCodeShouldBe(HttpStatusCode.UnsupportedMediaType);
        }

        [Test]
        public void requesting_an_unsupported_media_type_returns_406()
        {
            endpoints.PostJson(input, accept: "text/xml").StatusCodeShouldBe(HttpStatusCode.NotAcceptable);
        }

        [Test]
        public void send_the_request_as_http_form_expect_json_back()
        {
            endpoints.PostAsForm(input, accept: "text/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("text/json", expectedJson);

            endpoints.PostAsForm(input, accept: "application/json")
                .StatusCodeShouldBe(HttpStatusCode.OK)
                .ContentShouldBe("application/json", expectedJson);
        }
    }


    public class ConnegController
    {
        [SymmetricJson]
        public SymmetricJson post_send_symmetric(SymmetricJson message)
        {
            return message;
        }

        [AsymmetricJson]
        public AsymmetricJson post_send_asymmetric(AsymmetricJson message)
        {
            return message;
        }

        public XmlJsonHtmlMessage post_send_mixed(XmlJsonHtmlMessage message)
        {
            return message;
        }

        public XmlAndJsonOnlyMessage post_send_xmlorjson(XmlAndJsonOnlyMessage message)
        {
            return message;
        }
    }

    public interface ConnegMessage
    {
        Guid Id { get; set; }
    }

    public class SymmetricJson : ConnegMessage
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
    }

    public class AsymmetricJson : ConnegMessage
    {
        public Guid Id { get; set; }
    }

    public class XmlJsonHtmlMessage : ConnegMessage
    {
        public Guid Id { get; set; }
    }

    public class XmlAndJsonOnlyMessage : ConnegMessage
    {
        public Guid Id { get; set; }
    }
}