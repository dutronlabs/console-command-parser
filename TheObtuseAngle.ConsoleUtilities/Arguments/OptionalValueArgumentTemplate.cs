namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public sealed class OptionalValueArgumentTemplate : ArgumentTemplate
    {
        public OptionalValueArgumentTemplate()
            : this(null, null, null)
        {
        }

        public OptionalValueArgumentTemplate(string name, string[] aliases, string description)
            : base(name, aliases, description, true, false)
        {
        }
    }
}