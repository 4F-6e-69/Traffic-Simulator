using UnityEngine;
using CityPeople;

public class PeopleAnimation : MonoBehaviour
{
    public Animator animator;
    private movimento move;
    public bool isArrived = false;

    private void Start() {
        animator = gameObject.GetComponent<Animator>();
        move = gameObject.transform.parent.GetComponent<movimento>();
    }

    void Update() { 
        isArrived = move.IsArrived;
        animator.SetBool("isArrived", isArrived);

        if (isArrived) {
            
        }
    }
}

