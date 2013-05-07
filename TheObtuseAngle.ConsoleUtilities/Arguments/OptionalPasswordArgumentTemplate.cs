namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public sealed class OptionalPasswordArgumentTemplate : ArgumentTemplate
    {
        public OptionalPasswordArgumentTemplate()
            : this(null, null, null)
        {
        }

        public OptionalPasswordArgumentTemplate(string name, string[] aliases, string description)
            : base(name, aliases, description, true, false, 1, true)
        {
        }
    }
}