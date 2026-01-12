using UnityEngine;

public interface IStore
{

    int grabMaterial(int amount, string type);

    int displayMaterial(string type);

}