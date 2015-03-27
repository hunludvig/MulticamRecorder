using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Synchronizer
{
    class Program
    {
        // Paths and folders that are used. These need to be changed accordingly in order to use the program.
        static string sourceDir = @"C:\path\MulticamRecorder-master\MulticamRecorder\bin\x86\Debug\pics\"; // Where the picture folders are, must end with \
        static string webcam = "webcam"; // Slow camera / Main camera folder name
        static string eyecam = "eyecam"; // Fast camera 1 folder name
        static string ps3cam = "ps3cam"; // Fast camera 2 folder name
        static string filetype = ".png"; // Picture filetype - They must all be the same.

        static void Main(string[] args)
        {            
            // Retrieve all the file names into arrays
            string[] webcamFiles = Directory.GetFiles(sourceDir + webcam).Select(path => Path.GetFileName(path)).ToArray();
            string[] eyecamFiles = Directory.GetFiles(sourceDir + eyecam).Select(path => Path.GetFileName(path)).ToArray();
            string[] ps3camFiles = Directory.GetFiles(sourceDir + ps3cam).Select(path => Path.GetFileName(path)).ToArray();

            // Create textfiles containing the frames of every camera, before modifying the data in any way is done
            string[] preSyncweb = Directory.GetFiles(sourceDir + webcam).Select(path => Path.GetFileName(path)).ToArray();
            string[] preSynceye = Directory.GetFiles(sourceDir + eyecam).Select(path => Path.GetFileName(path)).ToArray();
            string[] preSyncps3 = Directory.GetFiles(sourceDir + ps3cam).Select(path => Path.GetFileName(path)).ToArray();
            
            char[] MyChar = { 'j', 'p', 'g', 'n', '.', '-' }; // Characters to remove from the filename like .png and .jpg

            string[] lines = preSynceye;
            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].TrimEnd(MyChar);
            System.IO.File.WriteAllLines(sourceDir + "PreSyncEyecamFrames.txt", lines);

            lines = preSyncweb;
            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].TrimEnd(MyChar);
            System.IO.File.WriteAllLines(sourceDir + "PreSyncWebcamFrames.txt", lines);

            lines = preSyncps3;
            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].TrimEnd(MyChar);
            System.IO.File.WriteAllLines(sourceDir + "PreSyncPs3camFrames.txt", lines);

            // Removing frames that were taken before maincam was activated
            Boolean RemovingFrames = true;
            int removerInt = 0;
            int removedFrom1 = 0;
            int removedFrom2 = 0;
            while (RemovingFrames)
            {
                RemovingFrames = false;
                // Take filename without the suffix .png or .jpg and compare to the first main frame
                if (Convert.ToInt64(Regex.Match(eyecamFiles[removerInt], @"\d+").Value) <
                    Convert.ToInt64(Regex.Match(webcamFiles[0], @"\d+").Value))
                {
                    File.Delete(sourceDir + eyecam + "\\" + eyecamFiles[removerInt]);
                    RemovingFrames = true;
                    removedFrom1++;
                }

                if (Convert.ToInt64(Regex.Match(ps3camFiles[removerInt], @"\d+").Value) <
                    Convert.ToInt64(Regex.Match(webcamFiles[0], @"\d+").Value))
                {
                    File.Delete(sourceDir + ps3cam + "\\" + ps3camFiles[removerInt]);
                    RemovingFrames = true;
                    removedFrom2++;
                }

                removerInt++;
            }
            Console.WriteLine("Extra frames in the beginning were removed.");
            Console.WriteLine(removedFrom1 + " frames removed from " + eyecam + ".");
            Console.WriteLine(removedFrom2 + " frames removed from " + ps3cam + ".\n");

            // Create a review file that contains all the information that the console outputs
            File.Delete(sourceDir + "Review.txt");
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(sourceDir + "Review.txt", true))
            {
                file.WriteLine("Extra frames in the beginning were removed.");
                file.WriteLine(removedFrom1 + " frames removed from " + eyecam + ".");
                file.WriteLine(removedFrom2 + " frames removed from " + ps3cam + ".\n");
            }
                        
            eyecamFiles = Directory.GetFiles(sourceDir + eyecam).Select(path => Path.GetFileName(path)).ToArray(); // Refresh, files were deleted
            ps3camFiles = Directory.GetFiles(sourceDir + ps3cam).Select(path => Path.GetFileName(path)).ToArray(); // Refresh, files were deleted


            // Removing frames that were taken after maincam was deactivated
            RemovingFrames = true;
            int removerInt1 = eyecamFiles.Length - 1;
            int removerInt2 = ps3camFiles.Length - 1;
            removedFrom1 = 0;
            removedFrom2 = 0;
            while (RemovingFrames)
            {
                RemovingFrames = false;
                // Take filename without the suffix .png or .jpg and compare to the first main frame
                if (Convert.ToInt64(Regex.Match(eyecamFiles[removerInt1], @"\d+").Value) >
                    Convert.ToInt64(Regex.Match(webcamFiles[webcamFiles.Length - 1], @"\d+").Value))
                {
                    File.Delete(sourceDir + eyecam + "\\" + eyecamFiles[removerInt1]);
                    RemovingFrames = true;
                    removedFrom1++;
                }

                if (Convert.ToInt64(Regex.Match(ps3camFiles[removerInt2], @"\d+").Value) >
                    Convert.ToInt64(Regex.Match(webcamFiles[webcamFiles.Length - 1], @"\d+").Value))
                {
                    File.Delete(sourceDir + ps3cam + "\\" + ps3camFiles[removerInt2]);
                    RemovingFrames = true;
                    removedFrom2++;
                }

                removerInt1--;
                removerInt2--;
            }
            Console.WriteLine("Extra frames in the end were removed.");
            Console.WriteLine(removedFrom1 + " frames removed from " + eyecam + ".");
            Console.WriteLine(removedFrom2 + " frames removed from " + ps3cam + ".\n");

            // Update the review file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(sourceDir + "Review.txt", true))
            {
                file.WriteLine("Extra frames in the end were removed.");
                file.WriteLine(removedFrom1 + " frames removed from " + eyecam + ".");
                file.WriteLine(removedFrom2 + " frames removed from " + ps3cam + ".\n");
            }
                        
            eyecamFiles = Directory.GetFiles(sourceDir + eyecam).Select(path => Path.GetFileName(path)).ToArray(); // Refresh, files were deleted
            ps3camFiles = Directory.GetFiles(sourceDir + ps3cam).Select(path => Path.GetFileName(path)).ToArray(); // Refresh, files were deleted
            
            // Start comparing the frames now that extras are removed                     
            framePairsInPs3cam = new long[ps3camFiles.Length]; // Gather the pairs into these
            framePairsInEyecam = new long[eyecamFiles.Length]; // Gather the pairs into these
            
            PairFrames(sourceDir + eyecam, sourceDir + ps3cam); // This calls the method that pairs the fast camera frames in the best possible way
            // This and other methods used for this can be found at the end of this file
            
            // Check which one had less frames to begin with and use that as the base length for the averageArray
            if (framePairsInPs3cam.Length < framePairsInEyecam.Length)
                averageValuesOfFramePairs = new long[framePairsInPs3cam.Length];
            else
                averageValuesOfFramePairs = new long[framePairsInEyecam.Length];

            // Calculate the averages of the frame pairs
            for (int i = 0; i < averageValuesOfFramePairs.Length; i++)
                averageValuesOfFramePairs[i] = (framePairsInEyecam[i] + framePairsInPs3cam[i]) / 2; // Flooring in the calculation
            
            // There are probably less pairs than the max length can hold so there are zeroes at the end of the array, take note of these
            int disregard = 0;
            for (int i = 0; i < averageValuesOfFramePairs.Length; i++)
            {
                //Console.WriteLine("averageValuesOfFramePairs[+" + i + "]  - value: " + averageValuesOfFramePairs[i]); // for debugging, average values
                if (averageValuesOfFramePairs[i] == 0)
                    disregard++;
            }
            //Console.WriteLine("Disregard: " + disregard); // for debugging, how many zeroes are in the end
            
            
            // Now we have arrays of the pairs and a third array of the average values between those
            // We will use the averages later to match the pairs to slow frames
            // First lets remove all the fast frames that were not paired and are now extra
            
            int kaunter = 0;
            int deletedFromEye = 0;
            for (int i = 0; i < eyecamFiles.Length; i++ )
            {
                if (Convert.ToInt64(Regex.Match(eyecamFiles[i], @"\d+").Value) != framePairsInEyecam[kaunter])
                {
                    File.Delete(sourceDir + eyecam + "\\" + (Convert.ToInt64(Regex.Match(eyecamFiles[i], @"\d+").Value) + filetype));
                    deletedFromEye++;
                }
                else
                    kaunter++;
            }

            kaunter = 0;
            int deletedFromPs3 = 0;
            for (int i = 0; i < ps3camFiles.Length; i++)
            {
                if (Convert.ToInt64(Regex.Match(ps3camFiles[i], @"\d+").Value) != framePairsInPs3cam[kaunter])
                {
                    File.Delete(sourceDir + ps3cam + "\\" + (Convert.ToInt64(Regex.Match(ps3camFiles[i], @"\d+").Value) + filetype));
                    deletedFromPs3++;
                }
                else
                    kaunter++;
            }
            

            // Next we compare what slow frames we can pair with the average frames
                        
            long main1 = 0;
            long main2 = 0;
            long difference1 = 0;
            long difference2 = 0;
            int maincamcounter = 0;
            int cam1counter = 0;
            int[] framesToCopyFromAverage = new int[webcamFiles.Length];
                        
            while (true)
            {
                main1 = Convert.ToInt64(Regex.Match(webcamFiles[maincamcounter], @"\d+").Value);
                if (maincamcounter + 1 < webcamFiles.Length)
                    main2 = Convert.ToInt64(Regex.Match(webcamFiles[maincamcounter + 1], @"\d+").Value);
                else
                {
                    framesToCopyFromAverage[maincamcounter] = averageValuesOfFramePairs.Length - disregard - cam1counter;
                    break;
                }

                difference1 = Math.Abs(averageValuesOfFramePairs[cam1counter] - main1);
                difference2 = Math.Abs(averageValuesOfFramePairs[cam1counter] - main2);
                
                if (difference1 < difference2) // Belongs to the earlier frame
                {
                    framesToCopyFromAverage[maincamcounter]++;
                    cam1counter++;
                }
                else                
                    maincamcounter++;                
            }

            //for (int i = 0; i < framesToCopyFromAverage.Length; i++ ) // for debugging
            //    Console.WriteLine(framesToCopyFromAverage[i]);
            

            // Now we know what pairs go with what slow frames so we can delete the unpaired slow frames
            int deletedFromWeb = 0;
            for (int i = 0; i < framesToCopyFromAverage.Length; i++)
            {
                if (framesToCopyFromAverage[i] == 0)
                {
                    File.Delete(sourceDir + webcam + "\\" + (Convert.ToInt64(Regex.Match(webcamFiles[i], @"\d+").Value) + filetype));
                    deletedFromWeb++;
                }
            }
            Console.WriteLine("Frames that did not result in triplets were deleted.");
            Console.WriteLine(deletedFromEye + " frames deleted from " + eyecam + ".");
            Console.WriteLine(deletedFromPs3 + " frames deleted from " + ps3cam + ".");
            Console.WriteLine(deletedFromWeb + " frames deleted from " + webcam + ".\n");

            // Update the review file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(sourceDir + "Review.txt", true))
            {
                file.WriteLine("Frames that did not result in triplets were deleted.");
                file.WriteLine(deletedFromEye + " frames deleted from " + eyecam + ".");
                file.WriteLine(deletedFromPs3 + " frames deleted from " + ps3cam + ".");
                file.WriteLine(deletedFromWeb + " frames deleted from " + webcam + ".\n");
            }

            Console.WriteLine("Press enter to start file duplication...");
            Console.ReadLine();
            
            // Next we duplicate the frames that we need into the slow camera, so a video can be made of these later
            int filesToCopy = framesToCopyFromAverage.Length;
            double perProcent = filesToCopy / 100.0;
            double done = perProcent;
            int percentCounter = 1;

            int copied = 0;
            for (int i = 0; i < framesToCopyFromAverage.Length; i++)
            {
                int suffix = 1;
                while (framesToCopyFromAverage[i] > 1)
                {
                    File.Copy(sourceDir + webcam + "\\" + webcamFiles[i], sourceDir + webcam + "\\" + 
                        (Convert.ToInt64(Regex.Match(webcamFiles[i], @"\d+").Value) + "-" + suffix + filetype), true);
                    framesToCopyFromAverage[i]--;
                    suffix++;
                    copied++;
                }
                if (i > done)
                {
                    Console.WriteLine(percentCounter + "%");
                    percentCounter++;
                    done = done + perProcent;
                }
            }

            Console.WriteLine("Done!\n");

            Console.WriteLine("Copied " + copied + " frames into webcam.\n");

            // Update the review file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(sourceDir + "Review.txt", true))
            {
                file.WriteLine("Copied " + copied + " frames into webcam.\n");
            }

            // Refreshing, useless if modifications are not made to use this data beyond this point in the future
            //webcamFiles = Directory.GetFiles(sourceDir + webcam).Select(path => Path.GetFileName(path)).ToArray();
            //eyecamFiles = Directory.GetFiles(sourceDir + eyecam).Select(path => Path.GetFileName(path)).ToArray();
            //ps3camFiles = Directory.GetFiles(sourceDir + ps3cam).Select(path => Path.GetFileName(path)).ToArray();

            // Create textfiles to show the frames after changes
            string[] postSyncweb = Directory.GetFiles(sourceDir + webcam).Select(path => Path.GetFileName(path)).ToArray();
            string[] postSynceye = Directory.GetFiles(sourceDir + eyecam).Select(path => Path.GetFileName(path)).ToArray();
            string[] postSyncps3 = Directory.GetFiles(sourceDir + ps3cam).Select(path => Path.GetFileName(path)).ToArray();

            lines = postSynceye;
            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].TrimEnd(MyChar);
            System.IO.File.WriteAllLines(sourceDir + "PostSyncEyecamFrames.txt", lines);

            lines = postSyncweb; int index;
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimEnd(MyChar); // Removing the dash and end numbers from the file names for this listing
                if (lines[i].Contains('-'))
                {
                    index = lines[i].IndexOf('-');
                    lines[i] = lines[i].Substring(0, index);
                }
            }
            System.IO.File.WriteAllLines(sourceDir + "PostSyncWebcamFrames.txt", lines);

            lines = postSyncps3;
            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].TrimEnd(MyChar);
            System.IO.File.WriteAllLines(sourceDir + "PostSyncPs3camFrames.txt", lines);


            // End the program
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }









        static long[] framePairsInEyecam;
        static long[] framePairsInPs3cam;
        static long[] averageValuesOfFramePairs;

        // Filename can be of different lengths around 11-13 characters so a fixed number as length will not work
        static string[] CheckingLength = Directory.GetFiles(sourceDir + webcam).Select(path => Path.GetFileName(path)).ToArray();
        static long filenameNumber = Convert.ToInt64(Regex.Match(CheckingLength[0], @"\d+").Value);
        static int LENGTH = filenameNumber.ToString().Length;

        //Hunludvig's code in the following methods
        static long timestampFromFilename(string filename) // Here I added the length adjustment because it varies
        {            
            int START = filename.Length - LENGTH - 4;
            return long.Parse(filename.Substring(START, LENGTH));
        }

        static void PairFrames(string pathLeft, string pathRight)
        {
            IEnumerable<String> imagesLeft = Directory.EnumerateFiles(pathLeft);
            IEnumerable<String> imagesRight = Directory.EnumerateFiles(pathRight);

            List<long> timestampsLeft = imagesLeft.Select(timestampFromFilename).ToList();
            List<long> timestampsRight = imagesRight.Select(timestampFromFilename).ToList();
            int counter = 0;
            foreach (Tuple<int, int> pair in matchTimestamps(timestampsLeft, timestampsRight))
            {
                framePairsInEyecam[counter] = timestampsLeft[pair.Item1];
                framePairsInPs3cam[counter] = timestampsRight[pair.Item2];
                /* // for debugging, these can be used to see what fast frames are paired exactly
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(sourceDir + "matching.txt", true))
                {
                    file.WriteLine("Matched pair: {0} -- {1}",
                    timestampsLeft[pair.Item1],
                    timestampsRight[pair.Item2]);
                }

                //Console.WriteLine("Matched pair: {0} -- {1}",
                //timestampsLeft[pair.Item1],
                //timestampsRight[pair.Item2]);
                */
                counter++;
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
