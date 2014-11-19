namespace CMS.BL
{
    public enum ContentType
    {
        Content,
        Blog,
        Product,
        Category,
        Associated,
        CaseStudy,
        Testimonial,
        Portfolio,
        Gallery
    }

    public enum ContentStatus
    {
        Saved,
        Live,
        Archived
    }

    public enum SettingsKey
    {
        adDefaultWYSIWYGContent,
        recaptcha_private_key,
        recaptcha_public_key,
        priceSaveIncludesVAT,
        priceDisplayIncludesVAT,
        useTax,
        zeroStockItemsPurchasable,
        taxPercentage,
        postageTypeSetting,
        payPalMerchantID,
        payPalUsername,
        payPalPassword,
        payPalSignature,
        payPalCurrencyCode,
        payPalApiUrl,
        payPalCgiUrl,
        payPalLanguageCode,
        payPalSendOrderItems,
        palPalLanguageCode,
        sagePayVendorName,
        sagePayMode,
        sagePayCurrencyCode,
        IsSagePayEnabled,
        IsPayPalEnabled,
        sagePayMethod,
        donationCharityNumber,
        secureTradingMode,
        secureTradingNotificationPassword,
        secureTradingRedirectPassword,
        secureTradingSiteReference,
        secureTradingTestSiteReference,
        secureTradingCurrencyCode,
        currencySign,
        cookieConsentAllSites,
        cookieConsentSinglePage,
        stripeApiKey
    }

    public enum SettingsCategory
    {
        Common = 1,
        Ecommerce,
        Paypal,
        SagePay,
        Donation,
        SecureTrading,
        CookieConsent,
        Stripe

    }

    public enum SettingsType
    {
        Bool = 1,
        Enum,
        String
    }

    public enum ImageVersionName
    {
        thumb,
        medium,
        large,
        category,
        lifestyle,
        productThumb,
        eventlistThumb,
        eventThumb,
        portfolioThumb,
        portfolio,
        galleryThumb,
        gallery
    }

    public enum SecureTradingErrorCodes
    {
        Paid = 0,
        Declined = 7000
    }

    public enum OrderStatus
    {
        Commited = 1,
        Processing,
        Despatched,
        Paid,
        PaymentFailed,
        Canceled,
        Refunded
    }

    public enum PaymentType
    {
        PayPal = 1,
        SagePay,
        SecureTrading,
        Stripe
    }

    public enum SagePayPaymentType
    {
        Direct,
        Server
    }

    public enum SecureTradingMode
    {
        live,
        test
    }

    public enum PaymentStatus
    {
        Initialized = 0,
        RedirectedToExternal = 1,

        PayPal_Initialized = 10,
        PayPal_SetExpressCheckout_Failure = 11,
        PayPal_SetExpressCheckout_Success = 12,
        PayPal_LandingPage = 13,
        PayPal_LandingPage_PayerUnknown = 14,
        PayPal_DoExpressCheckout_Failure = 15,
        PayPal_DoExpressCheckout_Unknown = 16,

        SagePay_Aborted = 30,
        SagePay_Authenticated = 31,
        SagePay_Invalid = 32,
        SagePay_Malformed = 33,
        SagePay_NotAuthed = 34,
        SagePay_Registered = 35,
        SagePay_Rejected = 36,
        SagePay_Unknown = 37,
        SagePay_Error = 38,

        SecureTrading_Error = 50,
        SecureTrading_Declined = 51,

        Paid = 100
    }

    public enum CardType
    {
        VISA = 1,
        MC,
        MCDEBIT,
        DELTA,
        MAESTRO,
        UKE,
        AMEX,
        DC,
        JCB,
        LASER
        //PAYPAL
    }

    public enum SagePayTxType
    {
        PAYMENT,
        DEFERRED,
        AUTHENTICATE
    }

    public enum EventViewType
    {
        Calendar = 1,
        List
    }

    public enum PostageType
    {
        Band,
        Weight
    }

    public enum ProductType
    {
        AllProducts = 0,
        Item,
        Event,
        Donation
    }

    public enum DonationType
    {
        Single = 1,
        Monthly
    }

    public enum MailChimpErrors
    {
        Email_NotExists,
        Email_NotSubscribed,
        List_NotSubscribed
    }

    public enum SiteMapType
    {
        ProductShop = 1,
        EventShop,
        News,
        ContactUs,
        Testimonials,
        Sitemap,
        Subscribe,
        Donation,
        PointsOfInterest,
        Portfolio,
        Gallery
    }

    public enum MenuDisplayType
    {
        UnderParent = 1,
        Directly,
        Both
    }

    public enum CashPayment
    {
        Cash = 1,
        Cheque
    }

    public enum OrderType
    {
        Order = 1,
        Donation
    }
}
