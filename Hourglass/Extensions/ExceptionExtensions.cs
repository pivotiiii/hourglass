using System;
using System.Threading;

namespace Hourglass.Extensions;

internal static class ExceptionExtensions
{
    public static bool CanBeHandled(this Exception ex) =>
        ex is not (
            ArgumentNullException    or
            NullReferenceException   or
            IndexOutOfRangeException or
            OutOfMemoryException     or
            AccessViolationException or
            ThreadAbortException     or
            StackOverflowException
        );
}
