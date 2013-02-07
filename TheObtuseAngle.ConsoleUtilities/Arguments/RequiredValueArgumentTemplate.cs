namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public sealed class RequiredValueArgumentTemplate : ArgumentTemplate
    {
        public RequiredValueArgumentTemplate()
            : this(null, null, null, 1)
        {
        }

        public RequiredValueArgumentTemplate(string name, string[] aliases, string description)
            : this(name, aliases, description, 1)
        {
        }

        public RequiredValueArgumentTemplate(string name, string[] aliases, string description, int numberOfValueArgs)
            : base(name, aliases, description, true, true, numberOfValueArgs)
        {
        }
    }
}