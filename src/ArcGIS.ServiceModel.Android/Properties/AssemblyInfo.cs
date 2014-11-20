using System.Reflection;
using Android.App;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("ArcGIS.ServiceModel.Android")]
[assembly: AssemblyDescription("Operations and data types used with the ArcGIS Server REST API.")]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en")]

// Add some common permissions, these can be removed if not needed
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
