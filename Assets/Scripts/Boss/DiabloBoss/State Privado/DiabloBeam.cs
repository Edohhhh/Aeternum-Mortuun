using UnityEngine;

public class DiabloBeam
{
    public GameObject GO { get; private set; }
    public LineRenderer LR { get; private set; }
    public BoxCollider2D COL { get; private set; }
    public DevilBeamDamage DMG { get; private set; }

    public static DiabloBeam Create(
        Transform parent,
        Vector2 localOffset,
        float angleDeg,
        float length,
        float width,
        Material mat,
        Color color,
        bool damageOn,
        float dps,
        float interval,
        LayerMask targetMask)
    {
        var b = new DiabloBeam();

        b.GO = new GameObject($"DevilBeam_{angleDeg:0}");
        b.GO.transform.SetParent(parent, worldPositionStays: false);
        b.GO.transform.localPosition = localOffset;
        b.GO.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);

        // Visual
        b.LR = b.GO.AddComponent<LineRenderer>();
        b.LR.useWorldSpace = false;
        b.LR.positionCount = 2;
        b.LR.numCapVertices = 6;
        b.LR.numCornerVertices = 6;
        b.LR.widthMultiplier = width;
        b.LR.material = mat ? mat : new Material(Shader.Find("Sprites/Default"));
        b.LR.startColor = b.LR.endColor = color;
        b.LR.sortingLayerName = "Effects";
        b.LR.sortingOrder = 10;

        float half = length * 0.5f;
        b.LR.SetPosition(0, new Vector3(-half, 0f, 0f));
        b.LR.SetPosition(1, new Vector3(+half, 0f, 0f));

        // Colisión
        b.COL = b.GO.AddComponent<BoxCollider2D>();
        b.COL.isTrigger = true;
        b.COL.size = new Vector2(length, width);
        b.COL.offset = Vector2.zero;
        b.COL.enabled = damageOn;

        var rb2d = b.GO.AddComponent<Rigidbody2D>();
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.simulated = true;

        // Daño por tick propio del rayo
        b.DMG = b.GO.AddComponent<DevilBeamDamage>();
        b.DMG.Init(dps, interval, targetMask);
        b.DMG.enabled = damageOn;

        return b;
    }

    public static DiabloBeam[] SpawnMany(
        Transform parent, Vector2 localOffset, float[] anglesDeg,
        float length, float width, Material mat, Color color,
        bool damageOn, float dps, float interval, LayerMask targetMask)
    {
        var arr = new DiabloBeam[anglesDeg.Length];
        for (int i = 0; i < anglesDeg.Length; i++)
            arr[i] = Create(parent, localOffset, anglesDeg[i], length, width, mat, color, damageOn, dps, interval, targetMask);
        return arr;
    }

    public static void SetWidth(DiabloBeam[] beams, float width, float lengthOverride = -1f)
    {
        foreach (var b in beams)
        {
            if (b == null) continue;
            if (b.LR) b.LR.widthMultiplier = width;
            if (b.COL)
            {
                var size = b.COL.size;
                if (lengthOverride > 0f) size.x = lengthOverride;
                size.y = width;
                b.COL.size = size;
            }
        }
    }

    public static void SetDamage(DiabloBeam[] beams, bool enabled)
    {
        foreach (var b in beams)
        {
            if (b == null) continue;
            if (b.COL) b.COL.enabled = enabled;
            if (b.DMG) b.DMG.enabled = enabled;
        }
    }

    public static void SetColor(DiabloBeam[] beams, Color c)
    {
        foreach (var b in beams)
            if (b?.LR) { b.LR.startColor = b.LR.endColor = c; }
    }

    public static void DestroyMany(DiabloBeam[] beams)
    {
        foreach (var b in beams)
            if (b?.GO) Object.Destroy(b.GO);
    }
}
