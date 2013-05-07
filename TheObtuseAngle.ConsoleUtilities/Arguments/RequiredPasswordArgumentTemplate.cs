namespace TheObtuseAngle.ConsoleUtilities.Arguments
{
    public sealed class RequiredPasswordArgumentTemplate : ArgumentTemplate
    {
        public RequiredPasswordArgumentTemplate()
        {
        }

        public RequiredPasswordArgumentTemplate(string name, string[] aliases, string description)
            : base(name, aliases, description, true, true, 1, true)
        {
        }
    }
}