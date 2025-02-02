﻿// Copyright 2015-2024 Rik Essenius
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace SeleniumFixture;

/// <summary>StopTestException stops execution of a test page</summary>
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Clashes with other static analysis findings")]
public class StopTestException : Exception
{
    public StopTestException()
    {
    }

    public StopTestException(string message) : base(message)
    {
    }

    public StopTestException(string message, Exception innerException) : base(message, innerException)

    {
    }
}