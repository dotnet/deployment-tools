// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Deployment.DotNet.Releases
{
    /// <summary>
    /// Represents a single file associated with a release component such as an SDK or runtime.
    /// </summary>
    public class ReleaseFile : IEquatable<ReleaseFile>
    {
        private static SHA512 HashAlgorithm = SHA512Managed.Create();

        /// <summary>
        /// The URL from where to download the file.
        /// </summary>
        public Uri Address
        {
            get;
            private set;
        }

        /// <summary>
        /// The filename and extension of this <see cref="ReleaseFile"/>.
        /// </summary>
        [JsonIgnore]
        public string FileName => Path.GetFileName(Address.LocalPath);

        /// <summary>
        /// The <see cref="SHA512"/> hash of the file.
        /// </summary>
        public string Hash
        {
            get;
            private set;
        }

        /// <summary>
        /// The version agnostic name and extension of the file.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The runtime identifier associated with the file.
        /// </summary>
        public string Rid
        {
            get;
            private set;
        }

        [JsonConstructor]
        internal ReleaseFile([JsonProperty(PropertyName = "hash")] string hash,
            [JsonProperty(PropertyName = "name")] string name,
            [JsonProperty(PropertyName = "rid")] string rid,
            [JsonProperty(PropertyName = "url")] string address)
        {
            Hash = hash;
            Name = name;
            Rid = rid;
            Address = new Uri(address);
        }

        /// <summary>
        /// Download this file to the specified local file and verify the file hash. If the hash is invalid, the
        /// file will be deleted.
        /// </summary>
        /// <param name="destinationPath">The path, including the filename of the local file. The file will be
        /// overwritten if it already exists.</param>
        /// <exception cref="InvalidDataException">Thrown if the downloaded file's hash does to match the 
        /// expected hash.</exception>
        public async Task DownloadAsync(string destinationPath)
        {
            if (destinationPath is null)
            {
                throw new ArgumentNullException(nameof(destinationPath));
            }

            if (destinationPath == string.Empty)
            {
                throw new ArgumentException(string.Format(ReleasesResources.ValueCannotBeEmpty, nameof(destinationPath)));
            }

            await Utils.DownloadFileAsync(Address, destinationPath);

            var actualHash = Utils.GetFileHash(destinationPath, HashAlgorithm);

            if (!string.Equals(Hash, actualHash, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(destinationPath);
                throw new InvalidDataException(string.Format(ReleasesResources.HashMismatch, Hash, actualHash, destinationPath));
            }
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare to the current object.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object; <see langword="false"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ReleaseFile);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ReleaseFile"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="ReleaseFile"/> to compare to this instance.</param>
        /// <returns><see langword="true"/> if the specified <see cref="ReleaseFile"/> is equal to this instance; <see langword="false"/> otherwise.</returns>
        public bool Equals(ReleaseFile other)
        {
            return ReferenceEquals(this, other) ||
                Name == other.Name &&
                Rid == other.Rid &&
                Hash == other.Hash &&
                Address == other.Address;
        }

        /// <summary>
        /// The default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            int hashCode = -28983843;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Hash);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Rid);
            hashCode = hashCode * -1521134295 + EqualityComparer<Uri>.Default.GetHashCode(Address);
            return hashCode;
        }
    }
}
