using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TheObtuseAngle.ConsoleUtilities.Arguments;
using System.Linq;

namespace TheObtuseAngle.ParseCommandsConsole
{
    [Export(typeof(ExampleCommandBase))]
    public sealed class AddUserCommand : AuthenticatedCommandBase
    {
        private string nickname;

        public AddUserCommand()
            : base("AddUser", "Adds a user to the user store")
        {
        }

        protected override IEnumerable<IArgument> GetArguments()
        {
            var arguments = base.GetArguments().ToList();
            arguments.Insert(0, new OptionalValueArgument("-nickname", "-nn", "The nickname of the new user", val => nickname = val));
            return arguments;
        }

        public override bool Execute()
        {
            Console.WriteLine("Adding user:{0}  Username: {1}{0}  Password: {2}{0}  Nickname: {3}", Environment.NewLine, userName, password, nickname);
            return true;
        }
    }
}