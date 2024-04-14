using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LethalLevelLoader
{
    internal static class DebugStopwatch
    {
        private static Dictionary<string, Stopwatch> stopWatchDict = new Dictionary<string, Stopwatch>();

        internal static void StartStopWatch(string newStopWatchText, bool stopPreviousStopWatch = true)
        {
            if (!stopWatchDict.ContainsKey(newStopWatchText))
            {
                if (stopPreviousStopWatch == true && stopWatchDict.Count > 0)
                    StopStopWatch(stopWatchDict.Keys.ToList()[stopWatchDict.Count - 1]);   
                Stopwatch newStopWatch = new Stopwatch();
                stopWatchDict.Add(newStopWatchText, newStopWatch);
                newStopWatch.Start();

            }
        }

        internal static void StopStopWatch(string stopWatchText)
        {
            if (!stopWatchDict.ContainsKey(stopWatchText))
                return;

            Stopwatch stopWatch = stopWatchDict[stopWatchText];
            stopWatch.Stop();
            DebugHelper.Log($"[Debug Stopwatch] {stopWatchText} : {stopWatch.Elapsed.TotalSeconds:0.##} Seconds. ({stopWatch.ElapsedMilliseconds}ms)", DebugType.IAmBatby);
            
            stopWatchDict.Remove(stopWatchText);
        }
    }
}
