using System;

namespace TheObtuseAngle.ConsoleUtilities
{
    /// <summary>
    /// An object that holds all the options the ConsoleHelper class will use when writing tabular data to the console.
    /// </summary>
    public sealed class TableOptions
    {
        /// <summary>
        /// The default instance of the <see cref="TableOptions"/> class.
        /// </summary>
        public static readonly TableOptions Defaults = new TableOptions();

        /// <summary>
        /// Constructs a new <see cref="TableOptions"/> instance with the default values.
        /// </summary>
        public TableOptions()
        {
            MinimumColumnWidth = 5;
            WriteEmptyLineBetweenRows = false;
            AlternateRowColor = true;
            RowColor = ConsoleColor.White;
            AlternatingRowColor = ConsoleColor.Gray;
            HeaderRowColor = ConsoleColor.Cyan;
            DefaultColumnWidthMode = ColumnWidthMode.Auto;
        }

        /// <summary>
        /// Gets or sets the minimum width for every column in the table. This is a global default. The <see cref="ColumnDefinition.MinWidth"/> property overrides this value if set. Only positive values are allowed. Default is 5.
        /// </summary>
        public int MinimumColumnWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to write a blank line between each row in the table. Default is false.
        /// </summary>
        public bool WriteEmptyLineBetweenRows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to alternate the color of each row in the table. Default is true.
        /// </summary>
        public bool AlternateRowColor { get; set; }

        /// <summary>
        /// Gets or sets the color to use when writing rows. Default is White.
        /// </summary>
        public ConsoleColor RowColor { get; set; }

        /// <summary>
        /// Gets or sets the color to use when writing alternating rows. Default is Gray.
        /// </summary>
        public ConsoleColor AlternatingRowColor { get; set; }

        /// <summary>
        /// Gets or sets the color to use when writing the header row of the table. Default is Cyan.
        /// </summary>
        public ConsoleColor HeaderRowColor { get; set; }

        /// <summary>
        /// Gets or sets the default column width mode to use for columns that do not have a definition. Default is Auto.
        /// </summary>
        public ColumnWidthMode DefaultColumnWidthMode { get; set; }
    }
}