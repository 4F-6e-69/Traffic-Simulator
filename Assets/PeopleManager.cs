using UnityEngine;

public class PeopleManager : MonoBehaviour
{
    private bool isWalking = false;
    public void setAnimationWalking(Animator animator)
    {
        if (!isWalking)
        {
            animator.CrossFadeInFixedTime("Walking", 0.35f);
            isWalking = true;
        }
        else
        {
            isWalking = false;
            animator.CrossFadeInFixedTime("idle_f_1_150f", 0.28f);
        }
    }
}
