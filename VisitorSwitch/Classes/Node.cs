namespace VisitorSwitch.Classes;

public class Node
{
    public ItemKind Kind { get; set; }
    public decimal Price { get; set; } // Только для товара
    public List<Node>? Children { get; set; } // Только для коробки

}