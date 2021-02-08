﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Deployment.DotNet.Releases
{
    /// <summary>
    /// Utitlity and help methods.
    /// </summary>
    internal class Utils
    {
        /// <summary>
        /// Gets the default <see cref="JsonSerializerSettings"/> to use.
        /// </summary>
        internal static JsonSerializerSettings DefaultSerializerSettings
        {
            get;
        }

        /// <summary>
        /// Gets the default <see cref="JsonSerializer"/> to use.
        /// </summary>
        internal static JsonSerializer DefaultSerializer
        {
            get;
        }

        /// <summary>
        /// Determines if a local file is the latest version compared to an online copy.
        /// </summary>
        /// <param name="fileName">The path of the local file.</param>
        /// <param name="address">The address pointing of the file.</param>
        /// <returns><see langword="true"/> if the local file is the latest; <see langword="false"/> otherwise.</returns>
        internal static async Task<bool> IsLatestFileAsync(string fileName, Uri address)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Head, address);
                HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);

                httpResponse.EnsureSuccessStatusCode();

                DateTime? onlineLastModified = httpResponse.Content.Headers.LastModified?.DateTime;
                FileInfo fileInfo = new FileInfo(fileName);

                return fileInfo.LastWriteTime >= onlineLastModified;
            }
        }

        /// <summary>
        /// Downloads a file from the specified address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="fileName"></param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        internal static async Task DownloadFileAsync(Uri address, string fileName)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string directory = Path.GetDirectoryName(fileName);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                HttpResponseMessage httpResponse = await httpClient.GetAsync(address);
                httpResponse.EnsureSuccessStatusCode();

                using (FileStream fileStream = File.Create(fileName))
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
            if (fileName is null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (fileName == string.Empty)
            {
                throw new ArgumentException(ReleasesResources.ValueCannotBeEmpty, nameof(fileName));
            }

            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException(nameof(hashAlgorithm));
            }

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(string.Format(ReleasesResources.FileNotFound, fileName));
            }

            using (FileStream stream = File.OpenRead(fileName))
            {
                byte[] checksum = hashAlgorithm.ComputeHash(stream);

                return BitConverter.ToString(checksum).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Checks whether a specified file exists, and if not, optionally downloads a copy from
        /// the specified address.
        /// </summary>
        /// <param name="path">The path of the local file to check.</param>
        /// <param name="downloadLatest">When <see langword="true"/>, the latest copy of the file is downloaded if a newer version
        /// exists online.</param>
        /// <param name="address">The address of the file to download.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        internal static async Task GetLatestFileAsync(string path, bool downloadLatest, Uri address)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (path == string.Empty)
            {
                throw new ArgumentException(ReleasesResources.ValueCannotBeEmpty, nameof(path));
            }

            if (!File.Exists(path))
            {
                if (!downloadLatest)
                {
                    throw new FileNotFoundException(string.Format(ReleasesResources.FileNotFound, path));
                }

                await Utils.DownloadFileAsync(address, path);
            }
            else if ((downloadLatest) && (!await Utils.IsLatestFileAsync(path, address)))
            {
                await Utils.DownloadFileAsync(address, path);
            }
        }

        static Utils()
        {
            DefaultSerializerSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                NullValueHandling = NullValueHandling.Ignore
            };

            DefaultSerializerSettings.Converters.Add(new ReleaseVersionConverter());

            DefaultSerializer = JsonSerializer.CreateDefault(DefaultSerializerSettings);
        }
    }
}
