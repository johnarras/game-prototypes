using OxDb.Client.Assets.Materials;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OxDb.Client.Assets.Scripts.Assets.Materials
{
    public class RendererWithMaterials
    {
        public MeshRenderer Renderer { get; set; }
        public Material[] SharedMaterials { get; set; }
        public Material[] FadeMaterials { get; set; }
        public Color[] SharedBaseColors { get; set; }
    }

    public class GameObjectRendererList
    {
        public GameObject Go { get; set; }
        public List<RendererWithMaterials> RendererMaterials { get; set; } = new List<RendererWithMaterials>();
    }
    public class ObjectFader : BaseBehaviour
    {
        /// <summary>
        /// Use this when players interact with the objects in the room
        /// </summary>
        public void ClearObjects()
        {
            foreach (GameObjectRendererList glist in _objects)
            {
                _clientEntityService.Destroy(glist.Go);
                foreach (RendererWithMaterials rmat in glist.RendererMaterials)
                {
                    if (rmat.FadeMaterials != null)
                    {
                        foreach (Material mat in rmat.FadeMaterials)
                        {
                            _clientEntityService.Destroy(mat);
                        }
                        rmat.FadeMaterials = null;
                    }
                    rmat.Renderer = null;
                    rmat.SharedMaterials = null;
                    rmat.FadeMaterials = null;
                }
            }
            _objects.Clear();
        }

        private List<GameObjectRendererList> _objects = new List<GameObjectRendererList>();

        public void AddObject(GameObject prop)
        {

            GameObjectRendererList glist = new GameObjectRendererList()
            {
                Go = prop,
            };

            List<MeshRenderer> renderers = _clientEntityService.GetComponents<MeshRenderer>(prop);

            foreach (MeshRenderer renderer in renderers)
            {
                RendererWithMaterials rendMat = new RendererWithMaterials()
                {
                    Renderer = renderer,
                    SharedMaterials = renderer.sharedMaterials,
                };

                rendMat.SharedBaseColors = new Color[rendMat.SharedMaterials.Length];

                for (int m = 0; m < rendMat.SharedMaterials.Length; m++)
                {
                    rendMat.SharedBaseColors[m] = rendMat.SharedMaterials[m].color;
                }

                glist.RendererMaterials.Add(rendMat);
            }

            _objects.Add(glist);
        }

        public void SetObjectAlphas(float alpha)
        {

            alpha = MathUtil.Clamp(0, alpha, 1);

            foreach (GameObjectRendererList rlist in _objects)
            {
                if (alpha == 0)
                {
                    _clientEntityService.SetActive(rlist.Go, false);
                }
                else if (alpha == 1)
                {
                    _clientEntityService.SetActive(rlist.Go, true);
                    foreach (RendererWithMaterials rmat in rlist.RendererMaterials)
                    {
                        rmat.Renderer.sharedMaterials = rmat.SharedMaterials;
                    }
                }
                else
                {
                    _clientEntityService.SetActive(rlist.Go, true);
                    foreach (RendererWithMaterials rmat in rlist.RendererMaterials)
                    {
                        if (rmat.FadeMaterials == null)
                        {
                            rmat.FadeMaterials = new Material[rmat.SharedMaterials.Length];
                            for (int m = 0; m < rmat.SharedMaterials.Length; m++)
                            {
                                rmat.FadeMaterials[m] = MaterialUtils.CreateTransparentVariant(rmat.SharedMaterials[m]);
                            }
                        }

                        for (int m = 0; m < rmat.FadeMaterials.Length; m++)
                        {
                            Color bc = rmat.SharedBaseColors[m];
                            rmat.FadeMaterials[m].color = new Color(bc.r, bc.g, bc.b, alpha);
                        }

                        rmat.Renderer.sharedMaterials = rmat.FadeMaterials;
                    }
                }
            }
        }
    }
}
