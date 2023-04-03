using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CommonEditorElements: Editor
{
    public static void MakePlayerButton(GameObject selected)
    {
        if (GUILayout.Button("Make Player"))
        {
            Area area = AreaManager.Instance.FindAreaOf(selected.transform);
            if (area != null)
            {
                Player player = area.AreaPlayer;
                int artIndex = selected.transform.GetSiblingIndex();
                if (player.transform.childCount > 0)
                {

                    GameObject oldPlayerGroup = FlatgameMakerUtils.InstantiateAtPath(FlatgameMaker.groupPrefabPath);
                    oldPlayerGroup.transform.position = player.transform.position;
                    oldPlayerGroup.transform.parent = player.transform.parent;
                    oldPlayerGroup.name = "Old " + player.gameObject.name;
                    foreach (Transform t in player.transform.GetChildren())
                    {
                        t.parent = oldPlayerGroup.transform;
                    }
                    oldPlayerGroup.transform.SetSiblingIndex(player.transform.GetSiblingIndex());
                }
                player.transform.SetSiblingIndex(artIndex);
                player.transform.parent = selected.transform.parent;
                selected.transform.parent = player.transform;
            }
        }
    }
}
