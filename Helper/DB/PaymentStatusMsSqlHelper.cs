namespace MyApp.Helper.DB;

public class PaymentStatusMsSqlHelper : MsSqlHelper
{
    public PaymentStatusMsSqlHelper(IConfiguration config, ILoggerFactory loggerFactory)
        : base(config, loggerFactory, nameof(PaymentStatusMsSqlHelper)) { }

    // TODO: implement when Order module is ready
    // public async Task<string?> GetPaymentStatusAsync(string orderReference)
    //     => await ExecuteAsync(async () =>
    //     {
    //         using var conn = await OpenAsync();
    //         using var cmd  = new SqlCommand(
    //             "SELECT payment_status FROM orders WHERE order_ref = @ref", conn);
    //         cmd.Parameters.AddWithValue("@ref", orderReference);
    //         var result = await cmd.ExecuteScalarAsync();
    //         return result as string;
    //     });
}
