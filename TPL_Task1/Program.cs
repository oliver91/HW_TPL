using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPL_Task1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Choose directory: ");
            string selectedDirectoryPath = Console.ReadLine();
            //string selectedDirectoryPath = "C:\\Users\\Oliver\\Desktop\\MyFolder";
            DirectoryInfo dirInfo = new DirectoryInfo(selectedDirectoryPath);

            List<FileInfo> filesList = new List<FileInfo>();
            filesList = GetAllInnerFiles(dirInfo, filesList);
            Console.WriteLine("\nAll inner files:");
            foreach (var file in filesList)
                Console.WriteLine("\t" + file.Name);
            Console.WriteLine("Number if files:" + filesList.Count);

            Console.WriteLine("\nEnter the string you are looking for:");
            string seekingString = Console.ReadLine();
 
            filesList = GetFilesContainesString(filesList, seekingString);

            filesList = GetFilesContainesStringParallel(filesList, seekingString);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n\nThis files contain's '{0}' string:", seekingString);
            if (filesList.Count > 0)
            {
                foreach (var file in filesList)
                    Console.WriteLine(file.Name);
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine("Enter the string you want to replace the found string:");
                string newString = Console.ReadLine();

                RewriteStringInAllFiles(filesList, seekingString, newString);

                filesList = GetFilesContainesString(filesList, seekingString);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n\nThis files contain's '{0}' string:", seekingString);
                if (filesList.Count > 0)
                {
                    foreach (var file in filesList)
                        Console.WriteLine(file.Name);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else Console.WriteLine("not found!");
            }
            else
                Console.WriteLine("not found!");
            Console.ForegroundColor = ConsoleColor.White;

            Console.ReadKey();
        }

        public static void RewriteStringInAllFiles(List<FileInfo> files, string str, string newStr)
        {
            foreach (var file in files)
            {
                string content = File.ReadAllText(file.DirectoryName + "\\" + file.Name);
                content = content.Replace(str, newStr);
                File.WriteAllText(file.DirectoryName + "\\" + file.Name, content);
            }
        }

        private static List<FileInfo> GetFilesContainesString(List<FileInfo> filesList, string str)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Stopwatch leadTime = Stopwatch.StartNew();
            List<FileInfo> filesThatContainedString = new List<FileInfo>();
            foreach (var file in filesList)
            {
                if (FindString(file, str)) filesThatContainedString.Add(file);
            }
            leadTime.Stop();
            Console.WriteLine("GetFilesContainesString() leadTime: {0} ms", leadTime.ElapsedMilliseconds);
            Console.ForegroundColor = ConsoleColor.White;
            return filesThatContainedString;
        }

        private static List<FileInfo> GetFilesContainesStringParallel(List<FileInfo> filesList, string str)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Stopwatch leadTime = Stopwatch.StartNew();
            BlockingCollection<FileInfo> filesThatContainedString = new BlockingCollection<FileInfo>();
            ParallelLoopResult loopResult = Parallel.ForEach(filesList, (file) =>
            {
                if (FindString(file, str)) filesThatContainedString.Add(file);
            });
            if (!loopResult.IsCompleted)
            {
                Console.WriteLine("Fail on iteration " + loopResult.LowestBreakIteration + "\n");
            }
            leadTime.Stop();
            Console.WriteLine("GetFilesContainesStringParallel() leadTime: {0} ms", leadTime.ElapsedMilliseconds);
            Console.ForegroundColor = ConsoleColor.White;
            return filesThatContainedString.ToList();
        }

        public static bool FindString(FileInfo file, string str)
        {
            string currentStr;
            FileStream fin = null;
            StreamReader reader = null;
            try
            {
                fin = new FileStream(file.DirectoryName + "\\" + file.Name, FileMode.Open);
                reader = new StreamReader(fin);
                while ((currentStr = reader.ReadLine()) != null)
                {
                    if (currentStr.Contains(str)) return true;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (fin != null) fin.Close();
                if (reader != null) fin.Close();
            }
            return false;
        }

        public static List<FileInfo> GetAllInnerFiles(DirectoryInfo dirInfo, List<FileInfo> filesList)
        {
            FileInfo[] files = dirInfo.GetFiles();
            for (int i = 0; i < files.Length; i++)
                filesList.Add(files[i]);
            DirectoryInfo[] innerDirectories = dirInfo.GetDirectories();
            foreach (var dir in innerDirectories)
                GetAllInnerFiles(dir, filesList);

            return filesList;
        }
    }
}
