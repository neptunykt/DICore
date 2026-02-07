namespace DIMainFrame.Classes;

public interface ICloset
{

}

public class Closet : ICloset
{
    public Closet(IDoor door, IHinges hinges)
    {
        
    }
}

public interface IDoor
{
    
}

public class Door : IDoor
{
    public Door()
    {
        
    }
}

public interface IHinges
{
    
}

public class Hinges : IHinges
{
    public Hinges()
    {
        
    }
}