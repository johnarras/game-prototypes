namespace Assets.Scripts.Updates
{
    public class GlobalUpdateObject : BaseBehaviour
    {
        private IClientUpdateService _clientUpdateService = null;

        private void Update()
        {
            _clientUpdateService?.OnUpdate();
        }

        private void LateUpdate()
        {
            _clientUpdateService?.OnLateUpdate();
        }

    }
}


