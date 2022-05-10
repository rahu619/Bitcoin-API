using System.Reflection;

namespace BitCoin.API.Test.Helper
{
    public static class TestHelperExtensions
    {
        private static BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public static TReturn InvokePrivateMethod<TReturn, TClass>(this TClass instance, string methodName, params object[] parameters) where TClass : class
        {
            var methodInfo = instance.GetType().GetMethod(methodName, bindingAttr);
            return (TReturn)methodInfo.Invoke(instance, parameters);
        }

        public static TReturn InvokePrivateField<TReturn, TClass>(this TClass instance, string propertyName) where TClass : class
        {
            var propertyInfo = instance.GetType().GetField(propertyName, bindingAttr);
            return (TReturn)propertyInfo.GetValue(instance);
        }
    }
}
