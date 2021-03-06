﻿namespace Feature.Workbox.Models.Response
{
    /// <summary>
    /// Class ChangeWorkflowResponse.
    /// </summary>
    public class ChangeWorkflowResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is success.
        /// </summary>
        /// <value><c>true</c> if this instance is success; otherwise, <c>false</c>.</value>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string Message { get; set; }
    }
}