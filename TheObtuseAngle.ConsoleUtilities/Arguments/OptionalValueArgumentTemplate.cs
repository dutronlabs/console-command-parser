namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public sealed class OptionalValueArgumentTemplate : ArgumentTemplate
    {
        public OptionalValueArgumentTemplate()
            : this(null, null, null, 1)
        {
        }

        public OptionalValueArgumentTemplate(string name, string[] aliases, string description)
            : this(name, aliases, description, 1)
        {
        }

        public OptionalValueArgumentTemplate(string name, string[] aliases, string description, int numberOfValueArgs)
            : base(name, aliases, description, true, false, numberOfValueArgs)
        {
        }
    }
}