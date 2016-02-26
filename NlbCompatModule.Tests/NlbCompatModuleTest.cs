namespace NlbCompatModule.Tests
{
    using System;
    using System.Collections.Specialized;
    using System.Web;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class NlbCompatModuleTest
    {
        [Test]
        public void module_OnBeginRequest_NullContext_ThrowArgumentNullException()
        {
            var module = new NlbCompatModule();

            Assert.That(() => module.ProcessBeginRequest(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ForwardedForHeader_SetsRemoteAddr([Values("192.168.0.1", "192.168.0.1,10.70.30.54")]string value)
        {
            var module = new NlbCompatModule();
            var httpContext = this.CreateHttpContextBase();
            httpContext.Request.ServerVariables.Add("HTTP_X_FORWARDED_FOR", value);

            module.ProcessBeginRequest(httpContext);

            Assert.That(httpContext.Request.ServerVariables["REMOTE_ADDR"], Is.EqualTo("192.168.0.1"));
        }
        
        [Test]
        public void ForwardedProtoHeaderDefineHttp_RequestIsHttp()
        {
            var module = new NlbCompatModule();

            var httpContext = this.CreateHttpContextBase();
            httpContext.Request.ServerVariables.Add("HTTP_X_FORWARDED_PROTO", "http");

            module.ProcessBeginRequest(httpContext);

            Assert.That(httpContext.Request.ServerVariables["SERVER_PORT"], Is.EqualTo("80"));
            Assert.That(httpContext.Request.ServerVariables["SERVER_PORT_SECURE"], Is.EqualTo("0"));
            Assert.That(httpContext.Request.ServerVariables["HTTPS"], Is.EqualTo("off"));
        }

        [Test]
        public void ForwardedProtoHeaderDefineHttps_RequestIsHttps()
        {
            var module = new NlbCompatModule();

            var httpContext = this.CreateHttpContextBase();
            httpContext.Request.ServerVariables.Add("HTTP_X_FORWARDED_PROTO", "https");

            module.ProcessBeginRequest(httpContext);

            Assert.That(httpContext.Request.ServerVariables["SERVER_PORT"], Is.EqualTo("443"));
            Assert.That(httpContext.Request.ServerVariables["SERVER_PORT_SECURE"], Is.EqualTo("1"));
            Assert.That(httpContext.Request.ServerVariables["HTTPS"], Is.EqualTo("on"));
        }
        
        private HttpContextBase CreateHttpContextBase()
        {
            var httpContext = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var serverVariables = new NameValueCollection();
            httpContext.SetupGet(x => x.Request).Returns(request.Object);
            request.SetupGet(x => x.ServerVariables).Returns(serverVariables);

            return httpContext.Object;
        }
    }
}
