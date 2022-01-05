// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// Represents the root of the dependency model.
    /// </summary>
    public class PlatformDependenciesModel : IPlatformContainer
    {
        /// <summary>
        /// Gets the .NET product version that the model describes.
        /// </summary>
        public Version ProductVersion { get; }

        /// <summary>
        /// Gets the set of available dependency usages that can be referenced by a platform dependency.
        /// </summary>
        /// <remarks>
        /// A dependency usage is a descriptor of how a dependency is used by a component (e.g. default, diagnostics, 
        /// localization). This maps the name of the usage to its description.
        /// </remarks>
        public IDictionary<string, string> DependencyUsages { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets the set of top-level platforms whose dependencies are described.
        /// </summary>
        public IList<Platform> Platforms { get; } = new List<Platform>();

        /// <summary>
        /// Initializes a new instance of <see cref="PlatformDependenciesModel"/>.
        /// </summary>
        /// <param name="productVersion">.NET product version that the model describes.</param>
        public PlatformDependenciesModel(Version productVersion)
        {
            if (productVersion is null)
            {
                throw new ArgumentNullException(nameof(productVersion));
            }

            ProductVersion = productVersion;
        }

        /// <summary>
        /// Validates the model is semantically correct.
        /// </summary>
        public void Validate()
        {
            HashSet<string> platformRids = new();
            foreach (Platform platform in Platforms)
            {
                if (platformRids.Contains(platform.Rid))
                {
                    throw new FormatException($"Duplicate platforms were found with RID '{platform.Rid}'.");
                }

                platformRids.Add(platform.Rid);
                platform.Validate(this);
            }
        }

        internal IEnumerable<Platform> GetAncestorsBottomUp(Platform childPlatform)
        {
            foreach (Platform platform in Platforms)
            {
                if (platform == childPlatform)
                {
                    return Enumerable.Empty<Platform>();
                }

                List<Platform> ancestors = new();
                platform.AddAncestorsBottomUp(childPlatform, ancestors);
                if (ancestors.Any())
                {
                    return ancestors;
                }
            }

            return Enumerable.Empty<Platform>();
        }

        internal Platform GetContainingPlatform(PlatformDependency platformDependency)
        {
            foreach (Platform platform in EnumeratePlatforms(this))
            {
                if (platform.Components.Any(component => component.PlatformDependencies.Contains(platformDependency)))
                {
                    return platform;
                }
            }

            throw new ArgumentException(
                $"Platform dependency with type '{platformDependency.DependencyType}' and ID '{platformDependency.Id}' does not exist within this model.",
                nameof(platformDependency));
        }

        internal Platform GetContainingPlatform(Component component)
        {
            foreach (Platform platform in EnumeratePlatforms(this))
            {
                if (platform.Components.Any(c => ReferenceEquals(c, component)))
                {
                    return platform;
                }
            }

            throw new ArgumentException(
                $"Component with name '{component.Name}' does not exist within this model.",
                nameof(component));
        }

        private static IEnumerable<Platform> EnumeratePlatforms(IPlatformContainer platformContainer)
        {
            foreach (Platform platform in platformContainer.Platforms)
            {
                yield return platform;
                foreach (Platform child in EnumeratePlatforms(platform))
                {
                    yield return child;
                }
            }
        }

        /// <summary>
        /// Loads the dependency model from the provided URL.
        /// </summary>
        /// <param name="platformDependenciesUrl">A URL pointing to a platform dependencies JSON file.</param>
        /// <returns>A <see cref="PlatformDependenciesModel"/> deserialized from the content of the file.</returns>
        public static Task<PlatformDependenciesModel> GetAsync(Uri platformDependenciesUrl) =>
            GetAsync(platformDependenciesUrl.ToString());

        /// <summary>
        /// Loads the dependency model from the provided URL.
        /// </summary>
        /// <param name="platformDependenciesUrl">A URL pointing to a platform dependencies JSON file.</param>
        /// <returns>A <see cref="PlatformDependenciesModel"/> deserialized from the content of the file.</returns>
        public static async Task<PlatformDependenciesModel> GetAsync(string platformDependenciesUrl)
        {
            if (platformDependenciesUrl is null)
            {
                throw new ArgumentNullException(nameof(platformDependenciesUrl));
            }

            using HttpClient httpClient = new();
            using MemoryStream stream = new(await httpClient.GetByteArrayAsync(platformDependenciesUrl).ConfigureAwait(false));
            using TextReader textReader = new StreamReader(stream);
            PlatformDependenciesModel? model = await GetAsync(textReader).ConfigureAwait(false);

            if (model is null)
            {
                throw new FormatException($"Unable to deserialize the content in URL '{platformDependenciesUrl}'.");
            }

            return model;
        }

        /// <summary>
        /// Loads the dependency model from the provided file path.
        /// </summary>
        /// <param name="path">The path to a platform dependencies JSON file.</param>
        /// <returns>A <see cref="PlatformDependenciesModel"/> deserialized from the content of the file.</returns>
        public static async Task<PlatformDependenciesModel> GetFromFileAsync(string path)
        {
            using TextReader textReader = File.OpenText(path);
            PlatformDependenciesModel? model = await GetAsync(textReader).ConfigureAwait(false);

            if (model is null)
            {
                throw new FormatException($"Unable to deserialize the content in file '{path}'.");
            }

            return model;
        }

        private static async Task<PlatformDependenciesModel?> GetAsync(TextReader reader)
        {
            PlatformDependenciesModel? model =
                JsonConvert.DeserializeObject<PlatformDependenciesModel>(await reader.ReadToEndAsync().ConfigureAwait(false));
            model?.Validate();
            return model;
        }
    }
}
