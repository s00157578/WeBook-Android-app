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
                string str = qrData.Substring(0, 7);

                if(str == "contact")
                {
                    var contactActivity = new Intent(this, typeof(Contact));
                    contactActivity.PutExtra("qrData", qrData);
                    StartActivity(contactActivity);
                }
                else
                {
                       // DateTime startDate = Convert.ToDateTime(values[4]);
                        //DateTime endDate = Convert.ToDateTime(values[5]);
                        //AddCalenderEvent(values[2], values[3], startDate, endDate);
                      var uri = Android.Net.Uri.Parse(qrData);
                      var intent = new Intent(Intent.ActionView, uri);
                      StartActivity(intent);
                }
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