using System.Net;

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
        }

        public void AddPrinter(string name, string address, string modelId)
        {
            PrinterModel model = FindModel(modelId);
            if (model == null)
                throw new Exception("Invalid model");
            _printers.Add(new Printer(name, address, model));
        }
        private PrinterModel FindModel(string id)
        {
            foreach (PrinterModel model in _printerModels)
                if (model.Id == id) return model;
            return null;
        }
        public void RegisterPrinterModel(PrinterModel model)
        {
            if (modelIdExists(model))
            {
                Logger.Log(LogType.WARNING, "Cannot register PrinterModel - Model already exists: [" + model.Id + "; " + model.Name + "]");
                return;
            }
            _printerModels.Add(model);
            Logger.Log(LogType.INFO, "Registerred PrinterModel: [" + model.Id + "; " + model.Name + "]");
        }

        public void ListPrinterModels()
        {
            Logger.Log(LogType.INFO, "List of avaliable printers:");
            Logger.Log(LogType.INFO, "Id\tName");
            foreach (PrinterModel model in _printerModels)
                Logger.Log(LogType.INFO, model.Id + "\t" + model.Name);
        }
        public void RunPrinterScan()
        {
            foreach(Printer p in _printers)
            {
                Logger.Log(LogType.V_INFO, "Scanning [" + p.Name + "; " +  p.Address + "]");
                try
                {
                    int tonerlevel = p.GetTonerLevel();
                    if (tonerlevel != -1)
                        Logger.Log(p, tonerlevel);
                    if (Alerter.Handler(p, tonerlevel))
                        Logger.Log(LogType.V_WARNING, "Printer meet some alerter condition.");
                } catch (Exception ex)
                {
                    Logger.Log(LogType.ERROR, ex.Message + " at " + p.Address);
                    Alerter.AddError(p, ex.Message);
                }
            }
            Logger.Log(LogType.INFO, "Scan ended");
        }
        private bool modelIdExists(PrinterModel model)
        {
            foreach (PrinterModel x in _printerModels)
                if (x.Id == model.Id)
                    return true;
            return false;
        }
    }
}
