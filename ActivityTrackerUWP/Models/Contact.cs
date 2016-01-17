using System;

namespace ActivityTrackerUWP.Models
{
    /// <summary>
    /// Contact data structure
    /// </summary>
    public class Contact
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        //public string AccountWithTitle { get; set; }
        public string ParentCompany { get; set; }
        public string Telephone1 { get; set; }
        public string Address1 { get; set; }
        public string Email1 { get; set; }
        public string JobTitle { get; set; }
        public byte[] EntityImage { get; set; }
    }
}