namespace PrinterStatusLogger.CommandHandling
{
    public class CommandHandler
    {
        public Command RootCommand;

        public CommandHandler()
        {
            RootCommand = new Command("", "", null);
            RootCommand.Leaves.Add(new Command("kot", "funkcja kotowa", () => { Console.WriteLine("Koty sa skonczone"); }));
        }

        public void Handle(string[] args)
        {
            if (args == null) return;
            Command finder = RootCommand;
            int i = 0;
            foreach (Command cmd in finder.Leaves)
            {
                if (cmd.Name == args[i])
                {
                    cmd.Call();
                    return;
                }
            }
        }
    }
}
