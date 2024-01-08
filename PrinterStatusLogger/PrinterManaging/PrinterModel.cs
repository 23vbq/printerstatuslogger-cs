using System.Text.RegularExpressions;

namespace PrinterStatusLogger.PrinterManaging
{
    public class PrinterModel
    {
        public string Id { get; private set; }
        public string Name { get; private set; }

        /*
         * Scan properties
         */
        public string ReadTonerLevelPath { get; private set; }
        private string ReadTonerLevelRegex;

        public PrinterModel(string id, string name, string readtonerlevelpath, string readtonerlevelregex)
        {
            Id = id;
            Name = name;
            ReadTonerLevelPath = readtonerlevelpath;
            ReadTonerLevelRegex = readtonerlevelregex;
        }

        public int ReadTonerLevelFromResponse(string http_response)
        {
            //return ReadTonerLevelFunction.Invoke(http_response);
            Match m = Regex.Match(http_response, ReadTonerLevelRegex);
            if (!m.Success)
                throw new Exception("Toner level not found.");
            return Int32.Parse(m.Value);
        }
    }
}
