
public interface IDummyAcquiringBank
{
    // Note: realistically Bank would need to make use of all parameters - for simplification purposes the Bank payment processing simulation only uses PAN
    public bool isValidPayment(string pan, string cardHolder, DateOnly cardExpiryDate, string cvv, decimal amount);
    public string getSwiftCode();
}
