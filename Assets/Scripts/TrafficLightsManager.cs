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
            SetRed(trafficLights[0]);
            SetGreen(trafficLights[1]);
            SetRed(trafficLights[2]);
            SetGreen(trafficLights[3]);

            currentState = IntersectionState.Vertical;
        } else {
            SetGreen(trafficLights[0]);
            SetRed(trafficLights[1]);
            SetGreen(trafficLights[2]);
            SetRed(trafficLights[3]);

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
            SetYellow(trafficLights[1]);
            SetYellow(trafficLights[3]);

            currentState = IntersectionState.VerticalOrange;
        }else {
            SetYellow(trafficLights[0]);
            SetYellow(trafficLights[2]);

            currentState = IntersectionState.HorizontalOrange;
        }
    }

    private void SwitchOrange () {
        prevState = currentState;
        
        SetRed(trafficLights[1]);
        SetRed(trafficLights[3]);
        SetRed(trafficLights[0]);
        SetRed(trafficLights[2]);

        currentState = IntersectionState.None;
        isOrange = true;
    }

    private void SwitchLights () {
        switchState = false; isOrange = false;

        if (prevState == IntersectionState.HorizontalOrange) {
            SetGreen(trafficLights[1]);
            SetGreen(trafficLights[3]);

            currentState = IntersectionState.Vertical;
        }else if (prevState == IntersectionState.VerticalOrange) {
            SetGreen(trafficLights[0]);
            SetGreen(trafficLights[2]);

            currentState = IntersectionState.Horizontal;
        }
    }

    private void SetRed(Light light) {
        var redPosition = new Vector3(light.transform.position.x, 1.4f, light.transform.position.z);
        light.transform.position = redPosition;
        light.color = new Color(250, 0, 0);
    }

    private void SetYellow (Light light) {
        var redPosition = new Vector3(light.transform.position.x, 1.25f, light.transform.position.z);
        light.transform.position = redPosition;
        light.color = new Color(205, 215, 108);
    }

    private void SetGreen (Light light) {
        var redPosition = new Vector3(light.transform.position.x, 1.13f, light.transform.position.z);
        light.transform.position = redPosition;
        light.color = new Color(0, 255, 0);
    }


}
