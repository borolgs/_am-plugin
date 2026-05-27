using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AlfaMap.Connector;
using AlfaMap.DataSync;
using AlfaMap.State;
using AlfaMap.Converter2d;
using AlfaMap.Batch;
using Workplace.Snapshots;
using Autodesk.Revit.DB.Architecture;

namespace AlfaMap
{
    public class AppViewModel : ViewModelBase {
        #region Revit Properties
        private ExternalEvent externalEvent;
        private RevitEventHandler externalHandler;


        private RelayCommand runWorkplaceCommand;
        public RelayCommand RunWorkplaceCommand {
            get {
                return runWorkplaceCommand ?? (runWorkplaceCommand = new RelayCommand(obj => {
                    var command = (WorkplaceCommand)obj;

                    if (command == WorkplaceCommand.LoadOfficeData) {
                        _ = SyncV2();
                        return;
                    } else if (command == WorkplaceCommand.CreateOrOverrideActiveVersion) {
                        _ = UploadV2();
                    }

                    externalHandler.Method = uiapp => {
                        RunRevit(uiapp, command);
                    };
                    externalEvent.Raise();
                }));
            }
        }

        private RelayCommand runScratchCommand;
        public RelayCommand RunScratchCommand {
            get {
                return runScratchCommand ?? (runScratchCommand = new RelayCommand(obj => {
                    var command = (string)obj;



                    //if (command == "BatchConvert") {
                    //    if (batchConvertDialog == null) {
                    //        batchConvertDialog = new BatchConvertDialog(batchConvertViewModel);
                    //        batchConvertDialog.Closed += (sender, args) => { this.batchConvertDialog = null; };
                    //    }
                    //    batchConvertDialog.Show();
                    //    return;
                    //}

                    //externalHandler.Output = output => {
                    //    ConsoleOutput = output;
                    //};
                    //externalHandler.Method = uiapp => {
                    //    var doc = uiapp.ActiveUIDocument.Document;
                    //    try {
                    //        if (command == "ConvertToSVG") {
                    //            var dialog = new TaskDialog("Экспорт помещений в SVG");
                    //            var selectedElement = uiapp.ActiveUIDocument.Selection.GetElementIds().Select(doc.GetElement).FirstOrDefault();
                    //            if (selectedElement == null) {
                    //                dialog.MainInstruction = "Ничего не выбрано";
                    //                dialog.MainContent = "Выберите помещение-коворкинг и повторите команду";
                    //                dialog.Show();
                    //                return;
                    //            }
                    //            handler.InitFromDocument(doc);
                    //            var selectedNode = handler.GetNode(selectedElement.UniqueId);
                    //            bool validNode = selectedNode?.Node?.type.id == 4 && selectedNode.Children.Where(workplaceNode => workplaceNode.Node?.type?.id == 5).Count() > 0;
                    //            if (!validNode) {
                    //                dialog.MainInstruction = "Выбранный элемент не помещение-коворкинг";
                    //                dialog.MainContent = "Выберите помещение-коворкинг и повторите команду";
                    //                dialog.Show();
                    //                return;
                    //            }
                    //            var converter = new SVGConverter();
                    //            string roomSvg = converter.Convert(selectedNode);
                    //            string svgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{selectedNode.Node?.name ?? "room"}.svg");
                    //            File.WriteAllText(svgPath, roomSvg, Encoding.UTF8);
                    //            //Clipboard.SetData(DataFormats.Text, roomSvg);
                    //            Clipboard.SetText(roomSvg, TextDataFormat.UnicodeText);
                    //            dialog.MainInstruction = $"SVG помещения {selectedNode.Node.name} успешно экспортирован.";
                    //            dialog.MainContent = $"{svgPath}\nПроверить можно здесь <a href=\"https://www.svgviewer.dev/\">svgviewer.dev</a>.\nSVG уже скопирован в буфер обмена.";
                    //            dialog.Show();
                    //        }

                    //        if (command == "ConvertLevelsToSVG") {
                    //            handler.InitFromDocument(doc);
                    //            //var level = uiapp.ActiveUIDocument.ActiveView.GenLevel;
                    //            //var levelNode = handler.GetNode(level.UniqueId);
                    //            //var converter = new SVGConverter(uiapp);
                    //            //string roomSvg = converter.ConvertLevel(levelNode);
                    //            //string svgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{levelNode.Node?.name ?? "level"}.svg");
                    //            //File.WriteAllText(svgPath, roomSvg, Encoding.UTF8);

                    //            var rootNode = handler.Tree.Root;
                    //            var converter = new BuildingTo2DConverter();
                    //            var data = converter.Convert(rootNode);

                    //            var jsonSettings = new JsonSerializerSettings { 
                    //                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    //                Converters = new JsonConverter[] {
                    //                    new BBoxConverter(),
                    //                    new XYZConverter(),
                    //                    new BoundariesConverter(),
                    //                    new CurveConverter(),
                    //                },
                    //                NullValueHandling = NullValueHandling.Ignore,
                    //            };
                    //            var json = JsonConvert.SerializeObject(data, jsonSettings);
                    //            string svgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{doc.Title}.json");
                    //            File.WriteAllText(svgPath, json, Encoding.UTF8);
                    //        }
                    //    } catch (Exception e) {
                    //        RunException = e.InnerException ?? e;
                    //    }
                    //};

                    externalHandler.Method = uiapp => {
                        try {
                            if (command == "GetModelSnapshot") {
                                var handler = new CreateSnapshothandler(uiapp);
                                handler.Run();
                            }

                            if (command == "ConvertToSVG") {
                                var dialog = new TaskDialog("Экспорт помещений в SVG");
                                var selectedElements = uiapp.ActiveUIDocument.Selection.GetElementIds().Select(doc.GetElement).ToList();
                                if (selectedElements.Count == 0) {
                                    dialog.MainInstruction = "Ничего не выбрано";
                                    dialog.MainContent = "Выберите помещение-коворкинг и повторите команду";
                                    dialog.Show();
                                    return;
                                }

                                handler.InitFromDocument(doc);
                                var converter = new SVGConverter();

                                if (selectedElements.Count == 1) {
                                    var selectedNode = handler.GetNode(selectedElements[0].UniqueId);
                                    bool validNode = selectedNode?.Node?.type.id == 4 && selectedNode.Children.Where(workplaceNode => workplaceNode.Node?.type?.id == 5).Count() > 0;
                                    if (!validNode) {
                                        dialog.MainInstruction = "Выбранный элемент не помещение-коворкинг";
                                        dialog.MainContent = "Выберите помещение-коворкинг и повторите команду\nЕсли нужно экспортировать любое помещение см. <a href=\"https://confluence.moscow.alfaintra.net/pages/viewpage.action?pageId=1573407323&preview=/1573407323/1573407363/svg-export.pdf\">инструкцию</a>";
                                        dialog.Show();
                                        return;
                                    }

                                    string roomSvg = converter.Convert(selectedNode);
                                    string svgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{selectedNode.Node?.name ?? "room"}.svg");
                                    File.WriteAllText(svgPath, roomSvg, Encoding.UTF8);
                                    Clipboard.SetText(roomSvg, TextDataFormat.UnicodeText);
                                    dialog.MainInstruction = $"SVG помещения {selectedNode.Node?.name ?? ""} успешно экспортирован.";
                                    dialog.MainContent = $"{svgPath}\nПроверить можно здесь <a href=\"https://www.svgviewer.dev/\">svgviewer.dev</a>.\nSVG уже скопирован в буфер обмена.";
                                    dialog.Show();
                                } else {
                                    // TEMP!!!!
                                    var room = selectedElements.Where(e => e is Room).First();
                                    var selectedRoomNode = handler.GetNode(room.UniqueId);
                                    selectedElements.Remove(room);

                                    var selectedNodes = selectedElements.Select(e => handler.GetNode(e.UniqueId)).ToList();

                                    var roomName = room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();

                                    string roomSvg = converter.ConvertForce(selectedRoomNode, selectedNodes, roomName);
                                    string svgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{roomName}.svg");
                                    File.WriteAllText(svgPath, roomSvg, Encoding.UTF8);
                                    Clipboard.SetText(roomSvg, TextDataFormat.UnicodeText);
                                    dialog.MainInstruction = $"SVG помещения {selectedRoomNode.Node?.name ?? ""} успешно экспортирован.";
                                    dialog.MainContent = $"{svgPath}\nПроверить можно здесь <a href=\"https://www.svgviewer.dev/\">svgviewer.dev</a>.\nSVG уже скопирован в буфер обмена.";
                                    dialog.Show();
                                }

                            }
                        } catch (Exception e) {
                            RunException = e.InnerException ?? e;
                        }
                    };
                    externalEvent.Raise();
                }));
            }
        }

