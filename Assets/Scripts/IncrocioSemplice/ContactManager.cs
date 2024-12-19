using UnityEngine;

public enum ContactDirection {
    Nord,
    Sud,
    Est,
    Ovest
}

public class ContactManager : MonoBehaviour
{
    [SerializeField] private GameObject Nord_object;
    private string nordRoadName;
    [SerializeField] private GameObject Sud_object;
    private string sudRoadName;
    [SerializeField] private GameObject Est_object;
    private string estRoadName;
    [SerializeField] private GameObject Ovest_object;
    private string ovestRoadName;
}
