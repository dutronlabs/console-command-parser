namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// Defines overrides to use when writing the usage of an argument to the console window.
    /// </summary>
    public class ArgumentWriteUsageOverride
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to skip the argument or command to the console.
        /// </summary>
        public bool Skip { get; set; }

        /// <summary>
        /// Gets or sets the description to use for the argument or command when writing usage. Empty string means use the original description.
        /// </summary>
        public string DescriptionOverride { get; set; }
    }
}