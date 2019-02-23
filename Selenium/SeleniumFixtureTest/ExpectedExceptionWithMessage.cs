﻿// Copyright 2015-2019 Rik Essenius
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumFixture.Utilities;

namespace SeleniumFixtureTest
{
    public class ExpectedExceptionWithMessageAttribute : ExpectedExceptionBaseAttribute
    {
        public ExpectedExceptionWithMessageAttribute(Type exceptionType, string expectedMessage)
        {
            ExceptionType = exceptionType;
            ExpectedMessage = expectedMessage;
        }

        private Type ExceptionType { get; }

        private string ExpectedMessage { get; }

        protected override void Verify(Exception e)
        {
            if (e.GetType() != ExceptionType)
            {
                Assert.Fail(
                    $"ExpectedExceptionWithMessageAttribute failed. Expected exception type: {ExceptionType.FullName}. " +
                    $"Actual exception type: {e.GetType().FullName}. Exception message: {e.Message}"
                );
            }

            var actualMessage = e.Message.Trim();

            if (ExpectedMessage != null) Assert.IsTrue(actualMessage.IsLike(ExpectedMessage));
            Console.Write("ExpectedExceptionWithMessageAttribute:" + e.Message);
        }
    }
}