namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// Defines the possible column width modes to use with the ConsoleHelper.WriteTable methods.
    /// </summary>
    public enum ColumnWidthMode
    {
        /// <summary>
        /// The column width will be automatically scaled to fit using the percentage of the original total width.
        /// </summary>
        /// <remarks>
        /// If the width of all the columns is 200, but the maximum width (console buffer) is 80 then a column that wants to
        /// have a width of 40 will actually use a width of (40 * 80) / 200 = 16.
        /// </remarks>
        Auto,

        /// <summary>
        /// The column will occupy all of the remaining width after all other column widths have been computed.
        /// </summary>
        Dynamic,

        /// <summary>
        /// The column width will be the length of the longest item in that column.
        /// </summary>
        Max,

        /// <summary>
        /// The column width will always be the given integer value no matter what.
        /// </summary>
        Fixed
    }

    /// <summary>
    /// Defines the attributes of a column when using the ConsoleHelper.WriteTable methods.
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// Constructs a new ColumnDefinition instance with no header and automatic width.
        /// </summary>
        public ColumnDefinition()
            : this(string.Empty, ColumnWidthMode.Auto)
        {
        }

        /// <summary>
        /// Constructs a new ColumnDefinition instance with the given header and automatic width.
        /// </summary>
        public ColumnDefinition(string header)
            : this(header, ColumnWidthMode.Auto)
        {
        }

        /// <summary>
        /// Constructs a new ColumnDefinition instance with the given header and fixed width.
        /// </summary>
        public ColumnDefinition(string header, int width)
            : this(header, ColumnWidthMode.Fixed, 0)
        {
            Width = width;
        }

        /// <summary>
        /// Constructs a new ColumnDefinition instance with the given header and width mode.
        /// </summary>
        public ColumnDefinition(string header, ColumnWidthMode widthMode)
            : this(header, widthMode, 0)
        {
        }

        /// <summary>
        /// Constructs a new ColumnDefinition instance with the given header, width mode, an minimum width.
        /// </summary>
        public ColumnDefinition(string header, ColumnWidthMode widthMode, int minWidth)
        {
            Header = header;
            WidthMode = widthMode;
            MinWidth = minWidth;
        }

        /// <summary>
        /// Gets or sets the header of the column.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the width mode of the column.
        /// </summary>
        /// <remarks>
        /// If the width mode is set to <see cref="ColumnWidthMode.Fixed"/> but the <see cref="Width"/> property is less than or equal to zero then the automatic width mode will be used instead.
        /// </remarks>
        public ColumnWidthMode WidthMode { get; set; }

        /// <summary>
        /// Gets or sets the minimum width to be used for this column. This is essentially an override and applies to any width mode.
        /// </summary>
        public int MinWidth { get; set; }

        /// <summary>
        /// Gets or sets the fixed width of the column. This value is only used when the <see cref="WidthMode"/> property is set to <see cref="ColumnWidthMode.Fixed"/>.
        /// </summary>
        public int Width { get; set; }
    }
}