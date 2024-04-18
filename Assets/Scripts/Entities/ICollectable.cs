public interface ICollectable : IDetectable
{
    public void Reserve();
    public bool IsReserved();
    public void Collect(IDetectable collectedBy);
    public bool IsCollected();
    public void Dispose();
}
