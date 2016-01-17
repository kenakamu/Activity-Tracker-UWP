using ActivityTrackerUWP.Models;
using ActivityTrackerUWP.Helpers;
using ActivityTrackerUWP.Services.SettingsServices;
using Microsoft.Crm.Sdk.Messages.Samples;
using Microsoft.Xrm.Sdk.Query.Samples;
using Microsoft.Xrm.Sdk.Samples;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivityTrackerUWP.Services.CrmServices
{
    /// <summary>
    /// Crm Service using SOAP endpoint and CRM Mobile SDK
    /// https://code.msdn.microsoft.com/Mobile-Development-Helper-3213e2e6
    /// </summary>
    public class CrmSoapService : ICrmService
    {
        public static CrmSoapService Instance { get; }
        
        static CrmSoapService()
        {
            // implement singleton pattern
            Instance = Instance ?? new CrmSoapService();
        }

        #region Members

        private OrganizationDataWebServiceProxy _proxy;

        private ISettingsService _settings;

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public CrmSoapService()
        {           
            // Get Setting SettingsHelper
            _settings = SettingsService.Instance;
            // Instantiate proxy
            _proxy = new OrganizationDataWebServiceProxy();
            _proxy.ServiceUrl = _settings.ServerUrl;
        }
        
        /// <summary>
        /// This method does Contact Search and return results. 
        /// </summary>
        /// <param name="searchCriteria">Search Criteria</param>
        /// <returns>List of Contacts</returns>
        public async Task<List<Contact>> SearchContacts(string searchCriteria)
        {
            // Get AccessToken
            _proxy.AccessToken = await ADALHelper.GetTokenSilent();

            var searchResults = new List<Contact>();

            string fetch = String.Format(@"<fetch version='1.0' output-format='xml-platform' top='50' mapping='logical' distinct='false'>
  <entity name='contact'>
    <attribute name='fullname' />
    <attribute name='jobtitle' />
    <order attribute='fullname' descending='false' />
    <filter type='and'>
      <condition attribute='fullname' operator='like' value='%{0}%' />
    </filter>
    <link-entity name='account' from='accountid' to='parentcustomerid' visible='false' link-type='outer' alias='ab'>
      <attribute name='name' />
    </link-entity>
  </entity>
</fetch>", searchCriteria);
                        
            EntityCollection results = await _proxy.RetrieveMultiple(new FetchExpression(fetch));

            // If there is results then process it.
            if (results.Entities.Count > 0)
            {
                foreach (Entity result in results.Entities)
                {
                    // Instantiate Contact and fullfill partially
                    Contact contact = new Contact()
                    {
                        Id = result.Id,
                        FullName = result["fullname"].ToString(),
                        // Combine Company Name and JobTitle
                        ParentCompany = (result["ab.name"] != null) ? (result["ab.name"] as AliasedValue).Value.ToString() : "",
                        JobTitle = (result.Contains("jobtitle")) ? result["jobtitle"].ToString() : ""
                    };
                    searchResults.Add(contact);
                }
            }

            return searchResults;
        }

        /// <summary>
        /// This method retrives a contact as late bound. 
        /// </summary>
        /// <param name="id">Contact Id</param>
        /// <returns>Contact Record</returns>
        public async Task<Contact> RetrieveContact(Guid id)
        {
            // Get AccessToken
            _proxy.AccessToken = await ADALHelper.GetTokenSilent();

            Entity result = await _proxy.Retrieve("contact", id, new ColumnSet("fullname", "address1_composite", "jobtitle", "parentcustomerid", "emailaddress1", "telephone1", "entityimage"));

            Contact contact = new Contact()
            {
                Id = result.Id,
                FullName = result["fullname"].ToString(),
                Address1 = (result.Contains("address1_composite")) ? result["address1_composite"].ToString() : "",
                JobTitle = (result.Contains("jobtitle")) ? result["jobtitle"].ToString() : "",               
                ParentCompany = (result["parentcustomerid"] == null) ? null : (result["parentcustomerid"] as EntityReference).Name.ToString(),
                Email1 = (result.Contains("emailaddress1")) ? result["emailaddress1"].ToString() : "",
                Telephone1 = (result.Contains("telephone1")) ? result["telephone1"].ToString() : "",
                EntityImage = result["entityimage"] as byte[]
            };

            // Return the result
            return contact;
        }        

        /// <summary>
        /// This method retrieves Activities for the Contact record
        /// </summary>
        /// <param name="id">Contact Id</param>
        /// <returns>List of Completed Activities</returns>
        public async Task<List<Activity>> RetrieveActivitiesOfContact(Guid id)
        {
            // Get AccessToken
            _proxy.AccessToken = await ADALHelper.GetTokenSilent();

            var searchResults = new List<Activity>();

            string fetch = String.Format(@"<fetch version='1.0' output-format='xml-platform' top='50' mapping='logical' distinct='false'>
  <entity name='activitypointer'>
    <attribute name='activitytypecode' />
    <attribute name='subject' />
    <attribute name='statecode' />
    <attribute name='actualend' />
    <order attribute='actualend' descending='true' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='1' />
    </filter>
    <link-entity name='contact' from='contactid' to='regardingobjectid' alias='aa'>
      <filter type='and'>
        <condition attribute='contactid' operator='eq' value='{0}' />
      </filter>
    </link-entity>
  </entity>
</fetch>", id.ToString());

            EntityCollection results = await _proxy.RetrieveMultiple(new FetchExpression(fetch));

            // If there is results then process it.
            if (results.Entities.Count > 0)
            {
                foreach (Entity activityresult in results.Entities)
                {
                    // Instantiate Contact and fullfill partially
                    // Instantiate Activity 
                    Activity activity = new Activity()
                    {
                        Id = activityresult.Id,
                        Subject = activityresult["subject"].ToString(),
                        ActualEndDate = ((System.DateTime)activityresult["actualend"]).ToString("d"),
                        ActivityTypeCode = activityresult["activitytypecode"].ToString()
                    };

                    // Assign icon depending on ActivityTypeCode. Add additional case when you need to.
                    switch (activityresult["activitytypecode"].ToString())
                    {
                        case "appointment":
                            activity.ActivityIcon = "../Assets/icon-activity-appt.png";
                            break;
                        case "task":
                            activity.ActivityIcon = "../Assets/icon-activity-note.png";
                            break;
                        case "phonecall":
                            activity.ActivityIcon = "../Assets/icon-activity-phone.png";
                            break;
                        default:
                            activity.ActivityIcon = "../Assets/icon-activity-generic.png";
                            break;
                    }
                    searchResults.Add(activity);
                }
            }

            return searchResults;
        }

        /// <summary>
        /// This method retrives a taks as late bound. 
        /// </summary>
        /// <param name="id">Activity Id</param>
        /// <returns>Activity Record</returns>
        public async Task<Activity> RetrieveTask(Guid id)
        {
            // Get AccessToken
            _proxy.AccessToken = await ADALHelper.GetTokenSilent();

            Entity result = await _proxy.Retrieve("task", id, new ColumnSet("subject", "actualend", "description"));

            Activity activity = new Activity()
            {
                Id = result.Id,
                Subject = result["subject"].ToString(),
                ActualEnd = new DateTimeOffset((result.Contains("actualend")) ? ((DateTime)result["actualend"]) : DateTime.Now),
                Notes = (result.Contains("description")) ? result["description"].ToString() : ""
            };

            // return result
            return activity;
        }

        /// <summary>
        /// This method firstly create task, then complete it if passed task does not have id.
        /// If passed entity has id, then change state back to active first, update data, then 
        /// complete it again.
        /// </summary>
        /// <param name="activity">Activity Record to update</param>
        /// <param name="contact">Contact Record for regarding information</param>
        public async Task UpsertTask(Activity activity, Contact contact)
        {
            // Get AccessToken
            _proxy.AccessToken = await ADALHelper.GetTokenSilent();

            // Instantiate a Task entity.
            Entity task = new Entity("task");
            task["subject"] = activity.Subject;
            task["actualstart"] = activity.ActualEnd.Date;
            task["actualend"] = activity.ActualEnd.Date;
            task["description"] = activity.Notes;
            task["regardingobjectid"] = new EntityReference("contact", contact.Id);
            task.Id = activity.Id;

            // If passed data does not have id, then its new operation.
            if (task.Id == null || task.Id == Guid.Empty)
            {
                // Create record first
                Guid taskId = await _proxy.Create(task);


                // Then complete it
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference("task", taskId),
                    State = new OptionSetValue(1),
                    Status = new OptionSetValue(5)
                };
                await _proxy.Execute(setStateRequest);
            }
            else
            {
                // First set state back to active
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference("task", task.Id),
                    State = new OptionSetValue(0),
                    Status = new OptionSetValue(3)
                };
                await _proxy.Execute(setStateRequest);

                // then update task
                await _proxy.Update(task);

                // finally complete it again
                setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference("task", task.Id),
                    State = new OptionSetValue(1),
                    Status = new OptionSetValue(5)
                };
                await _proxy.Execute(setStateRequest);
            }
        }

        /// <summary>
        /// This method delete task record
        /// </summary>
        /// <param name="activity">Activity Record to delete</param>
        public async Task DeleteTask(Activity activity)
        {
            // Get AccessToken
            _proxy.AccessToken = await ADALHelper.GetTokenSilent();

            // Delete record
            await _proxy.Delete("task", activity.Id);            
        }

        #endregion
    }
}
