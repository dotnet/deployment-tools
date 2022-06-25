// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.Deployment.DotNet.Releases
{
    /// <summary>
    /// Custom converter for <see cref="ReleaseLabel"/> enumeration.
    /// </summary>
    internal class ReleaseLabelConverter : JsonConverter<ReleaseLabel>
    {
        /// <summary>
        /// Reads a string value and converts it into a <see cref="ReleaseLabel"/> enumeration.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to use for reading the object value.</param>
        /// <param name="objectType">The type of the object.</param>
        /// <param name="existingValue">The existing value of the object.</param>
        /// <param name="hasExistingValue">The existing value of the object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>A <see cref="ReleaseLabel"/> created from the object value.</returns>
        public override ReleaseLabel ReadJson(
            JsonReader reader, Type objectType, ReleaseLabel existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                // Strip out dashes and underscores so we can parse the enum.
                string tokenValue = reader.Value.ToString().Replace("-", "").Replace("_", "");
                if (!string.IsNullOrWhiteSpace(tokenValue))
                {
                    return Enum.TryParse(tokenValue, ignoreCase: true, out ReleaseLabel result)
                        ? result
                        : ReleaseLabel.Unknown;
                }
            }

            return ReleaseLabel.Unknown;
        }

        /// <summary>
        /// Converts the specified <see cref="ReleaseLabel"/> to a string 
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to use for writing the value.</param>
        /// <param name="value">The <see cref="ReleaseLabel"/> to write.</param>
        /// <param name="serializer">The calling serializer</param>
        public override void WriteJson(JsonWriter writer, ReleaseLabel value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}
