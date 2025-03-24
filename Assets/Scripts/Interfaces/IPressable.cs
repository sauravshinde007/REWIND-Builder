// For buttons, Pressure-pads, etc.
public interface IPressable {

    public bool IsPressed { get; set; }     // Is Button Pressed?

    public void Press();
    public void Release();

    // Enable/Disable Buttons
    public void Enable();
    public void Disable();

}