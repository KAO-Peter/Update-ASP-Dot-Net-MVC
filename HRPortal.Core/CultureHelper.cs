using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HRPortal.Core;
    public static class CultureHelper
    {
        // Valid cultures from CultureInfo.GetCultures(CultureTypes.AllCultures)
        private static readonly HashSet<string> _validCultures = CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Select(c => c.Name)
            .Where(n => !string.IsNullOrEmpty(n))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// List of cultures actively supported by the application.
        /// </summary>
        /// <remarks>
        /// Add additional cultures here as they are implemented.
        /// </remarks>
        private static readonly IReadOnlyList<string> _cultures = new[] { "zh-TW" };

        /// <summary>
        /// Determines if the current culture uses right-to-left text direction.
        /// </summary>
        /// <returns>True if the current culture uses right-to-left text direction; otherwise, false.</returns>
        public static bool IsRighToLeft() => 
            CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
        /// <summary>
        /// Gets a valid implemented culture name based on the provided culture name.
        /// </summary>
        /// <param name="name">The culture name to validate (e.g., "en-US", "zh-TW").</param>
        /// <returns>
        /// A valid implemented culture name. If the provided culture is not valid or not implemented,
        /// returns the default culture.
        /// </returns>
        /// <remarks>
        /// This method attempts to find the best match for the requested culture:
        /// 1. If the exact culture is implemented, it is returned
        /// 2. If a culture with the same language prefix is implemented, it is returned
        /// 3. If no match is found, the default culture is returned
        /// </remarks>
        public static string GetImplementedCulture(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            // Check if the culture is valid
            if (!_validCultures.Contains(name))
                return GetDefaultCulture();

            // Check if the culture is directly implemented
            if (_cultures.Contains(name, StringComparer.OrdinalIgnoreCase))
                return name;

            // Try to find a close match based on the neutral culture
            var neutralCulture = GetNeutralCulture(name);
            return _cultures.FirstOrDefault(c => 
                c.StartsWith(neutralCulture, StringComparison.OrdinalIgnoreCase)) 
                ?? GetDefaultCulture();
        }

        /// <summary>
        /// Gets the default culture for the application.
        /// </summary>
        /// <returns>The default culture name.</returns>
        /// <remarks>
        /// Returns the first culture from the list of implemented cultures.
        /// </remarks>
        public static string GetDefaultCulture()
        {
            if (_cultures.Count == 0)
            {
                throw new InvalidOperationException("No cultures are implemented in the application.");
            }
            return _cultures[0];
        }

        /// <summary>
        /// Gets the current culture name from the thread's current culture.
        /// </summary>
        /// <returns>The current culture name.</returns>
        public static string GetCurrentCulture() =>
            CultureInfo.CurrentCulture.Name;

        /// <summary>
        /// Gets the neutral culture name from the current thread's culture.
        /// </summary>
        /// <returns>The neutral culture name (e.g., "en" from "en-US").</returns>
        public static string GetCurrentNeutralCulture() =>
            GetNeutralCulture(CultureInfo.CurrentCulture.Name);

        /// <summary>
        /// Extracts the neutral culture name from a specific culture name.
        /// </summary>
        /// <param name="name">The culture name to get the neutral culture from.</param>
        /// <returns>
        /// The neutral culture name (e.g., "en" from "en-US").
        /// If no specific culture is specified (no hyphen), returns the original name.
        /// </returns>
        public static string GetNeutralCulture(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            return !name.Contains('-') ? name : name.Split('-')[0];
        }
    }