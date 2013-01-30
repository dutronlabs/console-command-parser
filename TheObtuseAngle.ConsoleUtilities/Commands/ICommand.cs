using System.Collections.Generic;
using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities.Commands
{
    /// <summary>
    /// Represents a console command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The name of the command.  This is what the user types into the console to invoke the command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The title of the command.  This is a short, one line description of what the command does.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The description of the command.  This is the long, verbose description of what the command does.  For complicated commands examples are also appropriate here.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The position of this command in the list of all possible commands.  This is used to order the list of possible commands when writing usage help.
        /// </summary>
        int Ordinal { get; set; }

        /// <summary>
        /// The collection of arguments for this command.
        /// </summary>
        List<IArgument> Arguments { get; }

        /// <summary>
        /// Executes the command and returns true if the command executed successfully, otherwise false.
        /// </summary>
        /// <returns>True if the command executed successfully, otherwise false.</returns>
        bool Execute();

        /// <summary>
        /// Writes the usage help for this command to the console.  When deriving from CommandBase the default behavior will be the same as calling the WriteUsage method on a CommandParser&lt;T&gt; instance with the default ParseOptions.
        /// </summary>
        void WriteUsage();

        void OnBeforeParse();
    }
}
