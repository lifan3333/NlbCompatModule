# NlbCompatModule
Transparent SSL Offloading support for IIS applications made with a custom HTTP Module implementing the following behavior OnBeginRequest.

[![Build status](https://ci.appveyor.com/api/projects/status/6n1d8l7njs03f1rc/branch/master?svg=true)](https://ci.appveyor.com/project/ogaudefroy/nlbcompatmodule/branch/master)

Fix HttpContext server variables with X-FORWARDED-PROTO header:
 - Set HTTPS to on or off.
 - Set SERVER_PORT to 443 or 80
 - Set SERVER_PORT_SECURE to 1 or 0

Fix HttpContext server variables with X-FORWARDED-FOR header:
 - Set REMOTE_ADDR to the most right string contained in the header (comma separated list) just like any other LB system would do.

Code currently in production in a high traffic website ; works with IIS 7+, see  http://referencesource.microsoft.com/#System.Web/Hosting/IIS7WorkerRequest.cs.html. 

Please remember that this HTTP module MUST be executed first ; therefore do not forget to add it at the top most of your Web.config modules section.

You might encounter difficulties if your system is using systems like New Relic ; if you do meet this situation you can workaround this by using context.RewritePath https://msdn.microsoft.com/en-us/library/system.web.httpcontext.rewritepath(v=vs.110).aspx
