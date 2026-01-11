using UnityEngine;

public interface IMaterial
{
    int giveMaterial();

    void materialDamage(int amount);

    string materialType();
}
