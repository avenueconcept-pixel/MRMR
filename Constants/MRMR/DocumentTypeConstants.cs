namespace MyApp.Constants.MRMR;

public static class DocumentTypeConstants
{
    // String values stored in DB — map to DocumentType enum
    public const string IcPassport           = "ic_passport";
    public const string CompanyRegistration  = "company_registration";
    public const string AuditorReport        = "auditor_report";
    public const string BusinessProfile      = "business_profile";
    public const string TaxCompliance        = "tax_compliance";
    public const string SupportingDocument1  = "supporting_document_1";
    public const string SupportingDocument2  = "supporting_document_2";
    public const string SupportingDocument3  = "supporting_document_3";
    public const string SupportingDocument4  = "supporting_document_4";
    public const string SupportingDocument5  = "supporting_document_5";
    public const string PaymentSlip          = "payment_slip";

    // Required documents for Individual applications
    public static readonly string[] RequiredIndividual =
    [
        IcPassport, BusinessProfile, TaxCompliance
    ];

    // Required documents for Corporate applications
    public static readonly string[] RequiredCorporate =
    [
        IcPassport, CompanyRegistration, AuditorReport, BusinessProfile, TaxCompliance
    ];
}
