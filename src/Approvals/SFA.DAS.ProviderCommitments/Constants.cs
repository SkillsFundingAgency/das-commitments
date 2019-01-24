namespace SFA.DAS.ProviderCommitments
{
    public static class Constants
    {
        public static class EnvironmentVariableNames
        {
            /// <summary>
            ///     If defined this specifies the name of the current environment, e.g. Local, Test or Prod.
            ///     If this is not specified (or is empty) then the environment will be taken from the app
            ///     setting <see cref="AppSettingNames.EnvironmentName"/>.
            /// </summary>
            public const string EnvironmentName = "DASENV";
        }

        public static class AppSettingNames
        {
            /// <summary>
            ///     If defined  this specifies the name of the current environment, e.g. Local, Test or Prod.
            ///     This is used if the Environment is not specified by <see cref="EnvironmentVariableNames.EnvironmentName"/>.
            /// </summary>
            public const string EnvironmentName = "EnvironmentName";
        }

        /// <summary>
        ///     Specifies the recognised environment name values
        /// </summary>
        public static class EnvironmentNames
        {
            public const string Local = "LOCAL";
            public const string AT = "AT";
            public const string Test = "TEST";
            public const string Test2 = "TEST2";
            public const string PreProd = "PP";
            public const string Production = "PRD";
            public const string Demo = "DEMO";
            public const string ModelOffice = "MO";
        }
    }
}