namespace Barred_Client;

public static class VisualTreeExtensions
{
    public static IEnumerable<T> GetDescendantsOfType<T>(this Element element) where T : Element
    {
        if (element is IElementController elementController)
        {
            foreach (var child in elementController.LogicalChildren)
            {
                
                if (child is T match)
                    yield return match;

                foreach (var descendant in child.GetDescendantsOfType<T>())
                    yield return descendant;
            }
        }
    }
}