namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public sealed class RequiredArgumentTemplate : ArgumentTemplate
    {
        public RequiredArgumentTemplate()
            : this(null, null, null)
        {
        }

        public RequiredArgumentTemplate(string name, string[] aliases, string description)
            : base(name, aliases, description, false, true)
        {
        }
    }
}