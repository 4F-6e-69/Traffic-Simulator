using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum IntersectionState {
    Horizontal,
    Vertical,
    HorizontalOrange,
    VerticalOrange,
    None
}

public class TrafficLightsManager : MonoBehaviour
{
    [SerializeField] private List<Light> trafficLights; 
    [SerializeField] private List<GameObject> trafficLightsObjects;
    [SerializeField] private float switchRate = 15f;

    private float timer = 0f;
    private bool switchState = false; private bool isOrange = false;
    private IntersectionState currentState, prevState;

    private void Start() {
        var randInt = Random.Range(1, 100);

        if (randInt >= 50) {
            setRed(trafficLights[0], trafficLightsObjects[0]);
            setGreen(trafficLights[1], trafficLightsObjects[1]);
            setRed(trafficLights[2], trafficLightsObjects[2]);
            setGreen(trafficLights[3], trafficLightsObjects[3]);

            currentState = IntersectionState.Vertical;
        } else {
            setGreen(trafficLights[0], trafficLightsObjects[0]);
            setRed(trafficLights[1], trafficLightsObjects[1]);
            setGreen(trafficLights[2], trafficLightsObjects[2]);
            setRed(trafficLights[3], trafficLightsObjects[3]);

            currentState = IntersectionState.Horizontal;
        }
    }

    private void Update() {
        timer = timer + Time.deltaTime;

        if (timer >= switchRate) {
            if (!switchState)  {
                Switching();
            }

            if (timer >= (switchRate * 1.25f) && isOrange == false) {
                SwitchOrange();
            } 
        }

        if (isOrange == true) {
            if (timer >= 1.45f * switchRate) {
                SwitchLights();
                timer = 0f;
            }
        }

    }


    private void Switching() {
        switchState = true;

        if (currentState == IntersectionState.Vertical) {
            setYellow(trafficLights[1], trafficLightsObjects[1]);
            setYellow(trafficLights[3], trafficLightsObjects[3]);

            currentState = IntersectionState.VerticalOrange;
        }else {
            setYellow(trafficLights[0], trafficLightsObjects[0]);
            setYellow(trafficLights[2], trafficLightsObjects[2]);

            currentState = IntersectionState.HorizontalOrange;
        }
    }

    private void SwitchOrange () {
        prevState = currentState;
        
        setRed(trafficLights[1], trafficLightsObjects[1]);
        setRed(trafficLights[3], trafficLightsObjects[3]);
        setRed(trafficLights[0], trafficLightsObjects[0]);
        setRed(trafficLights[2], trafficLightsObjects[2]);

        currentState = IntersectionState.None;
        isOrange = true;
    }

    private void SwitchLights () {
        switchState = false; isOrange = false;

        if (prevState == IntersectionState.HorizontalOrange) {
            setGreen(trafficLights[1], trafficLightsObjects[1]);
            setGreen(trafficLights[3], trafficLightsObjects[3]);

            currentState = IntersectionState.Vertical;
        }else if (prevState == IntersectionState.VerticalOrange) {
            setGreen(trafficLights[0], trafficLightsObjects[0]);
            setGreen(trafficLights[2], trafficLightsObjects[2]);

            currentState = IntersectionState.Horizontal;
        }
    }

    private void setRed(Light light, GameObject objectLight) {
        var redPosition = new Vector3(light.transform.position.x, 1.4f, light.transform.position.z);
        light.transform.position = redPosition;
        light.color = new Color(250, 0, 0);
        objectLight.GetComponent<Renderer>().material.color = new Color(250, 0, 0, 0.1f);
    }

    private void setYellow (Light light, GameObject objectLight) {
        var redPosition = new Vector3(light.transform.position.x, 1.25f, light.transform.position.z);
        light.transform.position = redPosition;
        light.color = new Color(205, 215, 108);
        objectLight.GetComponent<Renderer>().material.color = new Color(205, 215, 108, 0.1f);
    }

    private void setGreen (Light light, GameObject objectLight) {
        var redPosition = new Vector3(light.transform.position.x, 1.13f, light.transform.position.z);
        light.transform.position = redPosition;
        light.color = new Color(0, 255, 0);
        objectLight.GetComponent<Renderer>().material.color = new Color(0, 255, 0, 0.1f);
    }

    public IntersectionState GetTrafficLightState() {
        return currentState;
    }

    public IntersectionState GetCurrenteAxis(Vector3 intersectionEnter) {
        Vector3 nearestLight = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        float distance = Mathf.Infinity;

        for (int i = 0; i < trafficLights.Count; i++) {
            if (Vector3.Distance(trafficLights[i].transform.position, intersectionEnter) < distance) {
                nearestLight = trafficLights[i].transform.position;
                distance = Vector3.Distance(trafficLights[i].transform.position, intersectionEnter);
            }

        }

        if (Vector3.Distance(nearestLight, trafficLights[0].transform.position) < 0.23f  || Vector3.Distance(nearestLight, trafficLights[2].transform.position) < 0.23f)  {
            return IntersectionState.Horizontal;
        } else if (Vector3.Distance(nearestLight, trafficLights[1].transform.position) < 0.23f || Vector3.Distance(nearestLight, trafficLights[3].transform.position) < 0.23f) {
            return IntersectionState.Vertical;
        }

        return IntersectionState.None;
    }

}
