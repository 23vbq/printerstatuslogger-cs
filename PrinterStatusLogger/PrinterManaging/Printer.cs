namespace PrinterStatusLogger.PrinterManaging
{
    public class Printer
    {
        public string Name;
        public string Address { get; private set; }
        public PrinterModel Model { get; private set; }

        public Printer(string name, string address, PrinterModel model)
        {
            Name = name;
            Address = address;
            Model = model;
        }

        public int GetTonerLevel()
        {
            if (Model == null)
                return -4;
            string? content = null;
            try
            {
                content = GetPrinterWebInterface();
            } catch (Exception ex)
            {
                Logger.Log(LogType.ERROR, ex.Message + " at " + Address);
                return -2;
            }
            if (content == null)
            {
                Logger.Log(LogType.ERROR, "Something gone wrong at " + Address);
                return -3;
            }
            return Model.ReadTonerLevelFromResponse(content);
        }
        private string? GetPrinterWebInterface()
        {
            string? content = null;
            using (var client = new HttpClient())
            {
                var result = client.GetAsync(this.Address + Model.ReadTonerLevelPath);
                result.Wait();
                if (result.Result.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception(result.Result.StatusCode.ToString());
                content = result.Result.Content.ReadAsStringAsync().Result;
            }
            return content;
        }
    }
}
