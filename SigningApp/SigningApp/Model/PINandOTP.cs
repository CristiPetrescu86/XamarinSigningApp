using System;
using System.Collections.Generic;
using System.Text;

namespace SigningApp.Model
{
    class PINandOTP
    {
        private string pin;
        private string otp;

        public string PIN
        {
            get { return pin; }
            set { pin = value; }
        }

        public string OTP
        {
            get { return otp; }
            set { otp = value; }
        }


        public PINandOTP(string pin, string otp)
        {
            PIN = pin;
            OTP = otp;
        }

        public PINandOTP()
        {

        }


    }
}
