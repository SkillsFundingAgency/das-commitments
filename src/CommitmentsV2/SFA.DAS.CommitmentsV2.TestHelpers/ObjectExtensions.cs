namespace SFA.DAS.CommitmentsV2.TestHelpers
{
    public static class ObjectExtensions
    {
        public static void SetValue(this object o, string propertyName, object value)
        {
            var t = o.GetType();
            t.GetProperty(propertyName).SetValue(o, value, null);
        }
    }
}
