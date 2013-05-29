using TheObtuseAngle.ConsoleUtilities.Arguments;

namespace TheObtuseAngle.ConsoleUtilities.Commands
{
    /// <summary>
    /// Defines a help command.
    /// </summary>
    public class HelpCommandTemplate : CommandTemplate
    {
        /// <summary>
        /// The <see cref="ArgumentTemplate"/> that defines the -command argument of the help command.
        /// </summary>
        public ArgumentTemplate CommandArgumentTemplate { get; set; }
    }
}