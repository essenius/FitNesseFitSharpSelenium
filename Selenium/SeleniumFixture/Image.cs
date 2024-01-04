// Copyright 2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Diagnostics.CodeAnalysis;

namespace SeleniumFixture;

/// <summary>Container for base64 encoded PNG images</summary>
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Image
{
    private readonly string _image;

    /// <summary>Create new image from base64 string</summary>
    /// <param name="base64Image">base64 string with png data</param>
    /// <param name="alt">short description</param>
    public Image(string base64Image, string alt = "Screenshot")
    {
        _image = base64Image;
        Alt = alt;
    }

    /// <summary>Alt text for the img element, and text to use for ToString</summary>
    public string Alt { get; set; }

    /// <summary>Render image as html img element</summary>
    public string Rendering => $"<img alt=\"{Alt}\" src=\"data:image/png;base64,{_image}\" />";

    /// <summary>Parse image. Used by FitSharp.</summary>
    /// <param name="input">base64 stream to parse</param>
    /// <returns>new image object</returns>
    public static Image Parse(string input) => new(input, "Parsed image");

    /// <summary>Same as Alt. Used in FitSharp</summary>
    public override string ToString() => Alt;
}