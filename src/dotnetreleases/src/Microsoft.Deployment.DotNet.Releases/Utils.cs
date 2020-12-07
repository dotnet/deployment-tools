// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Deployment.DotNet.Releases
{
    class Utils
    {
        private static JsonSerializer _defaultSerializer;
        private static JsonSerializerSettings _defaultSerializerSettings;

        internal static JsonSerializerSettings DefaultSerializerSettings
        {
            get
            {
                if (_defaultSerializerSettings == null)
                {
                    _defaultSerializerSettings = new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    _defaultSerializerSettings.Converters.Add(new ReleaseVersionConverter());
                }

                return _defaultSerializerSettings;
            }
        }

        internal static JsonSerializer DefaultSerializer
        {
            get
            {
                if (_defaultSerializer == null)
                {
                    _defaultSerializer = JsonSerializer.CreateDefault(DefaultSerializerSettings);
                }

                return _defaultSerializer;
            }
        }

        internal static async Task<bool> IsLatest(string fileName, Uri address)
        {
            using (var httpClient = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Head, address);
                var httpResponse = await httpClient.SendAsync(httpRequest);

                httpResponse.EnsureSuccessStatusCode();

                DateTime? onlineLastModified = httpResponse.Content.Headers.LastModified?.DateTime;
                FileInfo fileInfo = new FileInfo(fileName);

                return fileInfo.LastWriteTime >= onlineLastModified;
            }
        }

        internal static async Task DownloadFileAsync(Uri address, string fileName)
        {
            using (var httpClient = new HttpClient())
            {
                var directory = Path.GetDirectoryName(fileName);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var httpResponse = await httpClient.GetAsync(address);
                httpResponse.EnsureSuccessStatusCode();

                using (var fileStream = File.Create(fileName))
                {
                    await httpResponse.Content.CopyToAsync(fileStream);
                }
            }
        }

        /// <summary>
        /// Computes the hash for the specified file using the specified hash algorithm.
        /// </summary>
        /// <param name="fileName">The path, including the filename and extension of the file to use.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use.</param>
        /// <returns>A string containing the file hash.</returns>
        internal static string GetFileHash(string fileName, HashAlgorithm hashAlgorithm)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(ReleasesResources.CommonNullOrEmpty, nameof(fileName));
            }

            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException(nameof(hashAlgorithm));
            }

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(String.Format(ReleasesResources.FileNotFound, fileName));
            }

            using (FileStream stream = File.OpenRead(fileName))
            {
                byte[] checksum = hashAlgorithm.ComputeHash(stream);

                return BitConverter.ToString(checksum).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
