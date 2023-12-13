using System.Runtime.CompilerServices;

namespace PrinterStatusLogger.CommandHandling
{
    public class Command
    {
        public string Name;
        public string Description;

        public Action CommandAction;
        public List<Command> Leaves;

        public Command(string name, string description, Action commandAction)
        {
            Name = name;
            Description = description;
            CommandAction = commandAction;
            Leaves = new List<Command>();
        }

        public void Call()
        {
            CommandAction.Invoke();
        }
    }
}
