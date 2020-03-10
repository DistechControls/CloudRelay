using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.Common.Exceptions
{
    /// <summary>
    /// Represents type information about an error code to be translated to an RFC 7807 problem details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class ErrorCodeHelpAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Returns the title describing the error.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Returns the URL documenting the error.
        /// </summary>
        public string Url { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="uri"></param>
        public ErrorCodeHelpAttribute(string title, string uri)
        {
            Title = title;
            Url = uri;
        }

        #endregion

        #region Help URL

        /// <summary>
        /// Returns the full URL to the documentation resource.
        /// </summary>
        /// <returns></returns>
        internal string ResolveUrl()
        {
            return Url;

            //documentation URL will need to be based on current environment, next is an example of how we could implement this.
            //this will remain commented for now until we define whether we document the errors and where do we host it.
            //Uri result;

            //if (!Uri.TryCreate(Url, UriKind.Absolute, out result))
            //{
            //    //if not absolute, must be relative!
            //    var baseUri = new Uri(Helper.GetWebAppBaseUrl(), UriKind.Absolute);
            //    if (!Uri.TryCreate(baseUri, Url, out result))
            //    {
            //        throw new Exception($"Invalid URL format `{Url}`");
            //    }
            //}

            //return result.ToString();
        }

        #endregion
    }
}
