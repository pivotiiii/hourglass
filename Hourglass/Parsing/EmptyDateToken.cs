// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmptyDateToken.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Parsing;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Represents an unspecified date.
/// </summary>
public sealed class EmptyDateToken : DateToken
{
    /// <summary>
    /// Gets a value indicating whether the token is valid.
    /// </summary>
    public override bool IsValid => true;

    /// <summary>
    /// Returns the next date after <paramref name="minDate"/> that is represented by this token.
    /// </summary>
    /// <remarks>
    /// This method may return a date that is before <paramref name="minDate"/> if there is no date after <paramref
    /// name="minDate"/> that is represented by this token.
    /// </remarks>
    /// <param name="minDate">The minimum date to return. The time part is ignored.</param>
    /// <param name="inclusive">A value indicating whether the returned date should be on or after rather than
    /// strictly after <paramref name="minDate"/>.</param>
    /// <returns>The next date after <paramref name="minDate"/> that is represented by this token.</returns>
    public override DateTime ToDateTime(DateTime minDate, bool inclusive)
    {
        ThrowIfNotValid();

        return inclusive ? minDate.Date : minDate.Date.AddDays(1);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <param name="provider">An <see cref="IFormatProvider"/> to use.</param>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString(IFormatProvider provider)
    {
        return string.Empty;
    }

    /// <summary>
    /// Parses <see cref="EmptyDateToken"/> strings.
    /// </summary>
    public new sealed class Parser : DateToken.Parser
    {
        /// <summary>
        /// Singleton instance of the <see cref="Parser"/> class.
        /// </summary>
        public static readonly Parser Instance = new();

        /// <summary>
        /// Prevents a default instance of the <see cref="Parser"/> class from being created.
        /// </summary>
        private Parser()
        {
        }

        /// <summary>
        /// Returns a value indicating whether this parser can be used in conjunction with a specified <see
        /// cref="TimeToken.Parser"/>.
        /// </summary>
        /// <param name="timeTokenParser">A <see cref="TimeToken.Parser"/>.</param>
        /// <returns>A value indicating whether this parser can be used in conjunction with the specified <see
        /// cref="TimeToken.Parser"/>.</returns>
        public override bool IsCompatibleWith(TimeToken.Parser timeTokenParser)
        {
            return timeTokenParser is not EmptyTimeToken.Parser;
        }

        /// <summary>
        /// Returns a set of regular expressions supported by this parser.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>A set of regular expressions supported by this parser.</returns>
        public override IEnumerable<string> GetPatterns(IFormatProvider provider)
        {
            yield return string.Empty;
        }

        /// <summary>
        /// Parses a <see cref="Match"/> into a <see cref="DateToken"/>.
        /// </summary>
        /// <param name="match">A <see cref="Match"/> representation of a <see cref="DateToken"/>.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="DateToken"/> parsed from the <see cref="Match"/>.</returns>
        protected override DateToken ParseInternal(Match match, IFormatProvider provider)
        {
            return new EmptyDateToken();
        }
    }
}