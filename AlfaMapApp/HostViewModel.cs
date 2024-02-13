using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace AlfaMap {
    public class HostViewModel : ViewModelBase
    {

        private UIElement appUI = null;
        public UIElement AppUI {
            get { return appUI; }
            set {
                appUI = value;
                OnPropertyChanged();
            }
        }

        public AppViewModel appViewModel = new AppViewModel();

        public HostViewModel()
        {
            AppUI = new UI(appViewModel) as UIElement;
        }



        private Document doc;
        public Document Doc {
            get { return doc; }
            set {
                OnDocumentSwitch(value);
            }
        }

        private void OnDocumentSwitch(Document newDoc) {
            // Invalid or FamilyDocument
            if (newDoc == null || !newDoc.IsValidObject || newDoc.IsFamilyDocument) {
                doc = null;
                UpdateDoc(doc);
                return;
            }

            // Same Document
            if (doc != null && doc.IsValidObject && doc.Equals(newDoc)) {
                return;
            }

            // Udapte Document
            if (doc == null || !doc.IsValidObject || !doc.Equals(newDoc)) {
                doc = newDoc;
            }

            UpdateDoc(doc);
        }

        private void UpdateDoc(Document doc) {
            appViewModel.Doc = doc;
        }
    }
}
