namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public sealed class RequiredValueArgumentTemplate : ArgumentTemplate
    {
        public RequiredValueArgumentTemplate()
            : this(null, null, null)
        {
        }

        public RequiredValueArgumentTemplate(string name, string[] aliases, string description)
            : base(name, aliases, description, true, true)
        {
        }
    }
}