
using TheObtuseAngle.ConsoleUtilities.Commands;

namespace TheObtuseAngle.ConsoleUtilities
{
    public sealed class CommandParseResult
    {
        internal static readonly CommandParseResult Failure = new CommandParseResult(ParseResult.Failure, null);
        internal static readonly CommandParseResult DisplayedHelp = new CommandParseResult(ParseResult.DisplayedHelp, null);
        internal static readonly CommandParseResult NoMatchFound = new CommandParseResult(ParseResult.NoMatchFound, null);

        internal CommandParseResult(ParseResult parseResult, ICommand command)
        {
            ParseResult = parseResult;
            Command = command;
        }

        public ParseResult ParseResult { get; private set; }

        public ICommand Command { get; private set; }
    }
}
