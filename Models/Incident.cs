using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RSCHS.Models
{
    public class Incident : INotifyPropertyChanged
    {
        private int _id;
        private string _location;
        private string _type;
        private string _description;
        private string _priority;
        private string _status;
        private string _callPhone;
        private DateTime _date;

        public int ID
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Location
        {
            get => _location;
            set { _location = value; OnPropertyChanged(); }
        }

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public string Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string CallPhone
        {
            get => _callPhone;
            set { _callPhone = value; OnPropertyChanged(); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        // Конструкторы
        public Incident()
        {
            Date = DateTime.Now;
            Status = "Новый";
        }

        public Incident(string location, string type, string description, string priority, string callPhone)
        {
            Location = location;
            Type = type;
            Description = description;
            Priority = priority;
            CallPhone = callPhone;
            Status = "Новый";
            Date = DateTime.Now;
        }

        // Метод для удобного отображения в ComboBox
        public string DisplayInfo => $"#{ID} - {Location} ({Type})";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}