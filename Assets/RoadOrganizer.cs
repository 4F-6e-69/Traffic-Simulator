using UnityEngine;

public class RoadOrganizer : MonoBehaviour
{
    [ContextMenu("Organize")]
    public void Organize() {
        GameObject roadContainer = GameObject.Find("Roads");
        int i = roadContainer.transform.childCount;

        for (int j = 0; j < i; j++) {
            string name = "";
            int count = roadContainer.transform.GetChild(j).gameObject.name.Split('-').Length;
            for (int k = 0; k < count-1; k++) {
                name += roadContainer.transform.GetChild(j).gameObject.name.Split('-')[k];
                if (k < count-2) {
                    name += "-";
                }else {
                    name += "_";
                }
            }
            
            name += System.Guid.NewGuid().ToString();
            Debug.Log(name);
            roadContainer.transform.GetChild(j).gameObject.name = name;
        }
    }
}
