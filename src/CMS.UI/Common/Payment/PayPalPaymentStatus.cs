using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Common.Payment
{
    public class PayPalPaymentStatus
    {
        private string transactionID = "";
        private string token = "";
        private string ack = "";
        private string paymentStatus = "";
        private string pendingReason = "";
        private string errorCode = "";
        private string errorMessage = "";

        public string TransactionID
        {
            get { return transactionID; }
            set
            {
                if (value == null)
                    transactionID = "";
                else
                    transactionID = value;
            }
        }

        public string Token
        {
            get { return token; }
            set
            {
                if (value == null)
                    token = "";
                else
                    token = value;
            }
        }

        public string ACK
        {
            get { return ack; }
            set
            {
                if (value == null)
                    ack = "";
                else
                    ack = value;
            }
        }

        public string PaymentStatus
        {
            get { return paymentStatus; }
            set
            {
                if (value == null)
                    paymentStatus = "";
                else
                    paymentStatus = value;
            }
        }

        public string PendingReason
        {
            get { return pendingReason; }
            set
            {
                if (value == null)
                    pendingReason = "";
                else
                    pendingReason = value;
            }
        }

        public string ErrorCode
        {
            get { return errorCode; }
            set
            {
                if (value == null)
                    errorCode = "";
                else
                    errorCode = value;
            }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                if (value == null)
                    errorMessage = "";
                else
                    errorMessage = value;
            }
        }
    }
}