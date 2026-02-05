namespace VisitorSwitch.Classes;

public class PriceVisitor
{
    public decimal VisitPrice(Node node)
    {
        // Switch по Enum с рекурсией
        return node.Kind switch
        {
            ItemKind.SingleProduct => node.Price,
            ItemKind.Box => node.Children?.Sum(child => VisitPrice(child)) ?? 0,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

}