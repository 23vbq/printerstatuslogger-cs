using System.Text.RegularExpressions;

namespace PrinterStatusLogger.PrinterManaging
{
    public class PrinterManager
    {
        private List<PrinterModel> _printerModels;
        private List<Printer> _printers;

        public PrinterManager()
        {
            _printerModels = new List<PrinterModel>();
            _printers = new List<Printer>();

            RegisterPrinterModels();
            /*Printer test = new Printer("Fajna taka", "https://zetcode.com/csharp/httpclient/", _printerModels[0]);
            int t1 = -1;
            try
            {
                t1 = test.GetTonerLevel();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
            Console.WriteLine(t1);*/
        }

        public void AddPrinter(string name, string address, int modelId)
        {
            PrinterModel model = FindModel(modelId);
            if (model == null)
                throw new Exception("Invalid model");
            _printers.Add(new Printer(name, address, model));
        }
        private PrinterModel FindModel(int id)
        {
            foreach (PrinterModel model in _printerModels)
                if (model.Id == id) return model;
            return null;
        }
        private void RegisterPrinterModels()
        {
            PrinterModel model;
            /*model = new PrinterModel(0, "Elegancka drukarka", (c) =>
            {
                int r = -1;
                Match m = Regex.Match(c, @"/(?<=port = )[0-9]{2}/g");
                Console.WriteLine(m.Success ? 1 : 0); // FIXME nie działa
                if (m.Success)
                {
                    Int32.TryParse(m.Value, out r );
                }
                return r;
            });*/
            model = new PrinterModel(0, "Elegancka drukarka", @"(?<=port = )[0-9]{2}");
            _printerModels.Add(model);
        }

        public void ListPrinterModels()
        {
            Console.WriteLine("List of avaliable printers:");
            Console.WriteLine("Id\tName");
            foreach (PrinterModel model in _printerModels)
            {
                Console.WriteLine(model.Id + "\t" +  model.Name);
            }
        }
    }
}
