using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Services
{
    public class InvocationResult
    {
        #region Properties

        public int Status { get; private set; }

        public string Content { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="content"></param>
        public InvocationResult(int status, string content)
        {
            Status = status;
            Content = content;
        }

        #endregion
    }
}
