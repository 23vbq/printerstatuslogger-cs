using System.Text.RegularExpressions;

namespace PrinterStatusLogger.PrinterManaging
{
    public class PrinterModel
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        //private Func<string, int> ReadTonerLevelFunction;
        private string ReadTonerLevelRegex;

        public PrinterModel(int id, string name, string readtonerlevelregex) //Func<string, int> readtonerlevelfunction
        {
            Id = id;
            Name = name;
            //ReadTonerLevelFunction = readtonerlevelfunction;
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
