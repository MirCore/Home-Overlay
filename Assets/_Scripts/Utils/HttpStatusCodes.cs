using System.Net;
using System.Text.RegularExpressions;

namespace Utils
{
    public abstract class HttpStatusCodes
    {
        public static string GetDescription(int status)
        {
            return status switch
            {
                412 => "Precondition Failed",
                _ => GetReadableDescription(status)
            };
        }

        private static string GetReadableDescription(int code)
        {
            HttpStatusCode statusCode = (HttpStatusCode)code;
            // Convert the enum name to a string and split PascalCase
            string statusName = statusCode.ToString();
            string readableName = Regex.Replace(statusName, "([a-z])([A-Z])", "$1 $2");
            return readableName;
        }
    }
}