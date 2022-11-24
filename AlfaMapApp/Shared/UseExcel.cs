using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace AlfaMap.Shared {
    class UseExcel {
        private Excel.Application app;
        private Excel.Workbook workbook;
        private Excel._Worksheet worksheet;
        private Excel.Range range;

        public UseExcel(string filepath, Action<Excel.Range, Excel._Worksheet, Excel.Workbook, Excel.Application> action) {
            Init(filepath);
            action(range, worksheet, workbook, app);
            Release();
        }

        public UseExcel(string filepath, Action<Excel.Range, Excel._Worksheet, Excel.Workbook> action) {
            Init(filepath);
            action(range, worksheet, workbook);
            Release();
        }

        public UseExcel(string filepath, Action<Excel.Range, Excel._Worksheet> action) {
            Init(filepath);
            action(range, worksheet);
            Release();
        }

        private void Init(string filepath) {
            try {
                app = Marshal.GetActiveObject("Excel.Application") as Excel.Application;
            } catch (Exception) {
                app = new Excel.Application();
            }

            if (filepath == null) {
                workbook = app.Workbooks.Add(1);
            } else {
                foreach (Excel.Workbook w in app.Workbooks) {
                    if (w.FullName == filepath) {
                        workbook = w;
                        break;
                    }
                }
                if (workbook == null) {
                    workbook = app.Workbooks.Open(filepath);
                }
            }

            worksheet = workbook.Sheets[1];
            range = worksheet.UsedRange;
        }

        public void Release() {
            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(range);
            Marshal.ReleaseComObject(worksheet);

            //close and release
            //workbook.Close();
            Marshal.ReleaseComObject(workbook);

            //quit and release
            //app.Quit();
            Marshal.ReleaseComObject(app);
        }
    }
}
