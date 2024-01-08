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
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ex.Message);
                return -2;
            }
            if (content == null)
            {
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "\tSomething goes wrong");
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
