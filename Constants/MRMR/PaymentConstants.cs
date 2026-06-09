namespace MyApp.Constants.MRMR;

public static class PaymentConstants
{
    public const decimal NominationFeeAmount = 1000.00m;    // RM 1,000 — fixed, never from DB
    // Award fee is read from award_categories.price — never hardcoded here
    public const string Currency = "MYR";
    public const string CurrencySymbol = "RM";
    public const int AxaipayCallbackTimeoutMinutes = 30;
}
