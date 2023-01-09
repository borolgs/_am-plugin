using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AlfaMap.Batch {
    class BatchConverter {
        //private string Root = "C:\\Users\\borodatov.os\\Documents\\tmp\\background-handle-test";
        private string Root = @"T:\IT\Офисная недвижимость\Москва и Регионы";
        private UIApplication uiapp;
        private List<ConvertFileResult> results = new List<ConvertFileResult>();

        public BatchConverter(UIApplication uiapp) {
            this.uiapp = uiapp;
        }

        public List<ConvertFileResult> Check() {
            Walk(rvtFile => {
                var result = Check(rvtFile);
                if (!result.Skip) {
                    results.Add(result);
                }
            });
            return results;
        }

        public List<ConvertFileResult> QuickCheck() {
            Walk(rvtFile => {
                var result = QuickCheckFile(rvtFile);
                if (!result.Skip) {
                    results.Add(result);
                }
            });
            return results;
        }

        private void Walk(Action<string> onRvtFile) {
            var totalsw = new Stopwatch();
            totalsw.Start();
            Console.WriteLine($"Root directory: {Root}");

            var stack = new Stack<string>();

            stack.Push(Root);

            while (stack.Count > 0) {
                var currentDir = stack.Pop();

                string[] subDirs;
                try {
                    subDirs = Directory.GetDirectories(currentDir);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    subDirs = new string[] { };
                }

                string[] rvtFiles;
                try {
                    rvtFiles = Directory.GetFiles(currentDir, "*.rvt");
                } catch (Exception e) {
                    rvtFiles = new string[] { };
                    Console.WriteLine(e.Message);
                }

                foreach (string rvtFile in rvtFiles) {
                    onRvtFile(rvtFile);
                }

                foreach (string dir in subDirs) {
                    stack.Push(dir);
                }
            }
            totalsw.Stop();

            Console.WriteLine($"Execution time total: {totalsw.Elapsed}");
            var documents = uiapp.Application.Documents;
            Console.WriteLine($"Opened docs count: {documents.Size}");
        }

        private ConvertFileResult Check(string path) {
            var result = new ConvertFileResult {
                FilePath = path,
            };

            var sw = new Stopwatch();
            void stopsw() {
                sw.Stop();
                result.ProcessingTime = sw.Elapsed;
            }
            sw.Start();

            try {
                {
                    var match = Regex.Match(path, @"\.\d{4}\.rvt");
                    if (match.Success) {
                        result.Error = new Exception("It is a backup document");
                        result.Skip = true;
                        stopsw();
                        return result;
                    }
                    if(path.ToLower().Contains("архив") || path.ToLower().Contains("процесс"))
                    {
                        result.Error = new Exception("It is a WIP document");
                        result.Skip = true;
                        stopsw();
                        return result;
                    }
                }

                var fileInfo = new FileInfo(path);
                var rvtInfo = BasicFileInfo.Extract(path);
                if (rvtInfo.Format != "2020") {
                    result.Error = new Exception("Wrong version: " + rvtInfo.Format);
                    result.Skip = true;
                    stopsw();
                    return result;
                }
                result.RvtVersion = rvtInfo.Format;
                result.UpdatedAt = fileInfo.LastWriteTime;

                var openOptions = new OpenOptions();

                if (rvtInfo.IsCentral) {
                    openOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets;
                }

                var doc = uiapp.Application.OpenDocumentFile(ModelPathUtils.ConvertUserVisiblePathToModelPath(path), openOptions);
                int buildingId = doc.ProjectInformation.LookupParameter("AM_BuildingId")?.AsInteger() ?? -1;
                if (buildingId < 0) {
                    result.Error = new Exception("No BuildingId");
                    doc.Close(false);
                    stopsw();
                    return result;
                }
                result.BuildingId = buildingId;


                doc.Close(false);
                stopsw();

            } catch (Exception e) {
                result.Error = e;
                stopsw();
            }

            return result;
        }

        private ConvertFileResult QuickCheckFile(string path) {
            var result = new ConvertFileResult {
                FilePath = path,
            };
            try {

                {
                    var match = Regex.Match(path, @"\.\d{4}\.rvt");
                    if (match.Success) {
                        result.Error = new Exception("It is a backup document");
                        result.Skip = true;
                        return result;
                    }
                    if (path.ToLower().Contains("архив") || path.ToLower().Contains("процесс"))
                    {
                        result.Error = new Exception("It is a WIP document");
                        result.Skip = true;
                        return result;
                    }
                }

                var fileInfo = new FileInfo(path);
                var rvtInfo = BasicFileInfo.Extract(path);

                result.RvtVersion = rvtInfo.Format;
                result.UpdatedAt = fileInfo.LastWriteTime;

            } catch (Exception e) {
                result.Error = e;
            }

            return result;
        }
    }

}
