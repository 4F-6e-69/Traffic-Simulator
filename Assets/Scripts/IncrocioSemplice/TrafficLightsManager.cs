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
    [SerializeField] private float switchRate = 15f;

    private float timer = 0f;
    private bool switchState = false; private bool isOrange = false;
    private IntersectionState currentState, prevState;

    private void Start() {
        var randInt = Random.Range(1, 100);

        if (randInt >= 50) {
            setRed(trafficLights[0]);
            setGreen(trafficLights[1]);
            setRed(trafficLights[2]);
            setGreen(trafficLights[3]);

            currentState = IntersectionState.Vertical;
        } else {
            setGreen(trafficLights[0]);
            setRed(trafficLights[1]);
            setGreen(trafficLights[2]);
            setRed(trafficLights[3]);

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
            setYellow(trafficLights[1]);
            setYellow(trafficLights[3]);

            currentState = IntersectionState.VerticalOrange;
        }else {
            setYellow(trafficLights[0]);
            setYellow(trafficLights[2]);

            currentState = IntersectionState.HorizontalOrange;
        }
    }

    private void SwitchOrange () {
        prevState = currentState;
        
        setRed(trafficLights[1]);
        setRed(trafficLights[3]);
        setRed(trafficLights[0]);
        setRed(trafficLights[2]);

        currentState = IntersectionState.None;
        isOrange = true;
    }

    private void SwitchLights () {
        switchState = false; isOrange = false;

        if (prevState == IntersectionState.HorizontalOrange) {
            setGreen(trafficLights[1]);
            setGreen(trafficLights[3]);

            currentState = IntersectionState.Vertical;
        }else if (prevState == IntersectionState.VerticalOrange) {
            setGreen(trafficLights[0]);
            setGreen(trafficLights[2]);

            currentState = IntersectionState.Horizontal;
        }
    }

    private void setRed(Light light) {
        var redPosition = new Vector3(light.transform.position.x, 1.4f, light.transform.position.z);
        light.transform.position = redPosition;
        light.color = new Color(250, 0, 0);
    }

    private void setYellow (Light light) {
        var redPosition = new Vector3(light.transform.position.x, 1.25f, light.transform.position.z);
        light.transform.position = redPosition;
        light.color = new Color(205, 215, 108);
    }

    private void setGreen (Light light) {
        var redPosition = new Vector3(light.transform.position.x, 1.13f, light.transform.position.z);
        light.transform.position = redPosition;
        light.color = new Color(0, 255, 0);
    }
}
