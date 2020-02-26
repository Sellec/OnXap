namespace System.Web.Mvc.Html
{
    static class PvlExtensions
    {
        public static void MergePvlAttribute(this TagBuilder tagBuilder, VueAttribute tagAttribute, string htmlAttributeName, bool isModelBinding = false)
        {
            if (tagAttribute != null)
            {
                tagBuilder.MergeAttribute(tagAttribute.RenderName(htmlAttributeName, isModelBinding), tagAttribute.RenderValue(), true);
            }
        }
    }
}
