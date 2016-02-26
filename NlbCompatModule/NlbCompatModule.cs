namespace NlbCompatModule
{
    using System;
    using System.Collections.Specialized;
    using System.Web;

    /// <summary>
    /// HTTP Module in charge of handling Network Load Balancing concerns.
    /// </summary>
    public class NlbCompatModule : IHttpModule
    {
        /// <summary>
        /// The original user's IP addres is stored in a separate header with this name.
        /// </summary>
        /// <remarks>see http://en.wikipedia.org/wiki/X-Forwarded-For for more details</remarks>
        private const string ForwardedForHdr = "HTTP_X_FORWARDED_FOR";

        /// <summary>
        /// The original protocol is stored in a separate header with this name.
        /// </summary>
        private const string ForwardedProtocolHdr = "HTTP_X_FORWARDED_PROTO";

        /// <summary>
        /// Defines the separator which is to used to split the Forwarded for header.
        /// </summary>
        private const char ForwardedForAddressesSeparator = ',';

        /// <summary>
        /// Module initialization.
        /// </summary>
        /// <param name="context">The current application.</param>
        public void Init(HttpApplication context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.BeginRequest += BeginRequest;
        }

        /// <summary>
        /// Called on begin request.
        /// </summary>
        /// <param name="sender">The HTTP application.</param>
        /// <param name="e">The event argument.</param>
        private void BeginRequest(object sender, EventArgs e)
        {
            var ctx = new HttpContextWrapper(((HttpApplication) sender).Context);
            ProcessBeginRequest(ctx);
        }

        /// <summary>
        /// Process the HTTP request and handles X-Forwarded-For / X-Forwarded-Proto headers.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        internal void ProcessBeginRequest(HttpContextBase context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var serverVariables = context.Request.ServerVariables;
            HandleProxies(serverVariables);
            HandleSslOffLoading(serverVariables);
        }

        /// <summary>
        /// Handles the X-FORWARDED-FOR HTTP Header.
        /// </summary>
        /// <param name="serverVariables">The server variables.</param>
        /// <remarks>Sets REMOTE_ADDR to the first IP in string.</remarks>
        private void HandleProxies(NameValueCollection serverVariables)
        {
            var forwardedFor = serverVariables[ForwardedForHdr];
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var remoteAddr = forwardedFor;
                var forwardSeparatorIndex = forwardedFor.IndexOf(ForwardedForAddressesSeparator);
                if (forwardSeparatorIndex > 0)
                {
                    // Use the right-most address as the remote address, this is how any other non load-balanced web server would normally see it
                    remoteAddr = forwardedFor.Substring(forwardSeparatorIndex + 1);
                }
                serverVariables.Set("REMOTE_ADDR", remoteAddr);
            }
        }

        /// <summary>
        /// Handles the X-FORWARDED-PROTO HTTP Header.
        /// </summary>
        /// <param name="serverVariables">The server variables.</param>
        private void HandleSslOffLoading(NameValueCollection serverVariables)
        {
            var protocol = serverVariables[ForwardedProtocolHdr];
            if (!string.IsNullOrEmpty(protocol))
            {
                var isHttps = "HTTPS".Equals(protocol, StringComparison.OrdinalIgnoreCase);
                serverVariables.Set("HTTPS", isHttps ? "on" : "off");
                serverVariables.Set("SERVER_PORT", isHttps ? "443" : "80");
                serverVariables.Set("SERVER_PORT_SECURE", isHttps ? "1" : "0");
            }
        }

        /// <summary>
        /// Dispose the resources (other than memory) used by the module that implements IHttpModule.
        /// </summary>
        public void Dispose()
        {
        }
    }
}