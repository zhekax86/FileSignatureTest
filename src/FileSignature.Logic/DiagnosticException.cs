using FileSignature.Logic.Internal.Models;
using Newtonsoft.Json;
using System.Text;

namespace FileSignature.Logic
{
    public class DiagnosticException : AggregateException
    {
        public string StateDump { get; private set; }

        public DiagnosticException(IEnumerable<Exception> exceptions, object state)
            : base(exceptions)
        {
            try
            {
                StateDump = JsonConvert.SerializeObject(state);
            }
            catch (Exception exception)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("An error occured while serializing state");
                sb.Append(exception.Message);
                sb.AppendLine(exception.StackTrace);

                StateDump = sb.ToString();
            }
        }
    }
}
