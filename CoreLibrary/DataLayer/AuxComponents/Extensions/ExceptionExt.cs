namespace DataLayer.AuxComponents.Extensions;

public static class ExceptionExt
{
    public static string GetFullMessage(this Exception? value)
    {
        string message = "";

        while (value != null)
        {
            message += (value.Message + Environment.NewLine);
            value = value.InnerException;
        }

        return message;
    }
}
