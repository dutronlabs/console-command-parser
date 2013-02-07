namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public sealed class OptionalArgumentTemplate : ArgumentTemplate
    {
        public OptionalArgumentTemplate()
            : this(null, null, null)
        {
        }

        public OptionalArgumentTemplate(string name, string[] aliases, string description)
            : base(name, aliases, description, false, false, 0)
        {
        }
    }
}