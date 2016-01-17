# Activity Tracker UWP
This is a Dynamics CRM SDK sample applicaiton which uses Web API endpoint or SOAP endpoint depending on CRM version.

**Feature**
This application provides following capabilities
- SignIn to any Dynamics CRM Online organization
- Search Contact records for fullname
- Display the contact detail, with completed actvities
- Create/Update/Delete task for the contact
- Launch the application via Voice command (Cortana Integration)
- Take note via Voice (Voice to Text)
- Roam CRM Server settings when you use the application with multiple devices
- Change Theme (Dark/Light) from settings
- Adaptive UI

**SDK features**
This application uses following SDK features
- Use FetchXML to search contact/activity
- CRUD a record
- SetState or update state/status for Task
- Check Authority URL and CRM version

**Libraries**
This application uses following Libraries
- Template10 as base template. https://github.com/Windows-XAML/Template10
- ADAL for authentication. https://github.com/AzureAD/azure-activedirectory-library-for-dotnet
- JSON.NET for serialize/deserialize. https://github.com/JamesNK/Newtonsoft.Json
- CRM Mobile SDK for SOAP. https://code.msdn.microsoft.com/Mobile-Development-Helper-3213e2e6

***Other Activity Tracker Samples***
There are other Activity Tracker samples avaiable.
- For Windows Phone 8.1 https://code.msdn.microsoft.com/Activity-Tracker-Sample-c8da7a1e
- For Windows 8.1/Windows Phone 8.1 https://code.msdn.microsoft.com/Activity-Tracker-Plus-f62d80a5
- For Android https://github.com/DynamicsCRM/Android-Activity-Tracker-for-Dynamics-CRM
- For iOS https://github.com/DynamicsCRM/iOS-Activity-Tracker-for-Dynamics-CRM
