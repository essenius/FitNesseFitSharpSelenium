// Copyright 2015-2024 Rik Essenius
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using OpenQA.Selenium;

namespace SeleniumFixture.Model;

internal class KeyConverter(string keys)
{
    private const char EndDelimiter = '}';
    private const char StartDelimiter = '{';

    private static readonly Dictionary<string, string> KeyDictionary =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            { @"ADD", Keys.Add },
            { @"BACKSPACE", Keys.Backspace },
            { @"BS", Keys.Backspace },
            { @"BKSP", Keys.Backspace },
            { @"DEL", Keys.Delete },
            { @"DELETE", Keys.Delete },
            { @"DIVIDE", Keys.Divide },
            { @"DOWN", Keys.ArrowDown },
            { @"END", Keys.End },
            { @"ENTER", Keys.Enter },
            { @"ESC", Keys.Escape },
            { @"HELP", Keys.Help },
            { @"HOME", Keys.Home },
            { @"INS", Keys.Insert },
            { @"INSERT", Keys.Insert },
            { @"MULTIPLY", Keys.Multiply },
            { @"LEFT", Keys.ArrowLeft },
            { @"PGDN", Keys.PageDown },
            { @"PGUP", Keys.PageUp },
            { @"RIGHT", Keys.ArrowRight },
            { @"SPACE", Keys.Space },
            { @"SUBTRACT", Keys.Subtract },
            { @"TAB", Keys.Tab },
            { @"UP", Keys.ArrowUp },
            { @"F1", Keys.F1 },
            { @"F2", Keys.F2 },
            { @"F3", Keys.F3 },
            { @"F4", Keys.F4 },
            { @"F5", Keys.F5 },
            { @"F6", Keys.F6 },
            { @"F7", Keys.F7 },
            { @"F8", Keys.F8 },
            { @"F9", Keys.F9 },
            { @"F10", Keys.F10 },
            { @"F11", Keys.F11 },
            { @"F12", Keys.F12 }
        };

    public string ToSeleniumFormat
    {
        get
        {
            var builder = new StringBuilder();
            for (var i = 0; i < keys.Length; i++)
            {
                switch (keys[i])
                {
                    case '+':
                        builder.Append(Keys.Shift);
                        break;
                    case '^':
                        builder.Append(Keys.Control);
                        break;
                    case '%':
                        builder.Append(Keys.Alt);
                        break;
                    case '~':
                        builder.Append(Keys.Enter);
                        break;
                    case StartDelimiter:
                        // we have an opening curly brace. Find the corresponding closing one
                        var endDelimiterPosition = FindEndDelimiterPosition(i);
                        // Handle the content between the curly braces
                        var key = keys.Substring(i + 1, endDelimiterPosition - i - 1);
                        builder.Append(EscapedContent(key));
                        // start next iteration after the closing curly brace
                        i = endDelimiterPosition;
                        break;
                    default:
                        builder.Append(keys[i]);
                        break;
                }
            }
            return builder.ToString();
        }
    }

    private static string EscapedContent(string escapedString)
    {
        // check if we have a repeater, i.e. a space followed by an integer just prior to the closing curly brace
        var spacePosition = escapedString.IndexOf(" ", StringComparison.Ordinal);
        string finalKey;
        if (spacePosition != -1 && int.TryParse(escapedString[(spacePosition + 1)..], out var repeater))
        {
            // chop off the space and the repeater from the content, we don't want that in the result
            finalKey = escapedString[..spacePosition];
        }
        else
        {
            // no repeater found, so default to single occurrence, and nothing to chop off.
            repeater = 1;
            finalKey = escapedString;
        }

        //Replace content if it is a special key; otherwise just leave as is
        var singleResult = KeyDictionary.GetValueOrDefault(finalKey, finalKey);
        return string.Concat(Enumerable.Repeat(singleResult, repeater));
    }

    private int FindEndDelimiterPosition(int startDelimiterPosition)
    {
        var endDelimiterPosition = keys.IndexOf('}', startDelimiterPosition + 1);
        if (endDelimiterPosition == -1)
        {
            throw new ArgumentException("Could not find end delimiter '" + EndDelimiter + "'");
        }

        var secondEndDelimiterPosition = keys.IndexOf(EndDelimiter, endDelimiterPosition + 1);
        if (secondEndDelimiterPosition != -1 &&
            !keys.Substring(endDelimiterPosition + 1, secondEndDelimiterPosition - endDelimiterPosition - 1)
                .Contains(StartDelimiter.ToString(CultureInfo.InvariantCulture)))
        {
            // we have two closing curly braces without an opening curly brace in between. Everything between
            // the opening brace and the second closing brace is escaped
            endDelimiterPosition = secondEndDelimiterPosition;
        }

        return endDelimiterPosition;
    }
}