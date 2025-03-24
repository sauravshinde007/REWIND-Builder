using XNode;

public class ChoiceNode : BaseNode {

	[Input] public string input; // Input connection for the dialogue flow
	[Output(dynamicPortList = true)] public string[] choices; // Dynamic outputs for each choice

	// Ensure the choices array matches the number of texts
	public override object GetValue(NodePort port)
	{
		return null; // Values aren't used directly; ports define connections
	}
}