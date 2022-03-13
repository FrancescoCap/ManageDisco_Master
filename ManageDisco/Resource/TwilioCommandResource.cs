using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManageDisco.Resource
{
    public class TwilioCommandResource
    {
        //Function
        public const string SEND_COUPON = "SendCoupon";
        public const string SEND_PHONE_CONFIRM = "SendConfirmPhoneNumber";

        //Body field
        public const string FIELD_TO = "To";
        public const string FIELD_FROM = "From";
        public const string FIELD_ACCOUNTSID = "AccountSid";
        public const string FIELD_BODY = "Body";
        public const string FIELD_MEDIAURL = "MediaUrl";
    }
}
