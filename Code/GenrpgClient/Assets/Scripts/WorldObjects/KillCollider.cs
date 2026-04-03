using System;

public class KillCollider : BaseBehaviour
{

    protected IMapGenData _md;
    DateTime lastCollideTime = DateTime.UtcNow;
    private void OnTriggerEnter(UnityEngine.Collider other)
    {

        if (_md.GeneratingMap)
        {
            return;
        }
        if ((DateTime.UtcNow - lastCollideTime).TotalSeconds < 1)
        {
            return;
        }

        MonsterController cont = _clientEntityService.FindInParents<MonsterController>(other.gameObject);

        if (cont == null)
        {
            return;
        }
    }
}

