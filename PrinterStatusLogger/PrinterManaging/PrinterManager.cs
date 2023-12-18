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
            model = new PrinterModel(0, "Elegancka drukarka", @"(?<=port = )[0-9]{2}");
            model = new PrinterModel(1, "Elegancka drukarka", "(?<=<h1 id=\"trundeid\">)[0-9]{2}(?=%</h1>)");
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
        public void RunPrinterScan()
        {
            foreach(Printer p in _printers)
            {
                int tonerlevel = p.GetTonerLevel();
                Logger.Log(p, tonerlevel);
            }
            Logger.Log(LogType.INFO, "Scan ended");
        }
    }
}
