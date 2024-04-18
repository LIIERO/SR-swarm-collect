using UnityEngine;

public interface IVessel : IDetectable
{
    public void Fill(ICollectable collectableToFill);
    public int GetFilledCollectableCount();
}
