using System;
using System.Collections.Generic;

namespace TheObtuseAngle.ConsoleUtilities
{
    internal class ArgumentParseResult
    {
        public ArgumentParseResult()
        {
            IsSuccess = true;
        }

        public ArgumentParseResult(bool hasNoConsoleArgs)
        {
            this.HasNoConsoleArgs = hasNoConsoleArgs;
        }

        public ArgumentParseResult(IArgument argumentWithMissingValue)
        {
            this.ArgumentWithMissingValue = argumentWithMissingValue;
        }

        public ArgumentParseResult(Tuple<IArgument, Exception> argumentValueSetterExceptionPair)
        {
            this.ArgumentValueSetterExceptionPair = argumentValueSetterExceptionPair;
        }

        public ArgumentParseResult(IEnumerable<IArgument> missingRequiredArguments)
        {
            this.MissingRequiredArguments = missingRequiredArguments;
        }

        public bool IsSuccess { get; private set; }

        public bool HasNoConsoleArgs { get; private set; }

        public IArgument ArgumentWithMissingValue { get; private set; }

        public Tuple<IArgument, Exception> ArgumentValueSetterExceptionPair { get; private set; }

        public IEnumerable<IArgument> MissingRequiredArguments { get; private set; }
    }
}
