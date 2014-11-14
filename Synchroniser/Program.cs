using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MulticamRecorder
{
    class Program
    {
        static void Main(string[] args)
        {
            TestAlg(args[0], args[1]);
            ConsoleHelper.waitForKey();
        }


        static long timestampFromFilename(string filename)
        {
            int START = filename.Length - 13 - 4;
            int LENGTH = 13;
            return long.Parse(filename.Substring(START, LENGTH));
        }


        static void TestAlg(string pathLeft, string pathRight)
        {
            IEnumerable<String> imagesLeft = Directory.EnumerateFiles(pathLeft);
            IEnumerable<String> imagesRight = Directory.EnumerateFiles(pathRight);

            List<long> timestampsLeft = imagesLeft.Select(timestampFromFilename).ToList();
            List<long> timestampsRight = imagesRight.Select(timestampFromFilename).ToList();

            foreach (Tuple<int, int> pair in matchTimestamps(timestampsLeft, timestampsRight)) 
            {
                Console.WriteLine("Matched pair: {0} -- {1}",
                    timestampsLeft[pair.Item1],
                    timestampsRight[pair.Item2]);
            }
        }

        private static List<Tuple<int, int>> matchTimestamps(List<long> timestamps0, List<long> timestamps1)
        {
            Dictionary<int, int> closestNeighborOfT0 = closest(timestamps0, timestamps1, false);
            Dictionary<int, int> closestNeighborOfT1 = closest(timestamps1, timestamps0, true);

            return makeMatching(closestNeighborOfT0, closestNeighborOfT1);
        }

        private static List<Tuple<int, int>> makeMatching(Dictionary<int, int> ts0, Dictionary<int, int> ts1)
        {
            List<Tuple<int, int>> matching = new List<Tuple<int, int>>();
            for (int i = 0; i < ts0.Count; i++)
            {
                if (ts1[ts0[i]] == i)
                    matching.Add(new Tuple<int, int>(i, ts0[i]));
            }
            return matching;
        }

        private static Dictionary<int, int> closest(List<long> values, List<long> neighbours, bool equalityAllowed)
        {
            Dictionary<int, int> closest = new Dictionary<int, int>();
            int search = 0;
            for (int i = 0; i < values.Count; i++) 
            {
                while (search + 1 < neighbours.Count &&
                    nextCloserOrEqualThanCurrent(values[i], neighbours[search], neighbours[search + 1], equalityAllowed))
                    search++;
                closest[i] = search;
            }
                
            return closest;
        }

        private static bool nextCloserOrEqualThanCurrent(long value, long current, long next, bool equalityAllowed)
        {
            if (equalityAllowed)
                return distance(next, value) <= distance(current, value);
            else
                return distance(next, value) < distance(current, value);
        }

        private static long distance(long ts0, long ts1)
        {
            long diff = checked(ts0 - ts1);
            return diff < 0 ? -diff : diff;
        } 
    }
}