        private Document doc;
        public Document Doc {
            get { return doc; }
            set {
                OnDocumentSwitch(value);
            }
        }
        #endregion

        #region Main Properties
        public bool Test {
            get { return Config.Debug; }
            set {
                Config.Debug = value;
                InitDataHandler(value);
                OnPropertyChanged();
            }
        }

        private string consoleOutput;
        public string ConsoleOutput {
            get { return consoleOutput; }
            set {
                consoleOutput = value;
                OnPropertyChanged();
            }
        }

        private BuildingTree FreshTree {
            get {
                if (Doc == null)
                    return null;

                var tree = new BuildingTree(Doc);
                return tree;
            }
        }

        private string docName;
        public string DocName {
            get { return docName; }
            set {
                docName = value;
                OnPropertyChanged();
            }
        }

        private bool canBeLinked = false;
        public bool CanBeLinked {
            get { return canBeLinked; }
            set {
                canBeLinked = value;
                OnPropertyChanged();
            }
        }

        private bool canSync = false;
        public bool CanSync {
            get { return canSync; }
            set {
                canSync = value;
                OnPropertyChanged();
            }
        }

        private bool isUIEnabled = false;
        public bool IsUIEnabled {
            get { return isUIEnabled; }
            set {
                isUIEnabled = value;
                OnPropertyChanged();
            }
        }

