// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

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
        private const string PrereleasePattern =
            @"(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*)";

        /// <summary>
        /// Regular expression for capturing the build-metadata part of a semantic version.
        /// </summary>
        private const string BuildMetadataPattern =
            @"(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*)";

        /// <summary>
        /// A regular expression pattern to capture a semantic version (2.0).
        /// </summary>
        public static readonly string SemanticVersionPattern =
            $@"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-{PrereleasePattern})?(?:\+{BuildMetadataPattern})?$";

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
        }

        /// <summary>
        /// The minor version number.
        /// </summary>
        public int Minor
        {
            get;
            private set;
        }

        /// <summary>
        /// The patch number.
        /// </summary>
        public int Patch
        {
            get;
            private set;
        }

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
        /// The value is derived from the hundreds group of the version's patch field (third part of the version number).
        /// For example, if the SDK version is 2.1.809, the feature band is 800.
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
            ReleaseVersion v = Parse(version);
            Major = v.Major;
            Minor = v.Minor;
            Patch = v.Patch;
            Prerelease = v.Prerelease;
            BuildMetadata = v.BuildMetadata;
            SdkFeatureBand = v.SdkFeatureBand;
            SdkPatchLevel = v.SdkPatchLevel;
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
                throw new ArgumentOutOfRangeException(nameof(major), ReleasesResources.VersionPartLessThanZero);
            }

            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minor), ReleasesResources.VersionPartLessThanZero);
            }

            if (patch < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(patch), ReleasesResources.VersionPartLessThanZero);
            }

            Major = major;
            Minor = minor;
            Patch = patch;

            if (prerelease != null)
            {
                string[] dotSeparatedPrereleaseIdentifiers = prerelease.Split('.');

                if (dotSeparatedPrereleaseIdentifiers.Length > 0)
                {
                    for (int i = 0; i < dotSeparatedPrereleaseIdentifiers.Length; i++)
                    {
                        if (!IsPrereleaseIdentifier(dotSeparatedPrereleaseIdentifiers[i]))
                        {
                            throw new FormatException(string.Format(ReleasesResources.InvalidPrerelease, prerelease));
                        }
                    }
                }
            }

            Prerelease = prerelease;

            if (buildMetadata != null)
            {
                string[] dotSeperatedBuildIdentifiers = buildMetadata.Split('.');

                if (dotSeperatedBuildIdentifiers.Length > 0)
                {
                    for (int i = 0; i < dotSeperatedBuildIdentifiers.Length; ++i)
                    {
                        if (!IsBuildIdentifier(dotSeperatedBuildIdentifiers[i]))
                        {
                            throw new FormatException(string.Format(ReleasesResources.InvalidBuildMetadata, buildMetadata));
                        }
                    }
                }
            }

            BuildMetadata = buildMetadata;
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
        /// <param name="obj">An object to compare or null.</param>
        /// <returns>
        /// Returns a signed integer indicating whether this instance precedes, follows or appears in the same position in 
        /// the sort order as the specified <paramref name="obj"/>.
        /// </returns>
        public int CompareTo(object obj) => CompareTo((ReleaseVersion)obj);

        /// <summary>
        /// Compares this instance to the specified object and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">An object to compare.</param>
        /// <returns></returns>
        public int CompareTo(ReleaseVersion other)
        {
            int result = ComparePrecedenceTo(other);
            if (result != 0)
            {
                return result;
            }

            return CompareIdentifiers(BuildMetadata, other.BuildMetadata);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare to the current object.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object; <see langword="false"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is ReleaseVersion releaseVersion))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            ReleaseVersion other = releaseVersion;

            return Major == other.Major && Minor == other.Minor && Patch == other.Patch &&
                string.Equals(Prerelease, other.Prerelease, StringComparison.Ordinal) &&
                string.Equals(BuildMetadata, other.BuildMetadata, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="ReleaseVersion"/> object and a specified
        /// <see cref="ReleaseVersion"/> object represent the same value.
        /// </summary>
        /// <param name="other">The <see cref="ReleaseVersion"/> object to compare to this instance, or <see langword="null" />.</param>
        /// <returns><see langword="true"/> if every component of this <see cref="ReleaseVersion"/> matches the corresponding
        /// component of the <paramref name="other"/> parameter; <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(ReleaseVersion other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Major == other.Major && Minor == other.Minor && Patch == other.Patch &&
                string.Equals(Prerelease, other.Prerelease, StringComparison.Ordinal) &&
                string.Equals(BuildMetadata, other.BuildMetadata, StringComparison.Ordinal);
        }

        /// <summary>
        /// The default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Major ^ Minor ^ Patch ^ (!string.IsNullOrWhiteSpace(Prerelease) ? Prerelease.GetHashCode() : 0) ^
                (!string.IsNullOrWhiteSpace(BuildMetadata) ? BuildMetadata.GetHashCode() : 0);
        }

        /// <summary>
        /// Determines whether this instance and another <see cref="ReleaseVersion"/> share the same precedence.
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <returns>
        /// <see langword="true"/> if both values have the same sort order; <see langword="false"/> otherwise.
        /// </returns>
        public bool PrecedenceEquals(ReleaseVersion value) => ComparePrecedenceTo(value) == 0;

        /// <summary>
        /// Returns a <see cref="string"/> representation of this <see cref="ReleaseVersion"/>.
        /// </summary>
        /// <returns>The <see cref="string"/> representation of this <see cref="ReleaseVersion"/>.</returns>
        public override string ToString()
        {
            string v = $"{Major}.{Minor}.{Patch}";
            v += !string.IsNullOrWhiteSpace(Prerelease) ? $"-{Prerelease}" : string.Empty;
            v += !string.IsNullOrWhiteSpace(BuildMetadata) ? $"+{BuildMetadata}" : string.Empty;

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
            value += fieldCount >= 4 && !string.IsNullOrWhiteSpace(Prerelease) ? $"-{Prerelease}" : string.Empty;
            value += fieldCount >= 5 && !string.IsNullOrWhiteSpace(BuildMetadata) ? $"+{BuildMetadata}" : string.Empty;

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

            if (a is null || b is null)
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
        public static bool operator ==(ReleaseVersion a, ReleaseVersion b) => Equals(a, b);

        /// <summary>
        /// Determines whether two specified <see cref="ReleaseVersion"/> objects are unequal.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> does not equal <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator !=(ReleaseVersion a, ReleaseVersion b) => !Equals(a, b);

        /// <summary>
        /// Determines whether the first <see cref="ReleaseVersion"/> object is greater than the 
        /// second <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is greater than <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator >(ReleaseVersion a, ReleaseVersion b) => Compare(a, b) > 0;

        /// <summary>
        /// Determines whether the first <see cref="ReleaseVersion"/> object is greater than or equal to the 
        /// second <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is greater than or equal to <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator >=(ReleaseVersion a, ReleaseVersion b) => Compare(a, b) >= 0;

        /// <summary>
        /// Determines whether the first <see cref="ReleaseVersion"/> object is less than the 
        /// second <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is less than <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator <(ReleaseVersion a, ReleaseVersion b) => Compare(a, b) < 0;

        /// <summary>
        /// Determines whether the first <see cref="ReleaseVersion"/> object is less than or equal to the 
        /// second <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="a">The first <see cref="ReleaseVersion"/> object.</param>
        /// <param name="b">The second <see cref="ReleaseVersion"/> object.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is less than or equal to <paramref name="b"/>;otherwise <see langword="false"/>.</returns>
        public static bool operator <=(ReleaseVersion a, ReleaseVersion b) => Compare(a, b) <= 0;
        #endregion

        internal static int CompareIdentifiers(string identifierA, string identifierB)
        {
            // https://semver.org/#spec-item-11
            // A non-prerelease version has a higher precendence than pre-release, e.g. 1.0.0 and 1.0.0-preview2
            if (string.IsNullOrWhiteSpace(identifierA))
            {
                return string.IsNullOrWhiteSpace(identifierB) ? 0 : 1;
            }

            if (string.IsNullOrWhiteSpace(identifierB))
            {
                return -1;
            }

            string[] dotPartsA = identifierA.Split('.');
            string[] dotPartsB = identifierB.Split('.');

            int minParts = dotPartsA.Length < dotPartsB.Length ? dotPartsA.Length : dotPartsB.Length;

            for (int i = 0; i < minParts; i++)
            {
                // Can't use int.TryParse, e.g. 2147483648 will fail to convert, return false and not set the out
                // paramenter.
                bool isNumericIdentifierA = IsDigits(dotPartsA[i]);
                bool isNumericIdentifierB = IsDigits(dotPartsB[i]);

                int compareResult;
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

            if (a.Length < b.Length)
            {
                return -1;
            }

            // At this point, we have strings of equal length
            // so we'll traverse the number backwards and flip
            // between 1 and -1 depending on the ordinal value
            // of the character at the current index.
            int r = 0;
            for (int i = a.Length - 1; i >= 0; i--)
            {
                int r2 = a[i].CompareTo(b[i]);
                if (r2 != 0)
                {
                    r = r2 / Math.Abs(r2);
                }
            }

            return r;
        }

        /// <summary>
        /// Converts the string representation of a version to an equivalent <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="input">A string that contains a version to convert.</param>
        /// <returns>An object that is equivalent to the version number specified in the <paramref name="input"/> parameter.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="FormatException" />
        public static ReleaseVersion Parse(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!TryParse(input, canThrow: true, out ReleaseVersion parsedVersion))
            {
                throw new FormatException(ReleasesResources.InvalidReleaseVersion);
            }

            return parsedVersion;
        }

        /// <summary>
        /// Tries to convert the string representation of a version to an equivalent <see cref="ReleaseVersion"/> object,
        /// and returns a value indicating whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string that contains a version to convert.</param>
        /// <param name="result">Contains the <see cref="ReleaseVersion"/> equivalent of the value in <paramref name="input"/> if
        /// successful; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the string could be converted; <see langword="false"> otherwise.</see> </returns>
        public static bool TryParse(string input, out ReleaseVersion result)
        {
            return TryParse(input, canThrow: false, out result);
        }

        /// <summary>
        /// Determines whether a string is a valid numeric identifier.
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns><see langword="true"/> if the string is a numeric identifier; <see langword="false"> otherwise.</see></returns>
        internal static bool IsNumericIdentifier(string input)
        {
            // <numeric identifier> ::= "0" | <positive digit> | <positive digit> <digits>
            if (string.IsNullOrWhiteSpace(input) || (input[0] == '0' && input.Length > 1))
            {
                return false;
            }

            for (int i = 0; i < input.Length; i++)
            {
                if (!char.IsDigit(input[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if a string only contains digits.
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns><see langword="true"/> if the string only contains digits; <see langword="false"> otherwise.</see></returns>
        internal static bool IsDigits(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            for (int i = 0; i < input.Length; ++i)
            {
                if (!char.IsDigit(input[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether a string is a valid build identifier.
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns><see langword="true"/> if the string is a build identifier; <see langword="false"> otherwise.</see></returns>
        internal static bool IsBuildIdentifier(string input)
        {
            return IsAlphaNumericIdentifier(input) || IsDigits(input);
        }

        /// <summary>
        /// Determines whether a string is a valid alphanumeric identifier.
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns><see langword="true"/> if the string is an alphanumeric identifier; <see langword="false"> otherwise.</see></returns>
        internal static bool IsAlphaNumericIdentifier(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            for (int i = 0; i < input.Length; i++)
            {
                if (!(char.IsLetterOrDigit(input[i]) || input[i] == '-'))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether a string is a valid prelease identifier.
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns><see langword="true"/> if the string is a prerelease identifier; <see langword="false"> otherwise.</see></returns>
        internal static bool IsPrereleaseIdentifier(string input)
        {
            return IsNumericIdentifier(input) || IsAlphaNumericIdentifier(input);
        }


        /// <summary>
        /// Tries to convert the string representation of a version to an equivalent <see cref="ReleaseVersion"/> object,
        /// and returns a value indicating whether the conversion succeeded. 
        /// </summary>
        /// <param name="input">A string that contains a version to convert.</param>
        /// <param name="version">Contains the <see cref="ReleaseVersion"/> equivalent of the value in <paramref name="input"/> if
        /// successful; otherwise <see langword="null"/>.</param>
        /// <param name="canThrow">A boolean indicating whether an exception should be raised if the conversion failed.</param>
        /// <returns><see langword="true"/> if the string could be converted; <see langword="false"> otherwise.</see> </returns>
        /// <exception cref="FormatException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        internal static bool TryParse(string input, bool canThrow, out ReleaseVersion version)
        {
            version = null;

            try
            {
                string build = null;
                string prerelease = null;
                string versionCore = null;

                int buildSeparatorIndex = input.IndexOf('+');
                int prereleaseSeparatorIndex = input.IndexOf('-');

                if (buildSeparatorIndex == -1)
                {
                    if (prereleaseSeparatorIndex == -1)
                    {
                        // <valid semver> ::= <version core>
                        versionCore = input;
                    }
                    else
                    {
                        // <valid semver> ::= <version core> "-" <pre-release>
                        if (prereleaseSeparatorIndex + 1 > input.Length)
                        {
                            throw new FormatException();
                        }

                        prerelease = input.Substring(prereleaseSeparatorIndex + 1);
                        versionCore = input.Substring(0, prereleaseSeparatorIndex);
                    }
                }
                else
                {
                    if (prereleaseSeparatorIndex == -1 || prereleaseSeparatorIndex > buildSeparatorIndex)
                    {
                        // <valid semver> ::= <version core> "+" <build>
                        if (buildSeparatorIndex + 1 > input.Length)
                        {
                            throw new FormatException();
                        }

                        build = input.Substring(buildSeparatorIndex + 1);
                        versionCore = input.Substring(0, buildSeparatorIndex);
                    }
                    else
                    {
                        // <valid semver> ::= <version core> "-" <pre-release> "+" <build>
                        if (prereleaseSeparatorIndex + 1 < buildSeparatorIndex)
                        {
                            if (buildSeparatorIndex + 1 > input.Length)
                            {
                                throw new FormatException();
                            }

                            build = input.Substring(buildSeparatorIndex + 1);
                            prerelease = input.Substring(prereleaseSeparatorIndex + 1, buildSeparatorIndex - prereleaseSeparatorIndex - 1);
                            versionCore = input.Substring(0, prereleaseSeparatorIndex);
                        }
                        else
                        {
                            throw new FormatException();
                        }
                    }
                }

                string[] versionParts = versionCore.Split('.');

                if (versionParts.Length != 3)
                {
                    // <version core> ::= <major> "." <minor> "." <patch>
                    throw new FormatException(string.Format(ReleasesResources.InvalidVersion, versionCore));
                }

                int[] versions = new int[3] { 0, 0, 0 };

                for (int i = 0; i < 3; i++)
                {
                    if (!TryParseCoreVersionPart(versionParts[i], canThrow, out versions[i]))
                    {
                        return false;
                    }
                }

                version = new ReleaseVersion(versions[0], versions[1], versions[2], prerelease, build);

                return true;
            }
            catch
            {
                if (canThrow)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Tries to convert a part of the core version to an integer.
        /// </summary>
        /// <param name="input">A string containing the major, minor, or patch value of the version.</param>
        /// <param name="canThrow">A boolean indicating whether an exception should be raised if the conversion failed.</param>
        /// <param name="value">The converted value.</param>
        /// <returns><see langword="true"/> if the string could be converted; <see langword="false"> otherwise.</see> </returns>
        /// <exception cref="FormatException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        internal static bool TryParseCoreVersionPart(string input, bool canThrow, out int value)
        {
            value = -1;

            try
            {
                if (!IsNumericIdentifier(input))
                {
                    throw new FormatException(string.Format(ReleasesResources.InvalidNumericIdentifier, input));
                }

                value = int.Parse(input);

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(input), ReleasesResources.VersionPartLessThanZero);
                }
            }
            catch
            {
                if (canThrow)
                {
                    throw;
                }

                return false;
            }

            return true;
        }
    }
}
