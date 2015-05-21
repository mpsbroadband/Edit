﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Edit.AzureTableStorage
{
    public static class EnumExtentions
    {
        public static IEnumerable<T> OrderByAlphaNumeric<T>(this IEnumerable<T> items, Func<T, string> selector, StringComparer stringComparer = null)
        {
            var regex = new Regex(@"\d+", RegexOptions.Compiled);

            var maxDigits = items
                .SelectMany(
                    i => regex.Matches(selector(i)).Cast<Match>().Select(digitChunk => (int?) digitChunk.Value.Length))
                .Max() ?? 0;

            return items.OrderBy(i => regex.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')),
                stringComparer ?? StringComparer.CurrentCulture);
        }

    }
}
