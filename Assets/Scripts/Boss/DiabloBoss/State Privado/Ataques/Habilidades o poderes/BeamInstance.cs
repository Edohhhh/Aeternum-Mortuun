using UnityEngine;

public class BeamInstance
{
    public GameObject go;
    public SpriteRenderer sr;
    public DevilBeamDamage dmg;

    public BeamInstance(GameObject obj)
    {
        go = obj;
        sr = obj.GetComponent<SpriteRenderer>();
        dmg = obj.GetComponent<DevilBeamDamage>();
    }

    public void SetColor(Color c)
    {
        if (sr) sr.color = c;
    }

    public void SetWidth(float w)
    {
        Vector3 s = go.transform.localScale;
        s.x = w;
        go.transform.localScale = s;
    }

    public void SetDamage(bool enabled)
    {
        if (dmg) dmg.enabled = enabled;
    }

    public void Destroy()
    {
        if (go) Object.Destroy(go);
    }
}

