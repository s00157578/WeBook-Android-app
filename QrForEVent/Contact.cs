using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Provider;

namespace QrForEVent
{
    [Activity(Label = "Contact")]
    public class Contact : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Contact);
            //setting textviews
            TextView txtName = FindViewById<TextView>(Resource.Id.txtName);
            TextView txtEmail = FindViewById<TextView>(Resource.Id.txtEmail);
            TextView txtNumber = FindViewById<TextView>(Resource.Id.txtNumber);
            //if value is not correct go back to mainActivity
            string qrData = Intent.GetStringExtra("qrData") ?? "Data not available";
            if (qrData == "Data not available")
            {
                Finish();
            }
            string[] values = qrData.Split(',').ToArray();
            txtName.Text = values[1];
            txtEmail.Text = values[3];
            txtNumber.Text = values[2];
            // Create your application here
            Button cancel = FindViewById<Button>(Resource.Id.btnCancel);
            cancel.Click += delegate
            {
                Finish();
            };
            Button add = FindViewById<Button>(Resource.Id.btnAdd);
            add.Click += delegate
            {
                String displayName = values[1];
                String mobileNumber = values[2];
                String email = values[3];
                AddContact(displayName, mobileNumber, email);
                Finish();
            };
        }
        public void AddContact(String displayName, string mobileNumber, String email)
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
    }
}