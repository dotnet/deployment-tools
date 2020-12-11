// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.Deployment.DotNet.Releases
{
    /// <summary>
    /// Represents a version associated with a <see cref="ProductRelease"/> or <see cref="ReleaseComponent"/>. The
    /// version is treated as a semantic version.
    /// </summary>
    public class ReleaseVersion : IComparable, IComparable<ReleaseVersion>, IEquatable<ReleaseVersion>
    {
        // See https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string

        /// <summary>
        /// Regular expression for capturing the pre-release part of a semantic version.
        /// </summary>
        private static readonly string PrereleasePattern = @"(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*)";

        /// <summary>
        /// Regular expression for capturing the build-metadata part of a semantic version.
        /// </summary>
        private static readonly string BuildMetadataPattern = @"(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*)";

        /// <summary>
        /// Regular expression for v2 semantic version to capture components into separate groups: major, minor, patch, prerelease and build.
        /// </summary>
        private static readonly string SemanticVersion2Pattern = $@"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-{PrereleasePattern})?(?:\+{BuildMetadataPattern})?$";

        /// <summary>
        /// The build metadata of the version or <see langword="null"/>.
        /// </summary>
        public string BuildMetadata
        {
            get;
            private set;
        }

        /// <summary>
        /// The major version number.
        /// </summary>
        public int Major
        {
            get;
            private set;
        } = 0;

        /// <summary>
        /// The minor version number.
        /// </summary>
        public int Minor
        {
            get;
            private set;
        } = 0;

        /// <summary>
        /// The patch number.
        /// </summary>
        public int Patch
        {
            get;
            private set;
        } = 0;

        /// <summary>
        /// The prerelease label of the version or <see langword="null"/>.
        /// </summary>
        public string Prerelease
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the feature band of the version (also known as the SDK minor version). Feature bands only apply to SDK versions.
        /// The value is derived from the hundreds group of the version's patch field (third part of the version number). For example, 
        /// if the SDK version is 2.1.809, the feature band is 800.
        /// </summary>
        public int SdkFeatureBand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the patch level of the feature band if this version is associated with an SDK. The patch level will be between 0 and 99. 
        /// For example, if the SDK version is 2.1.516, the patch level is 16 
        /// </summary>
        public int SdkPatchLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new <see cref="ReleaseVersion"/> instance using a string representation of a version. The version may
        /// be expressed as a semantic version.
        /// </summary>
        /// <param name="version">The string value of the version, e.g. &quot;1.0.0-preview1&quot;.</param>
        /// <exception cref="FormatException">If the string represents an invalid version.</exception>
        public ReleaseVersion(string version)
        {
            Match match = Regex.Match(version, SemanticVersion2Pattern);

            if (!match.Success)
            {
                throw new FormatException(string.Format(ReleasesResources.InvalidVersion, version));
            }

            // Major.Minor.Patch are required, see https://semver.org/#spec-item-2
            if (!match.Groups["major"].Success)
            {
                throw new FormatException(ReleasesResources.InvalidMajorVersion);
            }

            if (!match.Groups["minor"].Success)
            {
                throw new FormatException(ReleasesResources.InvalidMinorVersion);
            }

            if (!match.Groups["patch"].Success)
            {
                throw new FormatException(ReleasesResources.InvalidPatch);
            }

            Major = Convert.ToInt32(match.Groups["major"].Value);
            Minor = Convert.ToInt32(match.Groups["minor"].Value);
            Patch = Convert.ToInt32(match.Groups["patch"].Value);
            Prerelease = match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null;
            BuildMetadata = match.Groups["buildmetadata"].Success ? match.Groups["buildmetadata"].Value : null;
            SdkFeatureBand = (Patch / 100) * 100;
            SdkPatchLevel = Patch % 100;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseVersion"/> class using the specified
        /// version components.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The patch number.</param>
        /// <param name="prerelease">The prerelease label.</param>
        /// <param name="buildMetadata">The build metadata.</param>
        /// <exception cref="FormatException">Thrown if the content of any field is invalid.</exception>
        public ReleaseVersion(int major, int minor, int patch, string prerelease = null, string buildMetadata = null)
        {
            if (major < 0)
            {
                throw new FormatException(string.Format(ReleasesResources.VersionPartLessThanZero, nameof(major)));
            }

            if (minor < 0)
            {
                throw new FormatException(string.Format(ReleasesResources.VersionPartLessThanZero, nameof(minor)));
            }

            if (patch < 0)
            {
                throw new FormatException(string.Format(ReleasesResources.VersionPartLessThanZero, nameof(patch)));
            }

            Major = major;
            Minor = minor;
            Patch = patch;

            if (!string.IsNullOrEmpty(prerelease))
            {
                Match prereleaseMatch = Regex.Match(prerelease, $@"^{PrereleasePattern}$");

                if (!prereleaseMatch.Groups["prerelease"].Success)
                {
                    throw new FormatException(string.Format(ReleasesResources.InvalidPrerelease, prerelease));
                }

                Prerelease = prereleaseMatch.Groups["prerelease"].Value;
            }

            if (!string.IsNullOrEmpty(buildMetadata))
            {
                Match buildMetadataMatch = Regex.Match(buildMetadata, $@"^{BuildMetadataPattern}$");

                if (!buildMetadataMatch.Groups["buildmetadata"].Success)
                {
                    throw new FormatException(string.Format(ReleasesResources.InvalidBuildMetadata, buildMetadata));
                }

                BuildMetadata = buildMetadataMatch.Groups["buildmetadata"].Value;
            }

            SdkFeatureBand = (Patch / 100) * 100;
            SdkPatchLevel = Patch % 100;
        }

        /// <summary>
        /// Compares this instance to another <see cref="ReleaseVersion"/> and returns an indication of their relative precedence.
        /// Dot-separated prerelease labels are individually compared. Versions that only differ in build metadata have the same
        /// precedence.
        /// </summary>
        /// <param name="value">The <see cref="ReleaseVersion"/> to compare against this instance.</param>
        /// <returns>Returns a signed integer indicating whether this instance precedes, follows or appears in the same position in 
        /// the sort order as the specified <paramref name="value"/>.
        /// </returns>
        public int ComparePrecedenceTo(ReleaseVersion value)
        {
            // Versions that only differ in build metadata have the same precedence.
            // Reference: https://semver.org/#spec-item-10 
            if (value is null)
            {
                return 1;
            }

            int result = Major.CompareTo(value.Major);

            if (result != 0)
            {
                return result;
            }

            result = Minor.CompareTo(value.Minor);

            if (result != 0)
            {
                return result;
            }

            result = Patch.CompareTo(value.Patch);

            if (result != 0)
            {
                return result;
            }

            return CompareIdentifiers(Prerelease, value.Prerelease);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current 
        /// instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="value">An object to compare or null.</param>
        /// <returns>
        /// Returns a signed integer indicating whether this instance precedes, follows or appears in the same position in 
        /// the sort order as the specified <paramref name="value"/>.
        /// </returns>
        public int CompareTo(object value)
        {
            return CompareTo((ReleaseVersion)value);
        }

        /// <summary>
        /// Compares this instance to the specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">An object to compare.</param>
        /// <returns></returns>
        public int CompareTo(ReleaseVersion value)
        {
            int result = ComparePrecedenceTo(value);

            if (result != 0)
            {
                return result;
            }

            return CompareIdentifiers(BuildMetadata, value.BuildMetadata);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare to the current object.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object; <see langword="false"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if ((obj is null) || !(obj is ReleaseVersion))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            ReleaseVersion other = (ReleaseVersion)obj;

            return Major == other.Major && Minor == other.Minor && Patch == other.Patch &&
                string.Equals(Prerelease, other.Prerelease, StringComparison.Ordinal) &&
                string.Equals(BuildMetadata, other.BuildMetadata, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="ReleaseVersion"/> object and a specified
        /// <see cref="ReleaseVersion"/> object represent the same value.
        /// </summary>
        /// <param name="obj">The <see cref="ReleaseVersion"/> object to compare to this instance, or <see langword="null" />.</param>
        /// <returns><see langword="true"/> if every component of this <see cref="ReleaseVersion"/> matches the corresponding
        /// component of the <paramref name="obj"/> parameter; <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(ReleaseVersion obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Major == obj.Major && Minor == obj.Minor && Patch == obj.Patch &&
                string.Equals(Prerelease, obj.Prerelease, StringComparison.Ordinal) &&
                string.Equals(BuildMetadata, obj.BuildMetadata, StringComparison.Ordinal);
        }

        /// <summary>
        /// The default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Major ^ Minor ^ Patch ^ (!string.IsNullOrEmpty(Prerelease) ? Prerelease.GetHashCode() : 0) ^
                (!string.IsNullOrEmpty(BuildMetadata) ? BuildMetadata.GetHashCode() : 0);
        }

        /// <summary>
        /// Determines whether this instance and another <see cref="ReleaseVersion"/> share the same precedence.
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <returns>
        /// <see langword="true"/> if both values have the same sort order; <see langword="false"/> otherwise.
        /// </returns>        
        public bool PrecedenceEquals(ReleaseVersion value)
        {
            return ComparePrecedenceTo(value) == 0;
        }

        /// <summary>
        /// Returns a <see cref="string"/> representation of this <see cref="ReleaseVersion"/>.
        /// </summary>
        /// <returns>The <see cref="string"/> representation of this <see cref="ReleaseVersion"/>.</returns>
        public override string ToString()
        {
            string v = $"{Major}.{Minor}.{Patch}";
            v += !string.IsNullOrEmpty(Prerelease) ? $"-{Prerelease}" : string.Empty;
            v += !string.IsNullOrEmpty(BuildMetadata) ? $"+{BuildMetadata}" : string.Empty;

            return v;
        }

        /// <summary>
        /// Returns a <see cref="string"/> representation of the current <see cref="ReleaseVersion"/>. A specified
        /// count indicates the number of components to return.
        /// </summary>
        /// <param name="fieldCount">The number of components to return. If the field count is less than 0 or greater than 3, all
        /// components including the prerelease and build metadata is returned.</param>
        /// <returns></returns>
        public string ToString(int fieldCount)
        {
            if (fieldCount <= 0)
            {
                return string.Empty;
            }

            string value = fieldCount == 1 ? $"{Major}" : fieldCount == 2 ? $"{Major}.{Minor}" : $"{Major}.{Minor}.{Patch}";
            value += fieldCount >= 4 && !string.IsNullOrEmpty(Prerelease) ? $"-{Prerelease}" : string.Empty;
            value += fieldCount >= 5 && !string.IsNullOrEmpty(BuildMetadata) ? $"+{BuildMetadata}" : string.Empty;

            return value;
        }

        #region static methods
        /// <summary>
        /// Compare two specified <see cref="ReleaseVersion"/> objects and returns an integer that indicates their relative position in
        /// the sort order.
        /// </summary>
        /// <param name="a">The first object to compare.</param>
        /// <param name="b">The second object to compare.</param>
        /// <returns>
        /// A signed integer indicating the relative position in the sort order. Less than zero indicates that the first value
        /// preceeds the second. Zero indicates that both values occur at the same sort position. Greater than zero indicates that
        /// the first value follows the second.
        /// </returns>
        public static int Compare(ReleaseVersion a, ReleaseVersion b)
        {
            if (ReferenceEquals(a, b))
            {
                return 0;
            }

            if (a is null)
            {
                return -1;
            }

            if (b is null)
            {
                return 1;
            }

            return a.CompareTo(b);
        }

        /// <summary>
        /// Determiens whether two <see cref="ReleaseVersion"/> objects have the same value.
        /// </summary>
        /// <param name="a">The first value to compare, or null.</param>
        /// <param name="b">The second value to compare, or null</param>
        /// <returns>
        /// <see langword="true"/> if the value of <paramref name="a"/> is the same as the value of <paramref name="b"/>. 
        /// If both values are null, the method returns <see langword="true"/>.
        /// </returns>
        public static bool Equals(ReleaseVersion a, ReleaseVersion b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if ((a is null) || (b is null))
            {
                return false;
            }

            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether two specified <see cref="ReleaseVersion"/> objects are equal.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> equals <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator ==(ReleaseVersion a, ReleaseVersion b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Determines whether two specified <see cref="ReleaseVersion"/> objects are unequal.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> does not equal <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator !=(ReleaseVersion a, ReleaseVersion b)
        {
            return !Equals(a, b);
        }

        /// <summary>
        /// Determines whether the first <see cref="ReleaseVersion"/> object is greater than the 
        /// second <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is greater than <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator >(ReleaseVersion a, ReleaseVersion b)
        {
            return Compare(a, b) > 0;
        }

        /// <summary>
        /// Determines whether the first <see cref="ReleaseVersion"/> object is greater than or equal to the 
        /// second <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is greater than or equal to <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator >=(ReleaseVersion a, ReleaseVersion b)
        {
            return Compare(a, b) >= 0;
        }

        /// <summary>
        /// Determines whether the first <see cref="ReleaseVersion"/> object is less than the 
        /// second <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is less than <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator <(ReleaseVersion a, ReleaseVersion b)
        {
            return Compare(a, b) < 0;
        }

        /// <summary>
        /// Determines whether the first <see cref="ReleaseVersion"/> object is less than or equal to the 
        /// second <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is less than or equal to <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator <=(ReleaseVersion a, ReleaseVersion b)
        {
            return Compare(a, b) <= 0;
        }
        #endregion

        internal static int CompareIdentifiers(string identifierA, string identifierB)
        {
            // https://semver.org/#spec-item-11
            // A non-prerelease version has a higher precendence than pre-release, e.g. 1.0.0 and 1.0.0-preview2
            if (string.IsNullOrEmpty(identifierA))
            {
                return string.IsNullOrEmpty(identifierB) ? 0 : 1;
            }

            if (string.IsNullOrEmpty(identifierB))
            {
                return -1;
            }

            var dotPartsA = identifierA.Split('.');
            var dotPartsB = identifierB.Split('.');

            int minParts = dotPartsA.Length < dotPartsB.Length ? dotPartsA.Length : dotPartsB.Length;

            for (int i = 0; i < minParts; i++)
            {
                // Can't use int.TryParse, e.g. 2147483648 will fail to convert, return false and not set the out
                // paramenter.
                bool isNumericIdentifierA = Regex.IsMatch(dotPartsA[i], @"^\d+$");
                bool isNumericIdentifierB = Regex.IsMatch(dotPartsB[i], @"^\d+$");

                int compareResult = 0;

                if (isNumericIdentifierA && isNumericIdentifierB)
                {
                    // Identifiers consisting of only digits are compared numerically
                    compareResult = CompareNumericIdentifier(dotPartsA[i], dotPartsB[i]);

                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
                else
                {
                    // Identifiers with letters or hyphens are compared lexically in ASCII sort order.
                    // Numeric identifiers always have lower precedence than non-numeric identifiers.
                    if (isNumericIdentifierA)
                    {
                        return -1;
                    }

                    if (isNumericIdentifierB)
                    {
                        return 1;
                    }

                    compareResult = string.CompareOrdinal(dotPartsA[i], dotPartsB[i]);

                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            // A larger set of pre-release fields has a higher precedence than a smaller set, if all
            // preceding identifiers are equal.
            return dotPartsA.Length.CompareTo(dotPartsB.Length);
        }

        internal static int CompareNumericIdentifier(string a, string b)
        {
            if (string.CompareOrdinal(a, b) == 0)
            {
                return 0;
            }

            if (a.Length > b.Length)
            {
                return 1;
            }

            if (b.Length < a.Length)
            {
                return -1;
            }

            // At this point, we have strings of equal length
            // so we'll traverse the number backwards and flip
            // between 1 and -1 depending on the ordinal value
            // of the character at the current index.
            int r = 0;

            for (int i = a.Length - 1; i > 0; i--)
            {
                int r2 = a[i].CompareTo(b[i]);

                if (r2 != 0)
                {
                    r = r2 / Math.Abs(r2);
                }
            }

            return r;
        }
    }
}
