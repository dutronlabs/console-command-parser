namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// Defines overrides to use when writing the usage of a command to the console window.
    /// </summary>
    public class WriteUsageOverrides
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to skip writing this command to the console.
        /// </summary>
        public bool Skip { get; set; }

        /// <summary>
        /// Gets or sets the name to use for the command when writing usage. Empty string means use the original name.
        /// </summary>
        public string NameOverride { get; set; }

        /// <summary>
        /// Gets or sets the title to use for the command when writing usage. Empty string means use the original title.
        /// </summary>
        public string TitleOverride { get; set; }

        /// <summary>
        /// Gets or sets the description to use for the command when writing usage. Empty string means use the original description.
        /// </summary>
        public string DescriptionOverride { get; set; }
    }
}