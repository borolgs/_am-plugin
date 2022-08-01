using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AlfaMap
{
    public class LinkFormViewModel : ViewModelBase
    {
        private string buildingId = "BID";
        public string BuildingId
        {
            get { return buildingId; }
            set
            {
                buildingId = value;
                OnPropertyChanged();
            }
        }

        private string officeId = "";
        public string OfficeId
        {
            get { return officeId; }
            set
            {
                officeId = value;
                OnPropertyChanged();
            }
        }

        private string errorMessage = "";
        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                OnPropertyChanged();
            }
        }

        private bool loading = false;
        public bool Loading
        {
            get { return loading; }
            set
            {
                loading = value;
                OnPropertyChanged();
            }
        }

        public Window Window { get; set; }

        public LinkFormViewModel()
        {
            BuildingId = Guid.NewGuid().ToString();
            Loading = false;
        }

        public void Generate()
        {
            BuildingId = Guid.NewGuid().ToString();
        }
    }

    #region Validation Rules
    public class InputBuildingIdRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string val = (string)value;
            if (String.IsNullOrEmpty(val))
            {
                return new ValidationResult(false, "Неправильный id Здания");
            }
            return ValidationResult.ValidResult;
        }
    }

    public class InputOfficeIdRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string val = (string)value;
            if (String.IsNullOrEmpty(val))
            {
                return new ValidationResult(false, "Неправильный имя офиса");
            }
            return ValidationResult.ValidResult;
        }
    }
    #endregion
}
