using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RSCHS.Models
{
    public class Assignment : INotifyPropertyChanged
    {
        private int _id;
        private int _incidentId;
        private int _employeeId;
        private int _transportId;
        private DateTime _date;

        // Для отображения в UI
        public string IncidentLocation { get; set; }
        public string EmployeeName { get; set; }
        public string TransportInfo { get; set; }

        public int ID
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public int IncidentID
        {
            get => _incidentId;
            set { _incidentId = value; OnPropertyChanged(); }
        }

        public int EmployeeID
        {
            get => _employeeId;
            set { _employeeId = value; OnPropertyChanged(); }
        }

        public int TransportID
        {
            get => _transportId;
            set { _transportId = value; OnPropertyChanged(); }
        }

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        // Конструктор
        public Assignment() { }

        public Assignment(int incidentId, int employeeId, int transportId)
        {
            IncidentID = incidentId;
            EmployeeID = employeeId;
            TransportID = transportId;
            Date = DateTime.Now;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string DisplayInfo => $"#{ID} - {IncidentLocation} ({Date:dd.MM.yyyy HH:mm})";
    }
}