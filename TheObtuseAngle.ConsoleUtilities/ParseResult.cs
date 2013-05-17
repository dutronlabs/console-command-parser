namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// Enum that indicates the result of the parsing.
    /// </summary>
    public enum ParseResult
    {
        /// <summary>
        /// Success. Continue execution.
        /// </summary>
        Success,

        /// <summary>
        /// Outright failure. Execution should not continue.
        /// </summary>
        Failure,

        /// <summary>
        /// Help text was written to the console.
        /// </summary>
        DisplayedHelp,

        /// <summary>
        /// No matching argument / command was found.
        /// </summary>
        NoMatchFound
    }
}
