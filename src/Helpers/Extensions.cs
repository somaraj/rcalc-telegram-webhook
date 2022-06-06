namespace RCalcTelegramWebhook.Helpers
{
    public static class Extensions
    {
        public static long ToLong(this long? str)
        {
            long.TryParse(str.ToString(), out long outValue);
            return outValue;
        }
    }
}
