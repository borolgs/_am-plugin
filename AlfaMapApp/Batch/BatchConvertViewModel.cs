using AlfaMap.Common;
using AlfaMap.Shared;
using Autodesk.Revit.UI;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Batch {
    public class BatchConvertViewModel : RevitViewModelBase {
        private BatchClient client = new BatchClient();
        private ObservableCollection<ConvertFileResult> batchConvertResults;
        public ObservableCollection<ConvertFileResult> BatchConvertResults {
            get { return batchConvertResults; }
            set {
                batchConvertResults = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CoworkingRoomEntity> coworkings;
        public ObservableCollection<CoworkingRoomEntity> Coworkings
        {
            get { return coworkings; }
            set
            {
                coworkings = value;
                OnPropertyChanged();
            }
        }


        public override void Run(object cmd, UIApplication uiapp) {
            var converter = new BatchConverter(uiapp);

            string command = cmd as string;
            if (command == "Check") {
                var results = converter.Check();
                BatchConvertResults = new ObservableCollection<ConvertFileResult>(results);
            } else if (command == "QuickCheck") {
                var results = converter.QuickCheck();
                BatchConvertResults = new ObservableCollection<ConvertFileResult>(results);
            } else if (command == "Convert") {
                //var results = converter.Convert();
                //BatchConvertResults = new ObservableCollection<ConvertFileResult>(results);
            } else if (command == "Export") {
                new UseExcel(null, (_, worksheet, workbook, app) => {
                    try {
                        int colNum = 4;
                        var data = new object[BatchConvertResults.Count + 1, colNum];
                        data[0, 0] = "Building Id";
                        data[0, 1] = "Path";
                        data[0, 2] = "Updated At";
                        data[0, 3] = "Error";
                        for (var i = 1; i < BatchConvertResults.Count; i++) {
                            var item = BatchConvertResults[i];
                            data[i, 0] = item.BuildingId;
                            data[i, 1] = item.FilePath;
                            data[i, 2] = item.UpdatedAt.ToString();
                            data[i, 3] = item.ErrorMessage;
                        }
                        var range = (Range)worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[BatchConvertResults.Count + 1, colNum]];

                        range.Value = data;

                        worksheet.Columns.AutoFit();

                        // Borders
                        Range usedRange = worksheet.UsedRange;
                        usedRange.VerticalAlignment = XlVAlign.xlVAlignTop;
                        usedRange.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                        usedRange.Borders.LineStyle = XlLineStyle.xlContinuous;
                        usedRange.Borders[XlBordersIndex.xlInsideVertical].Weight = XlBorderWeight.xlMedium;
                        usedRange.Borders[XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlMedium;
                        usedRange.Borders[XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlMedium;
                        usedRange.Borders[XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlMedium;
                        usedRange.Borders[XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlMedium;
                        usedRange.Rows[1].Borders[XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlMedium;


                        app.Visible = true;
                    } catch (Exception e) {
                        Console.WriteLine(e.ToString());
                    }
                    
                });
            } else if (command == "ReloadCoworkingList") {
                Task.Run(async () => {
                    var rooms = await client.FindCoworkingRooms();
                    Coworkings = new ObservableCollection<CoworkingRoomEntity>(
                        rooms.Select(room => new CoworkingRoomEntity { Id = room.Id, Address = room.Address })
                    );
                });
            }
        }
    }
}