        private string updateModelConsoleOutput;
        public string UpdateModelConsoleOutput {
            get {
                return updateModelConsoleOutput;
            }
            set {
                updateModelConsoleOutput = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Properties


        private string directory = "C:\\Users\\U_M12EE\\Desktop\\";
        public string Directory {
            get { return directory; }
            set {
                directory = value;
                OnPropertyChanged();
            }
        }

        private string buildingVersionUuid = "0E5CC281-A457-41AB-B8C6-11883908AD55";
        public string BuildingVersionUuid {
            get { return buildingVersionUuid; }
            set {
                buildingVersionUuid = value;
                OnPropertyChanged();
            }
        }

        private string modelUuid = "67731DAD-E5D1-4303-8219-708DEC568A7A";
        public string ModelUuid {
            get { return modelUuid; }
            set {
                modelUuid = value;
                OnPropertyChanged();
            }
        }

        private Status status;
        public Status Status {
            get { return status; }
            set {
                status = value;
                OnPropertyChanged();
                OnPropertyChanged("StatusMessage");
            }
        }
        public string StatusMessage {
            get {
                switch (Status) {
                    case Status.CreateTree:
                        return "";
                    case Status.ConnectingBuilding:
                        return "Подключение";
                    case Status.BuildingConnected:
                        return "";
                    case Status.Load:
                        return "Загрузка из BW";
                    case Status.UpdateModel:
                        return "Обновление Модели";
                    case Status.Upload:
                        return "Отправка модели";
                    default:
                        return "";
                }
            }
        }

        private string office;
        public string Office {
            get { return office; }
            set {
                office = value;
                OnPropertyChanged();
            }
        }

        private string description;
        public string UploadModelDescription {
            get { return description; }
            set {
                description = value;
                OnPropertyChanged();
            }
        }

        private bool uploadModelAsCurrent = true;
        public bool UploadModelAsCurrent {
            get { return uploadModelAsCurrent; }
            set {
                uploadModelAsCurrent = value;
                OnPropertyChanged();
            }
        }

        private bool keepPrevModel = false;
        public bool KeepPrevModel {
            get { return keepPrevModel; }
            set {
                keepPrevModel = value;
                OnPropertyChanged();
            }
        }

        private Exception runException;
        public Exception RunException {
            get { return runException; }
            set {
                runException = value;
                OnPropertyChanged();
                if (runException != null)
                    new ErrorDialog(this).Show();
            }
        }

        public string ExceptionMessage => RunException.ToString();

        private LinkFormViewModel linkForm = new LinkFormViewModel();
        public LinkFormViewModel LinkForm {
            get { return linkForm; }
            set {
                linkForm = value;
                OnPropertyChanged();
            }
        }

        private DisplayBuildingTree displayTree;
        public DisplayBuildingTree DisplayTree {
            get { return displayTree; }
            set {
                displayTree = value;
                OnPropertyChanged();
            }
        }
        #endregion

        private DataSync.Handler handler;

        private ObservableCollection<SyncNodeInfo> syncResults;
        public ObservableCollection<SyncNodeInfo> SyncResults {
            get { return syncResults; }
            set {
                syncResults = value;
                OnPropertyChanged();
            }
        }

        private BatchConvertDialog batchConvertDialog;
        private BatchConvertViewModel batchConvertViewModel = new BatchConvertViewModel();


        public AppViewModel()
        {
            externalHandler = new RevitEventHandler();
            externalEvent = ExternalEvent.Create(externalHandler);
            DisplayTree = new DisplayBuildingTree();
            InitDataHandler(Test);
        }

        private void InitDataHandler(bool test = false) {
            var baseUrl = test
             ? "https://aptest.moscow.alfaintra.net/amap/api/v1"
             : "https://ap.moscow.alfaintra.net/amap/api/v1";

            var httpHandler = new HttpClientHandler {
                UseDefaultCredentials = true,

            };
            httpHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            httpHandler.ServerCertificateCustomValidationCallback = (msg, cert, certChain, policyErrors) => true;

            //var queryHandler = new DataSync.DefaultQueryHandler(new Dictionary<string, string> {
            //    ["ap-direct-mode"] = "true",
            //}) {
            //    InnerHandler = httpHandler,
            //};

            var http = new HttpClient(httpHandler);
            http.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");

            var client = new DataSync.Client(http, baseUrl);
            handler = new DataSync.Handler(client);
        }

        private void OnDocumentSwitch(Document newDoc)
        {
            CanBeLinked = false;
            IsUIEnabled = true;

            // Invalid or FamilyDocument
            if (newDoc == null || !newDoc.IsValidObject || newDoc.IsFamilyDocument)
            {
                IsUIEnabled = false;
                doc = null;
                return;
            }

            // Same Document
            if (doc != null && doc.IsValidObject && doc.Equals(newDoc))
            {
                return;
            }

            // Udapte Document
            if (doc == null || !doc.IsValidObject || !doc.Equals(newDoc))
            {
                doc = newDoc;
            }

            if (doc.Title != DocName)
            {
                DocName = doc.Title;
            }

            IsUIEnabled = true;
            InitBuildingFromDocument();

            displayTree.Update(FreshTree);
        }

        private void InitBuildingFromDocument()
        {
            if (!CanBeLinked)
            {
                CanSync = false;
                return;
            }

            var buildingId = doc.ProjectInformation.get_Parameter(OldParams.BuildingId.Guid).AsString();
            if (String.IsNullOrEmpty(buildingId))
            {
                CanSync = false;
            }

            var officeKey = doc.ProjectInformation.get_Parameter(OldParams.OfficeKey.Guid).AsString();
            if (String.IsNullOrEmpty(officeKey))
            {
                CanSync = false;
            }

            CanSync = true;
        }

        public void RaiseSelectElement(ElementId id)
        {
            externalHandler.Method = uiapp =>
            {
                if (id != null)
                    uiapp.ActiveUIDocument.Selection.SetElementIds(new List<ElementId> { id });
            };
            externalEvent.Raise();
        }

        public void RaiseSelectNodeBranch(ModelNode node)
        {
            externalHandler.Method = uiapp =>
            {
                var toSelect = new List<ElementId>();
                var queue = new Queue<ModelNode>();
                queue.Enqueue(node);
                while(queue.Count > 0)
                {
                    var n = queue.Dequeue();
                    if (n.Element != null)
                        toSelect.Add(n.Element.Id);
                    
                    foreach (ModelNode child in n.Children)
                        queue.Enqueue(child);
                }
                if (toSelect.Count > 0)
                    uiapp.ActiveUIDocument.Selection.SetElementIds(toSelect);
            };
            externalEvent.Raise();
        }

        public void RaiseSelectOfficeBranch(string officeId)
        {
            // TODO
            externalHandler.Method = uiapp =>
            {

            };
            externalEvent.Raise();
        }


        public void RaiseRevitCommand(WorkplaceCommand command)
        {
            externalHandler.Method = uiapp =>
            {
                RunRevit(uiapp, command);
            };
            externalEvent.Raise();
        }

        private void RunRevit(UIApplication uiapp, WorkplaceCommand command)
        {
            RunException = null;
            Doc = uiapp.ActiveUIDocument.Document;
            using (Transaction t = new Transaction(Doc))
            {
                try
                {
                    switch (command)
                    {
                        case WorkplaceCommand.PrepareModel:
                            handler.PrepareDocument(Doc);
                            break;
                        default:
                            Debug.Print($"Command {command.ToString()} does not implemented");
                            break;
                    }
                }
                catch (Exception e)
                {
                    string message = $"Command {command.ToString()} error: {e.Message}";
                    Debug.Print(message);
                    if (t.HasStarted())
                        t.RollBack();
                    RunException = e;
                }
            }
        }

        private async Task SyncV2() {
            try {
                handler.InitFromDocument(Doc);
                (await handler.LoadData()).Unwrap();

                externalHandler.Method = uiapp => {
                    //string commandConsoleOutString;
                    //TextWriter originalConsoleOut = Console.Out;
                    //using (var writer = new StringWriter()) {
                        //Console.SetOut(writer);
                        using (Transaction t = new Transaction(Doc)) {
                            try {
                                t.Start("Sync");

                                handler.UpdateDocumentBuildingData();
                                handler.UpdateDocumentElements();

                                t.Commit();

                                SyncResults = new ObservableCollection<SyncNodeInfo>(handler.SyncResults.Nodes.Values.Concat(handler.SyncResults.Elements.Values));

                            } catch (Exception e) {
                                string message = $"Command Sync error: {e.Message}";
                                Debug.Print(message);
                                if (t.HasStarted())
                                    t.RollBack();
                                RunException = e.InnerException ?? e;
                            }  
                        }

                    //    writer.Flush();
                    //    commandConsoleOutString = writer.GetStringBuilder().ToString();
                    //    Console.SetOut(originalConsoleOut);
                    //    UpdateModelConsoleOutput = commandConsoleOutString;
                    //}
                    Status = Status.Nothing;
                };
                externalEvent.Raise();
            } catch (Exception e) {
                string message = $"Command Sync error: {e.Message}";
                Debug.Print(message);
                RunException = e.InnerException ?? e;
            }
        }

        private async Task UploadV2() {
            try {
                handler.InitFromDocument(Doc);
                var model = (await handler.UploadData(UploadModelDescription, UploadModelAsCurrent, KeepPrevModel)).Unwrap();

                UploadModelDescription = null;
                UploadModelAsCurrent = true;
                KeepPrevModel = false;

                externalHandler.Method = uiapp => {
                    using (Transaction t = new Transaction(Doc)) {
                        try {
                            t.Start("Update building data");

                            handler.UpdateDocumentBuildingData();

                            t.Commit();

                            var dialog = new TaskDialog("Sync");
                            dialog.MainInstruction = "Модель загружена на сервер";
                            string testPrefix = Test ? "test" : "";
                            string url = $"https://ap{testPrefix}.moscow.alfaintra.net/amap/pro/buildings/{model.buildingId}?officeId={handler.Building.place.offices[0].id}";
                            string link = $"<a href=\"{url}\">{url}</a>";
                            dialog.MainContent = link; // $"<a href=\"http://locvis=dev/buildings/{model.buildingId}?officeId={handler.Building.place.offices[0].id}\"></a>";
                            dialog.Show();

                        } catch (Exception e) {
                            string message = $"Command Sync error: {e.Message}";
                            Debug.Print(message);
                            if (t.HasStarted())
                                t.RollBack();
                            RunException = e.InnerException ?? e;
                        }
                    }
                    Status = Status.Nothing;
                };
                externalEvent.Raise();
            } catch (Exception e) {
                string message = $"Command Sync error: {e.Message}";
                Debug.Print(message);
                RunException = e.InnerException ?? e;
            }
        }

      
        public async void LinkModel(UIApplication uiapp)
        {
            Status = Status.ConnectingBuilding;
            
            LinkForm.Loading = true;

            // TODO
        }

    }

    public enum WorkplaceCommand {
        None,
        PrepareModel,
        LoadOfficeData, // TODO: rename
        CreateOrOverrideActiveVersion, // TODO: rename
    }

    public enum Status
    {
        CreateTree,
        ConnectingBuilding,
        BuildingConnected,
        Load,
        UpdateModel,
        Upload,
        Nothing,
        ConnectionFailed
    }
}
