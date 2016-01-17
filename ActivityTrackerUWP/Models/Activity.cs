using System;
using Template10.Mvvm;

namespace ActivityTrackerUWP.Models
{   
    /// <summary>
    /// Activity data structure
    /// </summary>
    public class Activity : BindableBase
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string ActualEndDate { get; set; }
        public string ActivityTypeCode { get; set; }
        public string ActivityIcon { get; set; }

        private string notes;
        public string Notes
        {
            get { return notes; }
            set { Set(ref notes, value); }
        }
        public DateTimeOffset ActualEnd { get; set; }
    }
}