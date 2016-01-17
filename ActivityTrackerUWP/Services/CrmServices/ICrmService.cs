using ActivityTrackerUWP.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivityTrackerUWP.Services.CrmServices
{
    /// <summary>
    /// CrmService Interface
    /// </summary>
    public interface ICrmService
    {
        Task<List<Activity>> RetrieveActivitiesOfContact(Guid id);
        Task<Contact> RetrieveContact(Guid id);
        Task<Activity> RetrieveTask(Guid id);
        Task<List<Contact>> SearchContacts(string searchCriteria);
        Task UpsertTask(Activity activity, Contact contact);
        Task DeleteTask(Activity activity);
    }
}