using ActivityTrackerUWP.Models;
using ActivityTrackerUWP.Helpers;
using ActivityTrackerUWP.Services.SettingsServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ActivityTrackerUWP.Services.CrmServices
{
    /// <summary>
    /// Crm Service using Web API endpoint
    /// https://msdn.microsoft.com/en-us/library/mt593051.aspx
    /// </summary>
    public class CrmWebAPIService : ICrmService
    {
        public static CrmSoapService Instance { get; }
        
        static CrmWebAPIService()
        {
            // implement singleton pattern
            Instance = Instance ?? new CrmSoapService();
        }

        #region Members

        private HttpClient _proxy;

        private ISettingsService _settings;

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public CrmWebAPIService()
        {
            // Get Setting SettingsHelper
            _settings = SettingsService.Instance;
            // Instantiate proxy
            _proxy = new HttpClient();
            string version = "v" + _settings.CrmVersion;
            _proxy.BaseAddress = new Uri(string.Format("{0}/api/data/{1}/", _settings.ServerUrl, version));
        }

        /// <summary>
        /// This method does Contact Search and return results. 
        /// </summary>
        /// <param name="searchCriteria">Search Criteria</param>
        /// <returns>List of Contacts</returns>
        public async Task<List<Contact>> SearchContacts(string searchCriteria)
        {
            // Get AccessToken
            _proxy.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ADALHelper.GetTokenSilent());

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

            // Use FetchXML to get data
            HttpResponseMessage retrieveRes = await _proxy.GetAsync(string.Format("contacts?fetchXml={0}", System.Uri.EscapeDataString(fetch)));
            JToken results = JObject.Parse(retrieveRes.Content.ReadAsStringAsync().Result)["value"];
            
            foreach (JToken result in results)
            {
                // Instantiate Contact and fullfill partially
                Contact contact = new Contact()
                {
                    Id = (Guid)result["contactid"],
                    FullName = result["fullname"].ToString(),
                    // Combine Company Name and JobTitle
                    ParentCompany = (result["ab_x002e_name"] != null) ? result["ab_x002e_name"].ToString() : "",
                    JobTitle = (result["jobtitle"] != null) ? result["jobtitle"].ToString() : ""
                };
                searchResults.Add(contact);
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
            _proxy.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ADALHelper.GetTokenSilent());
            _proxy.DefaultRequestHeaders.Add("Prefer", "odata.include-annotations=\"*\""); 
            HttpResponseMessage retrieveRes = await _proxy.GetAsync("contacts(" + id + ")?$select=fullname,address1_composite,jobtitle,_parentcustomerid_value,emailaddress1,telephone1,entityimage");

            JToken result = JObject.Parse(retrieveRes.Content.ReadAsStringAsync().Result);

            Contact contact = new Contact()
            {
                Id = id,
                FullName = result["fullname"].ToString(),
                Address1 = (result["address1_composite"] != null) ? result["address1_composite"].ToString() : "",
                JobTitle = (result["jobtitle"] != null) ? result["jobtitle"].ToString() : "",                
                ParentCompany = (result["_parentcustomerid_value"] != null) ? result["_parentcustomerid_value@OData.Community.Display.V1.FormattedValue"].ToString() : "",
                Email1 = (result["emailaddress1"] != null) ? result["emailaddress1"].ToString() : "",
                Telephone1 = (result["telephone1"] != null) ? result["telephone1"].ToString() : "",
                EntityImage = (result["entityimage"].ToString() != "") ? System.Convert.FromBase64String(result["entityimage"].ToString()) : null
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
            _proxy.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ADALHelper.GetTokenSilent());

            var searchResults = new List<Activity>();

            string fetch = String.Format(@"<fetch version='1.0' output-format='xml-platform' top='50' mapping='logical' distinct='false'>
  <entity name='activitypointer'>
    <attribute name='activityid' />
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

            // Use FetchXML to get data
            HttpResponseMessage retrieveRes = await _proxy.GetAsync(string.Format("activitypointers?fetchXml={0}", System.Uri.EscapeDataString(fetch)));
            JToken results = JObject.Parse(retrieveRes.Content.ReadAsStringAsync().Result)["value"];

            foreach (JToken activityresult in results)
            {
                // Instantiate Contact and fullfill partially
                // Instantiate Activity 
                Activity activity = new Activity()
                {
                    Id = (Guid)activityresult["activityid"],
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
            _proxy.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ADALHelper.GetTokenSilent());

            HttpResponseMessage retrieveRes = await _proxy.GetAsync("tasks(" + id + ")?$select=subject,actualend,description");

            JToken result = JObject.Parse(retrieveRes.Content.ReadAsStringAsync().Result);

            Activity activity = new Activity()
            {
                Id = id,
                Subject = result["subject"].ToString(),
                ActualEnd = new DateTimeOffset((result["actualend"] != null) ? ((DateTime)result["actualend"]) : DateTime.Now),
                Notes = (result["description"] != null) ? result["description"].ToString() : ""
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
            _proxy.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ADALHelper.GetTokenSilent());
            
            // If passed data does not have id, then its new operation.
            if (activity.Id == null || activity.Id == Guid.Empty)
            {                
                // Create task object
                JObject task = new JObject();
                task["subject"] = activity.Subject;

                task["actualstart"] = activity.ActualEnd;
                task["actualend"] = activity.ActualEnd;
                task["description"] = activity.Notes;
                task["regardingobjectid_contact@odata.bind"] = "/contacts(" + contact.Id + ")";
                
                // Create record first
                HttpRequestMessage createReq = new HttpRequestMessage(HttpMethod.Post, "tasks");
                createReq.Content = new StringContent(JsonConvert.SerializeObject(
                    task, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));

                createReq.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                HttpResponseMessage result = await _proxy.SendAsync(createReq);

                // Then update the status to close it.
                task = new JObject();
                task["statecode"] = 1;
                task["statuscode"] = 5;
                HttpRequestMessage updateReq = new HttpRequestMessage(new HttpMethod("PATCH"), result.Headers.Location.LocalPath);
                updateReq.Content = new StringContent(JsonConvert.SerializeObject(
                    task, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }), Encoding.UTF8, "application/json");

                result = await _proxy.SendAsync(updateReq);
            }
            else
            {
                // Create Task object to re-open completed task
                JObject task = new JObject();
                task["statecode"] = 0;
                task["statuscode"] = 3;

                HttpRequestMessage updateReq = new HttpRequestMessage(new HttpMethod("PATCH"), "tasks(" + activity.Id + ")");

                updateReq.Content = new StringContent(JsonConvert.SerializeObject(
                    task, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));
                updateReq.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                HttpResponseMessage result = await _proxy.SendAsync(updateReq);

                // Create task object to have new values. Set to complete it.
                task = new JObject();
                task["subject"] = activity.Subject;

                task["actualstart"] = activity.ActualEnd;
                task["actualend"] = activity.ActualEnd;
                task["description"] = activity.Notes;
                task["statecode"] = 1;
                task["statuscode"] = 5;

                updateReq = new HttpRequestMessage(new HttpMethod("PATCH"), "tasks(" + activity.Id + ")");

                updateReq.Content = new StringContent(JsonConvert.SerializeObject(
                    task, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));
                updateReq.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                result = await _proxy.SendAsync(updateReq);
            }
        }

        /// <summary>
        /// This method delete task record
        /// </summary>
        /// <param name="activity">Activity Record to delete</param>
        public async Task DeleteTask(Activity activity)
        {
            // Get AccessToken
            _proxy.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await ADALHelper.GetTokenSilent());
            HttpRequestMessage deleteReq = new HttpRequestMessage(new HttpMethod("PATCH"), "tasks(" + activity.Id + ")");

            await _proxy.DeleteAsync("tasks(" + activity.Id + ")");
        }

        #endregion
    }
}
