using UnityEngine;

public class RandomAnimation : StateMachineBehaviour
{
    [SerializeField] private string parameterName;
    [SerializeField] private int countAnimation;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) =>
		animator.SetFloat(parameterName, Random.Range(0, countAnimation - 1));
	
}
