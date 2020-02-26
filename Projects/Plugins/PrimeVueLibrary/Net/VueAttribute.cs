namespace System.Web.Mvc.Html
{
    /// <summary>
    /// Abstract representation of Vue's component's tag attribute value or binding.
    /// </summary>
    /// <seealso cref="VueValue"/>
    public abstract class VueAttribute
    {
        internal VueAttribute(string stringRepresentation)
        {
            StringRepresentation = stringRepresentation;
        }

        internal string StringRepresentation { get; private set; }

        internal abstract string RenderName(string attributeName, bool isTwoWay);
        internal abstract string RenderValue();
    }

    /// <summary>
    /// Direct value of component's tag attribute.
    /// </summary>
    public sealed class VueValue : VueAttribute
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public VueValue(object value) :
            base(value != null ? value.ToString() : "")
        {
        }

        internal override string RenderName(string attributeName, bool isTwoWay)
        {
            return attributeName;
        }

        internal override string RenderValue()
        {
            return StringRepresentation;
        }
    }

    /// <summary>
    /// One- or two-way binding of Vue's component's tag attribute with Vue's "data" object property.
    /// </summary>
    public sealed class VueBinding : VueAttribute
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="dataPropertyName">The name of component's "data" object property path, delimited with dots.</param>
        public VueBinding(string dataPropertyName) :
            base(CheckAndReturn(dataPropertyName))
        {
        }

        private static string CheckAndReturn(string dataPropertyName)
        {
            if (string.IsNullOrEmpty(dataPropertyName)) throw new ArgumentNullException(nameof(dataPropertyName));
            return dataPropertyName;
        }

        internal override string RenderName(string attributeName, bool isTwoWay)
        {
            return isTwoWay ? "v-model" : $"v-bind:{attributeName}";
        }

        internal override string RenderValue()
        {
            return StringRepresentation;
        }
    }
}
