using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Utils
{
    public static class CoroutineUtils
    {
        /// <summary>
        /// A delegate that processes a TSingleResult value and combines it with the TResults value. Called once per TSingleResult produced.
        /// </summary>
        /// <typeparam name="TResults">A combination of all TSingleResult values.</typeparam>
        /// <typeparam name="TSingleResult">A single piece of the TResults value.</typeparam>
        /// <param name="accumulatedResults">The partially completed TResults value to combine the TSingleResult value with.</param>
        /// <param name="singleResult">The TSingleResult value to combine with the TResults value.</param>
        public delegate void ProcessSingleResultDelegate<TResults, TSingleResult>(ref TResults accumulatedResults, ref TSingleResult singleResult);

        /// <summary>
        /// A delegate that processes the partially completed TResults value. Called each time the coroutine yields when it exceeds the frame time budget.
        /// </summary>
        /// <typeparam name="TResults">A combination of all TSingleResult values.</typeparam>
        /// <param name="accumulatedResults">The partially completed TResults value.</param>
        public delegate void ProcessAccumulatedResultsDelegate<TResults>(ref TResults accumulatedResults);

        /// <summary>
        /// A delegate that processes the completed TResults value. Called once when the work completes.
        /// </summary>
        /// <typeparam name="TResults">A combination of all TSingleResult values.</typeparam>
        /// <param name="results">The completed TResults value.</param>
        public delegate void ProcessResultsDelegate<TResults>(ref TResults results);

        /// <summary>
        /// Run this as a coroutine to do heavy work across multiple frames, if it takes longer than the frame time budget.
        /// </summary>
        /// <typeparam name="TResults">A combination of all TSingleResult values.</typeparam>
        /// <typeparam name="TSingleResult">A single piece of the TResults value.</typeparam>
        /// <param name="initialResults">Initial value of the results container.</param>
        /// <param name="sequence">The generator sequence that performs the heavy work that produces TSingleResult.</param>
        /// <param name="processSingleResultFunc">A delegate that processes a TSingleResult value and combines it with the TResults value. Called once per TSingleResult produced.</param>
        /// <param name="processResultsFunc">A delegate that processes the completed TResults value. Called once when the work completes.</param>
        /// <param name="frameTimeBudget">How many milliseconds to allow the generator sequence to run for each frame.</param>
        /// <returns>A IEnumerator to be run as a Unity coroutine.</returns>
        public static IEnumerator FrameTimeBudgettedCoroutine<TResults, TSingleResult>(TResults initialResults, IEnumerable<TSingleResult> sequence, ProcessSingleResultDelegate<TResults, TSingleResult> processSingleResultFunc, ProcessResultsDelegate<TResults> processResultsFunc, double frameTimeBudget)
        {
            return FrameTimeBudgettedCoroutine(initialResults, sequence, processSingleResultFunc, null, processResultsFunc, frameTimeBudget);
        }

        /// <summary>
        /// Run this as a coroutine to do heavy work across multiple frames, if it takes longer than the frame time budget.
        /// </summary>
        /// <typeparam name="TResults">A combination of all TSingleResult values.</typeparam>
        /// <typeparam name="TSingleResult">A single piece of the TResults value.</typeparam>
        /// <param name="initialResults">Initial value of the results container.</param>
        /// <param name="sequence">The generator sequence that performs the heavy work that produces TSingleResult.</param>
        /// <param name="processSingleResultFunc">A delegate that processes a TSingleResult value and combines it with the TResults value. Called once per TSingleResult produced.</param>
        /// <param name="processAccumulatedResultsFunc">A delegate that processes the partially completed TResults value. Called each time the coroutine yields when it exceeds the frame time budget.</param>
        /// <param name="processResultsFunc">A delegate that processes the completed TResults value. Called once when the work completes.</param>
        /// <param name="frameTimeBudget">How many milliseconds to allow the generator sequence to run for each frame.</param>
        /// <returns>A IEnumerator to be run as a Unity coroutine.</returns>
        public static IEnumerator FrameTimeBudgettedCoroutine<TResults, TSingleResult>(TResults initialResults, IEnumerable<TSingleResult> sequence, ProcessSingleResultDelegate<TResults, TSingleResult> processSingleResultFunc, ProcessAccumulatedResultsDelegate<TResults> processAccumulatedResultsFunc, ProcessResultsDelegate<TResults> processResultsFunc, double frameTimeBudget)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            double lastElapsed = 0L;
            TResults results = initialResults;
            IEnumerator<TSingleResult> enumerator = sequence.GetEnumerator();

            // Calculate next until sequence ends
            while (enumerator.MoveNext())
            {
                // Time spent in this frame
                double frameTimeSpent = stopwatch.Elapsed.TotalMilliseconds - lastElapsed;

                // If exceeded frame time budget
                if (frameTimeSpent >= frameTimeBudget)
                {
                    // Invoke callback for handling partial results
                    if (processAccumulatedResultsFunc != null)
                    {
                        processAccumulatedResultsFunc(ref results);
                    }
                    
                    // Wait for next frame
                    yield return null;

                    // Remember when we finished waiting for next frame
                    lastElapsed = stopwatch.ElapsedMilliseconds;
                }

                TSingleResult singleResult = enumerator.Current;

                // Invoke callback for handling single result
                if(processSingleResultFunc != null)
                {
                    processSingleResultFunc(ref results, ref singleResult);
                }
            }
            
            stopwatch.Stop();

            // Invoke callback for handling completed results
            if (processResultsFunc != null)
            {
                processResultsFunc(ref results);
            }
        }
    }
}
