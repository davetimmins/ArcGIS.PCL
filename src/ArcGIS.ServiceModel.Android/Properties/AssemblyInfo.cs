using System.Reflection;
using Android.App;
using System.Resources;

[assembly: AssemblyTitle("ArcGIS.ServiceModel.Android")]
[assembly: AssemblyDescription("Operations and data types used with the ArcGIS Server REST API.")]

[assembly: NeutralResourcesLanguage("en")]

// Add some common permissions, these can be removed if not needed
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
