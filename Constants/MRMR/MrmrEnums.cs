namespace MyApp.Constants.MRMR;

public enum ApplicationType
{
    Individual,
    Corporate
}

public enum ApplicationStatus
{
    Registered,               // After registration form submitted
    NominationFeePending,     // Awaiting nomination fee payment
    NominationFeeVerified,    // Nomination fee approved — credentials emailed
    AwardFeePending,          // Awaiting award fee payment
    AwardFeeVerified,         // Award fee approved — submission unlocked
    Submitted,                // Final submission submitted
    UnderReview,              // Admin/judge review in progress
    Shortlisted,              // Shortlisted by committee
    AwardRecipient,           // Final winner
    Rejected,                 // Rejected at any stage
    Withdrawn                 // Applicant withdrew
}

public enum PaymentType
{
    NominationFee,
    AwardFee
}

public enum PaymentMethod
{
    Axaipay,
    ManualBankTransfer
}

public enum PaymentStatus
{
    Pending,
    PendingVerification,  // Manual transfer: slip uploaded, awaiting admin
    Verified,
    Rejected,
    Cancelled
}

public enum DocumentType
{
    IcPassport,
    CompanyRegistration,
    AuditorReport,
    BusinessProfile,
    TaxCompliance,
    SupportingDocument1,
    SupportingDocument2,
    SupportingDocument3,
    SupportingDocument4,
    SupportingDocument5,
    PaymentSlip             // Manual payment bank slip
}

public enum DocumentVerificationStatus
{
    Pending,
    Verified,
    Rejected
}

public enum EvaluationStatus
{
    NotStarted,
    Draft,
    Submitted
}

public enum Industry
{
    Agriculture,
    Automotive,
    Construction,
    Education,
    Finance,
    FoodBeverage,
    Healthcare,
    Hospitality,
    ITTechnology,
    Manufacturing,
    Media,
    ProfessionalServices,
    Retail,
    Telecommunications,
    Trading,
    Transportation,
    Others
}
