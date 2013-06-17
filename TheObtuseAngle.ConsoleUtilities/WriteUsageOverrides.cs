namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// Defines overrides to use when writing the usage of a command to the console window.
    /// </summary>
    public class WriteUsageOverrides : ArgumentWriteUsageOverride
    {
        /// <summary>
        /// Gets or sets the name to use for the command when writing usage. Empty string means use the original name.
        /// </summary>
        public string NameOverride { get; set; }

        /// <summary>
        /// Gets or sets the title to use for the command when writing usage. Empty string means use the original title.
        /// </summary>
        public string TitleOverride { get; set; }
    }
}