using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Utility extension methods for dealing with <see cref="AggregateException"/>
    /// </summary>
    public static class AggregateExceptionExtensions
    {
        /// <summary>
        ///  Unwraps and re-throws an underlying inner exception instead of generic <see cref="AggregateException"/>
        /// </summary>
        /// <remarks>The original stack trace will be preserved</remarks>
        /// <param name="task">Task which throws <see cref="AggregateException"/></param>
        /// <returns>Unwrapped task</returns>
        public static async Task UnwrapExceptions(this Task task)
        {
            try
            {
                await task;
            }
            catch (AggregateException e)
            {
                throw e.OriginalExceptionPreservingStackTrace();
            }
        }

        /// <summary>
        ///  Unwraps and re-throws an underlying inner exception instead of generic <see cref="AggregateException"/>
        /// </summary>
        /// <remarks>The original stack trace will be preserved</remarks>
        /// <typeparam name="T">Type of task result</typeparam>
        ///  <param name="task">Task which throws <see cref="AggregateException"/></param>
        /// <returns>Unwrapped task</returns>
        public static async Task<T> UnwrapExceptions<T>(this Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (AggregateException e)
            {
                throw e.OriginalExceptionPreservingStackTrace();
            }
        }

        /// <summary>
        /// Extracts original inner exception while preserving stack trace
        /// </summary>
        /// <param name="e">An instance of <see cref="AggregateException"/></param>
        /// <returns>New <see cref="Exception"/></returns>
        public static Exception OriginalExceptionPreservingStackTrace(this AggregateException e)
        {
            return PreserveStackTrace(OriginalException(e));
        }

        static Exception OriginalException(AggregateException e)
        {
            return e.Flatten().InnerExceptions.First();
        }

        static Exception PreserveStackTrace(Exception ex)
        {
            var remoteStackTraceString = typeof(Exception)
                .GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

            Debug.Assert(remoteStackTraceString != null);
            remoteStackTraceString.SetValue(ex, ex.StackTrace);

            return ex;
        }
    }
}
