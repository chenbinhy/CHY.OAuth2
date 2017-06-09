using CHY.OAuth2.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core
{
    public static partial class Logger
    {
        /// <summary>
		/// The <see cref="ILog"/> instance that is to be used 
		/// by this static Logger for the duration of the appdomain.
		/// </summary>
		private static readonly ILog library = CreateWithBanner("DotNetOpenAuth");

		/// <summary>
		/// Backing field for the <see cref="Yadis"/> property.
		/// </summary>
		private static readonly ILog yadis = Create("DotNetOpenAuth.Yadis");

		/// <summary>
		/// Backing field for the <see cref="Messaging"/> property.
		/// </summary>
		private static readonly ILog messaging = Create("DotNetOpenAuth.Messaging");

		/// <summary>
		/// Backing field for the <see cref="Channel"/> property.
		/// </summary>
		private static readonly ILog channel = Create("DotNetOpenAuth.Messaging.Channel");

		/// <summary>
		/// Backing field for the <see cref="Bindings"/> property.
		/// </summary>
		private static readonly ILog bindings = Create("DotNetOpenAuth.Messaging.Bindings");

		/// <summary>
		/// Backing field for the <see cref="Signatures"/> property.
		/// </summary>
		private static readonly ILog signatures = Create("DotNetOpenAuth.Messaging.Bindings.Signatures");

		/// <summary>
		/// Backing field for the <see cref="Http"/> property.
		/// </summary>
		private static readonly ILog http = Create("DotNetOpenAuth.Http");

		/// <summary>
		/// Backing field for the <see cref="Controls"/> property.
		/// </summary>
		private static readonly ILog controls = Create("DotNetOpenAuth.Controls");

		/// <summary>
		/// Backing field for the <see cref="OpenId"/> property.
		/// </summary>
		private static readonly ILog openId = Create("DotNetOpenAuth.OpenId");

		/// <summary>
		/// Backing field for the <see cref="OAuth"/> property.
		/// </summary>
		private static readonly ILog oauth = Create("DotNetOpenAuth.OAuth");

		/// <summary>
		/// Gets the logger for general library logging.
		/// </summary>
        public static ILog Library { get { return library; } }

		/// <summary>
		/// Gets the logger for service discovery and selection events.
		/// </summary>
        public static ILog Yadis { get { return yadis; } }

		/// <summary>
		/// Gets the logger for Messaging events.
		/// </summary>
        public static ILog Messaging { get { return messaging; } }

		/// <summary>
		/// Gets the logger for Channel events.
		/// </summary>
        public static ILog Channel { get { return channel; } }

		/// <summary>
		/// Gets the logger for binding elements and binding-element related events on the channel.
		/// </summary>
        public static ILog Bindings { get { return bindings; } }

		/// <summary>
		/// Gets the logger specifically used for logging verbose text on everything about the signing process.
		/// </summary>
        public static ILog Signatures { get { return signatures; } }

		/// <summary>
		/// Gets the logger for HTTP-level events.
		/// </summary>
        public static ILog Http { get { return http; } }

		/// <summary>
		/// Gets the logger for events logged by ASP.NET controls.
		/// </summary>
        public static ILog Controls { get { return controls; } }

		/// <summary>
		/// Gets the logger for high-level OpenID events.
		/// </summary>
        public static ILog OpenId { get { return openId; } }

		/// <summary>
		/// Gets the logger for high-level OAuth events.
		/// </summary>
        public static ILog OAuth { get { return oauth; } }


		/// <summary>
		/// Creates an additional logger on demand for a subsection of the application.
		/// </summary>
		/// <param name="name">A name that will be included in the log file.</param>
		/// <returns>The <see cref="ILog"/> instance created with the given name.</returns>
        public static ILog Create(string name)
        {
			return InitializeFacade(name);
		}

		/// <summary>
		/// Creates the main logger for the library, and emits an INFO message
		/// that is the name and version of the library.
		/// </summary>
		/// <param name="name">A name that will be included in the log file.</param>
		/// <returns>The <see cref="ILog"/> instance created with the given name.</returns>
        public static ILog CreateWithBanner(string name)
        {
			ILog log = Create(name);
			log.Info(Util.LibraryVersion);
			return log;
		}

		/// <summary>
		/// Creates an additional logger on demand for a subsection of the application.
		/// </summary>
		/// <param name="type">A type whose full name that will be included in the log file.</param>
		/// <returns>The <see cref="ILog"/> instance created with the given type name.</returns>
        public static ILog Create(Type type)
        {
			return Create(type.FullName);
		}

		/// <summary>
		/// Discovers the presence of Log4net.dll and other logging mechanisms
		/// and returns the best available logger.
		/// </summary>
		/// <param name="name">The name of the log to initialize.</param>
		/// <returns>The <see cref="ILog"/> instance of the logger to use.</returns>
		private static ILog InitializeFacade(string name) {
			return LogProvider.GetLogger(name);
		}
    }
}
