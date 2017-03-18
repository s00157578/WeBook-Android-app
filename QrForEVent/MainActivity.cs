using Android.App;
using Android.Widget;
using Android.OS;
using ZXing;
using Android.Graphics;
using System.Collections.Generic;
using ZXing.Mobile;
using System.Linq;
using Android.Content;
using Android.Provider;
using System;
using Java.Util;

namespace QrForEVent
{
    [Activity(Label = "QrForEVent", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            Button qr = FindViewById<Button>(Resource.Id.btnScanQR);
            qr.Click += delegate
            {
                QrRead();
            };
        }
        
        public async void QrRead()
        {
            MobileBarcodeScanner.Initialize(Application);
            //sets the scanner to only scan Qr codes
            var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
            options.PossibleFormats = new List<ZXing.BarcodeFormat>()
            {
                ZXing.BarcodeFormat.QR_CODE
            };
            //scans the qr code and stores as variable
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();

            var result = await scanner.Scan(options);  
            if (result != null)
            {
                string qrData = result.Text;
                string[] values = qrData.Split(',').ToArray();

                if (values[0] == "contact")
                {
                    String displayName = values[1];
                    String mobileNumber = values[2];
                    String email = values[3];
                    AddContact(displayName, mobileNumber, email);
                }
                else
                {
                    if (values[0] == "event")
                    {
                        DateTime startDate = Convert.ToDateTime(values[4]);
                        DateTime endDate = Convert.ToDateTime(values[5]);
                        AddCalenderEvent(values[2], values[3], startDate, endDate);

                        var uri = Android.Net.Uri.Parse(qrData);
                        var intent = new Intent(Intent.ActionView, uri);
                        StartActivity(intent);
                    }
                }
            }

        }
        public void AddContact(String displayName,string mobileNumber, String email)
        {

            List<ContentProviderOperation> ops = new List<ContentProviderOperation>();

            int rawContactInsertIndex = ops.Count;

            ops.Add(ContentProviderOperation.NewInsert(Android.Provider.ContactsContract.RawContacts.ContentUri)
                 .WithValue(Android.Provider.ContactsContract.RawContacts.InterfaceConsts.AccountType, null)
                 .WithValue(Android.Provider.ContactsContract.RawContacts.InterfaceConsts.AccountName, null).Build());

            //Display Name

            ops.Add(ContentProviderOperation
                 .NewInsert(Android.Provider.ContactsContract.Data.ContentUri)
                 .WithValueBackReference(Android.Provider.ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex)
                 .WithValue(Android.Provider.ContactsContract.Data.InterfaceConsts.Mimetype, Android.Provider.ContactsContract.CommonDataKinds.StructuredName.ContentItemType)
                 .WithValue(Android.Provider.ContactsContract.CommonDataKinds.StructuredName.DisplayName, displayName).Build()); // Name of the person

            //mobile number

            ops.Add(ContentProviderOperation
                 .NewInsert(Android.Provider.ContactsContract.Data.ContentUri)
                 .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex)
                 .WithValue(Android.Provider.ContactsContract.Data.InterfaceConsts.Mimetype, Android.Provider.ContactsContract.CommonDataKinds.Phone.ContentItemType)
                 .WithValue(Android.Provider.ContactsContract.CommonDataKinds.Phone.Number, mobileNumber) // Number of the person
                 .WithValue(Android.Provider.ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type, "mobile").Build()); // Type of mobile number

            //email Address
            ops.Add(ContentProviderOperation
                .NewInsert(ContactsContract.Data.ContentUri)
                .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex)
                .WithValue(Android.Provider.ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Email.ContentItemType)
                .WithValue(Android.Provider.ContactsContract.CommonDataKinds.Email.Address, email).Build()); // Email Address
            //.WithValue(ContactsContract.CommonDataKinds.Email.TYPE, ContactsContract.CommonDataKinds.Email.TYPE_WORK)

            // Asking the Contact provider to create a new contact                 
            try
            {
                ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Exception: " + ex.Message, ToastLength.Long).Show();
            }
        }
        public void AddCalenderEvent(string title, string location,DateTime startDate, DateTime endDate)
        {
            int sdy = startDate.Day;
            int smn = startDate.Month;
            int syy = startDate.Year;
            int shh = startDate.Hour;
            int smm = startDate.Minute;

            int edy = endDate.Day;
            int emn = endDate.Month;
            int eyy = endDate.Year;
            int ehh = endDate.Hour;
            int emm = endDate.Minute;
            
            string[] calendarsProjection =
           {
                CalendarContract.Calendars.InterfaceConsts.Id,
                CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName,
                CalendarContract.Calendars.InterfaceConsts.AccountName,
        };

            ContentValues eventValues = new ContentValues();
            eventValues.Put(CalendarContract.Events.InterfaceConsts.CalendarId,1);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Title,title);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Description,"");
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtstart,GetDateTimeMS(syy, smm, sdy, shh, smm));
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtend,GetDateTimeMS(eyy, emm, edy, ehh, emm));

            eventValues.Put(CalendarContract.Events.InterfaceConsts.EventTimezone,"GMT");
            eventValues.Put(CalendarContract.Events.InterfaceConsts.EventEndTimezone,"GMT");

            var uri = ContentResolver.Insert(CalendarContract.Events.ContentUri,
                eventValues);
        }
        long GetDateTimeMS(int yr, int month, int day, int hr, int min)
        {
            Calendar c = Calendar.GetInstance(Java.Util.TimeZone.Default);

            c.Set(CalendarField.DayOfMonth, 15);
            c.Set(CalendarField.HourOfDay, hr);
            c.Set(CalendarField.Minute, min);
            c.Set(CalendarField.Month, Calendar.December);
            c.Set(CalendarField.Year, 2011);

            return c.TimeInMillis;
        }
    }
}