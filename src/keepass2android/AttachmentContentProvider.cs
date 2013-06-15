using System;
using Android.Content;
using Android.OS;
using Android.Util;
using Java.IO;

namespace keepass2android
{
	[ContentProvider(new[]{"keepass2android."+AppNames.PackagePart+".provider"})] 
	public class AttachmentContentProvider : ContentProvider {
		
		private const String ClassName = "AttachmentContentProvider";
		
		// The authority is the symbolic name for the provider class
		public const String Authority = "keepass2android."+AppNames.PackagePart+".provider";


		public override bool OnCreate() {
			return true;
		}
		
		public override ParcelFileDescriptor OpenFile(Android.Net.Uri uri, String mode)
		{
			
			const string logTag = ClassName + " - openFile";
			
			Log.Verbose(logTag,
			      "Called with uri: '" + uri + "'." + uri.LastPathSegment);
			
			if (uri.ToString().StartsWith("content://" + Authority))
			{
				// The desired file name is specified by the last segment of the
				// path
				// E.g.
				// 'content://keepass2android.provider/Test.txt'
				// Take this and build the path to the file
				String fileLocation = Context.CacheDir + File.Separator
					+ uri.LastPathSegment;
					
				// Create & return a ParcelFileDescriptor pointing to the file
				// Note: I don't care what mode they ask for - they're only getting
				// read only
				ParcelFileDescriptor pfd = ParcelFileDescriptor.Open(new File(
					fileLocation), ParcelFileMode.ReadOnly);
				return pfd;
					
			}
			Log.Verbose(logTag, "Unsupported uri: '" + uri + "'.");
			throw new FileNotFoundException("Unsupported uri: "
			                                + uri.ToString());
		}
		
		// //////////////////////////////////////////////////////////////
		// Not supported / used / required for this example
		// //////////////////////////////////////////////////////////////
		

		public override int Update(Android.Net.Uri uri, ContentValues contentvalues, String s,
			                 String[] strings) {
			return 0;
		}
		
		public override  int Delete(Android.Net.Uri uri, String s, String[] strings) {
			return 0;
		}
		

		public override Android.Net.Uri Insert(Android.Net.Uri uri, ContentValues contentvalues) {
			return null;
		}
		

		public override String GetType(Android.Net.Uri uri) {
			return null;
		}

		public override Android.Database.ICursor Query(Android.Net.Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
		{
			return null;
		}

	}
}
