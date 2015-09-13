using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using keepass2android.Io;
using keepass2android.view;
using AlertDialog = Android.App.AlertDialog;
using Object = Java.Lang.Object;

namespace keepass2android
{
    [Activity(Label = "@string/app_name", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, Theme = "@style/MyTheme_Blue")]
    public class FileStorageSelectionActivity : AppCompatActivity
	{
		private readonly ActivityDesign _design;

		private FileStorageAdapter _fileStorageAdapter;

		public FileStorageSelectionActivity()
		{
			_design = new ActivityDesign(this);
		}

		public const string AllowThirdPartyAppGet = "AllowThirdPartyAppGet";
		public const string AllowThirdPartyAppSend = "AllowThirdPartyAppSend";

		class FileStorageAdapter: BaseAdapter
		{

			private readonly FileStorageSelectionActivity _context;

			private readonly List<string> _protocolIds = new List<string>(); 

			public FileStorageAdapter(FileStorageSelectionActivity context)
			{
				_context = context;
				//show all supported protocols:
				foreach (IFileStorage fs in App.Kp2a.FileStorages)
					_protocolIds.AddRange(fs.SupportedProtocols);

				//special handling for local files:
				if (!Util.IsKitKatOrLater)
				{
					//put file:// to the top
					_protocolIds.Remove("file");
					_protocolIds.Insert(0, "file");
					
					//remove "content" (covered by androidget)
					//On KitKat, content is handled by AndroidContentStorage taking advantage 
					//of persistable permissions and ACTION_OPEN/CREATE_DOCUMENT
					_protocolIds.Remove("content");
					
				}
				else
				{
					_protocolIds.Remove("file");
				}
					

				if (context.Intent.GetBooleanExtra(AllowThirdPartyAppGet, false))
					_protocolIds.Add("androidget");
				if (context.Intent.GetBooleanExtra(AllowThirdPartyAppSend, false))
					_protocolIds.Add("androidsend");
#if NoNet
				_protocolIds.Add("kp2a");
#endif
			}

			public override Object GetItem(int position)
			{
				return _protocolIds[position];
			}

			public override long GetItemId(int position)
			{
				return position;
			}



            public static float convertDpToPixel(float dp, Context context)
            {
                Resources resources = context.Resources;
                DisplayMetrics metrics = resources.DisplayMetrics;
                float px = dp * metrics.Density;
                return px;
            }


			public override View GetView(int position, View convertView, ViewGroup parent)
			{

                Button btn;

                if (convertView == null)
                {  // if it's not recycled, initialize some attributes

                    btn = new Button(_context);
                    btn.LayoutParameters = new GridView.LayoutParams((int)convertDpToPixel(90, _context), (int)convertDpToPixel(110, _context));
                    btn.SetBackgroundResource(Resource.Drawable.storagetype_button_bg);
                    btn.SetPadding(8,64,8,8);
                    btn.SetTextSize(ComplexUnitType.Sp, 11);
                    btn.SetTextColor(new Color(115, 115, 115));
                    btn.SetSingleLine(false);
					btn.Gravity = GravityFlags.Center;
                    btn.Click += (sender, args) => _context.OnItemSelected( (string) ((Button)sender).Tag);
                }
                else
                {
                    btn = (Button)convertView;
                }
			    
			    var protocolId = _protocolIds[position];
                btn.Tag = protocolId;
                Drawable drawable = App.Kp2a.GetResourceDrawable("ic_storage_" + protocolId);
                
                String title = App.Kp2a.GetResourceString("filestoragename_" + protocolId);
                var str = new SpannableString(title);

			    btn.TextFormatted = str;
                //var drawable = ContextCompat.GetDrawable(context, Resource.Drawable.Icon);
                btn.SetCompoundDrawablesWithIntrinsicBounds(null, drawable, null, null);

                //TODO kp2a
                return btn;
                /*
				if (_protocolIds[position] == "kp2a")
				{
					return new FileStorageViewKp2a(_context);
				}
				else
				{
					var view = new FileStorageView(_context, _protocolIds[position], position);
					return view;	
				}
				*/

			}

			public override int Count
			{
				get { return _protocolIds.Count; }
			}
		}

		private void OnItemSelected(string protocolId)
		{
			if (protocolId == "kp2a")
			{
				//send user to market page of regular edition to get more protocols 
				Util.GotoUrl(this, GetString(Resource.String.MarketURL) + "keepass2android.keepass2android");
				return;
			}

			var field = typeof(Resource.String).GetField("filestoragehelp_" + protocolId);
			if (field == null)
			{
				//no help available
				ReturnProtocol(protocolId);
			}
			else
			{
				//set help:
				string help = GetString((int)field.GetValue(null));

				new AlertDialog.Builder(this)
					.SetTitle(GetString(Resource.String.app_name))
					.SetMessage(help)
					.SetPositiveButton(Android.Resource.String.Ok, (sender, args) => ReturnProtocol(protocolId))
					.Create()
					.Show();
			}
			

		}

		private void ReturnProtocol(string protocolId)
		{
			Intent intent = new Intent();
			intent.PutExtra("protocolId", protocolId);
			SetResult(KeePass.ExitFileStorageSelectionOk, intent);
			Finish();
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			_design.ApplyTheme();

			SetContentView(Resource.Layout.filestorage_selection);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.mytoolbar);
            
            SetSupportActionBar(toolbar);

		    SupportActionBar.Title = RemoveTrailingColon(GetString(Resource.String.select_storage_type));
            
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
		    SupportActionBar.SetDisplayShowHomeEnabled(true);
		    toolbar.NavigationClick += (sender, args) => OnBackPressed();

			_fileStorageAdapter = new FileStorageAdapter(this);
			var gridView = FindViewById<GridView>(Resource.Id.gridview);
			gridView.ItemClick +=
				(sender, args) => OnItemSelected((string)_fileStorageAdapter.GetItem(args.Position));
		    gridView.Adapter = _fileStorageAdapter;
		    
		}

        private string RemoveTrailingColon(string str)
        {
            if (str.EndsWith(":"))
                return str.Substring(0, str.Length - 1);
            return str;
        }

        protected override void OnResume()
		{
			base.OnResume();
			_design.ReapplyTheme();
		}
	}
}