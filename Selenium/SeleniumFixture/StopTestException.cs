// Copyright 2015-2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SeleniumFixture
{
    /// <summary>
    ///     Stop Test Exception - raise when you want FitNesse to stop testing the current test case
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Clashes with other static analysis findings")]
    public class StopTestException : Exception
    {
        /// <summary>
        ///     base constructor
        /// </summary>
        public StopTestException()
        {
        }

        /// <summary>
        ///     constructor with message param
        /// </summary>
        /// <param name="message">the exception message</param>
        public StopTestException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Constructor with message and inner exception
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="innerException">the inner exception</param>
        public StopTestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        ///     Constructor with serialization info and context
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">context</param>
        protected StopTestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}